using System.Text.Json.Serialization;
namespace OpenGDS2.Domain;

public sealed class RaceRunner
{
    [JsonRequired] public int SourceIndex {get;set;}
    [JsonRequired] public string Name {get;set;}=string.Empty;
    [JsonRequired] public int JockeySourceIndex {get;set;}
    [JsonRequired] public string JockeyName {get;set;}=string.Empty;
    [JsonRequired] public double Strength {get;set;}
    [JsonRequired] public int WeightChange {get;set;}
}
public sealed class HorseRaceState
{
    [JsonRequired] public int Stage {get;set;} // 0 draft, 1 race, 2 result timer, 3 payout modal.
    [JsonRequired] public int RaceCounter {get;set;}
    [JsonRequired] public long DraftCash {get;set;}
    [JsonRequired] public decimal TemporaryBalance {get;set;}
    [JsonRequired] public List<RaceRunner> Runners {get;set;}=[];
    [JsonRequired] public List<List<int>> ForecastRanks {get;set;}=[];
    [JsonRequired] public List<double> WinOdds {get;set;}=[];
    [JsonRequired] public List<List<double>> PairOdds {get;set;}=[];
    [JsonRequired] public List<long> SingleStakes {get;set;}=[0,0,0,0,0,0,0,0];
    [JsonRequired] public List<List<long>> PairStakes {get;set;}=Enumerable.Range(0,8).Select(_=>new List<long>{0,0,0,0,0,0,0,0}).ToList();
    [JsonRequired] public int SelectedFirst {get;set;}
    [JsonRequired] public int SelectedSecond {get;set;}=1;
    [JsonRequired] public bool PairSelected {get;set;}
    [JsonIgnore] public int HeldAdjustment {get;set;} // Physical input never resumes from a save.
    [JsonRequired] public int Tick {get;set;}
    [JsonRequired] public int RacePhase {get;set;}
    [JsonRequired] public int Mode {get;set;}
    [JsonRequired] public double Camera {get;set;}
    [JsonRequired] public int Leader {get;set;}
    [JsonRequired] public double FrontDistance {get;set;}
    [JsonRequired] public List<double> Performance {get;set;}=[];
    [JsonRequired] public List<double> Positions {get;set;}=[];
    [JsonRequired] public List<double> FinishCoordinates {get;set;}=[];
    [JsonRequired] public List<int> Arrival {get;set;}=[];
    [JsonRequired] public int ResultTick {get;set;}
    [JsonRequired] public decimal DisplayedAward {get;set;}
    [JsonRequired] public bool NumericBoundary {get;set;}
    [JsonRequired] public bool CatalogBoundary {get;set;}
}
public static class CasinoHorseService
{
    public const double BetSeconds=.100,RaceSeconds=.080,ResultSeconds=.100;
    public static readonly IReadOnlyList<double> Odds=Array.AsReadOnly(new[]{2.1,4.5,5.7,7.5,8.9,11,28.6,47.7});
    public static bool HasCatalog(OriginalCatalog? c)=>c?.Horses is {Count:44} horses && c.Jockeys is {Count:10} jockeys &&
        horses.All(h=>h is not null) && jockeys.All(j=>j is not null) &&
        horses.Select(h=>h.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,44)) && jockeys.Select(j=>j.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,10)) &&
        horses.All(h=>!string.IsNullOrWhiteSpace(h.Name) && double.IsFinite(h.Strength) && h.Strength is >=0 and <44 && h.Strength==h.SourceIndex+h.RawParameter/100) &&
        jockeys.All(j=>!string.IsNullOrWhiteSpace(j.Name));
    public static CasinoResult Open(GameState s,OriginalCatalog? catalog,Random random)
    {
        if (!CasinoPokerService.OwnsCasino(s)) return CasinoResult.NotOwned;
        if (FacilityService.HasPendingDecision(s)) return CasinoResult.EventPending;
        if (s.Money is null) return CasinoResult.MoneyUnknown;
        if (!HasCatalog(catalog)) return CasinoResult.CatalogMissing;
        _=random.NextSingle(); // Form_Load 009896AC entry.
        int counter=s.Casino.Horse.RaceCounter;Setup(s,catalog!,random,counter);s.Casino.HorseOpen=true;return CasinoResult.Success;
    }
    private static void Setup(GameState s,OriginalCatalog catalog,Random random,int counter)
    {
        _=random.NextSingle(); // 009D46D0 entry, ordinary mode 0 has no special-mode draw.
        var r=new HorseRaceState {DraftCash=s.Money!.Value,TemporaryBalance=s.Money.Value,RaceCounter=counter};
        var horses=catalog.Horses.OrderBy(h=>h.SourceIndex).ToArray();var jockeys=catalog.Jockeys.OrderBy(j=>j.SourceIndex).ToArray();
        var selected=new List<int>();var chosenJockeys=new List<int>();
        while(selected.Count<8) {int id=(int)Math.Floor((double)random.NextSingle()*44);if(!selected.Contains(id)) selected.Add(id);}
        while(chosenJockeys.Count<8) {int id=(int)Math.Floor((double)random.NextSingle()*10);if(!chosenJockeys.Contains(id)) chosenJockeys.Add(id);}
        for(int i=0;i<8;i++) r.Runners.Add(new() {SourceIndex=selected[i],Name=horses[selected[i]].Name,Strength=horses[selected[i]].Strength,JockeySourceIndex=chosenJockeys[i],JockeyName=jockeys[chosenJockeys[i]].Name});
        for(int row=0;row<3;row++)
        {
            var scores=r.Runners.Select(h=>h.Strength+Math.Floor((double)random.NextSingle()*3)).ToArray();
            r.ForecastRanks.Add(scores.Select(score=>scores.Count(other=>other>=score)-1).ToList());
        }
        for(int i=0;i<8;i++)
        {
            int sign=(int)Math.Floor((double)random.NextSingle()*2)==0 ? -1 : 1;
            r.Runners[i].WeightChange=sign*((int)Math.Floor((double)random.NextSingle()*8)+1);
            r.WinOdds.Add(Odds[r.ForecastRanks[0][i]]);
            if(r.ForecastRanks[0][i]==0) r.Leader=i;
        }
        for(int first=0;first<8;first++)
        {
            var row=new List<double>();for(int second=0;second<8;second++) if(first!=second) row.Add(r.WinOdds[first]*r.WinOdds[second]/2.4);
            row.Add(0);r.PairOdds.Add(row); // Compressed columns 0..6; source leaves column 7 zero.
        }
        r.Performance=Enumerable.Repeat(0d,8).ToList();r.Positions=Enumerable.Repeat(0d,8).ToList();r.FinishCoordinates=Enumerable.Repeat(0d,8).ToList();
        s.Casino.Horse=r;
    }
    public static bool Close(GameState s)
    {
        if (!s.Casino.HorseOpen) return false;
        s.Casino.HorseOpen=false;s.Casino.Horse.HeldAdjustment=0;s.CompanyActivityPerformed=true;return true;
    }
    public static int CompressedColumn(int first,int second)=>second<first ? second : second-1;
    public static CasinoResult AdjustWager(GameState s,int direction)
    {
        var r=s.Casino.Horse;
        if (!s.Casino.HorseOpen || r.Stage!=0 || r.NumericBoundary || r.CatalogBoundary || direction is not (-1 or 1)) return CasinoResult.InvalidAction;
        int first=r.SelectedFirst,second=r.SelectedSecond,column=CompressedColumn(first,second);
        if (direction>0 && r.DraftCash<=0) return CasinoResult.InvalidAction;
        if (!r.PairSelected)
        {
            long value=r.SingleStakes[first];if(direction<0 && value<=0) return CasinoResult.InvalidAction;
            if(direction<0 && r.DraftCash>long.MaxValue-100) return CasinoResult.NumericLimit;
            r.SingleStakes[first]+=direction*100;r.DraftCash-=direction*100;
            for(int i=0;i<8;i++) r.SingleStakes[i]=direction>0 ? Math.Min(r.SingleStakes[i],99900) : r.SingleStakes[i]<100 ? 0 : r.SingleStakes[i];
        }
        else
        {
            // Original removal guard and clamp read the compressed column, not target.
            if(direction<0 && r.PairStakes[first][column]<=0) return CasinoResult.InvalidAction;
            int target=first==4 && second==3 ? 4 : second; // Recovered diagonal-write typo.
            long value=r.PairStakes[first][target];
            if(direction>0 && value>long.MaxValue-100 || direction<0 && (value<long.MinValue+100 || r.DraftCash>long.MaxValue-100)) return CasinoResult.NumericLimit;
            r.PairStakes[first][target]+=direction*100;r.DraftCash-=direction*100;
            long cell=r.PairStakes[first][column];r.PairStakes[first][column]=direction>0 ? Math.Min(cell,99900) : cell<100 ? 0 : cell;
        }
        r.TemporaryBalance=r.DraftCash;return CasinoResult.Success;
    }
    public static void TickBet(GameState s)
    {
        if(s.Casino.HorseOpen && s.Casino.Horse.HeldAdjustment is -1 or 1)
        {if(AdjustWager(s,s.Casino.Horse.HeldAdjustment)==CasinoResult.NumericLimit) s.Casino.Horse.NumericBoundary=true;}
    }
    public static CasinoResult Start(GameState s,Random random)
    {
        if(!s.Casino.HorseOpen) return CasinoResult.InvalidAction;
        _=random.NextSingle(); // 0099061C entry, including guarded modern rejects.
        var r=s.Casino.Horse;if(r.Stage!=0 || r.NumericBoundary || r.CatalogBoundary) return CasinoResult.InvalidAction;
        s.Money=r.DraftCash; // Direct writer, not financial accounting helper.
        r.HeldAdjustment=0;r.Stage=1;r.Tick=0;r.RacePhase=0;r.Arrival=[];
        for(int i=0;i<8;i++)
        {
            double p=r.Runners[i].Strength/1.2;
            if(Math.Floor((double)random.NextSingle()*12)==1) p+=6;
            else if(Math.Floor((double)random.NextSingle()*12)==2) p+=4;
            else p+=Math.Floor((double)random.NextSingle()*6)-2;
            r.Performance[i]=p;r.Positions[i]=40;
        }
        return CasinoResult.Success;
    }
    public static int? UniqueMaximum(IReadOnlyList<double> values)
    {
        double max=values.Max();return values.Count(x=>x==max)==1 ? Enumerable.Range(0,values.Count).First(i=>values[i]==max) : null;
    }
    public static double PhaseOneDelta(int tick,int mode,int slot,double p)
    {
        double divisor=0;
        if(tick is >=65 and <=80) divisor=(mode,slot) switch {(0,0)=>-5,(0,4)=>-4,(0,7)=>5,(1,0)=>5,(1,2)=>-4,(1,5)=>5,(1,7)=>-3,_=>0};
        if(tick is >=110 and <=130) divisor=(mode,slot) switch {(0,1)=>4,(0,2)=>3,(0,7)=>-4,(1,0)=>-4,(1,3)=>-3,(1,7)=>2.5,_=>0};
        if(tick is >=140 and <=180) divisor=(mode,slot) switch {(0,3)=>5,(0,4)=>5,(0,6)=>-5,(1,1)=>-7,(1,5)=>7,(1,7)=>-7,_=>0};
        return divisor==0 ? 0 : p/divisor;
    }
    // Signed divisors preserve each source division before addition/subtraction.
    private static readonly double[][][] Divisors=[
        [[7,-9,-9,9,-8,-9,9,10],[-8,7,-6,-8,9,7,-12,11]],
        [[-9,7,9,6,-9,8,-10,-9],[10,-10,10,-9,8,-7,10,-11]],
        [[8,-11,7,-10,7,7,6,9],[-9,-8,-9,-8,9,8,9,8]],
        [[8,-7,-9,6,-9,-8,6,-9],[-4,7,-6,6,7,-8,-6,8]]];
    public static (double X,double Z) PhaseTwoDelta(int tick,int mode,int slot,double p,double cameraStep)
    {
        double x=0,z=0;
        void Block(double divisor,bool initial=false) {double d=p/divisor;x+=d-(initial ? 0 : cameraStep);z+=d-10;}
        if(tick is >=200 and <=230) Block(-5,true);
        if(tick is >=231 and <=260) Block(Divisors[0][mode][slot]);
        if(tick is >=261 and <=290) Block(Divisors[1][mode][slot]);
        if(tick is >=290 and <=330) Block(-4); // Tick 290 runs BOTH blocks, in this order.
        if(tick is >=331 and <=360) Block(Divisors[2][mode][slot]);
        if(tick>=361) Block(Divisors[3][mode][slot]);
        return (x,z);
    }
    public static void TickRace(GameState s,Random random)
    {
        var r=s.Casino.Horse;if(!s.Casino.HorseOpen || r.Stage!=1 || r.NumericBoundary) return;
        if(r.RacePhase==0) for(int i=0;i<8;i++) r.Positions[i]+=7+r.Performance[i]/3;
        else if(r.RacePhase==1) for(int i=0;i<8;i++) r.Positions[i]+=PhaseOneDelta(r.Tick,r.Mode,i,r.Performance[i]);
        else
        {
            double step=0;if(r.Camera>-1010) r.Camera-=10;else step=10;
            for(int i=0;i<8;i++)
            {
                // Preserve individual source stores and operation grouping, including tick 290.
                void Block(double divisor)
                {double delta=r.Performance[i]/divisor;r.Positions[i]=(r.Positions[i]-step)+delta;r.FinishCoordinates[i]=(r.FinishCoordinates[i]-10)+delta;}
                if(r.Tick is >=200 and <=230) {r.Positions[i]-=r.Performance[i]/5;r.FinishCoordinates[i]-=10+r.Performance[i]/5;}
                if(r.Tick is >=231 and <=260) Block(Divisors[0][r.Mode][i]);
                if(r.Tick is >=261 and <=290) Block(Divisors[1][r.Mode][i]);
                if(r.Tick is >=290 and <=330) Block(-4);
                if(r.Tick is >=331 and <=360) Block(Divisors[2][r.Mode][i]);
                if(r.Tick>=361) Block(Divisors[3][r.Mode][i]);
                if(r.FinishCoordinates[i]<=-1920 && !r.Arrival.Contains(i)) r.Arrival.Add(i);
            }
            if(r.Arrival.Count==8) {r.Stage=2;r.ResultTick=0;}
        }
        r.Tick++;
        if(r.Tick==45)
        {
            r.Mode=(int)Math.Floor((double)random.NextSingle()*2);
            if(UniqueMaximum(r.Positions) is int leader) r.Leader=leader;r.RacePhase=1;
        }
        if(r.Tick==200)
        {
            if(UniqueMaximum(r.Positions) is int leader) r.FrontDistance=r.Positions[leader];
            for(int i=0;i<8;i++) r.Positions[i]=r.FinishCoordinates[i]=(r.FrontDistance-r.Positions[i])/1.6;
            r.Camera=700;r.RacePhase=2;
        }
    }
    public static decimal Award(HorseRaceState r)
    {
        if(r.Arrival.Count!=8) return 0;int first=r.Arrival[0],second=r.Arrival[1];decimal value=0;
        if(r.SingleStakes[first]>0) value=r.SingleStakes[first]*(decimal)r.WinOdds[first];
        // Native payout indexes compressed matrix using the raw arrival indices.
        if(r.PairStakes[first][second]>0) value+=r.PairStakes[first][second]*(decimal)Math.Round(r.PairOdds[first][second],1,MidpointRounding.ToEven);
        return value;
    }
    public static void TickResult(GameState s,OriginalCatalog? catalog,Random random)
    {
        var r=s.Casino.Horse;if(!s.Casino.HorseOpen || r.Stage!=2 || r.CatalogBoundary || r.NumericBoundary) return;
        if(++r.ResultTick==100)
        {
            int first=r.Arrival[0],second=r.Arrival[1];
            if(r.SingleStakes[first]>0 || r.PairStakes[first][second]>0) {r.DisplayedAward=Award(r);r.Stage=3;}
            else NextCard(s,catalog,random);
        }
    }
    private static void NextCard(GameState s,OriginalCatalog? catalog,Random random)
    {
        var r=s.Casino.Horse;if(!HasCatalog(catalog)) {r.CatalogBoundary=true;return;}
        int counter=r.RaceCounter+1;if(counter>=13) counter=1;
        // 009A12F8 +03FC -> Setup: overwrites temporary winnings with company funds.
        Setup(s,catalog!,random,counter);
    }
    public static bool ResumeCatalogBoundary(GameState s,OriginalCatalog? catalog,Random random)
    {
        if(!s.Casino.HorseOpen || !s.Casino.Horse.CatalogBoundary || s.Casino.Horse.Stage!=2 || !HasCatalog(catalog)) return false;
        NextCard(s,catalog,random);return true;
    }
    public static bool AcceptPayout(GameState s,OriginalCatalog? catalog,Random random)
    {
        var r=s.Casino.Horse;if(!s.Casino.HorseOpen || r.Stage!=3 || r.CatalogBoundary || !HasCatalog(catalog)) return false;
        r.TemporaryBalance+=r.DisplayedAward;NextCard(s,catalog,random);return true;
    }
    public static void Validate(GameState s)
    {
        if(s.Casino is not { } c || c.Horse is not { } r || r.Stage is <0 or >3 || r.RaceCounter is <0 or >12 || r.Tick<0 || r.RacePhase is <0 or >2 || r.Mode is <0 or >1 ||
            r.Leader is <0 or >7 || r.SelectedFirst is <0 or >7 || r.SelectedSecond is <0 or >7 || r.SelectedFirst==r.SelectedSecond || r.ResultTick is <0 or >100 || r.DisplayedAward<0 ||
            r.Runners is null || r.ForecastRanks is null || r.WinOdds is null || r.PairOdds is null || r.SingleStakes is null || r.PairStakes is null || r.Performance is null || r.Positions is null || r.FinishCoordinates is null || r.Arrival is null ||
            r.Runners.Count is not (0 or 8) || r.SingleStakes.Count!=8 || r.SingleStakes.Any(v=>v is <0 or >99900 || v%100!=0) || r.PairStakes.Count!=8 || r.PairStakes.Any(row=>row is null || row.Count!=8 || row.Any(v=>v%100!=0)) ||
            r.Arrival.Any(i=>i is <0 or >7) || r.Arrival.Distinct().Count()!=r.Arrival.Count || !double.IsFinite(r.Camera) || !double.IsFinite(r.FrontDistance) ||
            r.Runners.Count==8 && (r.Runners.Any(h=>h is null || h.SourceIndex is <0 or >43 || h.JockeySourceIndex is <0 or >9 || string.IsNullOrWhiteSpace(h.Name) || string.IsNullOrWhiteSpace(h.JockeyName) || !double.IsFinite(h.Strength) || h.Strength is <0 or >=44 || h.WeightChange is 0 or <-8 or >8) || r.Runners.Select(h=>h.SourceIndex).Distinct().Count()!=8 || r.Runners.Select(h=>h.JockeySourceIndex).Distinct().Count()!=8 ||
            r.ForecastRanks.Count!=3 || r.ForecastRanks.Any(row=>row is null || row.Count!=8 || row.Any(rank=>rank is <0 or >7)) || r.WinOdds.Count!=8 || r.WinOdds.Any(odd=>!Odds.Contains(odd)) || r.PairOdds.Count!=8 || r.PairOdds.Any(row=>row is null || row.Count!=8 || row.Any(v=>!double.IsFinite(v) || v is <0 or >1000)) ||
            r.WinOdds.Where((v,i)=>v!=Odds[r.ForecastRanks[0][i]]).Any() || r.PairOdds.Where((row,i)=>row[7]!=0 || row.Take(7).Where((v,col)=>v!=r.WinOdds[i]*r.WinOdds[col<i ? col : col+1]/2.4).Any()).Any() ||
            r.Performance.Count!=8 || r.Performance.Any(v=>!double.IsFinite(v) || v is <-2 or >43) || r.Positions.Count!=8 || r.FinishCoordinates.Count!=8 || r.Positions.Concat(r.FinishCoordinates).Any(v=>!double.IsFinite(v))) ||
            r.Stage>0 && r.Runners.Count!=8 || r.Stage is 2 or 3 && r.Arrival.Count!=8 || r.Stage==3 && r.ResultTick!=100 ||
            c.HorseOpen && (r.Runners.Count!=8 || !CasinoPokerService.OwnsCasino(s) || FacilityService.HasPendingDecision(s,ignoreCasino:true)) ||
            new[]{c.PokerOpen,c.BlackjackOpen,c.CardChainOpen,c.HorseOpen}.Count(x=>x)>1)
            throw new InvalidDataException("Invalid casino horse-racing state.");
    }
}
