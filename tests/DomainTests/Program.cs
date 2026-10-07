using OpenGDS2.Domain;
using OpenGDS2.Core;
using OpenGDS2.Persistence;
using OpenGDS2.Import;

bool AcknowledgeReviews(GameState state,ReleasedGameState product)
{
    while (ShipmentDialogueRules.Acknowledge(state,product)) {}
    return ReviewService.Acknowledge(state,product);
}

int passed = 0;
void Test(string name, Action test)
{
    try { test(); passed++; Console.WriteLine("PASS " + name); }
    catch (Exception ex) { Console.Error.WriteLine($"FAIL {name}: {ex}"); Environment.ExitCode = 1; }
}
void Check(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
void Throws<T>(Action action) where T : Exception
{ try { action(); } catch (T) { return; } throw new Exception("Expected " + typeof(T).Name); }
Test("meeting soundtrack maps planning alpha beta and final dialogue phases",()=>
{
    Check(Enum.GetValues<MeetingMusicPhase>().Select(MeetingMusicRules.ResourcePath).Distinct().Count()==4);
    Check(MeetingMusicRules.ResourcePath(MeetingMusicPhase.Planning).EndsWith("/planning.ogg"));
    Check(MeetingMusicRules.ForMilestone(DevelopmentMilestone.SelectType)==MeetingMusicPhase.Alpha);
    Check(MeetingMusicRules.ForMilestone(DevelopmentMilestone.SelectGenre)==MeetingMusicPhase.Beta);
    Check(MeetingMusicRules.ForMilestone(DevelopmentMilestone.StartDebugging)==MeetingMusicPhase.Final);
    Throws<ArgumentOutOfRangeException>(()=>MeetingMusicRules.ResourcePath((MeetingMusicPhase)99));
    Throws<ArgumentOutOfRangeException>(()=>MeetingMusicRules.ForMilestone((DevelopmentMilestone)99));
});
OriginalCatalog SecretaryCatalog()=>new() {Secretaries=Enumerable.Range(0,5).Select(i=>new SecretaryDefinition
    {SourceIndex=i,Name=$"Test secretary {i}",RankCode=Math.Min(i,3),ContractFee=100+i,AdvertisedSalary=10+i,
     PortraitId=SecretaryAdviceRules.PortraitId(i)!.Value,Description=$"Authored biography {i}"}).ToList()};
Test("secretary replacement charges once and changes identity between persisted conversations",()=>
{
    var s=new GameState {SecretarySourceIndex=1,Money=1000,Company=new() {Name="Synthetic company",PresidentName="Tester",WeeklyNet=3,MonthlyNet=4,YearlyNet=5}};
    var c=SecretaryCatalog();Check(SecretaryManagementRules.TryHire(s,c,2)==SecretaryHireResult.Success);
    Check(s.Money==898 && s.Company.WeeklyNet==-99 && s.Company.MonthlyNet==-98 && s.Company.YearlyNet==-97);
    Check(s.SecretarySourceIndex==1 && s.PendingSecretaryReplacement!.Phase==SecretaryReplacementPhase.Farewell);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(SecretaryManagementRules.TryHire(s,c,3)==SecretaryHireResult.EventPending && s.Money==898);
    Check(SecretaryManagementRules.Acknowledge(s));Check(s.SecretarySourceIndex==2 && s.PendingSecretaryReplacement!.Phase==SecretaryReplacementPhase.Greeting);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.Money==898 && SecretaryManagementRules.Acknowledge(s));
    Check(s.PendingSecretaryReplacement is null && !SecretaryManagementRules.Acknowledge(s) && s.SecretarySourceIndex==2);
    Check(SecretaryManagementRules.TryHire(s,c,1)==SecretaryHireResult.Success && s.Money==797);
});
Test("secretary hiring rejects unknown catalogs identities insufficient funds and current secretary without mutation",()=>
{
    var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},SecretarySourceIndex=1,Money=102};var c=SecretaryCatalog();
    Check(SecretaryManagementRules.CheckHire(s,c,1)==SecretaryHireResult.AlreadyEmployed);
    Check(SecretaryManagementRules.TryHire(s,c,3)==SecretaryHireResult.InsufficientFunds && s.Money==102 && s.PendingSecretaryReplacement is null);
    c.Secretaries.RemoveAt(4);Check(SecretaryManagementRules.TryHire(s,c,2)==SecretaryHireResult.StateUnknown);
    c=SecretaryCatalog();s.SecretarySourceIndex=null;Check(SecretaryManagementRules.TryHire(s,c,2)==SecretaryHireResult.StateUnknown);
    s.SecretarySourceIndex=1;s.PendingHireGreeting=new();Check(SecretaryManagementRules.TryHire(s,c,2)==SecretaryHireResult.EventPending);
    Throws<ArgumentOutOfRangeException>(()=>SecretaryManagementRules.TryHire(s,c,5));
});
Test("pending secretary replacement pauses simulation recruitment advice and policy mutations",()=>
{
    var(s,c)=AdviceFixture();s.SecretarySourceIndex=1;c.Secretaries=SecretaryCatalog().Secretaries;
    Check(SecretaryManagementRules.TryHire(s,c,2)==SecretaryHireResult.Success);
    var session=new GameSession(new SequenceRandom());session.Restore(s);long? money=s.Money;int? ticks=s.CompanyActivityTicks;
    session.TickOffice(c);Check(s.Money==money && s.CompanyActivityTicks==ticks);
    Throws<InvalidOperationException>(()=>session.OpenRecruitment());Check(session.Hire(Candidate(0,100))==HireResult.ConversationPending);
    Check(session.AskSecretary(c)==SecretaryAdviceResult.EventPending && !SecretaryPolicyRules.TrySet(s,new()));
});
Test("invalid replacement phases identities and overlapping events are rejected by saves",()=>
{
    Action<GameState>[] changes=[s=>s.SecretarySourceIndex=2,s=>s.PendingSecretaryReplacement!.NextSourceIndex=1,
        s=>s.PendingSecretaryReplacement!.Phase=(SecretaryReplacementPhase)9,s=>s.PendingSecretaryReplacement!.NextSourceIndex=5,
        s=>s.PendingSecretaryAdvice=new() {Messages=[SecretaryAdviceMessage.FundsHealthy]}];
    foreach(var change in changes)
    {
        var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},SecretarySourceIndex=1,Money=1000};SecretaryManagementRules.TryHire(s,SecretaryCatalog(),2);
        change(s);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    }
});
Test("secretary transaction arithmetic fails before charging or queueing",()=>
{
    var s=new GameState {SecretarySourceIndex=1,Money=1000,Company=new() {Name="Synthetic company",PresidentName="Tester",WeeklyNet=long.MinValue}};
    Throws<OverflowException>(()=>SecretaryManagementRules.TryHire(s,SecretaryCatalog(),2));
    Check(s.Money==1000 && s.SecretarySourceIndex==1 && s.PendingSecretaryReplacement is null);
});
Test("secretary production policy overrides difficulty and manual or unknown policy consumes no random draws",()=>
{
    foreach (var mode in Enum.GetValues<GameMode>())
    foreach (var policy in new SecretaryPolicyState?[]{null,new() {Production=ProductionPolicy.Manual},new() {Production=ProductionPolicy.Funds100Million},new()})
    {
        var s=new GameSession().StartNewGame("Policy","Tester",mode);s.Money=150000000;s.SecretaryPolicies=policy;s.CompanyActivityTicks=159;
        bool expands=policy?.Production==ProductionPolicy.Funds100Million;
        var rng=expands ? new SequenceRandom(0,0) : new SequenceRandom();CompanyActivityService.Tick(s,rng);
        Check(s.Money==(expands ? 145000000 : 150000000) && rng.Calls==(expands ? 2 : 0));
    }
});
Test("new imported policies use verified break defaults and retain existing production selection",()=>
{
    var c=SecretaryCatalog();c.SourceHash=EmployeeDatabaseImporter.ExpectedHash;
    var session=new GameSession();var s=session.StartNewGame("Policies","Tester",GameMode.Beginner,null,c);
    Check(s.SecretaryPolicies!.Production==ProductionPolicy.Funds300Million && s.SecretaryPolicies.Break==BreakPolicy.Stress80 && s.SecretaryPolicies.Return==ReturnPolicy.Stress0);
    Check(SecretaryPolicyRules.TrySet(s,new() {Production=ProductionPolicy.Manual,Break=BreakPolicy.Voluntary,Return=ReturnPolicy.Stress40}));
    var next=session.StartNewGame("Next","Tester",GameMode.Normal,null,c);Check(next.SecretaryPolicies!.Production==ProductionPolicy.Manual && next.SecretaryPolicies.Break==BreakPolicy.Stress80);
});
Test("automatic stress thresholds are inclusive and voluntary never routes staff",()=>
{
    foreach (var policy in Enum.GetValues<BreakPolicy>())
    {
        int threshold=SecretaryPolicyRules.BreakThreshold(policy) ?? 100;
        foreach (double stress in new[]{threshold-0.001,(double)threshold})
        {
            var e=Worker();e.Office!.AutomaticBreak=false;e.Office.BeforeAutomaticBreakAction=0;e.Stress=stress;
            var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},Employees=[e],SecretaryPolicies=new() {Break=policy}};
            bool enters=policy!=BreakPolicy.Voluntary && stress>=threshold;
            var rng=enters ? new SequenceRandom(0) : new SequenceRandom();SecretaryPolicyRules.TickEmployee(s,e,rng);
            Check(e.Office.AutomaticBreak==enters && e.Office.Action==(enters ? 18 : 6) && rng.Calls==(enters ? 1 : 0));
            if(enters) Check(e.Office.BeforeAutomaticBreakAction==6 && e.Office.LoungeSeat==0);
        }
    }
});
Test("automatic returns use 40 20 or 0 stress and resume the remembered chair",()=>
{
    foreach(var policy in Enum.GetValues<ReturnPolicy>())
    {
        int threshold=SecretaryPolicyRules.ReturnThreshold(policy);
        var e=Worker();e.Office!.AutomaticBreak=false;e.Stress=80;
        var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},Employees=[e],SecretaryPolicies=new() {Return=policy}};
        SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom(0));e.Stress=threshold+.001;
        SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());Check(e.Office.AutomaticBreak==true && e.Office.Action==18);
        e.Stress=threshold;s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];
        SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());Check(e.Office!.AutomaticBreak==false && e.Office!.Action==6 && e.Office!.MovementState==3 && e.Office!.LoungeSeat is null);
    }
});
Test("full lounges mark the automatic attempt once without retrying or stealing another chair",()=>
{
    var e=Worker();e.Office!.AutomaticBreak=false;e.Stress=80;
    var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},Employees=[e],SecretaryPolicies=new()};
    for (int seat=0;seat<12;seat++) s.Employees.Add(new() {Name=$"Synthetic lounge {seat}",Office=new() {SlotIndex=seat+1,LoungeSeat=seat,Action=18+seat/4}});
    SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());Check(e.Office.AutomaticBreak==true && e.Office.Action==6);
    SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());Check(e.Office.Action==6);
    e.Stress=0;SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());Check(e.Office.AutomaticBreak==false && e.Office.Action==6);
});
Test("an occupied former desk is left occupied when returning and unknown legacy rest fields remain inert",()=>
{
    var e=Worker();e.Office!.AutomaticBreak=false;e.Stress=80;
    var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},Employees=[e],SecretaryPolicies=new()};SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom(0));
    s.Employees.Add(new() {Name="Synthetic occupant",Office=new() {Action=6}});e.Stress=0;SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());
    Check(e.Office.Action==18 && e.Office.AutomaticBreak==false);
    e.Office.AutomaticBreak=null;e.Stress=100;SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom());Check(e.Office.AutomaticBreak is null);
});
Test("manual assignment respects the entry stress threshold and clears automatic return intent",()=>
{
    var e=Worker();e.Office!.AutomaticBreak=false;e.Stress=80;var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},Employees=[e],SecretaryPolicies=new()};
    SecretaryPolicyRules.TickEmployee(s,e,new SequenceRandom(0));e.Stress=80.001;
    Check(OfficeAssignmentService.TryAssignDesk(s,e,7)==OfficeAssignmentResult.RestRequired && e.Office.AutomaticBreak==true);
    e.Stress=80;Check(OfficeAssignmentService.TryAssignDesk(s,e,7)==OfficeAssignmentResult.Success && e.Office.AutomaticBreak==false);
    e.Stress=100;s.SecretaryPolicies.Break=BreakPolicy.Voluntary;
    Check(OfficeAssignmentService.TryAssignDesk(s,e,8)==OfficeAssignmentResult.Success);
});
Test("policy validation and save omission preserve unknown projections",()=>
{
    var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"}};Check(SaveStore.Deserialize(SaveStore.Serialize(s)).SecretaryPolicies is null);
    s.SecretaryPolicies=new() {Break=(BreakPolicy)8};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.SecretaryPolicies=null;s.Employees=[Worker()];s.Employees[0].Office!.AutomaticBreak=true;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("office secretary sprite follows the active identity and changes only its source crop",()=>
{
    var scene=new OfficeSceneData {Sprites=[new(141,101,79,200,32,16,32,26)]};var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"},SecretarySourceIndex=4};
    var sprite=OfficePresentationRules.ForState(scene,s).Single();Check(sprite.SourceX==808 && sprite.X==101 && sprite.Y==79 && sprite.SourceY==32);
});

(GameState State,OriginalCatalog Catalog) AdviceFixture(long money=150_000_000)
{
    var (s,c)=CalendarFixture(new(3,5,1),money);
    c.Technologies=Enumerable.Range(0,8).Select(i=>new TechnologyDefinition {SourceIndex=i}).ToList();
    s.TechnologyOwnershipLevels=Enumerable.Range(0,8).ToDictionary(i=>i,_=>0);
    return(s,c);
}
Test("fresh imported campaigns preserve the verified initial secretary without guessing old identities",()=>
{
    var(s,c)=AdviceFixture();c.SourceHash=EmployeeDatabaseImporter.ExpectedHash;
    var session=new GameSession();s=session.StartNewGame("Secretary test","Tester",GameMode.Normal,null,c);
    Check(s.SecretarySourceIndex==1 && SecretaryAdviceRules.PortraitId(s.SecretarySourceIndex)==2);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).SecretarySourceIndex==1);
    var legacy=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;legacy["State"]!.AsObject().Remove("SecretarySourceIndex");
    Check(SaveStore.Deserialize(legacy.ToJsonString()).SecretarySourceIndex is null && SecretaryAdviceRules.PortraitId(null) is null);
    Check(new int?[]{0,1,2,3,4}.Select(SecretaryAdviceRules.PortraitId).SequenceEqual(new int?[]{1,2,4,5,6}));
    s.SecretarySourceIndex=5;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("secretary click uses the original inclusive native rectangle",()=>
{
    foreach(var(x,y) in new[]{(104.0,80.0),(114.0,104.0),(109.0,92.0)}) Check(OfficeInteractionRules.SecretaryAt(x,y));
    foreach(var(x,y) in new[]{(103.99,80.0),(114.01,104.0),(109.0,79.99),(109.0,104.01),(double.NaN,92.0),(109.0,double.PositiveInfinity)}) Check(!OfficeInteractionRules.SecretaryAt(x,y));
});
Test("secretary advice preserves all message ordering and eligible draw ordering",()=>
{
    var(s,c)=AdviceFixture();s.Month=2;s.Calendar!.HardwareStage=1;s.ReleasedGames=[new() {Title="Stock",Stock=200000}];
    var r=new SequenceRandom(0,0,0,0,0);Check(SecretaryAdviceRules.Prepare(s,c,r)==SecretaryAdviceResult.Success);
    Check(r.Calls==5 && s.PendingSecretaryAdvice!.Messages.SequenceEqual(new[]{SecretaryAdviceMessage.DevelopGame,SecretaryAdviceMessage.HotSpring,SecretaryAdviceMessage.BuyTechnology,SecretaryAdviceMessage.ContractsNextMonth,SecretaryAdviceMessage.MoreLicenses,SecretaryAdviceMessage.UpgradeComputer,SecretaryAdviceMessage.ExcessStock,SecretaryAdviceMessage.FundsHealthy}));
    Check(s.Money==150000000 && s.ReleasedGames[0].Stock==200000 && s.Calendar.HardwareStage==1);
});
Test("technology advice uses three ordered ownership and funds tiers with one draw",()=>
{
    foreach(var(money,count,eligible) in new[]{(149999999L,0,false),(150000000L,0,true),(249999999L,1,false),(250000000L,2,true),(250000000L,3,false),(349999999L,3,false),(350000000L,4,true),(350000000L,5,false)})
    {
        var(s,c)=AdviceFixture(money);s.Calendar!.OriginalSlots0444[0]=1;
        for(int i=0;i<count;i++) s.TechnologyOwnershipLevels![i]=3;
        var r=eligible ? new SequenceRandom(0,1) : new SequenceRandom(1);
        Check(SecretaryAdviceRules.Prepare(s,c,r)==SecretaryAdviceResult.Success);
        Check(s.PendingSecretaryAdvice!.Messages.Contains(SecretaryAdviceMessage.BuyTechnology)==eligible && r.Calls==(eligible ? 2 : 1));
    }
    var(state,catalog)=AdviceFixture(350000000);state.Calendar!.OriginalSlots0444[0]=1;
    var noFallback=new SequenceRandom(1,1);SecretaryAdviceRules.Prepare(state,catalog,noFallback);
    Check(noFallback.Calls==2 && !state.PendingSecretaryAdvice!.Messages.Contains(SecretaryAdviceMessage.BuyTechnology));
});
Test("hot-spring advice uses tenant presence and the 120 million inclusive threshold",()=>
{
    foreach(var(money,slot,eligible) in new[]{(119999999L,-1,false),(120000000L,-1,true),(120000000L,1,false)})
    {
        var(s,c)=AdviceFixture(money);s.Calendar!.OriginalSlots0444[2]=slot;
        var r=eligible ? new SequenceRandom(0,1) : new SequenceRandom(1);SecretaryAdviceRules.Prepare(s,c,r);
        Check(s.PendingSecretaryAdvice!.Messages.Contains(SecretaryAdviceMessage.HotSpring)==eligible && r.Calls==(eligible ? 2 : 1));
    }
});
Test("contract and contest advice are manual calendar branches without extra draws",()=>
{
    foreach(var(month,week,message) in new[]{(2,1,(SecretaryAdviceMessage?)SecretaryAdviceMessage.ContractsNextMonth),(3,4,SecretaryAdviceMessage.ContractsThisMonth),(12,3,SecretaryAdviceMessage.Contest),(12,4,null),(4,1,null)})
    {
        var(s,c)=AdviceFixture(1);s.Month=month;s.Week=week;var r=new SequenceRandom(1);
        SecretaryAdviceRules.Prepare(s,c,r);Check(r.Calls==1 && s.PendingSecretaryAdvice!.Messages.Count==(message.HasValue ? 3 : 2));
        if(message.HasValue) Check(s.PendingSecretaryAdvice!.Messages[1]==message);
    }
});
Test("license advice counts only active licensed hardware but always draws",()=>
{
    foreach(int count in Enumerable.Range(0,4))
    {
        var(s,c)=AdviceFixture(1);s.HardwareLicenses=Enumerable.Range(0,count).ToHashSet();s.AvailableHardware=[0,1,2];s.HardwareLicenses.Add(10);
        var r=new SequenceRandom(0);SecretaryAdviceRules.Prepare(s,c,r);Check(r.Calls==1);
        SecretaryAdviceMessage? expected=count switch {0=>SecretaryAdviceMessage.LicenseUrgent,1=>SecretaryAdviceMessage.MoreLicenses,2=>SecretaryAdviceMessage.AnotherLicense,_=>null};
        Check(s.PendingSecretaryAdvice!.Messages.Count==(expected.HasValue ? 3 : 2));
        if(expected.HasValue) Check(s.PendingSecretaryAdvice.Messages[1]==expected);
    }
});
Test("computer advice checks numeric bits and hardware-stage thresholds",()=>
{
    foreach(var(bits,stage,eligible) in new[]{(8,0,false),(8,1,true),(16,2,false),(16,3,true),(32,4,false),(32,5,true),(64,9,false)})
    {
        var(s,c)=AdviceFixture(1);c.DevelopmentComputers[0].BitWidth=bits;s.Calendar!.HardwareStage=stage;
        var r=eligible ? new SequenceRandom(1,0) : new SequenceRandom(1);SecretaryAdviceRules.Prepare(s,c,r);
        Check(s.PendingSecretaryAdvice!.Messages.Contains(SecretaryAdviceMessage.UpgradeComputer)==eligible && r.Calls==(eligible ? 2 : 1));
    }
});
Test("stock advice sums supported own shipments and uses the 200000-copy boundary",()=>
{
    foreach(long stock in new[]{199999L,200000L})
    {
        var(s,c)=AdviceFixture(1);s.ReleasedGames=[new() {Title="A",Stock=stock-100000},new() {ProductIndex=1,Title="B",Stock=100000}];
        var r=stock==200000 ? new SequenceRandom(1,0) : new SequenceRandom(1);SecretaryAdviceRules.Prepare(s,c,r);
        Check(s.PendingSecretaryAdvice!.Messages.Contains(SecretaryAdviceMessage.ExcessStock)==(stock==200000));
    }
});
Test("funds advice keeps low-funds precedence and the strict contract comparison",()=>
{
    foreach(var(money,contracts,message) in new[]{(50000000L,100000000L,SecretaryAdviceMessage.FundsLow),(50000001L,50000002L,SecretaryAdviceMessage.FundsContracts),(50000001L,50000001L,SecretaryAdviceMessage.FundsHealthy)})
    {
        var(s,c)=AdviceFixture(money);s.Employees=[Candidate(0,contracts)];SecretaryAdviceRules.Prepare(s,c,new SequenceRandom(1));
        Check(s.PendingSecretaryAdvice!.Messages.Last()==message && s.Money==money && s.Employees[0].ContractFee==contracts);
    }
});
Test("advice save/load reuses its random outcome, advances once and freezes the clock",()=>
{
    var(s,c)=AdviceFixture(1);var session=new GameSession(new SequenceRandom(0));session.Restore(s);
    Check(session.AskSecretary(c)==SecretaryAdviceResult.Success);
    var saved=SaveStore.Serialize(s);s=SaveStore.Deserialize(saved);session.Restore(s);
    var replay=new SequenceRandom();Check(SecretaryAdviceRules.Prepare(s,c,replay)==SecretaryAdviceResult.Success && replay.Calls==0);
    session.TickOffice(c);Check(SaveStore.Serialize(s)==saved);
    Throws<InvalidOperationException>(()=>session.OpenRecruitment());Check(session.Hire(Candidate(0))==HireResult.ConversationPending);
    Check(SecretaryAdviceRules.Acknowledge(s));s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.PendingSecretaryAdvice!.MessageIndex==1);
    while(SecretaryAdviceRules.Acknowledge(s)) {} Check(s.PendingSecretaryAdvice is null && !SecretaryAdviceRules.Acknowledge(s));
    var legacy=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;legacy["State"]!.AsObject().Remove("PendingSecretaryAdvice");
    Check(SaveStore.Deserialize(legacy.ToJsonString()).PendingSecretaryAdvice is null);
});
Test("unknown advice projections consume no random draws and create no queue",()=>
{
    Action<GameState,OriginalCatalog>[] corrupt=[(s,c)=>s.Money=null,(s,c)=>s.Month=null,(s,c)=>s.Week=null,(s,c)=>s.Calendar=null,(s,c)=>s.TechnologyOwnershipLevels=null,(s,c)=>s.TechnologyOwnershipLevels!.Remove(7),(s,c)=>s.DevelopmentComputerIndex=null,(s,c)=>s.AvailableHardware=null,(s,c)=>s.HardwareLicenses=null,(s,c)=>s.Employees=[Candidate(0,null)],(s,c)=>c.DevelopmentComputers.Clear(),(s,c)=>c.Technologies.Clear()];
    foreach(var change in corrupt) {var(s,c)=AdviceFixture();change(s,c);Check(SecretaryAdviceRules.Prepare(s,c,new SequenceRandom())==SecretaryAdviceResult.StateUnknown && s.PendingSecretaryAdvice is null);}
});
Test("advice waits for existing modal game transactions without draws",()=>
{
    var(s,c)=AdviceFixture();s.DevelopmentProjects=[new() {Title="Pending",PendingDecision=DevelopmentMilestone.SelectType}];
    Check(SecretaryAdviceRules.Prepare(s,c,new SequenceRandom())==SecretaryAdviceResult.EventPending);
    s.DevelopmentProjects.Clear();s.PendingHireGreeting=new();Check(SecretaryAdviceRules.Prepare(s,c,new SequenceRandom())==SecretaryAdviceResult.EventPending);
    s.PendingHireGreeting=null;s.Calendar!.Pending=new();Check(SecretaryAdviceRules.Prepare(s,c,new SequenceRandom())==SecretaryAdviceResult.EventPending);
});
Test("advice validation rejects malformed queues and completed or negative cursors",()=>
{
    var(s,c)=AdviceFixture(1);
    List<SecretaryAdviceMessage>[] bad=[[],[(SecretaryAdviceMessage)99],[SecretaryAdviceMessage.DevelopGame],[SecretaryAdviceMessage.FundsLow,SecretaryAdviceMessage.FundsHealthy],[SecretaryAdviceMessage.HotSpring,SecretaryAdviceMessage.DevelopGame,SecretaryAdviceMessage.FundsHealthy],[SecretaryAdviceMessage.MoreLicenses,SecretaryAdviceMessage.MoreLicenses,SecretaryAdviceMessage.FundsHealthy],[SecretaryAdviceMessage.ContractsNextMonth,SecretaryAdviceMessage.Contest,SecretaryAdviceMessage.FundsLow],[SecretaryAdviceMessage.LicenseUrgent,SecretaryAdviceMessage.AnotherLicense,SecretaryAdviceMessage.FundsLow]];
    foreach(var messages in bad) {s.PendingSecretaryAdvice=new() {Messages=messages};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));}
    s.PendingSecretaryAdvice=new() {Messages=[SecretaryAdviceMessage.FundsLow],MessageIndex=-1};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingSecretaryAdvice.MessageIndex=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingSecretaryAdvice=new() {Messages=null!};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("advice arithmetic failure occurs before its lottery and queue mutation",()=>
{
    var(s,c)=AdviceFixture();s.Employees=[Candidate(0,long.MaxValue),Candidate(1,1)];
    Throws<OverflowException>(()=>SecretaryAdviceRules.Prepare(s,c,new SequenceRandom()));Check(s.PendingSecretaryAdvice is null);
    s.Employees.Clear();s.ReleasedGames=[new() {Title="A",Stock=long.MaxValue},new() {Title="B",Stock=1}];
    Throws<OverflowException>(()=>SecretaryAdviceRules.Prepare(s,c,new SequenceRandom()));Check(s.PendingSecretaryAdvice is null);
});

Test("checkpoint introductions acknowledge presentation without advancing the simulation",()=>
{
    var s=PlanningState(10000);var p=new DevelopmentProjectState {Title="Checkpoint",RequiredPoints=100,Progress=51,Focus=new(25,25,25,25),PendingDecision=DevelopmentMilestone.SelectType};
    s.DevelopmentProjects.Add(p);
    Check(CheckpointDialogueRules.HasPending(p));
    Check(!CheckpointDialogueRules.Acknowledge(s,p,DevelopmentMilestone.SelectGenre));
    Check(!CheckpointDialogueRules.Acknowledge(s,new(),DevelopmentMilestone.SelectType));
    Check(CheckpointDialogueRules.Acknowledge(s,p,DevelopmentMilestone.SelectType));
    Check(!CheckpointDialogueRules.HasPending(p) && !CheckpointDialogueRules.Acknowledge(s,p,DevelopmentMilestone.SelectType));
    Check(s.Money==10000 && p.Progress==51 && p.OriginalStage==0 && p.PendingDecision==DevelopmentMilestone.SelectType);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects[0];Check(!CheckpointDialogueRules.HasPending(p));
    p.OriginalStage=1;p.GameTypeSourceIndex=0;p.Progress=81;p.PendingDecision=DevelopmentMilestone.SelectGenre;
    Check(CheckpointDialogueRules.HasPending(p) && CheckpointDialogueRules.Acknowledge(s,p,DevelopmentMilestone.SelectGenre));
    p.OriginalStage=3;p.GenreSourceIndex=0;p.Progress=100;p.PendingDecision=DevelopmentMilestone.StartDebugging;
    Check(CheckpointDialogueRules.HasPending(p) && CheckpointDialogueRules.Acknowledge(s,p,DevelopmentMilestone.StartDebugging));
    Check(p.Progress==100 && p.PendingDecision==DevelopmentMilestone.StartDebugging);
});
Test("legacy checkpoint saves replay only their current unresolved introduction",()=>
{
    var s=PlanningState();s.DevelopmentProjects.Add(new() {Title="Old checkpoint",RequiredPoints=100,Progress=51,Focus=new(25,25,25,25),PendingDecision=DevelopmentMilestone.SelectType});
    var json=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;
    json["State"]!["DevelopmentProjects"]![0]!.AsObject().Remove("DecisionIntroductionAcknowledged");
    Check(CheckpointDialogueRules.HasPending(SaveStore.Deserialize(json.ToJsonString()).DevelopmentProjects[0]));
    s.DevelopmentProjects[0].DecisionIntroductionAcknowledged=(DevelopmentMilestone)99;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("market commentary shares balance rolls and preserves the first-two quality suppression",()=>
{
    for (int roll=0;roll<20;roll++)
    {
        var r=new SequenceRandom(roll);var market=ReleaseService.CalculateMarket(0,new(999,999,999,999),r);
        ShipmentMessage[] expected=roll switch {0=>[ShipmentMessage.BalanceHuge],1 or 2=>[ShipmentMessage.BalanceHit],3 or 4 or 5=>[ShipmentMessage.BalancePoor],_=>[]};
        Check(market.Messages.SequenceEqual(expected) && r.Calls==1);
        Check(market.Factor==ReleaseService.MarketFactor(0,new(999,999,999,999),new SequenceRandom(roll)));
    }
});
Test("market commentary records quality success and rejection with no additional random calls",()=>
{
    var r=new SequenceRandom(6,0,0,1);var m=ReleaseService.CalculateMarket(2,new(201,201,201,0),r);
    Check(r.Calls==4 && Math.Abs(m.Factor-5.1)<1e-12 && m.Messages.SequenceEqual(new[]{ShipmentMessage.NoveltyHuge,ShipmentMessage.FunHuge,ShipmentMessage.MusicNotice}));
    r=new(6,1,1,0);m=ReleaseService.CalculateMarket(2,new(101,101,101,999),r);
    Check(r.Calls==4 && m.Factor==.8 && m.Messages.SequenceEqual(new[]{ShipmentMessage.NoveltyRejected}));
    m=ReleaseService.CalculateMarket(2,new(101,101,0,0),new SequenceRandom(6,0,0));
    Check(m.Factor==2.1 && m.Messages.SequenceEqual(new[]{ShipmentMessage.NoveltyHit,ShipmentMessage.FunHit}));
    m=ReleaseService.CalculateMarket(2,new(0,201,0,0),new SequenceRandom(3,1));Check(m.Factor==0 && m.Messages.Last()==ShipmentMessage.NoveltyRejected);
});
Test("shipment price comments use the original five independent boundaries",()=>
{
    long[] prices=[0,3999,4000,6999,7000,7999,8000,9999,10000,19999,20000];
    ShipmentMessage?[] expected=[ShipmentMessage.PriceCheap,ShipmentMessage.PriceCheap,null,null,ShipmentMessage.PriceSlightlyHigh,ShipmentMessage.PriceSlightlyHigh,ShipmentMessage.PriceHigh,ShipmentMessage.PriceHigh,ShipmentMessage.PriceVeryHigh,ShipmentMessage.PriceVeryHigh,ShipmentMessage.PriceAbsurd];
    for(int i=0;i<prices.Length;i++) Check(ShipmentDialogueRules.Price(prices[i])==expected[i]);
    int[] pop=[0,2,3,5,6,9,10,14];ShipmentMessage[] adaptation=[ShipmentMessage.AdaptationUnpopular,ShipmentMessage.AdaptationUnpopular,ShipmentMessage.AdaptationReaders,ShipmentMessage.AdaptationReaders,ShipmentMessage.AdaptationPopular,ShipmentMessage.AdaptationPopular,ShipmentMessage.AdaptationHuge,ShipmentMessage.AdaptationHuge];
    for(int i=0;i<pop.Length;i++) Check(ShipmentDialogueRules.Adaptation(pop[i])==adaptation[i]);
});
(GameState State,ReleasedGameState Product) ShipmentFixture()
{
    var s=PlanningState(1000000);var p=new DevelopmentProjectState {Title="Shipment",RequiredPoints=1,Progress=1,OriginalStage=3,GameTypeSourceIndex=0,GenreSourceIndex=0,Focus=new(25,25,25,25)};
    s.DevelopmentProjects.Add(p);Check(ReleaseService.TryRelease(s,p,CoreCatalog(),p.Title,7000,100,new SequenceRandom(0,0,0,0,0))==ReleaseResult.Success);
    return(s,s.ReleasedGames[0]);
}
Test("shipment queues block reviews and survive save/load without repeating a transaction",()=>
{
    var(s,g)=ShipmentFixture();Check(g.ShipmentMessages!.SequenceEqual(new[]{ShipmentMessage.BalanceHuge,ShipmentMessage.PriceSlightlyHigh,ShipmentMessage.Reviews}));
    Check(g.ShipmentMessageIndex==0 && !ReviewService.Acknowledge(s,g));
    long? money=s.Money;double factor=g.MarketFactor;int[] scores=g.ReviewScores!.ToArray();
    Check(ShipmentDialogueRules.Acknowledge(s,g));s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames[0];
    Check(g.ShipmentMessageIndex==1 && s.Money==money && g.MarketFactor==factor && g.ReviewScores!.SequenceEqual(scores));
    var session=new GameSession(new SequenceRandom());session.Restore(s);session.TickOffice();Check(s.CompanyActivityTicks==0 && s.SalesTicks==0 && s.Money==money);
    Check(ShipmentDialogueRules.Acknowledge(s,g) && !ReviewService.Acknowledge(s,g));
    Check(ShipmentDialogueRules.Acknowledge(s,g) && !ShipmentDialogueRules.HasPending(g));
    Check(ReviewService.Acknowledge(s,g) && !ShipmentDialogueRules.Acknowledge(s,g));
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].ShipmentMessageIndex==3 && s.Money==money);
});
Test("legacy releases do not invent lost shipment lottery history",()=>
{
    var(s,g)=ShipmentFixture();var json=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;
    var record=json["State"]!["ReleasedGames"]![0]!.AsObject();record.Remove("ShipmentMessages");record.Remove("ShipmentMessageIndex");
    s=SaveStore.Deserialize(json.ToJsonString());g=s.ReleasedGames[0];Check(g.ShipmentMessages is null && !ShipmentDialogueRules.HasPending(g) && ReviewService.Acknowledge(s,g));
});
Test("save validation rejects malformed shipment order, cursors and premature review acknowledgment",()=>
{
    var(s,g)=ShipmentFixture();
    List<ShipmentMessage>[] bad=[[],[ShipmentMessage.Reviews,ShipmentMessage.BalanceHuge],[ShipmentMessage.BalanceHit,ShipmentMessage.BalanceHuge,ShipmentMessage.Reviews],[(ShipmentMessage)99,ShipmentMessage.Reviews],[ShipmentMessage.AdaptationHuge,ShipmentMessage.Reviews],[ShipmentMessage.PriceCheap]];
    foreach(var messages in bad) {g.ShipmentMessages=messages;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));}
    g.ShipmentMessages=[ShipmentMessage.Reviews];g.ShipmentMessageIndex=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.ShipmentMessageIndex=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));g.ShipmentMessageIndex=0;g.ReviewPending=false;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.ReviewPending=true;g.ShipmentMessages=null;g.ShipmentMessageIndex=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("failed shipment creates no commentary and consumes no lottery draws",()=>
{
    var s=PlanningState(0);var p=new DevelopmentProjectState {Title="Unaffordable",RequiredPoints=1,Progress=1,OriginalStage=3,GameTypeSourceIndex=0,GenreSourceIndex=0,Focus=new(25,25,25,25)};s.DevelopmentProjects.Add(p);
    string before=SaveStore.Serialize(s);var r=new SequenceRandom();Check(ReleaseService.TryRelease(s,p,CoreCatalog(),p.Title,5800,100,r)==ReleaseResult.InsufficientFunds && r.Calls==0 && SaveStore.Serialize(s)==before);
});
(GameState State,DevelopmentProjectState Project,OriginalCatalog Catalog) CheckpointFixture()
{
    var s=PlanningState(100000);s.Company.OriginalMember90=100;s.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);
    var p=new DevelopmentProjectState {Title="Checkpoint meeting",RequiredPoints=100,Progress=51,Recognition=12,Focus=new(25,25,25,25),PendingDecision=DevelopmentMilestone.SelectType,TypeCandidates=[0,1,0,1,0]};s.DevelopmentProjects.Add(p);
    return(s,p,CoreCatalog());
}
void DrainMeeting(GameState s,DevelopmentProjectState p) {while(CheckpointMeetingRules.Acknowledge(s,p)) {}}
Test("checkpoint introduction gates end after the third product while completion remains unconditional",()=>
{
    var(s,p,_)=CheckpointFixture();
    for(int count=0;count<5;count++)
    {
        Check(CheckpointDialogueRules.ShouldIntroduce(s,p)==(count<=2));
        s.ReleasedGames.Add(new(){ProductIndex=count});
    }
    p.PendingDecision=DevelopmentMilestone.StartDebugging;Check(CheckpointDialogueRules.ShouldIntroduce(s,p));
    p.DecisionIntroductionAcknowledged=DevelopmentMilestone.StartDebugging;Check(!CheckpointDialogueRules.ShouldIntroduce(s,p));
});
Test("type meeting suggestions preserve source order, duplicate candidates and silent variant three",()=>
{
    var(s,p,c)=CheckpointFixture();var r=new SequenceRandom(0,0,1,1,2,0,3,1,5);CheckpointMeetingRules.Prepare(s,p,c,r);
    Check(r.Calls==9 && p.CheckpointMeeting!.Lines.SequenceEqual(new[]{new CheckpointMeetingLine(CheckpointMeetingMessage.TypeSuggestion,0),new CheckpointMeetingLine(CheckpointMeetingMessage.TypeFits,1),new CheckpointMeetingLine(CheckpointMeetingMessage.TypeReaders,0)}));
    var restored=SaveStore.Deserialize(SaveStore.Serialize(s));var replay=new SequenceRandom();
    CheckpointMeetingRules.Prepare(restored,restored.DevelopmentProjects[0],c,replay);Check(replay.Calls==0);
    Check(s.Money==100000 && p.OriginalStage==0 && p.Recognition==12);
});
Test("silent type discussions stay prepared across save/load without regenerating a roster",()=>
{
    var(s,p,c)=CheckpointFixture();var r=new SequenceRandom(2,2,2,2,2);CheckpointMeetingRules.Prepare(s,p,c,r);
    Check(r.Calls==5 && p.CheckpointMeeting!.Lines.Count==0 && !CheckpointMeetingRules.HasPending(p));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects[0];r=new();CheckpointMeetingRules.Prepare(s,p,c,r);
    Check(r.Calls==0 && p.TypeCandidates!.SequenceEqual(new[]{0,1,0,1,0}));
});
Test("genre discussion branches use recognition boundaries ten and thirty",()=>
{
    foreach(var(row,expected) in new[]{(0.0,CheckpointMeetingMessage.RecognitionLow),(10.0,CheckpointMeetingMessage.RecognitionLow),(10.01,CheckpointMeetingMessage.RecognitionMedium),(30.0,CheckpointMeetingMessage.RecognitionMedium),(30.01,CheckpointMeetingMessage.RecognitionHigh)})
    {
        var(s,p,c)=CheckpointFixture();p.OriginalStage=1;p.GameTypeSourceIndex=0;p.Progress=81;p.PendingDecision=DevelopmentMilestone.SelectGenre;p.Recognition=row;
        var r=new SequenceRandom();CheckpointMeetingRules.Prepare(s,p,c,r);var m=p.CheckpointMeeting!;
        Check(r.Calls==0 && m.Lines.Count==4 && m.Lines[1].Message==expected && m.Lines[^1].TypeSourceIndex==0);
        Check(SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects[0].CheckpointMeeting!.Lines.SequenceEqual(m.Lines));
    }
});
Test("type selection waits for discussion and confirms once before publicity",()=>
{
    var(s,p,c)=CheckpointFixture();CheckpointMeetingRules.Prepare(s,p,c,new SequenceRandom(0,0,2,2,2,2));
    string before=SaveStore.Serialize(s);Check(CheckpointMeetingRules.TrySelectType(s,p,c,0)==DevelopmentSelectionResult.WrongStage && SaveStore.Serialize(s)==before);
    DrainMeeting(s,p);Check(CheckpointMeetingRules.TrySelectType(s,p,c,0)==DevelopmentSelectionResult.Success);
    Check(s.Money==99995 && p.OriginalStage==1 && p.Recognition==16 && p.RequiredPoints==110 && p.PendingDecision is null && CheckpointMeetingRules.HasPending(p));
    Check(p.CheckpointMeeting!.Lines.Select(x=>x.Message).SequenceEqual(new[]{CheckpointMeetingMessage.TypeChosen,CheckpointMeetingMessage.Publicity}));
    Check(CheckpointMeetingRules.TrySelectType(s,p,c,0)==DevelopmentSelectionResult.WrongStage && s.Money==99995);
    Check(CheckpointMeetingRules.Acknowledge(s,p));s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects[0];
    Check(p.CheckpointMeeting!.MessageIndex==1 && p.Recognition==16 && s.Money==99995);
    DrainMeeting(s,p);Check(p.CheckpointMeeting is null && !CheckpointMeetingRules.Acknowledge(s,p) && s.Money==99995);
});
Test("confirmed meeting suspends company and work clocks even after clearing the selection decision",()=>
{
    var(s,p,c)=CheckpointFixture();Check(CheckpointMeetingRules.TrySelectType(s,p,c,0)==DevelopmentSelectionResult.Success);
    var e=Worker();s.Employees.Add(e);s.CompanyActivityTicks=199;var r=new SequenceRandom();var session=new GameSession(r);session.Restore(s);
    session.TickOffice(c);DevelopmentSimulation.TickEmployee(s,e,c,r);
    Check(r.Calls==0 && s.CompanyActivityTicks==199 && p.Progress==51 && e.Office!.WorkIdleTicks==0);
});
Test("genre affinity confirmation reports and awards twenty fun exactly once",()=>
{
    var(s,p,c)=CheckpointFixture();p.OriginalStage=1;p.GameTypeSourceIndex=0;p.Progress=81;p.PendingDecision=DevelopmentMilestone.SelectGenre;
    CheckpointMeetingRules.Prepare(s,p,c,new SequenceRandom());DrainMeeting(s,p);
    Check(CheckpointMeetingRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success);
    Check(p.Quality.Fun==20 && p.OriginalStage==2 && s.Money==99993 && p.CheckpointMeeting!.Lines.Select(x=>x.Message).SequenceEqual(new[]{CheckpointMeetingMessage.GenreMatch,CheckpointMeetingMessage.FunBonus}));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects[0];
    Check(CheckpointMeetingRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.WrongStage && p.Quality.Fun==20);
    DrainMeeting(s,p);Check(p.CheckpointMeeting is null && p.Quality.Fun==20 && s.Money==99993);
});
Test("nonmatching and broad-audience genres have one confirmation and no bonus",()=>
{
    for(int audience=0;audience<2;audience++)
    {
        var(s,p,c)=CheckpointFixture();p.OriginalStage=1;p.GameTypeSourceIndex=0;p.Progress=81;p.PendingDecision=DevelopmentMilestone.SelectGenre;
        if(audience==0)c.GameTypes[0].RawTable13CC=Enumerable.Repeat(99,15).ToList();else c.Audiences[0].RawMembers["14A8"]=1;
        Check(CheckpointMeetingRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success && p.Quality.Fun==0 && p.CheckpointMeeting!.Lines.Single().Message==CheckpointMeetingMessage.GenreChosen);
    }
});
Test("invalid, unaffordable and locked checkpoint choices preserve state",()=>
{
    var(s,p,c)=CheckpointFixture();s.Money=0;string before=SaveStore.Serialize(s);
    Check(CheckpointMeetingRules.TrySelectType(s,p,c,0)==DevelopmentSelectionResult.InsufficientFunds && SaveStore.Serialize(s)==before);
    Check(CheckpointMeetingRules.TrySelectType(s,new(),c,0)==DevelopmentSelectionResult.WrongStage);
    p.OriginalStage=1;p.GameTypeSourceIndex=0;p.Progress=81;p.PendingDecision=DevelopmentMilestone.SelectGenre;s.Company.OriginalMember90=0;before=SaveStore.Serialize(s);
    Check(CheckpointMeetingRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.WrongStage && SaveStore.Serialize(s)==before);
});
Test("save validation rejects corrupt checkpoint phases, message arguments and cursors",()=>
{
    var(s,p,c)=CheckpointFixture();CheckpointMeetingRules.Prepare(s,p,c,new SequenceRandom(0,0,2,2,2,2));var m=p.CheckpointMeeting!;
    m.MessageIndex=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));m.MessageIndex=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));m.MessageIndex=0;
    m.Lines[0]=new((CheckpointMeetingMessage)99,0);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    m.Lines[0]=new(CheckpointMeetingMessage.TypeSuggestion);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    m.Lines[0]=new(CheckpointMeetingMessage.TypeSuggestion,50);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    m.Lines[0]=new(CheckpointMeetingMessage.TypeSuggestion,0);m.IsConfirmation=true;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    m.IsConfirmation=false;m.Milestone=DevelopmentMilestone.SelectGenre;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    p.CheckpointMeeting=null;CheckpointMeetingRules.TrySelectType(s,p,c,0);m=p.CheckpointMeeting!;m.MessageIndex=m.Lines.Count;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("conversation timer reveals UTF-16 text and restores the four-step mouth cycle",()=>
{
    var c=new ConversationRevealState(new string('x',30));
    int[] expected=[0,0,0,1,1,2,2,1,1,1,1,0];
    for (int tick=0;tick<expected.Length;tick++) {c.Tick();Check(c.Cursor==tick+1 && c.PortraitFrame==expected[tick]);}
    Check(!c.Advance() && c.Cursor==30 && !c.Finished);c.Tick();Check(c.Finished && c.Advance());
    c.Tick();Check(c.PortraitFrame==0 && c.VisibleCharacters==30);
    c=new("A😀B");c.Tick();Check(c.VisibleCharacters==1);c.Tick();Check(c.VisibleCharacters==1);
    c.Tick();Check(c.VisibleCharacters==2);c.Tick();Check(c.VisibleCharacters==3);
});
Test("meeting dialogue follows all seven profiles and original early-company branches",()=>
{
    int[] early=[4,5,4,6,5,5,5],late=[2,3,2,4,3,3,4];
    for (int profile=0;profile<7;profile++)
    {
        Check(DevelopmentFocusRules.DialogueKeys(profile,0).Count()==early[profile]);
        Check(DevelopmentFocusRules.DialogueKeys(profile,4).Count()==late[profile]);
        var a=DevelopmentFocusRules.GenerateProposal(new Random(profile));
        var b=DevelopmentFocusRules.Generate(new Random(profile));Check(a.Focus==b);
        Check(DevelopmentFocusRules.DialogueKeys(profile,3).First()=="MEETING_INTRO");
    }
});
Test("speech bubble requests persist once, drop replacements and expire after thirty ticks",()=>
{
    var s=PlanningState();var e=Worker();s.Employees.Add(e);var o=e.Office!;
    o.PendingSpeechBubble=10;OfficeVisualRules.TickBubble(o);Check(o.SpeechBubble==10 && o.SpeechBubbleTicks==1 && o.PendingSpeechBubble==0);
    o.PendingSpeechBubble=15;OfficeVisualRules.TickBubble(o);Check(o.SpeechBubble==10 && o.PendingSpeechBubble==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));o=s.Employees[0].Office!;
    for (int tick=2;tick<30;tick++) OfficeVisualRules.TickBubble(o);
    Check(o.SpeechBubble==10 && o.SpeechBubbleTicks==30);OfficeVisualRules.TickBubble(o);Check(o.SpeechBubble==0 && o.SpeechBubbleTicks==0);
    o.PendingSpeechBubble=17;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("office work particles rise, expand and show the recovered burst counter cells",()=>
{
    var e=Worker();var o=e.Office!;o.WorkTier=3;o.WorkCategory=DevelopmentWorkCategory.Graphics;
    o.WorkTicks=2;var first=OfficeVisualRules.Sprites(e).Single();Check(first.X==193 && first.Y==112 && first.SourceX==182 && first.SourceY==359 && first.Width==5 && first.SortKey==2000);
    o.WorkTicks=12;Check(OfficeVisualRules.Sprites(e).Single().Y==99);
    o.WorkTicks=21;var expanded=OfficeVisualRules.Sprites(e).Single();Check(expanded.X==190 && expanded.Y==93 && expanded.SourceX==179 && expanded.SourceY==374 && expanded.Width==12);
    o.WorkTicks=33;o.BurstCount=27;var number=OfficeVisualRules.Sprites(e).Last();Check(number.SourceX==417 && number.SourceY==310 && number.X==187 && number.Y==94);
    o.WorkTicks=0;Check(!OfficeVisualRules.Sprites(e).Any());
    o.PendingSpeechBubble=0;OfficeVisualRules.StartWork(o,new SequenceRandom());Check(o.PendingSpeechBubble==10);
    o.WorkCategory=DevelopmentWorkCategory.Bugs;OfficeVisualRules.StartWork(o,new SequenceRandom());Check(o.PendingSpeechBubble==15);
    o.WorkCategory=DevelopmentWorkCategory.Debugging;OfficeVisualRules.StartWork(o,new SequenceRandom(1));Check(o.PendingSpeechBubble==11);
});
Test("English bitmap lettering alters only opaque local caption interiors",()=>
{
    var colors=Enumerable.Repeat((byte)80,1600*530*3).ToArray();var masks=new byte[colors.Length];
    var original=(byte[])colors.Clone();var patched=LocalBitmapLetteringRules.PatchOfficeCaptions(colors,masks,1600,530,new Dictionary<int,string>{{1,"TIRED!"},{10,"HEAVE!"}});
    Check(colors.SequenceEqual(original) && !patched.SequenceEqual(original));
    for (int pixel=0;pixel<1600*530;pixel++)
    {
        int x=pixel%1600,y=pixel/1600;
        if (x is >=5 and <40 && y is >=369 and <377 || x is >=630 and <676 && y is >=373 and <381) continue;
        for (int channel=0;channel<3;channel++) Check(patched[pixel*3+channel]==colors[pixel*3+channel]);
    }
    var bad=new Dictionary<int,string>{{1,"THIS IS TOO LONG"}};Throws<InvalidDataException>(()=>LocalBitmapLetteringRules.PatchOfficeCaptions(colors,masks,1600,530,bad));
    masks[(369*1600+5)*3]=255;Throws<InvalidDataException>(()=>LocalBitmapLetteringRules.PatchOfficeCaptions(colors,masks,1600,530,new Dictionary<int,string>{{1,"TIRED!"}}));
});
Test("local name-reading corrections require matching saved names and a supported source",()=>
{
    var d=new EmployeeDatabase {SourceHash=EmployeeDatabaseImporter.ExpectedHash,Employees=[new() {SourceIndex=0,Name="Synthetic kanji name"}]};
    var e=new EmployeeState {SourceIndex=0,Name="Synthetic kanji name"};
    var file=new EmployeeNameReadingFile {SourceHash=d.SourceHash,Readings=[new() {SourceIndex=0,OriginalName=e.Name,EnglishName="Reviewed Reading"}]};
    var readings=EmployeeNameLocalization.ReadOverrides(System.Text.Json.JsonSerializer.Serialize(file));
    Check(EmployeeNameLocalization.Display(e,d,true,k=>k,readings)=="Reviewed Reading");
    Check(EmployeeNameLocalization.Display(e,d,false,k=>k,readings)==e.Name);
    e=new EmployeeState {SourceIndex=0,Name="Custom name"};Check(EmployeeNameLocalization.Display(e,d,true,k=>k,readings)==e.Name);
    file.SourceHash="unknown";Throws<InvalidDataException>(()=>EmployeeNameLocalization.ReadOverrides(System.Text.Json.JsonSerializer.Serialize(file)));
    Check(EmployeeNameLocalization.HasKanaReadingEvidence(16) && !EmployeeNameLocalization.HasKanaReadingEvidence(0));
});
Test("community seasons cover all twelve months with winter across New Year",()=>
{
    MusicSeason[] expected=[MusicSeason.Winter,MusicSeason.Winter,MusicSeason.Spring,MusicSeason.Spring,MusicSeason.Spring,
        MusicSeason.Summer,MusicSeason.Summer,MusicSeason.Summer,MusicSeason.Autumn,MusicSeason.Autumn,MusicSeason.Autumn,MusicSeason.Winter];
    for (int month=1;month<=12;month++) Check(SeasonalMusicRules.ForMonth(month)==expected[month-1]);
    foreach (var season in Enum.GetValues<MusicSeason>()) Check(expected.Count(x=>x==season)==3);
    foreach (int? month in new int?[] {null,0,-1,13}) Check(SeasonalMusicRules.ForMonth(month) is null);
    Check(SeasonalMusicRules.BundledPaths(MusicSeason.Summer).Count()==8);
    foreach (var season in new[] {MusicSeason.Spring,MusicSeason.Autumn,MusicSeason.Winter})
    {
        var paths=SeasonalMusicRules.BundledPaths(season).ToArray();
        Check(paths.Length==4 && paths.All(x=>x.StartsWith($"audio/community/{SeasonalMusicRules.Folder(season)}/")));
    }
    Check(Enum.GetValues<MusicSeason>().SelectMany(SeasonalMusicRules.BundledPaths).Distinct().Count()==20);
    Check(SeasonalMusicRules.Folder(MusicSeason.Autumn)=="autumn");
});
Test("audio crossfade has exact endpoints and maintains equal power",()=>
{
    Check(SeasonalMusicRules.CrossfadeGains(0)==(0,1));
    var end=SeasonalMusicRules.CrossfadeGains(SeasonalMusicRules.CrossfadeSeconds);
    Check(end.Incoming==1 && Math.Abs(end.Outgoing)<.000001);
    for (int index=0;index<=100;index++)
    {
        var gains=SeasonalMusicRules.CrossfadeGains(index*SeasonalMusicRules.CrossfadeSeconds/100);
        Check(Math.Abs(gains.Incoming*gains.Incoming+gains.Outgoing*gains.Outgoing-1)<.000001);
    }
    Check(SeasonalMusicRules.CrossfadeGains(-1)==(0,1));
    Check(SeasonalMusicRules.CrossfadeGains(999)==end);
    Check(SeasonalMusicRules.CrossfadeGains(double.NaN)==end);
});
(GameState State,OriginalCatalog Catalog) CalendarFixture(GameDate? date=null,long money=2_000_000_000)
{
    var c=CoreCatalog();c.Hardware.Clear();
    for (int id=0;id<=10;id++) c.Hardware.Add(new() {SourceIndex=id,Name=$"Synthetic platform {id}",
        RawMembers=new() {{"0EC8",id},{"0F1C",id<3 ? 1 : 0},{"1050",40},{"0F54",15},{"10A4",0},{"10C0",0}},
        ProductionBands=[new(16,300),new(32,700),new(48,1200),new(64,1800)]});
    var d=date ?? new GameDate(3,5,1);var s=PlanningState(money);
    s.Year=d.Year;s.Month=d.Month;s.Week=d.Week;s.AvailableHardware=[0,1,2];
    s.Company.WeeklyNet=11;s.Company.MonthlyNet=22;s.Company.YearlyNet=33;
    s.Calendar=CalendarService.CreateOpening(c);return (s,c);
}
(GameState State,OriginalCatalog Catalog) CompetitionFixture(GameDate? date=null)
{
    var (s,c)=CalendarFixture(date);c.InitialRivalProducts=[
        new() {Title="Synthetic rival A",Owner=0,Hardware=0,UnitsSold=100,Demand=10,SalesPower=140},
        new() {Title="Synthetic rival B",Owner=1,Hardware=0,UnitsSold=100,Demand=1,SalesPower=140},
        new() {Title="Synthetic newest rival",Owner=0,Hardware=0,UnitsSold=50,Demand=0,SalesPower=0}];
    c.StagedRivalProducts=[new() {Age=0,Product=new() {Title="Synthetic launch rival",Owner=1,Hardware=0,UnitsSold=0,Demand=10,SalesPower=70}}];
    c.RivalCompanies=["Synthetic company A","Synthetic company B"];
    s.Competition=CompetitionService.Opening(c);return (s,c);
}
Test("hire conversation persists once and blocks the clock without repeating the fee",()=>
{
    for (int variant=0;variant<10;variant++)
    {
        var random=new GreetingRandom((variant+.5f)/10);var session=new GameSession(random);
        var state=PlanningState(100);session.Restore(state);
        var candidate=new EmployeeState {SourceIndex=0,Name="Synthetic applicant",PortraitId=1,ContractFee=100};
        Check(session.Hire(candidate)==HireResult.Success && state.Money==0);
        Check(state.PendingHireGreeting?.Variant==variant && random.GreetingCalls==1);
        string saved=SaveStore.Serialize(state);
        for (int tick=0;tick<20;tick++) session.TickOffice(CoreCatalog());
        Check(SaveStore.Serialize(state)==saved);
        Check(session.Hire(candidate)==HireResult.ConversationPending && random.GreetingCalls==1);
        Throws<InvalidOperationException>(()=>session.OpenRecruitment());
        session.Restore(SaveStore.Deserialize(saved));
        Check(session.AcknowledgeHireGreeting() && !session.AcknowledgeHireGreeting());
        Check(session.Current!.Money==0 && session.Current.Employees.Count==1 && random.GreetingCalls==1);
        Check(session.Hire(candidate)==HireResult.AlreadyEmployed && random.GreetingCalls==1);
    }
});
Test("rejected hire never creates a greeting or consumes its lottery",()=>
{
    var random=new GreetingRandom(.5f);var session=new GameSession(random);var state=PlanningState(99);session.Restore(state);
    Check(session.Hire(new() {SourceIndex=0,Name="Synthetic",PortraitId=1,ContractFee=100})==HireResult.InsufficientFunds);
    Check(state.PendingHireGreeting is null && random.GreetingCalls==0 && state.Employees.Count==0);
});
Test("save rejects invalid pending greeting identities and variants",()=>
{
    var state=PlanningState();state.PendingHireGreeting=new() {EmployeeSourceIndex=0,Variant=0};
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.Employees=[new() {SourceIndex=0,PortraitId=1,Name="Synthetic"}];state.PendingHireGreeting.Variant=10;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.PendingHireGreeting.Variant=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
});
Test("employee display readings preserve saved identity and custom names",()=>
{
    var db=new EmployeeDatabase {SourceHash=EmployeeDatabaseImporter.ExpectedHash,
        Employees=[new() {SourceIndex=7,Name="Synthetic Family  Synthetic Given"},new() {SourceIndex=35,GeneratedName=true}],
        NameComponents=Enumerable.Range(0,8).Select(i=>new NameComponents {Family=$"Family{i}",Given=$"Given{i}"}).ToList()};
    string Translate(string key)=>"English:"+key;
    var e=new EmployeeState {SourceIndex=7,Name="Synthetic Family\u3000Synthetic Given"};
    var state=new GameState {Company=new() {Name="Synthetic Company",PresidentName="Synthetic President"},Employees=[e]};string saved=SaveStore.Serialize(state);
    Check(EmployeeNameLocalization.Display(e,db,true,Translate)=="English:EMPLOYEE_NAME_7");
    Check(EmployeeNameLocalization.Display(e,db,false,Translate)==e.Name);
    Check(EmployeeNameLocalization.Display(e,db,true,key=>key)==e.Name);
    Check(EmployeeNameLocalization.Display(e,db,true,key=>"")==e.Name);
    Check(EmployeeNameLocalization.Display(e,null,true,Translate)==e.Name);
    Check(EmployeeNameLocalization.Display(new() {SourceIndex=7,Name="Custom"},db,true,Translate)=="Custom");
    Check(EmployeeNameLocalization.Display(new() {SourceIndex=99,Name="Custom"},db,true,Translate)=="Custom");
    Check(SaveStore.Serialize(state)==saved && SaveStore.Deserialize(saved).Employees.Single().Name==e.Name);
    db.SourceHash="unsupported";Check(EmployeeNameLocalization.Display(e,db,true,Translate)==e.Name);
});
Test("generated employee readings follow all independent saved name combinations",()=>
{
    var db=new EmployeeDatabase {SourceHash=EmployeeDatabaseImporter.ExpectedHash,
        Employees=[new() {SourceIndex=35,GeneratedName=true}],
        NameComponents=Enumerable.Range(0,8).Select(i=>new NameComponents {Family=$"F{i}",Given=$"G{i}"}).ToList()};
    for (int family=0;family<8;family++) for (int given=0;given<8;given++)
    {
        var e=new EmployeeState {SourceIndex=35,Name=$"F{family} G{given}"};
        Check(EmployeeNameLocalization.Display(e,db,true,key=>"English:"+key)==$"English:EMPLOYEE_FAMILY_{family} English:EMPLOYEE_GIVEN_{given}");
        Check(EmployeeNameLocalization.Display(e,db,true,key=>key.StartsWith("EMPLOYEE_GIVEN") ? key : "Family")==e.Name);
    }
    Check(EmployeeNameLocalization.Display(new() {SourceIndex=35,Name="Custom"},db,true,key=>"English")=="Custom");
});
Test("office room interiors use original strict transformed bounds",()=>
{
    Check(OfficeInteractionRules.DevelopmentRoomAt(192,148)==0);
    Check(OfficeInteractionRules.DevelopmentRoomAt(288,196)==1);
    foreach (var (sum,diff) in new[] {(228d,52d),(260d,52d),(244d,34d),(244d,69d),(324d,52d),(356d,52d)})
        Check(OfficeInteractionRules.DevelopmentRoomAt(sum-diff,(sum+diff)/2) is null);
    Check(OfficeInteractionRules.DevelopmentRoomAt(0,0) is null);
    Check(OfficeInteractionRules.DevelopmentRoomAt(double.NaN,148) is null);
    Check(OfficeInteractionRules.DevelopmentRoomAt(192,double.PositiveInfinity) is null);
});
Test("office tenant hit regions map three slots without inventing overlap order",()=>
{
    Check(OfficeInteractionRules.TenantSlotAt(66,199)==0);
    Check(OfficeInteractionRules.TenantSlotAt(146,239)==1);
    Check(OfficeInteractionRules.TenantSlotAt(401,239)==2);
    foreach (var (sum,diff) in new[] {(190d,166d),(232d,124d),(232d,208d),(354d,166d),(398d,39d),(482d,39d),(440d,-2d),(440d,80d),(272d,166d)})
        Check(OfficeInteractionRules.TenantSlotAt(sum-diff,(sum+diff)/2) is null);
    Check(OfficeInteractionRules.TenantSlotAt(double.NaN,0) is null);
    Check(OfficeInteractionRules.TenantSlotAt(0,double.NegativeInfinity) is null);
});
Test("school purchase uses empty slots and exact cash without changing period accounting",()=>
{
    var (s,c)=CalendarFixture(money:SchoolService.PurchasePrice);s.Company.Rank=null;
    Check(SchoolService.Open(s,2)==SchoolResult.Success && s.Money==0 && s.Calendar!.OriginalSlots0444.SequenceEqual(new[] {-1,-1,0}));
    Check(s.Company.WeeklyNet==11 && s.Company.MonthlyNet==22 && s.Company.YearlyNet==33);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(SchoolService.Open(s,1)==SchoolResult.AlreadyOwned && s.Money==0);
    Check(SchoolService.Close(s,2)==SchoolResult.Success && s.Money==0 && s.Calendar!.OriginalSlots0444[2]==-1);
    Check(SchoolService.Close(s,2)==SchoolResult.NotOwned);
});
Test("school rejection does not mutate cash slots or accounting",()=>
{
    var (s,c)=CalendarFixture(money:SchoolService.PurchasePrice-1);string before=SaveStore.Serialize(s);
    Check(SchoolService.Open(s,0)==SchoolResult.InsufficientFunds && SaveStore.Serialize(s)==before);
    Check(SchoolService.Open(s,-1)==SchoolResult.InvalidSlot && SchoolService.Close(s,3)==SchoolResult.InvalidSlot);
    s.Calendar!.OriginalSlots0444[0]=1;Check(SchoolService.Open(s,0)==SchoolResult.SlotOccupied);
    s.Money=null;Check(SchoolService.Open(s,1)==SchoolResult.MoneyUnknown);
    s.Calendar.Pending=new() {ClosingDate=new(3,5,1)};Check(SchoolService.Open(s,1)==SchoolResult.EventPending && SchoolService.Close(s,0)==SchoolResult.EventPending);
    s.Calendar=null;Check(SchoolService.Open(s,0)==SchoolResult.CalendarUnknown);
});
Test("buying school through service enables April graduates and closing prevents them",()=>
{
    var s=GraduateFixture(2);s.Calendar!.OriginalSlots0444[0]=-1;var (_,c)=CalendarFixture();
    Check(SchoolService.Open(s,1)==SchoolResult.Success);
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom(0,0)) && s.Calendar!.Pending!.PendingEvent==CalendarEvent.Graduates);
    Check(SchoolService.Close(s,1)==SchoolResult.EventPending);
    Check(GraduateService.Decline(s) && GraduateService.Decline(s) && CalendarService.Resume(s,c,new SequenceRandom()));
    Check(SchoolService.Close(s,1)==SchoolResult.Success);
    s.Year=4;s.Month=4;s.Week=1;Check(CalendarService.BeginWeek(s,c,new SequenceRandom()));
});
Test("rival opening copies imported values and staged releases advance once across pauses",()=>
{
    var (s,c)=CompetitionFixture(new(3,12,3));s.Competition!.Products[0].Demand=1;
    Check(c.InitialRivalProducts[0].Demand==10);
    Check(!CalendarService.BeginWeek(s,c,new ConstantRandom(0)));
    Check(s.Competition.Products.Count==4 && s.Competition.PlatformAges[0]==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(!CalendarService.Resume(s,c,new SequenceRandom()) && s.Competition!.Products.Count==4 && s.Competition.PlatformAges[0]==1);
    Check(s.Calendar!.Pending!.Awards!.RivalEntries!.Count==31);
});
Test("staged rival trigger follows hardware age independently of the product market",()=>
{
    var (s,c)=CompetitionFixture();s.Calendar!.HardwareStage=1;
    c.StagedRivalProducts=[new() {TriggerPlatform=2,Age=0,Product=new() {Title="Synthetic older-market launch",Owner=0,Hardware=0}}];
    CompetitionService.AdvanceWeek(s,c);Check(s.Competition!.Products.Count==3 && s.Competition.PlatformAges[2]==0);
    s.Calendar.HardwareStage=2;CompetitionService.AdvanceWeek(s,c);
    Check(s.Competition.Products.Count==4 && s.Competition.Products[3].Hardware==0 && s.Competition.PlatformAges[2]==1);
});
Test("rival batch floors twice uses market noise and reduces demand by two",()=>
{
    var (s,c)=CompetitionFixture();var p=s.Competition!.Products[0];
    Check(CompetitionService.RivalSales(p,40,3)==19);
    Check(CompetitionService.RivalSales(s.Competition.Products[1],40,19)==28);
    Throws<ArgumentOutOfRangeException>(()=>CompetitionService.RivalSales(p,40,20));
    var rng=new SequenceRandom(3,19);CompetitionService.SellRivals(s,c,rng);
    Check(rng.Calls==2 && p.UnitsSold==119 && p.Demand==8 && s.Competition.Products[1].UnitsSold==128 && s.Competition.Products[1].Demand==0);
});
Test("sales chart keeps append order for ties limits one hundred and queues own number one",()=>
{
    var (s,c)=CompetitionFixture();Check(CompetitionService.Chart(s).SequenceEqual(new[] {0,1,2}));
    s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic own",UnitsSold=101}];CompetitionService.AppendOwn(s,s.ReleasedGames[0]);
    CompetitionService.UpdateOwnChart(s);Check(CompetitionService.Chart(s)[0]==3 && s.PendingRankings.Single().Kind==RankingKind.NumberOneSales);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.Competition!.Products[3].OwnProduct==0);
    s.Competition.Products.AddRange(Enumerable.Range(0,110).Select(i=>new RivalProduct {Title=$"Synthetic {i}",Owner=0,Hardware=0}));
    Check(CompetitionService.Chart(s).Count==100);
});
Test("contest replacement lottery rejects own entries and excludes newest catalog row",()=>
{
    var (s,c)=CompetitionFixture();s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic own"}];CompetitionService.AppendOwn(s,s.ReleasedGames[0]);
    s.Competition!.Products.Add(new() {Title="Synthetic excluded newest",Owner=1,Hardware=0});
    var rng=new SequenceRandom(new[] {3}.Concat(Enumerable.Repeat(0,31)).ToArray());
    Check(CompetitionService.DrawContestEntries(s,rng).SequenceEqual(Enumerable.Repeat(0,31)) && rng.Calls==32);
    s.Competition.Products.RemoveRange(0,3);Throws<InvalidOperationException>(()=>CompetitionService.DrawContestEntries(s,new SequenceRandom()));
});
Test("rival award stores thirteen reel draws and fallback winner without payment or reroll",()=>
{
    var (s,c)=CompetitionFixture(new(3,12,3));Check(!CalendarService.BeginWeek(s,c,new ConstantRandom(0)));
    var a=s.Calendar!.Pending!.Awards!;a.RivalEntries=Enumerable.Range(0,31).Select(i=>i%2).ToList();long cash=s.Money!.Value;
    var rng=new SequenceRandom(Enumerable.Range(0,14).ToArray());Check(AwardService.Reveal(s,rng) && rng.Calls==14);
    Check(a.AnimationProducts!.SequenceEqual(Enumerable.Range(0,13).Select(i=>i%2)) && a.WinningRival==1 && a.WinningProduct is null && s.Money==cash);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(!AwardService.Reveal(s,new SequenceRandom()) && s.Calendar!.Pending!.Awards!.WinningRival==1);
    Check(AwardService.Continue(s) && s.Calendar!.Pending!.Awards!.AnimationProducts is null && s.Calendar.Pending.Awards.RivalEntries!.Count==31);
});
Test("own award draws follow reel and grand prize queues its dated global achievement",()=>
{
    var (s,c)=CompetitionFixture(new(3,12,3));s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic own",ReleasedOn=new(3,1,1),UnitsSold=1_000_000}];
    CompetitionService.AppendOwn(s,s.ReleasedGames[0]);Check(!CalendarService.BeginWeek(s,c,new ConstantRandom(0)));
    s.Calendar!.Pending!.Awards!.CategoryIndex=4;var rng=new SequenceRandom(Enumerable.Repeat(0,13).ToArray());
    Check(AwardService.Reveal(s,rng) && rng.Calls==13 && s.Calendar.Pending.Awards.WinningProduct==0 && s.Calendar.Pending.Awards.WinningRival is null);
    Check(s.PendingRankings.Single().Kind==RankingKind.GrandPrize && s.PendingRankings[0].AchievedOn==new GameDate(3,12,4));
});
Test("save rejects missing rival fields broken own links and forged rival award results",()=>
{
    var (s,c)=CompetitionFixture(new(3,12,3));string json=SaveStore.Serialize(s);
    Throws<InvalidDataException>(()=>SaveStore.Deserialize(json.Replace("\"PlatformAges\":","\"MissingPlatformAges\":")));
    s.Competition!.Products[0].Owner=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.Competition.Products[0].Owner=0;
    Check(!CalendarService.BeginWeek(s,c,new ConstantRandom(0)) && AwardService.Reveal(s,new ConstantRandom(0)));
    s.Calendar!.Pending!.Awards!.WinningRival=200;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.Calendar.Pending.Awards.WinningRival=0;s.Calendar.Pending.Awards.AnimationProducts!.RemoveAt(0);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("global rankings replace only strictly earlier dates for each independent category",()=>
{
    var book=new RankingBook();var a=new RankingAchievement {Kind=RankingKind.Sales250000,Company="Synthetic A",AchievedOn=new(3,2,2)};
    Check(RankingService.Record(book,a,"2030/01/01"));a.Company="Synthetic B";
    Check(!RankingService.Record(book,a,"2030/01/02") && book.Records[0].Company=="Synthetic A");
    foreach (var date in new[] {new GameDate(3,2,3),new(3,3,1),new(4,1,1)}) {a.AchievedOn=date;Check(!RankingService.Record(book,a,"ignored"));}
    foreach (var date in new[] {new GameDate(3,2,1),new(3,1,4),new(2,12,4)}) {a.AchievedOn=date;Check(RankingService.Record(book,a,"earlier"));}
    Check(book.Records.Skip(1).All(r=>r.AchievedOn is null) && book.Records[0].AchievedOn==new GameDate(2,12,4));
});
Test("sales milestone rankings begin at quarter million and queue earliest dates once",()=>
{
    var (s,c)=CalendarFixture();var g=new ReleasedGameState {Title="Synthetic milestones",UnitsSold=1_000_000,SalesMilestoneStage=0};s.ReleasedGames.Add(g);
    SalesMilestoneService.QueueReached(g);Check(SalesMilestoneService.Acknowledge(s,g) && s.PendingRankings.Count==0);
    for (int i=0;i<3;i++) Check(SalesMilestoneService.Acknowledge(s,g));
    Check(s.PendingRankings.Select(p=>p.Kind).SequenceEqual(new[] {RankingKind.Sales250000,RankingKind.Sales500000,RankingKind.Sales1000000}));
    s.Month=6;RankingService.Queue(s,RankingKind.Sales250000);Check(s.PendingRankings[0].AchievedOn==new GameDate(3,5,1));
    s.Month=4;RankingService.Queue(s,RankingKind.Sales250000);Check(s.PendingRankings.Single(p=>p.Kind==RankingKind.Sales250000).AchievedOn==new GameDate(3,4,1));
    s.Year=null;RankingService.Queue(s,RankingKind.OwnHardware);Check(s.PendingRankings.Count==3);
    s.Year=3;s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.PendingRankings.Count==3);
});
Test("ranking profile persists independently and rejects corrupt schema and dates",()=>
{
    string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+"-rankings.json");
    try
    {
        var book=RankingStore.Load(path);Check(book.Records.Count==6 && !File.Exists(path));
        RankingService.Record(book,new() {Kind=RankingKind.GrandPrize,Company="Synthetic \"A\"",AchievedOn=new(2,12,4)},"2030/01/01");
        RankingStore.Save(path,book);Check(!File.Exists(path+".tmp"));
        Check(RankingStore.Load(path).Records[4].Company=="Synthetic \"A\"");
        File.WriteAllText(path,"{}");Throws<System.Text.Json.JsonException>(()=>RankingStore.Load(path));
        book.Records[4].AchievedOn=new(2,13,1);Throws<InvalidDataException>(()=>RankingStore.Save(path,book));
        Check(File.ReadAllText(path)=="{}");
    }
    finally {File.Delete(path);File.Delete(path+".tmp");}
});
Test("original ranking importer reads CP932 seven CSV rows and preserves quoted names",()=>
{
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    string text="1,-1,-1,-1,-1,-1\r\n\"合成,会社\"\"A\"\"\",,,,,\r\n\"合成日付\",,,,,\r\n\"2030/01/01\",,,,,\r\n2,,,,,\r\n4,,,,,\r\n1,,,,,\r\n";
    var book=RankingStore.ImportOriginal(System.Text.Encoding.GetEncoding(932).GetBytes(text));
    Check(book.Records[0].Company=="合成,会社\"A\"" && book.Records[0].OriginalGameDateText=="合成日付" && book.Records[0].AchievedOn==new GameDate(2,4,1));
    Check(book.Records.Skip(1).All(r=>r.AchievedOn is null));
    Throws<InvalidDataException>(()=>RankingStore.ImportOriginal(System.Text.Encoding.ASCII.GetBytes(text.Replace("1,-1","0,-1"))));
    Throws<InvalidDataException>(()=>RankingStore.ImportOriginal(System.Text.Encoding.ASCII.GetBytes(text.Replace("2,,,,,","2,,"))));
    Throws<InvalidDataException>(()=>RankingStore.ImportOriginal(System.Text.Encoding.ASCII.GetBytes(text.Replace("4,,,,,","13,,,,,"))));
    Throws<InvalidDataException>(()=>RankingStore.ImportOriginal(System.Text.Encoding.ASCII.GetBytes("\"unterminated")));
});
Test("exhibitions reject invalid selections without payment or visitor rolls",()=>
{
    var (s,c)=CalendarFixture(new(3,9,4),9_999_999);
    s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic product"}];
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(ExhibitionService.Participate(s,new HashSet<int>(),new SequenceRandom())==ExhibitionResult.NoSelection);
    Check(ExhibitionService.Participate(s,new HashSet<int> {6},new SequenceRandom())==ExhibitionResult.InvalidSelection);
    Check(ExhibitionService.Participate(s,new HashSet<int> {0},new SequenceRandom())==ExhibitionResult.InsufficientFunds);
    Check(s.Money==9_999_999 && s.Calendar!.Pending!.Exhibition!.Visitors is null);
    Check(ExhibitionService.Close(s) && !ExhibitionService.Close(s));
    Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Month==10 && s.Week==1);
});
Test("exhibition charges each item once and preserves source order shared cap defect",()=>
{
    var (s,c)=CalendarFixture(new(3,9,4),100_000_000);
    var p=new DevelopmentProjectState {Title="Synthetic room",RequiredPoints=100,Focus=new(25,25,25,25),Recognition=99.5};s.DevelopmentProjects=[p];
    s.ReleasedGames=[new() {ProductIndex=0,Title="Older",Recognition=20},new() {ProductIndex=1,Title="Newer",Recognition=110}];
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    var rng=new SequenceRandom(Enumerable.Repeat(0,22).ToArray());
    Check(ExhibitionService.Participate(s,new HashSet<int> {0,1,-1},rng)==ExhibitionResult.Success && rng.Calls==22);
    var e=s.Calendar!.Pending!.Exhibition!;
    Check(e.SelectedProducts.SequenceEqual(new[] {-1,1,0}) && e.Visitors==9900 && e.ReportedAwarenessGain==.5);
    Check(p.Recognition==100 && s.ReleasedGames[0].Recognition==20.5 && s.ReleasedGames[1].Recognition==110.5);
    Check(s.Money==70_000_000 && s.Company.MonthlyNet==-30_000_000 && s.Calendar.ExhibitionVisitorRecord==9900);
    string before=SaveStore.Serialize(s);s=SaveStore.Deserialize(before);
    Check(ExhibitionService.Participate(s,new HashSet<int> {0},new SequenceRandom())==ExhibitionResult.NotPending && SaveStore.Serialize(s)==before);
    Check(ExhibitionService.Close(s) && CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Money==70_000_000);
});
Test("exhibition visitor maximum and uncapped product awareness are preserved",()=>
{
    var (s,c)=CalendarFixture(new(3,3,4),100_000_000);
    // The annual renewal resolves before the spring exhibition.
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(CalendarService.RenewContracts(s,new HashSet<int>())==ContractRenewalResult.Success);
    s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic",Recognition=95}];
    Check(!CalendarService.Resume(s,c,new SequenceRandom()));
    Check(ExhibitionService.Participate(s,new HashSet<int> {0},new SequenceRandom(Enumerable.Repeat(349,22).ToArray()))==ExhibitionResult.Success);
    Check(s.Calendar!.Pending!.Exhibition!.Visitors==17578 && s.ReleasedGames[0].Recognition==130);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));s.Calendar!.ExhibitionVisitorRecord=null;
    Check(ExhibitionService.Close(s) && CalendarService.Resume(s,c,new SequenceRandom()));
});
Test("exhibition rejects more than five and corrupt or detached saved selections",()=>
{
    var (s,c)=CalendarFixture(new(3,9,4));
    s.ReleasedGames=Enumerable.Range(0,6).Select(i=>new ReleasedGameState {ProductIndex=i,Title=$"Synthetic {i}"}).ToList();
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(ExhibitionService.Participate(s,Enumerable.Range(0,6).ToHashSet(),new SequenceRandom())==ExhibitionResult.TooManyProducts);
    s.Calendar!.Pending!.Exhibition!.EligibleProducts.RemoveAt(0);
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.Calendar.Pending.Exhibition.EligibleProducts=ExhibitionService.Eligible(s);s.Calendar.Pending.PendingEvent=null;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("award music graphics and design use the first threshold and the last successful entry",()=>
{
    var entries=new[] {new ReleasedGameState {ProductIndex=0,Quality=new(300,300,300,300)},
        new ReleasedGameState {ProductIndex=1,Quality=new(80,80,70,70)},new ReleasedGameState {ProductIndex=2,Quality=new(0,0,0,0)}};
    foreach (var award in new[] {GameAward.Music,GameAward.Graphics,GameAward.Design})
    {
        var rng=new SequenceRandom(1,1);
        Check(AwardService.DrawWinner(award,entries,rng)==1 && rng.Calls==2);
        Check(AwardService.DrawWinner(award,entries,new SequenceRandom(0,0)) is null);
    }
});
Test("worst award discards first pass and draws even for high fun entries",()=>
{
    var entries=new[] {new ReleasedGameState {ProductIndex=0,Quality=new(40,0,0,0)},
        new ReleasedGameState {ProductIndex=1,Quality=new(30,100,0,0)},new ReleasedGameState {ProductIndex=2,Quality=new(0,100,0,0)}};
    // First pass hits entries 0 and 2; second pass consumes one draw per entry.
    var rng=new SequenceRandom(1,1,0,0,0);
    Check(AwardService.DrawWinner(GameAward.Worst,entries,rng)==2 && rng.Calls==5);
    rng=new SequenceRandom(1,1,0,1,1);
    Check(AwardService.DrawWinner(GameAward.Worst,entries,rng) is null && rng.Calls==5);
});
Test("grand prize sales override quality roll while preserving its random draw",()=>
{
    var entries=new[] {new ReleasedGameState {ProductIndex=0,Quality=new(300,300,300,300),UnitsSold=1_000_000},
        new ReleasedGameState {ProductIndex=1,Quality=new(),UnitsSold=1_000_000}};
    var rng=new SequenceRandom(0);Check(AwardService.DrawWinner(GameAward.GrandPrize,entries,rng)==1 && rng.Calls==1);
    entries[1].UnitsSold=999_999;Check(AwardService.DrawWinner(GameAward.GrandPrize,entries,new SequenceRandom(0))==0);
});
Test("award ceremony tracks release year and serializes each signed payout without replay",()=>
{
    var (s,c)=CalendarFixture(new(3,12,3),1_000_000);
    s.ReleasedGames=[new() {ProductIndex=0,Title="Previous year",ReleasedOn=new(2,12,1),Quality=new(300,300,300,300)},
        new() {ProductIndex=1,Title="This year",ReleasedOn=new(3,6,1),Quality=new(30,140,70,70),OriginalAwardCode=0,UnitsSold=1_000_000}];
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Calendar!.Pending!.Awards!.Entries.SequenceEqual(new[] {1}) && s.Month==12 && s.Week==4);
    foreach (var award in AwardService.Order)
    {
        long money=s.Money!.Value;
        var rng=award==GameAward.Worst ? new SequenceRandom(0) : award==GameAward.GrandPrize ? new SequenceRandom() : new SequenceRandom(1);
        Check(AwardService.Reveal(s,rng));
        Check(s.Money==money+AwardService.Prize(award) && s.ReleasedGames[1].OriginalAwardCode==AwardService.OriginalCode(award));
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
        Check(!AwardService.Reveal(s,new SequenceRandom()) && s.Money==money+AwardService.Prize(award));
        Check(AwardService.Continue(s) && !AwardService.Continue(s));
    }
    Check(s.Calendar!.FirstGrandPrizeDate==new GameDate(3,12,4));
    Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==801_000_000);
    Check(s.Company.MonthlyNet==800_000_022 && s.Company.WeeklyNet==0 && s.Company.YearlyNet==800_000_033);
});
Test("no award entry produces competitor results without invented payouts",()=>
{
    var (s,c)=CalendarFixture(new(3,12,3),1000);Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    foreach (var award in AwardService.Order)
    {
        Check(AwardService.Reveal(s,new SequenceRandom()));
        Check(s.Calendar!.Pending!.Awards!.WinningProduct is null && s.Money==1000);
        Check(AwardService.Continue(s));
    }
    Check(CalendarService.Resume(s,c,new SequenceRandom()));
});
Test("unknown original release dates stop awards instead of inventing eligibility",()=>
{
    var (s,c)=CalendarFixture(new(3,12,3));s.ReleasedGames=[new() {Title="Legacy product"}];
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar!.Pending!.Block==CalendarBlock.Awards);
    Check(!CalendarService.Resume(s,c,new SequenceRandom()));
    s.ReleasedGames[0].ReleasedOn=new(3,5,1);Check(!CalendarService.Resume(s,c,new SequenceRandom()) && s.Calendar!.Pending!.PendingEvent==CalendarEvent.Awards);
});
Test("normal mode crosses the beginner deadline and beginner ending skips all later transactions",()=>
{
    var (s,c)=CalendarFixture(new(2,12,4),1000);Check(CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Year==3 && s.Month==1);
    (s,c)=CalendarFixture(new(2,12,4),1000);s.Mode=GameMode.Beginner;s.Rules=GameModeRules.For(s.Mode);
    s.Calendar!.Tenants[0]=new() {OriginalState=1,Weeks=1};s.CompanyActivityTicks=199;s.CompanyActivityPerformed=true;
    CompanyActivityService.Tick(s,new SequenceRandom(),c);
    Check(s.Calendar.Pending!.PendingEvent==CalendarEvent.BeginnerEnding && s.Year==2 && s.Month==12 && s.Week==4 && s.Calendar.Tenants[0].Weeks==1);
    string snapshot=SaveStore.Serialize(s);Check(!CalendarService.Resume(s,c,new SequenceRandom()) && !CalendarService.Acknowledge(s));
    Check(SaveStore.Serialize(SaveStore.Deserialize(snapshot))==snapshot && s.Money==1000);
});
GameState GraduateFixture(int count=4)
{
    var (s,c)=CalendarFixture(new(3,4,1));s.MinimumSourceIndex=0;s.MaximumSourceIndex=count-1;s.SourceUpperBound=count;
    s.EmployeeSources=Enumerable.Range(0,count).Select(i=>new RecruitmentSourceRecord {SourceIndex=i,GateValue=0,
        Employee=new() {SourceIndex=i,Name=$"Synthetic graduate {i}",ContractFee=100,Level=1}}).ToList();
    s.Calendar!.OriginalSlots0444[0]=0;return s;
}
Test("graduate lottery uses inclusive bounds and consumes employed source rolls",()=>
{
    var s=GraduateFixture();s.MinimumSourceIndex=1;s.MaximumSourceIndex=3;s.EmployeeSources[2].GateValue=1;
    var rng=new SequenceRandom(0,0,0);
    Check(GraduateService.DrawCandidates(s,rng).SequenceEqual(new[] {1,3}) && rng.Calls==3);
});
Test("graduate lottery repeats complete passes until two and does not cap accepted roster",()=>
{
    var s=GraduateFixture();var rng=new SequenceRandom(0,1,1,1,0,0,0,0);
    Check(GraduateService.DrawCandidates(s,rng).SequenceEqual(new[] {0,1,2,3}) && rng.Calls==8);
    s=GraduateFixture(2);Check(GraduateService.DrawCandidates(s,new SequenceRandom(1,1)).Count==0);
});
Test("graduate prompts survive reload and hire decline each advance one candidate",()=>
{
    var s=GraduateFixture(2);var (_,c)=CalendarFixture();s.Money=150;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom(0,0)) && s.Calendar!.Pending!.PendingEvent==CalendarEvent.Graduates);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(GraduateService.Hire(s,new SequenceRandom(1))==HireResult.Success);
    Check(s.Money==50 && s.EmployeeSources[0].GateValue==1 && s.Calendar!.Pending!.Graduates!.CandidateIndex==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(GraduateService.AcknowledgeGreeting(s) && !GraduateService.AcknowledgeGreeting(s));
    Check(s.Calendar!.Pending!.Graduates!.CandidateIndex==1);
    Check(GraduateService.Hire(s,new SequenceRandom())==HireResult.InsufficientFunds && s.Money==50);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(GraduateService.Decline(s) && !GraduateService.Decline(s));
    Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Month==4 && s.Week==2 && s.Employees.Count==1);
});
Test("graduate school gate absence and empty lottery never pause the calendar",()=>
{
    var s=GraduateFixture(2);var (_,c)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=-1;
    Check(CalendarService.BeginWeek(s,c,new SequenceRandom()));
    s=GraduateFixture(2);Check(CalendarService.BeginWeek(s,c,new SequenceRandom(1,1)));
});
Test("annual saves reject forged awards graduates and missing required payload cells",()=>
{
    var (s,c)=CalendarFixture(new(3,12,3));Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    string json=SaveStore.Serialize(s);Throws<InvalidDataException>(()=>SaveStore.Deserialize(json.Replace("\"CategoryIndex\": 0","\"CategoryIndex\": 6")));
    Throws<InvalidDataException>(()=>SaveStore.Deserialize(json.Replace("\"CategoryIndex\": 0,","")));
    s=GraduateFixture(2);Check(!CalendarService.BeginWeek(s,c,new SequenceRandom(0,0)));
    s.Calendar!.Pending!.Graduates!.Candidates=[0,0];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("live calendar boundary shifts history once and resumes monthly signed payroll after load",()=>
{
    var (s,c)=CalendarFixture(new(3,5,4),100);s.CompanyActivityTicks=199;s.CompanyActivityPerformed=true;
    s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=new() {Name="Synthetic worker",RawParameter40=80}}];
    s.SourceUpperBound=1;
    var g=new ReleasedGameState {Title="Synthetic history",WeeklySalesHistory=SalesHistoryRules.NewHistory()};s.ReleasedGames.Add(g);
    g.WeeklySalesHistory[0]=12;
    CompanyActivityService.Tick(s,new SequenceRandom(),c);
    Check(s.Calendar!.Pending!.Block==CalendarBlock.PayrollUnknown && s.Month==5 && s.Week==4 && s.Money==100);
    Check(g.WeeklySalesHistory[0]==0 && g.WeeklySalesHistory[1]==12 && s.CompanyActivityTicks==0);
    CompanyActivityService.Tick(s,new SequenceRandom(),c);Check(g.WeeklySalesHistory[1]==12);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(OriginalPayrollRules.CaptureStaffList(s));
    // Scratch becomes 1: this source is missing. No invented salary zero.
    Check(!CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==100);
    s.EmployeeSources.Add(new() {SourceIndex=1,GateValue=0,Employee=new() {Name="Synthetic salary row",RawParameter40=120}});
    s.SourceUpperBound=2;Check(OriginalPayrollRules.CaptureStaffList(s));
    Check(CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Money==-20 && s.Year==3 && s.Month==6 && s.Week==1);
    Check(s.Company.WeeklyNet==-120 && s.Company.MonthlyNet==-120 && s.Company.YearlyNet==-87);
    Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==-20 && s.ReleasedGames[0].WeeklySalesHistory![1]==12);
});
Test("annual renewal pauses before payroll and rejected selection cannot advance date",()=>
{
    var (s,c)=CalendarFixture(new(3,3,4),1000);
    s.Employees=[new() {Name="Synthetic A",SourceIndex=0,ContractFee=70},new() {Name="Synthetic B",SourceIndex=1,ContractFee=80}];
    s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=new() {Name="Synthetic A",RawParameter40=30}},
        new() {SourceIndex=1,GateValue=1,Employee=new() {Name="Synthetic B",RawParameter40=90}}];
    s.SourceUpperBound=2;s.OriginalPayrollScratchIndex=1;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar!.Pending!.PendingEvent==CalendarEvent.ContractRenewal);
    Check(s.Month==3 && s.Week==4 && s.Company.MonthlyNet==22 && s.Money==1000);
    s.Employees[0].ContractFee=2000;
    Check(CalendarService.RenewContracts(s,new HashSet<int> {0})==ContractRenewalResult.InsufficientFunds);
    Check(s.Employees.Count==2 && s.Month==3 && s.Calendar!.Pending!.PendingEvent==CalendarEvent.ContractRenewal);
    s.Employees[0].ContractFee=70;s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(CalendarService.RenewContracts(s,new HashSet<int> {0})==ContractRenewalResult.Success);
    Check(s.Money==930 && s.Employees.Count==1 && s.EmployeeSources[1].GateValue==0);
    Check(!CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar!.Pending!.PendingEvent==CalendarEvent.Exhibition && s.Month==4 && s.Week==1 && s.Money==840);
    Check(s.Company.WeeklyNet==-90 && s.Company.MonthlyNet==-90 && s.Company.YearlyNet==-127);
    Check(!CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==840);
});
Test("January payroll follows every accounting reset and tenant expiry occurs once",()=>
{
    var (s,c)=CalendarFixture(new(3,12,4),1000);
    s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=new() {Name="Synthetic",RawParameter40=7}}];s.OriginalPayrollScratchIndex=0;s.SourceUpperBound=1;
    s.Calendar!.Tenants[0]=new() {OriginalState=1,Weeks=1};s.Calendar.Tenants[1]=new() {OriginalState=1,Weeks=2};
    Check(CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Year==4 && s.Month==1 && s.Week==1 && s.Money==993);
    Check(s.Company.WeeklyNet==-7 && s.Company.MonthlyNet==-7 && s.Company.YearlyNet==-7);
    Check(s.Calendar.Tenants[0].OriginalState==0 && s.Calendar.Tenants[1].Weeks==1);
});
Test("calendar payroll overflow leaves the pending rollover retryable without a partial debit",()=>
{
    var (s,c)=CalendarFixture(new(3,5,4),0);s.Company.YearlyNet=long.MinValue;
    s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=new() {Name="Synthetic",RawParameter40=1}}];s.OriginalPayrollScratchIndex=0;s.SourceUpperBound=1;
    Throws<OverflowException>(()=>CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Month==5 && s.Week==4 && s.Money==0 && s.Company.YearlyNet==long.MinValue && s.Calendar!.Pending!.Phase==CalendarPhase.TenantsAndPayroll);
    s.Company.YearlyNet=0;Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==-1);
});
Test("hardware trigger requires four first shipments stage funds and zero countdown",()=>
{
    foreach (var (count,stage,funds,countdown,triggered) in new[]
        {(3,0,150000000L,0,false),(4,1,150000000L,0,false),(4,0,149999999L,0,false),
         (4,0,150000000L,1,false),(4,0,150000000L,0,true)})
    {
        var (s,c)=CalendarFixture(money:funds);s.Calendar!.ShipmentsSinceHardwareTrigger=count;s.Calendar.HardwareStage=stage;
        var h=s.Calendar.Hardware[3];h.LaunchWeeks=countdown;
        Check(HardwareCalendarRules.TryTrigger(s,c,h)==triggered);
        Check(s.Money==funds && h.LaunchWeeks==(triggered ? 24 : countdown));
    }
});
Test("hardware trigger news saves before same-pass decrement and consumes the shipment gate",()=>
{
    var (s,c)=CalendarFixture();s.Calendar!.ShipmentsSinceHardwareTrigger=4;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Calendar.Pending!.PendingEvent==CalendarEvent.HardwareDevelopment && s.Calendar.Hardware[3].LaunchWeeks==24);
    long money=s.Money!.Value;s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(!CalendarService.Resume(s,c,new SequenceRandom()));
    Check(CalendarService.Acknowledge(s) && !CalendarService.Acknowledge(s));
    Check(CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar!.Hardware[3].LaunchWeeks==23 && s.Calendar.ShipmentsSinceHardwareTrigger==0 && s.Money==money);
});
Test("hardware launch availability changes after acknowledgement and retirement removes licenses before news",()=>
{
    var (s,c)=CalendarFixture();s.Calendar!.Hardware[3].LaunchWeeks=1;s.Calendar.Hardware[0].RetirementWeeks=1;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Calendar.Pending!.PendingEvent==CalendarEvent.HardwareRetired && !s.AvailableHardware!.Contains(0) && !s.HardwareLicenses!.Contains(0));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CalendarService.Acknowledge(s));
    Check(!CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar!.Pending!.PendingEvent==CalendarEvent.HardwareLaunch && s.Calendar.HardwareStage==1 && !s.AvailableHardware!.Contains(3));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CalendarService.Acknowledge(s) && s.AvailableHardware!.Contains(3));
    Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Calendar!.Hardware[0].RetirementWeeks==0);
});
Test("hardware advance preserves original warning thresholds and retirement assignment",()=>
{
    var (s,c)=CalendarFixture();s.Calendar!.HardwareStage=1;s.Calendar.ShipmentsSinceHardwareTrigger=4;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Calendar.Hardware[0].RetirementWeeks==14 && s.Calendar.Hardware[4].LaunchWeeks==24);
    CalendarService.Acknowledge(s);Check(CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar.Hardware[0].RetirementWeeks==13 && s.Calendar.Hardware[4].LaunchWeeks==23);
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar.Pending!.PendingEvent==CalendarEvent.HardwareRetiring);
    Check(s.Calendar.Hardware[0].RetirementWeeks==12);CalendarService.Acknowledge(s);Check(CalendarService.Resume(s,c,new SequenceRandom()));
    s.Calendar.Hardware[4].LaunchWeeks=5;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar.Pending!.PendingEvent==CalendarEvent.HardwareSoon);
    Check(s.Calendar.Hardware[4].LaunchWeeks==4);
});
Test("hardware stage-six alternatives retain source order rather than an invented generation eight",()=>
{
    var (s,c)=CalendarFixture();s.Calendar!.HardwareStage=6;s.Calendar.ShipmentsSinceHardwareTrigger=4;
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Calendar.Hardware[6].LaunchWeeks==24 && s.Calendar.Hardware[7].LaunchWeeks==0);
    CalendarService.Acknowledge(s);Check(CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar.Hardware[4].RetirementWeeks==13 && s.Calendar.Hardware[2].RetirementWeeks==16);
});
Test("hardware market growth updates saved cells and actual sales without modifying catalog",()=>
{
    var (s,c)=CalendarFixture();var h=s.Calendar!.Hardware[0];h.GrowthWeeks=2;h.GrowthBoost=1;
    double expected=40+3*10.0/900;
    Check(CalendarService.BeginWeek(s,c,new SequenceRandom()) && Math.Abs(h.Market-expected)<1e-10);
    Check(h.GrowthWeeks==1 && h.GrowthBoost==0 && c.Hardware[0].RawMembers["1050"]==40);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    var g=new ReleasedGameState {Title="Synthetic",Price=5800,Stock=10000,Demand=100,MarketFactor=1,Quality=new(100,100,100,100)};
    s.Calendar!.Hardware[0].Market=120;s.ReleasedGames.Add(g);s.SalesFee1=s.SalesFee2=1500;s.SalesTicks=19;
    long predicted=SalesService.PotentialSales(g,c,0,120);SalesService.Tick(s,c,new SequenceRandom(0,0));
    Check(g.UnitsSold==predicted && predicted>SalesService.PotentialSales(g,c,0));
    Check(HardwareCalendarRules.NextMarket(40,0,9,15)==40);
});
Test("loan slots pause in order before signed repayment and cannot charge twice after reload",()=>
{
    var (s,c)=CalendarFixture(new(2,5,1),200000000);
    s.Calendar!.Loans[0]=new() {OriginalType=1,BorrowedOn=new(1,5,2)};
    s.Calendar.Loans[1]=new() {OriginalType=2,BorrowedOn=new(1,5,2)};
    s.Calendar.Loans[2]=new() {OriginalType=3,BorrowedOn=new(1,5,3)};
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar.Pending!.PendingEvent==CalendarEvent.LoanRepayment && s.Money==200000000);
    Check(s.Calendar!.Pending!.Amount==13000000);s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(CalendarService.Acknowledge(s) && s.Money==187000000 && !CalendarService.Acknowledge(s));
    Check(!CalendarService.Resume(s,c,new SequenceRandom()) && s.Calendar!.Pending!.Amount==65000000);
    Check(CalendarService.Acknowledge(s) && s.Money==122000000);Check(CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar!.Loans[0].OriginalType==0 && s.Calendar.Loans[1].OriginalType==0 && s.Calendar!.Loans[2].OriginalType==3);
    Check(s.Company.WeeklyNet==-78000000 && s.Company.MonthlyNet==22-78000000);
    Check(CalendarService.LoanRepayment(3)==130000000 && CalendarService.LoanRepayment(4)==260000000);
});
Test("calendar repayment dialogue survives reload and charges only after acknowledgement",()=>
{
    var (s,c)=CalendarFixture(new(2,5,1),200000000);
    s.Calendar!.Loans[0]=new() {OriginalType=1,BorrowedOn=new(1,5,2)};
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Check(s.Calendar.Pending!.DialoguePage==0 && CalendarDialogueRules.MessageKey(s)=="CALENDAR_DIALOGUE_LoanRepayment");
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(s.Money==200000000 && s.Calendar!.Loans[0].OriginalType==1);
    Check(CalendarDialogueRules.Acknowledge(s) && s.Money==187000000 && s.Calendar.Loans[0].OriginalType==0);
    Check(!CalendarDialogueRules.Acknowledge(s) && s.Money==187000000);
});
Test("calendar conversation keys cover every page and reject forged cursors",()=>
{
    foreach (var kind in Enum.GetValues<CalendarEvent>())
    {
        int count=CalendarDialogueRules.PageCount(kind);
        var state=new GameState {Month=4,Calendar=new() {Pending=new() {PendingEvent=kind,DialoguePage=0}}};
        for(int page=0;page<count;page++)
        {
            state.Calendar.Pending.DialoguePage=page;
            Check(CalendarDialogueRules.ImportKeys.Contains(CalendarDialogueRules.MessageKey(state)));
        }
        state.Calendar.Pending.DialoguePage=count;
        Throws<InvalidOperationException>(()=>CalendarDialogueRules.MessageKey(state));
    }
    Check(CalendarDialogueRules.ImportKeys.Distinct().Count()==27);
});
Test("calendar recall results stop before later product countdowns market growth and loan debt",()=>
{
    var (s,c)=CalendarFixture(new(2,5,1),200000000);
    s.Calendar!.Hardware[0].GrowthWeeks=1;s.Calendar.Loans[0]=new() {OriginalType=1,BorrowedOn=new(1,5,2)};
    for (int i=0;i<2;i++) s.ReleasedGames.Add(new() {ProductIndex=i,Title=$"Synthetic {i}",
        Recall=new() {OriginalState=1,WeeksRemaining=1}});
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom(3)));
    Check(s.ReleasedGames[0].Recall!.PendingEvent==RecallEvent.Result && s.ReleasedGames[1].Recall!.WeeksRemaining==1 && s.Calendar.Hardware[0].GrowthWeeks==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));RecallService.Acknowledge(s,s.ReleasedGames[0]);
    Check(!CalendarService.Resume(s,c,new SequenceRandom(3)) && s.ReleasedGames[1].Recall!.PendingEvent==RecallEvent.Result);
    RecallService.Acknowledge(s,s.ReleasedGames[1]);Check(!CalendarService.Resume(s,c,new SequenceRandom()));
    Check(s.Calendar!.Hardware[0].GrowthWeeks==0 && s.Calendar.Pending!.PendingEvent==CalendarEvent.LoanRepayment);
});
Test("annual events stop at source phases and unknown graduate bounds remain explicit",()=>
{
    foreach (var (date,kind) in new[] {(new GameDate(2,12,4),CalendarEvent.BeginnerEnding),
        (new GameDate(3,12,3),CalendarEvent.Awards),(new GameDate(3,9,4),CalendarEvent.Exhibition)})
    {
        var (s,c)=CalendarFixture(date);if (kind==CalendarEvent.BeginnerEnding) {s.Mode=GameMode.Beginner;s.Rules=GameModeRules.For(s.Mode);}
        Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
        Check(s.Calendar!.Pending!.PendingEvent==kind);string before=SaveStore.Serialize(s);
        Check(!CalendarService.Resume(s,c,new SequenceRandom()) && SaveStore.Serialize(s)==before);
        Check(!CalendarService.Acknowledge(s));
    }
    var (g,gc)=CalendarFixture(new(3,4,1));g.Calendar!.OriginalSlots0444[0]=0;
    Check(!CalendarService.BeginWeek(g,gc,new SequenceRandom()) && g.Calendar.Pending!.Block==CalendarBlock.Graduates);
});
Test("M35 exhibition and normal ending stops resume their existing cursors",()=>
{
    var (s,c)=CalendarFixture(new(3,3,4));
    s.Month=4;s.Week=1;s.Calendar!.Pending=new() {ClosingDate=new(3,3,4),Phase=CalendarPhase.Exhibition,Block=CalendarBlock.Exhibition};
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(!CalendarService.Resume(s,c,new SequenceRandom()) && s.Calendar!.Pending!.PendingEvent==CalendarEvent.Exhibition);
    Check(ExhibitionService.Close(s) && CalendarService.Resume(s,c,new SequenceRandom()));
    (s,c)=CalendarFixture(new(2,12,4));
    s.Calendar!.Pending=new() {ClosingDate=new(2,12,4),Phase=CalendarPhase.EarlyEvents,Block=CalendarBlock.BeginnerEnding};
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Year==3 && s.Month==1);
});
Test("award financial overflow cannot commit a winner marker or partial payment",()=>
{
    var (s,c)=CalendarFixture(new(3,12,3),long.MaxValue);
    s.ReleasedGames=[new() {Title="Synthetic",ReleasedOn=new(3,5,1),OriginalAwardCode=0,Quality=new(0,0,0,70)}];
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    Throws<OverflowException>(()=>AwardService.Reveal(s,new SequenceRandom(1)));
    Check(s.Money==long.MaxValue && s.ReleasedGames[0].OriginalAwardCode==0 && s.Calendar!.Pending!.Awards!.ResultReady is null);
});
Test("calendar modals pause office clocks and completing a boundary resumes its sales batch once",()=>
{
    var (s,c)=CalendarFixture();s.CompanyActivityTicks=199;s.CompanyActivityPerformed=true;s.SalesTicks=19;
    s.Calendar!.Hardware[3].LaunchWeeks=5;s.SalesFee1=s.SalesFee2=1500;
    var g=new ReleasedGameState {Title="Synthetic",Stock=10000,Price=5800,Demand=100,MarketFactor=1,Quality=new(100,100,100,100),WeeklySalesHistory=SalesHistoryRules.NewHistory()};
    s.ReleasedGames.Add(g);g.WeeklySalesHistory[0]=12;
    var session=new GameSession(new SequenceRandom(0,0));session.Restore(s);session.TickOffice(c);
    Check(s.Calendar.Pending!.PendingEvent==CalendarEvent.HardwareSoon && s.SalesTicks==19 && g.UnitsSold==0);
    session.TickOffice(c);Check(s.CompanyActivityTicks==0 && s.SalesTicks==19);
    CalendarService.Acknowledge(s);session.TickOffice(c);
    Check(s.Calendar.Pending is null && s.CompanyActivityTicks==0 && s.SalesTicks==0 && g.UnitsSold>0 && g.WeeklySalesHistory[1]==12);
    long units=g.UnitsSold;session.TickOffice(c);Check(s.CompanyActivityTicks==1 && g.UnitsSold==units);
});
Test("old modern saves do not fabricate calendar countdowns and malformed cursors are rejected",()=>
{
    var old=SaveStore.Deserialize(SaveStore.Serialize(State()));Check(old.Calendar is null);
    var (complete,_)=CalendarFixture();
    Throws<InvalidDataException>(()=>SaveStore.Deserialize(SaveStore.Serialize(complete).Replace("\"GrowthWeeks\": 0,","")));
    var (s,c)=CalendarFixture();s.Calendar!.Hardware[3].LaunchWeeks=5;
    CalendarService.BeginWeek(s,c,new SequenceRandom());
    s.Calendar.Pending!.Index=11;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.Calendar.Pending.Index=3;s.Calendar.Hardware[3].Market=double.NaN;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.Calendar.Hardware[3].Market=40;s.Calendar.Pending.ClosingDate=new(3,5,3);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("only successful first shipments increment the resettable hardware gate and reprints do not",()=>
{
    var (s,c)=CalendarFixture();s.Calendar!.ShipmentsSinceHardwareTrigger=3;
    var project=new DevelopmentProjectState {Title="Synthetic release",RequiredPoints=1,Progress=1,OriginalStage=3,GameTypeSourceIndex=0,GenreSourceIndex=0};
    s.DevelopmentProjects.Add(project);long opening=s.Money!.Value;
    s.Calendar.ShipmentsSinceHardwareTrigger=int.MaxValue;
    Throws<OverflowException>(()=>ReleaseService.TryRelease(s,project,c,project.Title,5800,100,new SequenceRandom()));
    Check(s.Money==opening && s.DevelopmentProjects.Count==1 && s.ReleasedGames.Count==0);
    s.Calendar.ShipmentsSinceHardwareTrigger=3;
    Check(ReleaseService.TryRelease(s,project,c,project.Title,5800,100,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    Check(s.Calendar.ShipmentsSinceHardwareTrigger==4);AcknowledgeReviews(s,s.ReleasedGames[0]);
    s.ProductionPerformedThisWeek=false;
    Check(ReleaseService.TryProduce(s,s.ReleasedGames[0],c,100)==ReleaseResult.Success && s.Calendar.ShipmentsSinceHardwareTrigger==4);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).Calendar!.ShipmentsSinceHardwareTrigger==4);
});
Test("third loan acknowledgement remains saveable before final calendar resumption",()=>
{
    var (s,c)=CalendarFixture(new(2,5,1),1);
    s.Calendar!.Loans[2]=new() {OriginalType=4,BorrowedOn=new(1,5,2)};
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar.Pending!.Index==2);
    Check(CalendarService.Acknowledge(s) && s.Money==-259999999);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.Calendar!.Pending!.Index==3 && s.Calendar.Pending.PendingEvent is null);
    Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==-259999999 && s.Calendar.Pending is null);
});
Test("signed financial helper updates all known periods without clamping negative funds",()=>
{
    var s=new GameState {Money=10,Company=new() {WeeklyNet=5,MonthlyNet=20,YearlyNet=30}};
    FinancialRules.Apply(s,-15);
    Check(s.Money==-5 && s.Company.WeeklyNet==-10 && s.Company.MonthlyNet==5 && s.Company.YearlyNet==15);
    FinancialRules.Apply(s,7);
    Check(s.Money==2 && s.Company.WeeklyNet==-3 && s.Company.MonthlyNet==12 && s.Company.YearlyNet==22);
});
Test("financial helper preserves unknown old periods through save load",()=>
{
    var s=State(100);s.Company.MonthlyNet=-30;
    FinancialRules.Apply(s,20);s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(s.Money==120 && s.Company.WeeklyNet is null && s.Company.MonthlyNet==-10 && s.Company.YearlyNet is null);
});
Test("financial overflow rejects before any cash or period mutation",()=>
{
    foreach (int cell in Enumerable.Range(0,4))
    {
        var s=State(0);s.Company.WeeklyNet=s.Company.MonthlyNet=s.Company.YearlyNet=0;
        if (cell==0) s.Money=long.MaxValue;
        if (cell==1) s.Company.WeeklyNet=long.MaxValue;
        if (cell==2) s.Company.MonthlyNet=long.MaxValue;
        if (cell==3) s.Company.YearlyNet=long.MaxValue;
        string before=SaveStore.Serialize(s);
        Throws<OverflowException>(()=>FinancialRules.Apply(s,1));
        Check(SaveStore.Serialize(s)==before);
    }
});
Test("financial period reset follows weekly monthly and December boundaries before charges",()=>
{
    var s=new GameState {Money=100,Company=new() {WeeklyNet=1,MonthlyNet=2,YearlyNet=3}};
    FinancialRules.ResetAtWeekEnd(s,new(1,4,1));
    Check(s.Company.WeeklyNet==0 && s.Company.MonthlyNet==2 && s.Company.YearlyNet==3 && s.Money==100);
    FinancialRules.ResetAtWeekEnd(s,new(1,4,4));
    Check(s.Company.MonthlyNet==0 && s.Company.YearlyNet==3);
    FinancialRules.ResetAtWeekEnd(s,new(1,12,4));FinancialRules.Apply(s,-7);
    Check(s.Money==93 && s.Company.WeeklyNet==-7 && s.Company.MonthlyNet==-7 && s.Company.YearlyNet==-7);
});
Test("fresh company accounting starts empty zero without counting opening funds as income",()=>
{
    var s=new GameSession().StartNewGame("Synthetic","Tester",GameMode.Normal);
    Check(s.Money==450000000 && s.Company.WeeklyNet==0 && s.Company.MonthlyNet==0 && s.Company.YearlyNet==0);
});
Test("renewal accounting overflow cannot dismiss unchecked employees",()=>
{
    var s=State(1000);s.Company.WeeklyNet=long.MinValue;
    s.Employees.Add(new() {Name="Synthetic zero",SourceIndex=0,ContractFee=1});s.Employees.Add(new() {Name="Synthetic one",SourceIndex=1,ContractFee=1});
    s.EmployeeSources.Add(new() {SourceIndex=0,GateValue=1,Employee=s.Employees[0]});
    s.EmployeeSources.Add(new() {SourceIndex=1,GateValue=1,Employee=s.Employees[1]});
    string before=SaveStore.Serialize(s);
    Throws<OverflowException>(()=>ContractRenewalService.TryRenew(s,new HashSet<int> {0}));
    Check(SaveStore.Serialize(s)==before);
});
Test("hiring and actual sales feed the same financial periods without duplicate charges",()=>
{
    var s=PlanningState(10000);var c=CoreCatalog();
    s.Company.WeeklyNet=s.Company.MonthlyNet=s.Company.YearlyNet=0;
    var candidate=new EmployeeState {Name="Synthetic applicant",SourceIndex=0,ContractFee=100};
    Check(HiringService.TryHire(s,candidate)==HireResult.Success);
    Check(s.Company.WeeklyNet==-100 && s.Company.MonthlyNet==-100 && s.Company.YearlyNet==-100);
    Check(HiringService.TryHire(s,candidate)==HireResult.AlreadyEmployed);
    Check(s.Company.WeeklyNet==-100);
    s.SalesFee1=s.SalesFee2=1500;s.SalesTicks=19;
    var product=new ReleasedGameState {Title="Synthetic",Stock=100,Price=5800,Demand=100,MarketFactor=1,
        Quality=new(10,10,10,10),SalesMilestoneStage=0,Recall=new(),WeeklySalesHistory=SalesHistoryRules.NewHistory()};
    s.ReleasedGames.Add(product);SalesService.Tick(s,c,new SequenceRandom(0,0));
    long change=s.Money!.Value-10000;
    Check(product.UnitsSold>0 && s.Company.WeeklyNet==change && s.Company.MonthlyNet==change && s.Company.YearlyNet==change);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.Company.WeeklyNet==change);
});
Test("office employee selection uses inclusive original sprite inset", () =>
{
    var state=new GameState();
    var employee=new EmployeeState {Name="Synthetic worker",Office=new() {X=100,Y=50,SlotIndex=0}};
    state.Employees.Add(employee);
    foreach (var point in new[] {(102d,52d),(114d,80d),(108d,66d)})
        Check(ReferenceEquals(OfficeInteractionRules.EmployeeAt(state,point.Item1,point.Item2),employee));
    foreach (var point in new[] {(101.99,52d),(114.01,80d),(108d,51.99),(108d,80.01)})
        Check(OfficeInteractionRules.EmployeeAt(state,point.Item1,point.Item2) is null);
});
Test("office selection follows workspace order rather than paint depth or roster order", () =>
{
    var state=new GameState();
    var first=new EmployeeState {Name="First workspace",Office=new() {SlotIndex=1,X=100,Y=50}};
    var last=new EmployeeState {Name="Later workspace",Office=new() {SlotIndex=7,X=100,Y=50,DepthAdjustment=100}};
    state.Employees.Add(last);state.Employees.Add(first);
    Check(ReferenceEquals(OfficeInteractionRules.EmployeeAt(state,108,66),first));
    first.Office.X=200;
    Check(ReferenceEquals(OfficeInteractionRules.EmployeeAt(state,108,66),last));
});
Test("office selection tracks walking coordinates and seated visual offsets without mutation", () =>
{
    var state=new GameState();
    var employee=new EmployeeState {Name="Synthetic worker",Office=new() {SlotIndex=0,X=100,Y=50,OffsetX=-4,OffsetY=2}};
    state.Employees.Add(employee);
    Check(ReferenceEquals(OfficeInteractionRules.EmployeeAt(state,98,54),employee));
    Check(OfficeInteractionRules.EmployeeAt(state,114,66) is null);
    employee.Office.X=104;employee.Office.Y=52;employee.Office.OffsetX=employee.Office.OffsetY=0;
    Check(ReferenceEquals(OfficeInteractionRules.EmployeeAt(state,106,54),employee));
    Check(employee.Office.X==104 && employee.Office.Y==52 && state.Money is null);
});
Test("unknown workspaces and nonfinite office pointers cannot select an employee", () =>
{
    var state=new GameState();state.Employees.Add(new() {Name="Legacy unknown"});
    Check(OfficeInteractionRules.EmployeeAt(state,0,0) is null);
    state.Employees.Add(new() {Name="Synthetic worker",Office=new()});
    foreach (double value in new[] {double.NaN,double.PositiveInfinity,double.NegativeInfinity})
    {
        Check(OfficeInteractionRules.EmployeeAt(state,value,60) is null);
        Check(OfficeInteractionRules.EmployeeAt(state,135,value) is null);
    }
});
Test("modern saves validate progression in inactive source records too", () =>
{
    var state = new GameState { Company = new() { Name = "Test", PresidentName = "Tester" } };
    var employee = new EmployeeState { SourceIndex = 0, Name = "Synthetic source", Level = 1, Experience = 5, Stress = 10 };
    state.EmployeeSources.Add(new() { SourceIndex = 0, GateValue = 0, Employee = employee });
    foreach (double invalid in new[] { -1d, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    {
        employee.Experience = invalid;
        Throws<InvalidDataException>(() => SaveStore.Serialize(state));
        employee.Experience = 5;
        employee.Stress = invalid;
        Throws<InvalidDataException>(() => SaveStore.Serialize(state));
        employee.Stress = 10;
    }
    foreach (int invalid in new[] { 0, 11 })
    {
        employee.Level = invalid;
        Throws<InvalidDataException>(() => SaveStore.Serialize(state));
    }
});
Test("modern loads reject corrupt source progression before recruitment", () =>
{
    var state = new GameState { Company = new() { Name = "Test", PresidentName = "Tester" } };
    state.EmployeeSources.Add(new() { SourceIndex = 0, GateValue = 0,
        Employee = new() { SourceIndex = 0, Name = "Synthetic source", Level = 1, Experience = 5, Stress = 10 } });
    string text = SaveStore.Serialize(state);
    foreach (var field in new[] { "Level", "Experience", "Stress" })
    {
        var envelope = System.Text.Json.Nodes.JsonNode.Parse(text)!;
        var employee = envelope["State"]!["EmployeeSources"]![0]!["Employee"]!;
        employee[field] = field == "Level" ? 0 : -1;
        Throws<InvalidDataException>(() => SaveStore.Deserialize(envelope.ToJsonString()));
    }
});
Test("invalid source progression cannot replace a readable modern slot", () =>
{
    string directory = Path.Combine(Path.GetTempPath(), "OpenGDS2-source-" + Guid.NewGuid());
    string slot = Path.Combine(directory, "slot1.json");
    var state = new GameState { Company = new() { Name = "Test", PresidentName = "Tester" } };
    var employee = new EmployeeState { SourceIndex = 0, Name = "Synthetic source", Level = 1 };
    state.EmployeeSources.Add(new() { SourceIndex = 0, GateValue = 0, Employee = employee });
    try
    {
        SaveStore.Save(slot, state);
        byte[] original = File.ReadAllBytes(slot);
        employee.Level = 0;
        Throws<InvalidDataException>(() => SaveStore.Save(slot, state));
        Check(File.ReadAllBytes(slot).SequenceEqual(original));
        Check(SaveStore.Load(slot).EmployeeSources.Single().Employee.Level == 1);
        Check(Directory.GetFiles(directory).Length == 1);
    }
    finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
});
Test("unknown source progression remains nullable across modern save load", () =>
{
    var state = new GameState { Company = new() { Name = "Test", PresidentName = "Tester" } };
    state.EmployeeSources.Add(new() { SourceIndex = 0, GateValue = 0, Employee = new() { SourceIndex = 0, Name = "Synthetic source" } });
    var restored = SaveStore.Deserialize(SaveStore.Serialize(state)).EmployeeSources.Single().Employee;
    Check(restored.Level is null && restored.Experience is null && restored.Stress is null);
});
Test("oversized modern saves cannot replace an existing readable slot", () =>
{
    string directory = Path.Combine(Path.GetTempPath(), "OpenGDS2-size-" + Guid.NewGuid());
    string slot = Path.Combine(directory, "slot1.json");
    var state = new GameState { Company = new() { Name = "Test", PresidentName = "Tester" } };
    try
    {
        SaveStore.Save(slot, state);
        byte[] original = File.ReadAllBytes(slot);
        state.Company.Name = new string('A', 4 * 1024 * 1024);
        Throws<InvalidDataException>(() => SaveStore.Serialize(state));
        Throws<InvalidDataException>(() => SaveStore.Save(slot, state));
        Check(File.ReadAllBytes(slot).SequenceEqual(original));
        Check(SaveStore.Load(slot).Company.Name == "Test");
        Check(Directory.GetFiles(directory).Length == 1);
    }
    finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
});
Test("modern save size boundary includes JSON overhead and accepts an exact-limit round trip", () =>
{
    const int limit = 4 * 1024 * 1024;
    var state = new GameState { Company = new() { Name = "Test", PresidentName = "Tester" } };
    int overhead = System.Text.Encoding.UTF8.GetByteCount(SaveStore.Serialize(state)) - state.Company.Name.Length;
    state.Company.Name = new string('A', limit - overhead);
    string text = SaveStore.Serialize(state);
    Check(System.Text.Encoding.UTF8.GetByteCount(text) == limit);
    Check(SaveStore.Deserialize(text).Company.Name == state.Company.Name);
    state.Company.Name += "A";
    Throws<InvalidDataException>(() => SaveStore.Serialize(state));
});
Test("negotiation outcome preserves exact probability boundaries for both fee modes", () =>
{
    foreach (long fee in new long[] {1300, 1100, 900, 700})
    {
        int width = FeeNegotiationRules.OutcomeDrawRange(fee);
        var success = new DoubleSequenceRandom(Math.BitDecrement(1.0 / width));
        var failure = new DoubleSequenceRandom(1.0 / width);
        Check(FeeNegotiationRules.Resolve(fee, success) == fee - 200 && success.Calls == 1);
        Check(FeeNegotiationRules.Resolve(fee, failure) == fee && failure.Calls == 1);
    }
});
Test("guaranteed first negotiation and terminal or unmatched fees consume no outcome draw", () =>
{
    var random = new DoubleSequenceRandom();
    Check(FeeNegotiationRules.Resolve(1500, random) == 1300);
    foreach (long fee in new long[] {500, 0, 1250, 1700, -1})
        Check(FeeNegotiationRules.Resolve(fee, random) == fee);
    Check(random.Calls == 0);
});
Test("successive negotiation outcomes reach five hundred without another reduction", () =>
{
    var random = new DoubleSequenceRandom(0, 0, 0, 0);
    long fee = 1500;
    foreach (long expected in new long[] {1300, 1100, 900, 700, 500, 500})
    { fee = FeeNegotiationRules.Resolve(fee, random); Check(fee == expected); }
    Check(random.Calls == 4);
});
Test("negotiation rejects invalid random outcome values", () =>
{
    foreach (double draw in new[] {double.NaN, double.PositiveInfinity, -0.01, 1.0})
        Throws<ArgumentOutOfRangeException>(() => FeeNegotiationRules.Resolve(1300, new UncheckedDoubleRandom(draw)));
    Throws<ArgumentNullException>(() => FeeNegotiationRules.Resolve(1500, null!));
});
Test("local artwork overlays require original assets and cannot replace rule data", () =>
{
    string root = Path.Combine(Path.GetTempPath(), "OpenGDS2-overlays-" + Guid.NewGuid());
    string original = Path.Combine(root, "original"), overlay = Path.Combine(root, "overlays");
    Directory.CreateDirectory(Path.Combine(original, "Graphic")); Directory.CreateDirectory(Path.Combine(overlay, "Graphic"));
    try
    {
        string source = Path.Combine(original, "Graphic", "Test.bmp");
        string replacement = Path.Combine(overlay, "Graphic", "Test.png");
        File.WriteAllText(replacement, "synthetic");
        Check(LocalAssetOverlay.Resolve(original, overlay, "Graphic/Test.bmp") == source);
        File.WriteAllText(source, "synthetic");
        Check(LocalAssetOverlay.Resolve(original, overlay, "Graphic/Test.bmp") == replacement);
        File.WriteAllText(Path.Combine(overlay, "catalog.json"), "synthetic");
        Check(LocalAssetOverlay.Resolve(original, overlay, "catalog.json") == Path.Combine(original, "catalog.json"));
        foreach (string unsafePath in new[] { "../Graphic/Test.bmp", "/Graphic/Test.bmp", "C:/Test.bmp", "Graphic/./Test.bmp", "Graphic//Test.bmp", "Graphic/../Test.bmp" })
            Throws<ArgumentException>(() => LocalAssetOverlay.Resolve(original, overlay, unsafePath));
    }
    finally { Directory.Delete(root, true); }
});

EmployeeState Candidate(int id, long? fee = 100) => new() { SourceIndex = id, Name = "Synthetic " + id, ContractFee = fee, OriginalAttributes = [1, 2, 3, 4, 5, 6, 7] };
RecruitmentSourceRecord Row(int id, int gate = 0) => new() { SourceIndex = id, GateValue = gate, Employee = Candidate(id) };
GameState State(long? money = 100) => new() { Money = money, Company = new() { Name = "Test", PresidentName = "Tester" } };


OriginalCatalog PlanningCatalog()=>new()
{
    DevelopmentComputers=[new() {SourceIndex=0,BitWidth=8}],
    Hardware=[new() {SourceIndex=0,RawMembers=new() {{"0F00",8},{"0FE0",100},{"0FFC",10},{"0EC8",0}}}],
    Audiences=[new() {SourceIndex=0,RawMembers=new() {{"1454",300},{"148C",20},{"14A8",2}}}],
    GameTypes=[
        new() {SourceIndex=0,RawMembers=new() {{"1378",5},{"1308",10},{"13B0",8}},RawTable13CC=Enumerable.Repeat(42,15).ToList()},
        new() {SourceIndex=1,RawMembers=new() {{"1378",6},{"1308",20},{"13B0",9}},RawTable13CC=Enumerable.Repeat(99,15).ToList()}],
    Genres=[new() {SourceIndex=0,RawMembers=new() {{"1224",7},{"1208",30},{"1240",42},{"125C",3}}}]
};
GameState PlanningState(long? money=100)=>new()
{
    Money=money,Company=new() {Name="Synthetic",PresidentName="Tester",Fame=14},
    DevelopmentComputerIndex=0,AvailableHardware=[0],HardwareLicenses=[0],
    CompanyActivityTicks=0,CompanyActivityPerformed=false,ProductionPerformedThisWeek=false
};
OriginalCatalog CoreCatalog()
{
    var c=PlanningCatalog();c.Hardware[0].RawMembers["1050"]=40;
    c.Hardware[0].ProductionBands=[new(16,300),new(32,700),new(48,1200),new(64,1800)];
    c.GameTypes[0].RawMembers["12EC"]=3;c.GameTypes[1].RawMembers["12EC"]=3;
    return c;
}
EmployeeState Worker(int action=6)=>new()
{
    Name="Synthetic Worker",SourceIndex=0,PortraitId=1,Level=1,Experience=0,Stress=0,
    Fun=50,Novelty=50,Graphics=50,Music=50,Accuracy=50,DevelopmentSpeed=50,Inspiration=50,
    Office=new(){Action=action,SeatedAction=action,WorkEngaged=true,MovementState=0,Facing=1}
};
TechnologyDefinition TrainingTechnology(int id=0)=>new() {SourceIndex=id,RequiredTrainingWork=[3,6,9],ExperienceCosts=[10,20,30],PurchaseFee=50};
(GameState State,EmployeeState Employee,OriginalCatalog Catalog) TrainingFixture()
{
    var s=State();var e=Worker(1);e.Experience=100;e.TechnologyLevels=new() {{0,0},{1,0}};
    e=new EmployeeState {Name=e.Name,SourceIndex=0,PortraitId=1,Experience=100,Stress=0,DevelopmentSpeed=100,
        RawParameter34=1,TechnologyLevels=e.TechnologyLevels,Office=e.Office};
    s.Employees.Add(e);s.TechnologyOwnershipLevels=new() {{0,3},{1,3}};
    return (s,e,new() {Technologies=[TrainingTechnology(),TrainingTechnology(1)]});
}
Test("technology purchase charges once and unlocks all three training levels",()=>
{
    var s=State(50);s.TechnologyOwnershipLevels=new() {{0,0}};var t=TrainingTechnology();
    Check(TechnologyService.TryPurchase(s,t)==TechnologyPurchaseResult.Success && s.Money==0 && s.TechnologyOwnershipLevels[0]==3);
    Check(TechnologyService.TryPurchase(s,t)==TechnologyPurchaseResult.AlreadyOwned && s.Money==0);
    s.TechnologyOwnershipLevels[0]=0;Check(TechnologyService.TryPurchase(s,t)==TechnologyPurchaseResult.InsufficientFunds && s.TechnologyOwnershipLevels[0]==0);
    s.Money=null;Check(TechnologyService.TryPurchase(s,t)==TechnologyPurchaseResult.StateUnknown);
});
Test("training acceptance spends experience once and upgrades a known skill at capacity",()=>
{
    var (s,e,c)=TrainingFixture();e.TechnologyLevels![0]=1;
    Check(TrainingService.TryStart(s,e,c.Technologies[1])==TrainingResult.CapacityReached && e.Experience==100 && e.Office!.Training is null);
    Check(TrainingService.TryStart(s,e,c.Technologies[0])==TrainingResult.Success && e.Experience==80 && e.Office!.Training!.Progress==0);
    e.Experience=19;Check(TrainingService.TryStart(s,e,c.Technologies[0])==TrainingResult.InsufficientExperience && e.Experience==19);
    e.TechnologyLevels[0]=3;Check(TrainingService.CheckStart(s,e,c.Technologies[0])==TrainingResult.Unavailable);
});
Test("training rejects duplicate source identities before spending experience",()=>
{
    var (s,e,c)=TrainingFixture();s.EmployeeSources.Add(Row(0));s.EmployeeSources.Add(Row(0));
    Throws<InvalidOperationException>(()=>TrainingService.TryStart(s,e,c.Technologies[0]));
    Check(e.Experience==100 && e.Office!.Training is null);
});
Test("training preserves original fractional cadence and random draw order",()=>
{
    var (s,e,c)=TrainingFixture();Check(TrainingService.TryStart(s,e,c.Technologies[0])==TrainingResult.Success);
    var r=new SequenceRandom(2,4,0);Check(!TrainingService.Tick(s,e,c,r) && e.Office!.Training!.Progress==0 && r.Calls==1);
    Check(TrainingService.Tick(s,e,c,r) && e.TechnologyLevels![0]==1 && e.Office!.Training is null && r.Calls==3);
    Check(Math.Abs(e.Stress!.Value-4/30.0)<1e-12 && e.Experience==90);
    e.DevelopmentSpeed=85;Check(TrainingService.TryStart(s,e,c.Technologies[0])==TrainingResult.Success);
    for (int i=0;i<3;i++) {TrainingService.Tick(s,e,c,new ConstantRandom(0));Check(e.Office!.Training!.Progress==0);}
    TrainingService.Tick(s,e,c,new ConstantRandom(0));Check(e.Office!.Training!.Progress==1);
});
Test("training pauses while walking resting or exhausted and persists across reload",()=>
{
    var (s,e,c)=TrainingFixture();TrainingService.TryStart(s,e,c.Technologies[0]);
    e.Office!.MovementState=3;e.Office.SeatedAction=0;e.Office.WorkEngaged=false;
    Check(!TrainingService.Tick(s,e,c,new SequenceRandom()) && e.Stress==0 && e.Office.Training!.Progress==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];
    Check(e.Office!.Training is not null && e.Experience==90);
    Check(OfficeAssignmentService.TryAssignDesk(s,e,6)==OfficeAssignmentResult.Success);
    Check(!TrainingService.Tick(s,e,c,new SequenceRandom()) && e.Office.Training is not null);
    Check(OfficeAssignmentService.TryAssignStudySeat(s,e,1)==OfficeAssignmentResult.Success);
    for (int i=0;i<1000 && e.Office.MovementState!=0;i++) OfficeStaffRules.Tick(e,new ConstantRandom(0));
    Check(e.Office.MovementState==0 && e.Office.SeatedAction==1);
    e.Stress=100;Check(!TrainingService.Tick(s,e,c,new SequenceRandom()) && e.Office.Training!.Progress==0);
    e.Stress=99.99;TrainingService.Tick(s,e,c,new ConstantRandom(0));Check(e.Stress==100);
});
Test("all five study seats route and reserve independently from development desks",()=>
{
    var s=State();var workers=Enumerable.Range(0,5).Select(id=>new EmployeeState {Name="Synthetic "+id,SourceIndex=id,PortraitId=1,Office=new() {SlotIndex=id}}).ToArray();s.Employees.AddRange(workers);
    for (int i=0;i<5;i++) Check(OfficeAssignmentService.TryAssignStudySeat(s,workers[i],i+1)==OfficeAssignmentResult.Success);
    Check(OfficeAssignmentService.TryAssignStudySeat(s,workers[1],1)==OfficeAssignmentResult.Occupied);
    for (int tick=0;tick<2000;tick++) foreach (var e in workers) OfficeStaffRules.Tick(e,new ConstantRandom(0));
    Check(workers.All(x=>x.Office!.MovementState==0 && x.Office.SeatedAction==x.SourceIndex+1));
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).Employees.Count==5);
});
Test("learned technologies survive dismissal recruitment cloning and rehiring",()=>
{
    var (s,e,c)=TrainingFixture();var row=new RecruitmentSourceRecord {SourceIndex=0,GateValue=1,Employee=new() {Name=e.Name,SourceIndex=0,PortraitId=1,ContractFee=0}};s.EmployeeSources.Add(row);
    TrainingService.TryStart(s,e,c.Technologies[0]);TrainingService.Tick(s,e,c,new ConstantRandom(0));
    for (int i=0;i<10 && e.Office!.Training is not null;i++) TrainingService.Tick(s,e,c,new ConstantRandom(0));
    Check(row.Employee.TechnologyLevels![0]==1 && row.Employee.Experience==90);
    Check(EmployeeDismissalService.TryDismiss(s,e)==DismissalResult.Success);
    var applicant=RecruitmentGenerator.BuildRoster(s.EmployeeSources,0,1,new ConstantRandom(0))[0];
    Check(applicant.TechnologyLevels![0]==1 && HiringService.TryHire(s,applicant,new ConstantRandom(0))==HireResult.Success);
    s.Employees[0].TechnologyLevels![0]=2;Check(applicant.TechnologyLevels[0]==1 && row.Employee.TechnologyLevels[0]==1);
});
Test("unknown old save training data stays unknown and malformed skill saves are rejected",()=>
{
    var s=SaveStore.Deserialize(SaveStore.Serialize(State()));Check(s.TechnologyOwnershipLevels is null);
    s.TechnologyOwnershipLevels=new() {{0,4}};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    var (known,e,c)=TrainingFixture();e.TechnologyLevels![0]=4;Throws<InvalidDataException>(()=>SaveStore.Serialize(known));
    e.TechnologyLevels[0]=0;e.Office!.Training=new() {TechnologySourceIndex=1,Progress=-1};Throws<InvalidDataException>(()=>SaveStore.Serialize(known));
    e.Office.Training=null;e.TechnologyLevels=null;Check(TrainingService.CheckStart(known,e,c.Technologies[0])==TrainingResult.StateUnknown);
});
TechnologyDefinition UseTechnology(int id=0,int units=2)=>new() {SourceIndex=id,Identifier=id,
    RawTable1568=[5,10,15],RawTable158C=[units,units,units]};
(GameState State,EmployeeState Employee,OriginalCatalog Catalog,DevelopmentProjectState Project) UseFixture()
{
    var state=State();var employee=Worker();employee.Experience=100;
    employee.TechnologyLevels=Enumerable.Range(0,8).ToDictionary(x=>x,_=>1);
    state.Employees.Add(employee);
    var catalog=CoreCatalog();catalog.Technologies=Enumerable.Range(0,8).Select(x=>UseTechnology(x)).ToList();
    var project=new DevelopmentProjectState {Title="Synthetic technology project",RequiredPoints=100,
        OriginalStage=3,GameTypeSourceIndex=0,GenreSourceIndex=0,Focus=new(25,25,25,25),
        Quality=new(100,100,100,100),DataSize=101.9,Bugs=101,
        TechnologyApplicationCounts=TechnologyApplicationService.NewProjectCounters()};
    state.DevelopmentProjects.Add(project);return (state,employee,catalog,project);
}
Test("technology diminishing returns and original random-width differences",()=>
{
    foreach (int id in TechnologyApplicationService.ProjectTechnologyIdentifiers)
    for (int level=1;level<=3;level++)
    {
        int width=id is 1 or 6 or 7 ? 2*level+1 : 3;
        Check(TechnologyApplicationService.Percentage(id,level,0,new SequenceRandom(0))==10+10*level);
        Check(TechnologyApplicationService.Percentage(id,level,0,new SequenceRandom(width-1))==10+10*level+width-1);
        Check(TechnologyApplicationService.Percentage(id,level,1,new SequenceRandom(0))==(level==1 ? 5 : level==2 ? 10 : 20));
        Check(TechnologyApplicationService.Percentage(id,level,2,new SequenceRandom(width-1))==width-1);
        Check(TechnologyApplicationService.Percentage(id,level,20,new SequenceRandom(0))==0);
    }
    Throws<ArgumentOutOfRangeException>(()=>TechnologyApplicationService.Percentage(4,1,0,new Random()));
});
Test("technology completion floors affected values and keeps separate project counters",()=>
{
    var (s,e,c,p)=UseFixture();
    TechnologyApplicationService.ApplyCompleted(p,c.Technologies[0],1,new SequenceRandom(0));Check(p.DataSize==81 && p.TechnologyApplicationCounts![0]==1);
    TechnologyApplicationService.ApplyCompleted(p,c.Technologies[0],1,new SequenceRandom(0));Check(p.DataSize==76 && p.TechnologyApplicationCounts![0]==2);
    TechnologyApplicationService.ApplyCompleted(p,c.Technologies[0],1,new SequenceRandom(2));Check(p.DataSize==74 && p.TechnologyApplicationCounts![0]==3);
    foreach (int id in new[]{1,2,3,6,7}) TechnologyApplicationService.ApplyCompleted(p,c.Technologies[id],1,new SequenceRandom(0));
    Check(p.Quality==new DevelopmentQuality(120,120,120,120) && p.Bugs==80);
    Check(p.TechnologyApplicationCounts!.Where(x=>x.Key!=0).All(x=>x.Value==1));
    p.Quality=new();TechnologyApplicationService.ApplyCompleted(p,c.Technologies[1],3,new SequenceRandom(6));Check(p.Quality.Graphics==0);
});
Test("technology use spends experience without requiring company ownership",()=>
{
    var (s,e,c,p)=UseFixture();s.TechnologyOwnershipLevels=null;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[0])==TechnologyApplicationResult.Success);
    Check(e.Experience==95 && e.Office!.TechnologyApplication!.RemainingUnits==2 && s.Money==100);
    e.TechnologyLevels![1]=0;Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[1])==TechnologyApplicationResult.NotLearned);
    e.Experience=4;Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[0])==TechnologyApplicationResult.InsufficientExperience && e.Experience==4);
    e.Experience=100;p.PendingDecision=DevelopmentMilestone.StartDebugging;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[0])==TechnologyApplicationResult.DecisionPending);
    p.PendingDecision=null;p.TechnologyApplicationCounts=null;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[0])==TechnologyApplicationResult.StateUnknown);
});
Test("timed technology uses both source increments: first unit 11 passes, subsequent units 10",()=>
{
    var (s,e,c,p)=UseFixture();Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[0])==TechnologyApplicationResult.Success);
    for (int tick=0;tick<9;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office!.WorkTicks==19 && e.Office.WorkTier==4 && e.Office.TechnologyApplication!.RemainingUnits==2);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.WorkTicks==21 && e.Office.TechnologyApplication!.RemainingUnits==2 && p.DataSize==101.9);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];p=s.DevelopmentProjects[0];
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office!.WorkTicks==3 && e.Office.TechnologyApplication!.RemainingUnits==1);
    for (int tick=0;tick<9;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.WorkTicks==21 && e.Office.TechnologyApplication!.RemainingUnits==1 && p.DataSize==101.9);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.TechnologyApplication is null && e.Office.WorkTicks==0 && p.DataSize==81 && e.Experience==95);
});
Test("technology waits for an ordinary contribution then replaces further ordinary work",()=>
{
    var (s,e,c,p)=UseFixture();p.OriginalStage=0;p.GameTypeSourceIndex=null;p.GenreSourceIndex=null;p.RequiredPoints=1000;
    e.Office!.WorkTier=1;e.Office.WorkTicks=16;e.Office.WorkCategory=DevelopmentWorkCategory.Fun;
    TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);double before=p.Quality.Fun;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.WorkTicks==0 && p.Quality.Fun>before && e.Office.TechnologyApplication!.RemainingUnits==2);
    double contributed=p.Quality.Fun;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office.WorkTier==4 && e.Office.WorkTicks==3);
    for (int tick=0;tick<19;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.TechnologyApplication!.RemainingUnits==1 && p.Quality.Fun==contributed);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.TechnologyApplication is null && p.Quality.Fun==contributed && e.Experience==95);
});
Test("saved even technology counters cross the threshold and apply exactly once",()=>
{
    var (s,e,c,p)=UseFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);
    e.Office!.WorkTier=4;e.Office.WorkTicks=20;
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];p=s.DevelopmentProjects[0];
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office!.WorkTicks==3 && e.Office.TechnologyApplication!.RemainingUnits==1 && p.DataSize==101.9);
    for (int tick=0;tick<10;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.TechnologyApplication is null && e.Office.WorkTicks==0 && p.DataSize==81);
    for (int tick=0;tick<10;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.DataSize==81 && p.TechnologyApplicationCounts![0]==1 && e.Experience==95);
});
Test("technology pauses away from desks and cancels when its room is empty",()=>
{
    var (s,e,c,p)=UseFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);
    OfficeAssignmentService.TryAssignStudySeat(s,e,1);
    DevelopmentSimulation.TickEmployee(s,e,c,new SequenceRandom());Check(e.Office!.TechnologyApplication!.RemainingUnits==2);
    e.Office.Action=e.Office.SeatedAction=6;e.Office.WorkEngaged=true;e.Office.MovementState=0;
    s.DevelopmentProjects.Clear();DevelopmentSimulation.TickEmployee(s,e,c,new SequenceRandom());
    Check(e.Office.TechnologyApplication is null && e.Experience==95);
});
Test("stress relief floors the selected employee stress and leaves unresolved branches untouched",()=>
{
    var (s,e,c,p)=UseFixture();e.Stress=99.9;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success && e.Stress==49 && e.Experience==95);
    e.TechnologyLevels![5]=2;e.Stress=99.9;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success && e.Stress==19 && e.Experience==85);
    e.TechnologyLevels[5]=3;e.Stress=100;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.UnresolvedBehavior && e.Stress==100 && e.Experience==85);
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[4])==TechnologyApplicationResult.UnresolvedBehavior && e.Experience==85);
});
Test("new plans initialize all technology counters while legacy plans remain unknown",()=>
{
    var s=PlanningState();var c=PlanningCatalog();
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Synthetic",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    Check(s.DevelopmentProjects[0].TechnologyApplicationCounts!.Count==6 && s.DevelopmentProjects[0].TechnologyApplicationCounts!.Values.All(x=>x==0));
    s.DevelopmentProjects[0].TechnologyApplicationCounts![0]=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.DevelopmentProjects[0].TechnologyApplicationCounts=null;Check(SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects[0].TechnologyApplicationCounts is null);
});

Test("review strict thresholds and maximum scores",()=>
{
    double[] bases=[0,129.99,130,199.99,200,249.99,250,299.99,300,349.99,350,399.99,400,449.99,450,799.99,800];
    int[] low=[1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,8];
    for (int i=0;i<bases.Length;i++)
    {
        Check(ReviewService.Score(bases[i],1,new SequenceRandom(0))==low[i]);
        int width=bases[i]<400 || bases[i]>=800 ? 3 : 2;
        Check(ReviewService.Score(bases[i],1,new SequenceRandom(width-1))==low[i]+width-1);
    }
    Throws<ArgumentOutOfRangeException>(()=>ReviewService.Score(double.NaN,1,new Random()));
});
Test("review market-factor redraw replaces score and consumes initial draw",()=>
{
    var r=new SequenceRandom(1,0);Check(ReviewService.Score(450,2,r)==7 && r.Calls==2);
    Check(ReviewService.Score(400,2,new SequenceRandom(1,2))==9);
    Check(ReviewService.Score(800,2,new SequenceRandom(2,0))==7);
    Check(ReviewService.Score(800,2,new SequenceRandom(0,3))==10);
    var below=new SequenceRandom(2);Check(ReviewService.Score(399.99,20,below)==8 && below.Calls==1);
    Check(ReviewService.Score(450,1.999,new SequenceRandom(1))==9);
});
Test("review basis uses type and market factor but excludes bugs and recognition",()=>
{
    var g=new ReleasedGameState {Quality=new(100,200,300,400),MarketFactor=2,Bugs=1000,Recognition=90};
    Check(ReviewService.Basis(g,CoreCatalog())==2000);
    var c=CoreCatalog();c.GameTypes[0].RawMembers["12EC"]=8;
    Check(ReviewService.Basis(g,c)==3000);
});
Test("pending reviews survive save/load and pause simulation until acknowledged",()=>
{
    var session=new GameSession();var s=session.StartNewGame("Synthetic","Tester",GameMode.Normal);
    var g=new ReleasedGameState {Title="Synthetic",Quality=new(100,100,100,100),MarketFactor=2};s.ReleasedGames.Add(g);
    ReviewService.Generate(g,CoreCatalog(),new SequenceRandom(0,1,0,1,0,1,0,1));
    Check(g.ReviewScores!.Sum()==32 && g.ReviewPending && !g.HallOfFame && s.Company.HallOfFameCount==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames.Single();session.Restore(s);
    session.TickOffice(CoreCatalog());Check(s.SalesTicks==0 && s.CompanyActivityTicks==0);
    Check(AcknowledgeReviews(s,g) && g.HallOfFame && !g.ReviewPending && s.Company.HallOfFameCount==1);
    Check(!AcknowledgeReviews(s,g) && s.Company.HallOfFameCount==1);
    session.TickOffice(CoreCatalog());Check(s.SalesTicks==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.ReleasedGames[0].HallOfFame && s.Company.HallOfFameCount==1);
});
Test("review validation and legacy unknown values",()=>
{
    var s=State();var g=new ReleasedGameState {Title="Legacy"};s.ReleasedGames.Add(g);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames[0];
    Check(g.ReviewScores is null && s.Company.HallOfFameCount is null);
    g.ReviewPending=true;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));g.ReviewPending=false;
    g.ReviewScores=[8,8,8];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.ReviewScores=[8,8,8,11];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.ReviewScores=[8,8,8,8];g.ReviewPending=true;
    Check(AcknowledgeReviews(s,g) && s.Company.HallOfFameCount is null && g.HallOfFame);
    g.ReviewScores=[1,1,1,1];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.HallOfFame=false;Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].ReviewScores!.Sum()==4);
});
Test("sales milestone thresholds queue in order without bonuses or duplicate announcements",()=>
{
    var s=State();var g=new ReleasedGameState {Title="Synthetic",SalesMilestoneStage=0,UnitsSold=99999};s.ReleasedGames.Add(g);
    SalesMilestoneService.QueueReached(g);Check(g.PendingSalesMilestones.Count==0);
    g.UnitsSold=100000;SalesMilestoneService.QueueReached(g);Check(g.PendingSalesMilestones.SequenceEqual(new[]{1}));
    SalesMilestoneService.QueueReached(g);Check(g.PendingSalesMilestones.Count==1);
    g.UnitsSold=1000000;SalesMilestoneService.QueueReached(g);Check(g.PendingSalesMilestones.SequenceEqual(new[]{1,2,3,4}));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames[0];
    for (int stage=1;stage<=4;stage++) {Check(SalesMilestoneService.Acknowledge(s,g));Check(g.SalesMilestoneStage==stage);}
    Check(!SalesMilestoneService.Acknowledge(s,g) && s.Money==100);
    g.UnitsSold=-1;SalesMilestoneService.QueueReached(g);Check(g.SalesMilestoneStage==4 && g.PendingSalesMilestones.Count==0);
    g.SalesMilestoneStage=null;g.UnitsSold=1000000;SalesMilestoneService.QueueReached(g);Check(g.PendingSalesMilestones.Count==0);
});
Test("sales batch creates a saved modal milestone and freezes following office ticks",()=>
{
    var session=new GameSession();var s=session.StartNewGame("Synthetic","Tester",GameMode.Normal);
    var g=new ReleasedGameState {Title="Synthetic",SalesMilestoneStage=0,Quality=new(100,100,100,100),MarketFactor=1,
        Price=5800,Stock=1000,UnitsSold=99999};s.ReleasedGames.Add(g);
    for (int i=0;i<20;i++) session.TickOffice(CoreCatalog());
    Check(g.PendingSalesMilestones.SequenceEqual(new[]{1}));
    session.TickOffice(CoreCatalog());Check(s.SalesTicks==0);
    Check(AcknowledgeReviews(s,g)==false && SalesMilestoneService.Acknowledge(s,g));
    session.TickOffice(CoreCatalog());Check(s.SalesTicks==1);
    g.PendingSalesMilestones=[3];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.PendingSalesMilestones=[2,3,4,5];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("dismissal frees seated or walking desks and permits rehiring without lost progression",()=>
{
    foreach (bool walking in new[]{false,true})
    {
        var s=State(10000);var e=Worker();e.ContractFee=100;e.Experience=47;e.Stress=60;
        e.Office!.WorkTicks=16;e.Office.WorkTier=1;s.Employees.Add(e);
        var source=new RecruitmentSourceRecord {SourceIndex=0,GateValue=1,Employee=Candidate(0)};s.EmployeeSources.Add(source);
        var p=new DevelopmentProjectState {Title="Synthetic",RequiredPoints=100,Progress=20,Focus=new(25,25,25,25)};s.DevelopmentProjects.Add(p);
        if (walking) Check(OfficeAssignmentService.TryAssignDesk(s,e,7)==OfficeAssignmentResult.Success);
        Check(EmployeeDismissalService.TryDismiss(s,e)==DismissalResult.Success);
        Check(e.Office is null && e.Stress==0 && source.GateValue==0 && source.Employee.Experience==47 && s.Money==10000);
        Check(p.Progress==20 && s.Employees.Count==0);
        Check(EmployeeDismissalService.TryDismiss(s,e)==DismissalResult.NotEmployed);
        var rehire=new EmployeeState {Name=e.Name,SourceIndex=0,PortraitId=1,ContractFee=100,Experience=47};
        Check(HiringService.TryHire(s,rehire,new SequenceRandom(0))==HireResult.Success);
        var hired=s.Employees.Single();Check(hired.Office!.SlotIndex==0 && hired.Experience==47 && s.Money==9900 && source.GateValue==1);
        Check(OfficeAssignmentService.TryAssignDesk(s,hired,walking ? 7 : 6)==OfficeAssignmentResult.Success);
        Check(SaveStore.Deserialize(SaveStore.Serialize(s)).Employees.Single().Experience==47);
    }
});
Test("dismissal frees lounge reservations without touching another employee or project",()=>
{
    var s=State();var e=Worker();e.Office=OfficeStaffRules.CreateHire(s,new SequenceRandom(0));s.Employees.Add(e);
    var other=Candidate(1);other.Office=OfficeStaffRules.CreateHire(s,new SequenceRandom(1));s.Employees.Add(other);
    Check(EmployeeDismissalService.TryDismiss(s,e)==DismissalResult.Success);
    Check(s.Employees.Single()==other && other.Office.LoungeSeat==1);
    var replacement=OfficeStaffRules.CreateHire(s,new SequenceRandom(0));Check(replacement.LoungeSeat==0 && replacement.SlotIndex==0);
});
Test("dismissal corrupt or unknown identities fail before mutation",()=>
{
    var s=State();var unknown=new EmployeeState {Name="Unknown",Stress=12};s.Employees.Add(unknown);
    Check(EmployeeDismissalService.TryDismiss(s,unknown)==DismissalResult.IdentityUnknown && unknown.Stress==12);
    var e=Worker();s.Employees.Add(e);s.Employees.Add(Worker());
    Throws<InvalidDataException>(()=>EmployeeDismissalService.TryDismiss(s,e));Check(e.Office is not null && s.Employees.Count==3);
    s.Employees.RemoveAt(2);s.EmployeeSources.Add(Row(0,1));s.EmployeeSources.Add(Row(0,1));
    Throws<InvalidOperationException>(()=>EmployeeDismissalService.TryDismiss(s,e));Check(e.Office is not null && s.EmployeeSources.All(x=>x.GateValue==1));
});
Test("annual nonrenewal uses dismissal cleanup for active supported workspaces",()=>
{
    var s=State(1000);var retained=Worker();retained.ContractFee=100;s.Employees.Add(retained);
    var removed=new EmployeeState {Name="Synthetic",SourceIndex=1,PortraitId=1,ContractFee=200,Stress=70,Experience=15};
    removed.Office=OfficeStaffRules.CreateHire(s,new SequenceRandom(0));s.Employees.Add(removed);
    s.EmployeeSources.Add(Row(0,1));s.EmployeeSources.Add(Row(1,1));
    Check(ContractRenewalService.TryRenew(s,new HashSet<int>{0})==ContractRenewalResult.Success);
    Check(s.Money==900 && s.Employees.Single()==retained && removed.Office is null && s.EmployeeSources[1].GateValue==0);
    Check(retained.Office is not null && s.EmployeeSources[1].Employee.Experience==15);
});
GameState PayrollFixture()
{
    var s=State();s.MinimumSourceIndex=0;s.SourceUpperBound=3;
    s.EmployeeSources=[
        new() {SourceIndex=0,GateValue=0,Employee=new() {Name="Synthetic salary 0",SourceIndex=0,RawParameter40=700}},
        new() {SourceIndex=1,GateValue=1,Employee=new() {Name="Synthetic salary 1",SourceIndex=1,RawParameter40=100}},
        new() {SourceIndex=2,GateValue=1,Employee=new() {Name="Synthetic salary 2",SourceIndex=2,RawParameter40=300}}];
    return s;
}
Test("staff entry captures the original scratch count and retains the distinct displayed salary total",()=>
{
    var s=PayrollFixture();
    Check(s.OriginalPayrollScratchIndex is null && OriginalPayrollRules.MonthlyDebit(s) is null);
    Check(OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex==2);
    Check(OriginalPayrollRules.StaffSalaryTotal(s.EmployeeSources)==400);
    Check(OriginalPayrollRules.MonthlyDebit(s)==600 && s.Money==100);
    OriginalPayrollRules.InvalidateScratch(s);
    Check(s.OriginalPayrollScratchIndex is null && OriginalPayrollRules.MonthlyDebit(s) is null);
    Check(OriginalPayrollRules.CaptureStaffList(s));
    // A later gate change does not continuously recompute the shared scratch.
    s.EmployeeSources[2].GateValue=0;
    Check(s.OriginalPayrollScratchIndex==2 && OriginalPayrollRules.MonthlyDebit(s)==300);
    Check(OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex==1);
    Check(OriginalPayrollRules.MonthlyDebit(s)==100);
    s.EmployeeSources[1].GateValue=0;
    Check(OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex==0);
    Check(OriginalPayrollRules.StaffSalaryTotal(s.EmployeeSources)==0 && OriginalPayrollRules.MonthlyDebit(s)==0);
});
Test("partial or duplicate source tables cannot establish a staff scratch snapshot",()=>
{
    var s=PayrollFixture();s.OriginalPayrollScratchIndex=2;s.SourceUpperBound=null;
    Check(!OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex is null);
    s.SourceUpperBound=3;s.EmployeeSources.RemoveAt(0);s.OriginalPayrollScratchIndex=1;
    Check(!OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex is null);
    s=PayrollFixture();s.EmployeeSources[0]=Row(1);
    Check(!OriginalPayrollRules.CaptureStaffList(s));
    s=PayrollFixture();s.SourceUpperBound=0;Check(!OriginalPayrollRules.CaptureStaffList(s));
});
Test("salary unknowns and count beyond the salary table remain explicit",()=>
{
    var s=PayrollFixture();
    s.EmployeeSources[1]=new() {SourceIndex=1,GateValue=1,Employee=Candidate(1)};
    Check(OriginalPayrollRules.StaffSalaryTotal(s.EmployeeSources) is null);
    Check(OriginalPayrollRules.CaptureStaffList(s) && OriginalPayrollRules.MonthlyDebit(s)==600);
    s.EmployeeSources[0].GateValue=1;
    Check(OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex==3);
    Check(OriginalPayrollRules.MonthlyDebit(s) is null);
});
Test("salary projections retain signed values and detect overflowing totals",()=>
{
    var s=PayrollFixture();
    s.EmployeeSources[2]=new() {SourceIndex=2,GateValue=1,Employee=new() {Name="Synthetic signed salary",RawParameter40=-300}};
    Check(OriginalPayrollRules.CaptureStaffList(s));
    Check(OriginalPayrollRules.StaffSalaryTotal(s.EmployeeSources)==-200 && OriginalPayrollRules.MonthlyDebit(s)==-600);
    s.EmployeeSources[2]=new() {SourceIndex=2,GateValue=1,Employee=new() {Name="Synthetic large salary",RawParameter40=long.MaxValue}};
    Throws<OverflowException>(()=>OriginalPayrollRules.MonthlyDebit(s));
    Throws<OverflowException>(()=>OriginalPayrollRules.StaffSalaryTotal(s.EmployeeSources));
    Check(s.Money==100 && s.OriginalPayrollScratchIndex==2);
});
Test("save preserves stale and absent payroll scratch without serializing salary aliases",()=>
{
    var s=PayrollFixture();Check(OriginalPayrollRules.CaptureStaffList(s));
    s.EmployeeSources[2].GateValue=0;
    var json=SaveStore.Serialize(s);
    Check(!json.Contains("\"MonthlySalary\"") && json.Contains("\"RawParameter40\""));
    var restored=SaveStore.Deserialize(json);
    Check(restored.OriginalPayrollScratchIndex==2 && OriginalPayrollRules.MonthlyDebit(restored)==300);
    Check(restored.EmployeeSources[2].Employee.RawParameter40==300);
    s.OriginalPayrollScratchIndex=null;Check(SaveStore.Deserialize(SaveStore.Serialize(s)).OriginalPayrollScratchIndex is null);
    s.OriginalPayrollScratchIndex=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("payroll preserves stale scratch indexing instead of intended employee salaries",()=>
{
    var rows=new List<RecruitmentSourceRecord>
    {
        new() {SourceIndex=0,GateValue=0,Employee=new() {Name="Synthetic0",RawParameter40=700}},
        new() {SourceIndex=1,GateValue=1,Employee=new() {Name="Synthetic1",RawParameter40=100}},
        new() {SourceIndex=2,GateValue=1,Employee=new() {Name="Synthetic2",RawParameter40=300}}
    };
    Check(OriginalPayrollRules.MonthlyDebit(rows,0)==1400);
    Check(OriginalPayrollRules.MonthlyDebit(rows,1)==200);
    Check(OriginalPayrollRules.MonthlyDebit(rows,2)==600);
    Check(OriginalPayrollRules.MonthlyDebit(rows,null) is null && OriginalPayrollRules.MonthlyDebit(rows,99) is null);
    Throws<ArgumentOutOfRangeException>(()=>OriginalPayrollRules.MonthlyDebit(rows,-1));
    rows.ForEach(x=>x.GateValue=0);Check(OriginalPayrollRules.MonthlyDebit(rows,null)==0);
});
Test("recall bug brackets consume all draws and retain signed random ranges",()=>
{
    var r=new DoubleSequenceRandom(1.1/317,0,0);Check(RecallRules.Investigate(0,r) && r.Calls==3);
    Check(!RecallRules.Investigate(0,new DoubleSequenceRandom(0,.99,.99)));
    Check(RecallRules.Investigate(1,new DoubleSequenceRandom(0,1.1/25,.99)));
    Check(RecallRules.Investigate(10,new DoubleSequenceRandom(0,1.1/250,.99)));
    Check(!RecallRules.Investigate(10,new DoubleSequenceRandom(0,2.1/250,0)));
    Check(RecallRules.Investigate(11,new DoubleSequenceRandom(0,0,5.9/39)));
    Check(!RecallRules.Investigate(11,new DoubleSequenceRandom(0,0,6.1/39)));
    Check(RecallRules.Investigate(50,new DoubleSequenceRandom(0,0,.99)));
    Check(RecallRules.Investigate(70,new DoubleSequenceRandom(0,0,.99)));
    Throws<ArgumentOutOfRangeException>(()=>RecallRules.Investigate(-1,new Random()));
});
Test("recall eligibility countdown and refunds match original branches",()=>
{
    Check(RecallRules.CheckEligible(15,0) && !RecallRules.CheckEligible(16,0) && !RecallRules.CheckEligible(0,1));
    int countdown=RecallRules.InvestigationWeeks;for (int i=0;i<5;i++) countdown=RecallRules.NextCountdown(countdown);
    Check(countdown==0 && RecallRules.NextCountdown(0)==0);
    Check(RecallRules.Refund(120,30,5800,0)==870000);
    Check(RecallRules.Refund(120,30,5800,1)==150000 && RecallRules.Refund(120,30,5800,2)==150000);
    Check(RecallRules.Refund(120,30,5800,3)==0);
    Check(RecallRules.Refund(-20,30,5800,0)==58000);
    Throws<OverflowException>(()=>RecallRules.Refund(long.MaxValue,1,5800,0));
});
Test("late-demand recall check runs once even when no investigation begins",()=>
{
    var g=new ReleasedGameState {Title="Checked",Demand=16,Recall=new()};var r=new DoubleSequenceRandom(0,0,0);
    Check(!RecallService.CheckAfterSales(g,r) && r.Calls==0 && g.Recall.OriginalState==0);
    g.Demand=15;Check(!RecallService.CheckAfterSales(g,r) && r.Calls==3 && g.Recall.OriginalState==1 && g.Recall.WeeksRemaining==0);
    g.Bugs=50;Check(!RecallService.CheckAfterSales(g,r) && r.Calls==3);
    var s=PlanningState();s.ReleasedGames.Add(g);RecallService.AdvanceWeek(s,new SequenceRandom());
    Check(g.Recall.OriginalState==1 && g.Recall.PendingEvent is null && g.Recall.WeeksRemaining==0);
});
Test("all recall decisions preserve original refunds stock disposal and reprint eligibility",()=>
{
    for (int outcome=0;outcome<4;outcome++)
    {
        var s=PlanningState(900000);var g=new ReleasedGameState {Title="Outcome",UnitsSold=120,Stock=30,Price=5800,
            Recall=new() {OriginalState=1,WeeksRemaining=1}};s.ReleasedGames.Add(g);
        RecallService.AdvanceWeek(s,new SequenceRandom(outcome));
        long refund=outcome==0 ? 870000 : outcome==3 ? 0 : 150000;
        Check(s.Money==900000-refund && g.Recall.RefundAmount==refund && g.Recall.OutcomeRoll==outcome);
        Check(g.Recall.WeeksRemaining==0 && g.Recall.PendingEvent==RecallEvent.Result && g.Recall.OriginalState==(outcome==3 ? 3 : 2));
        Check(g.Stock==(outcome==3 ? 30 : 0));
        Check(ReleaseService.CheckProduction(s,g)==(outcome==3 ? ReleaseResult.Success : ReleaseResult.Recalled));
        Check(RecallService.Acknowledge(s,g) && !RecallService.Acknowledge(s,g));
        RecallService.AdvanceWeek(s,new SequenceRandom());Check(s.Money==900000-refund);
        Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].Recall!.OutcomeRoll==outcome);
    }
});
Test("investigation modals pause budget weeks and a saved result never charges twice",()=>
{
    var s=PlanningState(1000000);s.CompanyActivityTicks=199;
    var g=new ReleasedGameState {Title="Investigated",UnitsSold=120,Stock=30,Price=5800,Bugs=50,Demand=15,Recall=new()};s.ReleasedGames.Add(g);
    Check(RecallService.CheckAfterSales(g,new DoubleSequenceRandom(.99,.99,.99)) && g.Recall!.WeeksRemaining==5);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames[0];
    var session=new GameSession(new SequenceRandom(0));session.Restore(s);
    for (int i=0;i<20;i++) session.TickOffice();
    Check(s.CompanyActivityTicks==199 && g.Recall!.WeeksRemaining==5 && s.Money==1000000);
    Check(RecallService.Acknowledge(s,g));session.TickOffice();Check(s.CompanyActivityTicks==0 && g.Recall!.WeeksRemaining==4);
    for (int i=0;i<799;i++) session.TickOffice();
    Check(g.Recall!.WeeksRemaining==1 && g.Recall.PendingEvent is null && s.Money==1000000);
    session.TickOffice();Check(g.Recall.WeeksRemaining==0 && g.Recall.PendingEvent==RecallEvent.Result && s.Money==130000);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames[0];session.Restore(s);
    for (int i=0;i<20;i++) session.TickOffice();Check(s.Money==130000 && s.CompanyActivityTicks==0);
    Check(RecallService.Acknowledge(s,g));session.TickOffice();Check(s.Money==130000 && s.CompanyActivityTicks==1);
});
Test("recall refunds retain negative funds and signed original quantity behavior",()=>
{
    var s=PlanningState(1000);var g=new ReleasedGameState {Title="Debt",UnitsSold=120,Stock=30,Price=5800,
        Recall=new() {OriginalState=1,WeeksRemaining=1}};s.ReleasedGames.Add(g);
    RecallService.AdvanceWeek(s,new SequenceRandom(0));Check(s.Money==-869000 && g.Stock==0);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).Money==-869000);
    s=PlanningState(1000);g=new ReleasedGameState {Title="Signed",UnitsSold=-20,Stock=10,Price=5800,
        Recall=new() {OriginalState=1,WeeksRemaining=1}};s.ReleasedGames.Add(g);
    RecallService.AdvanceWeek(s,new SequenceRandom(0));Check(s.Money==59000 && g.Recall.RefundAmount==-58000 && g.Stock==0);
});
Test("unknown recall data and corrupt event saves are never silently reconstructed",()=>
{
    var s=PlanningState(1000);var g=new ReleasedGameState {Title="Unknown",Stock=100,Bugs=50,Demand=0};s.ReleasedGames.Add(g);
    Check(!RecallService.CheckAfterSales(g,new DoubleSequenceRandom()) && ReleaseService.CheckProduction(s,g)==ReleaseResult.StateUnknown);
    RecallService.AdvanceWeek(s,new SequenceRandom());Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].Recall is null);
    g.Recall=new() {WeeksRemaining=1};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.Recall=new() {OriginalState=1,WeeksRemaining=4,PendingEvent=RecallEvent.Investigation};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.Recall=new() {OriginalState=2,OutcomeRoll=0,RefundAmount=100};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.Stock=0;g.Recall=new() {OriginalState=3,OutcomeRoll=2,RefundAmount=0};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("recall resolution rejects overflow before changing funds stock or countdown",()=>
{
    var s=PlanningState(1000);var g=new ReleasedGameState {Title="Overflow",UnitsSold=long.MaxValue,Stock=1,Price=5800,
        Recall=new() {OriginalState=1,WeeksRemaining=1}};s.ReleasedGames.Add(g);
    Throws<OverflowException>(()=>RecallService.AdvanceWeek(s,new SequenceRandom(0)));
    Check(s.Money==1000 && g.Stock==1 && g.Recall.OriginalState==1 && g.Recall.WeeksRemaining==1 && g.Recall.PendingEvent is null);
});
Test("sales decay starts and saves a guaranteed high-bug investigation",()=>
{
    var s=PlanningState(1000);s.SalesFee1=s.SalesFee2=1500;s.SalesTicks=19;
    var g=new ReleasedGameState {Title="Sales Investigation",Stock=0,Demand=16,Bugs=50,Recall=new()};s.ReleasedGames.Add(g);
    var r=new RecallSalesRandom();SalesService.Tick(s,CoreCatalog(),r);
    Check(g.Demand==14 && g.Recall.OriginalState==1 && g.Recall.WeeksRemaining==5 && g.Recall.PendingEvent==RecallEvent.Investigation);
    Check(r.Bounds.SequenceEqual(new[] {50,7}) && r.DoubleCalls==3 && s.Money==1000);
    SalesService.Tick(s,CoreCatalog(),r);Check(s.SalesTicks==0 && r.DoubleCalls==3);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].Recall!.PendingEvent==RecallEvent.Investigation);
});
Test("company activities charge shared cost, increase original fields and cap research/fame",()=>
{
    var session=new GameSession();var s=session.StartNewGame("Synthetic","Tester",GameMode.Normal);
    Check(s.Company.ActivityCost==5000000 && s.CompanyActivityPerformed==false && s.CompanyActivityTicks==0);
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom(2))==CompanyActivityResult.Success);
    Check(s.Company.Fame==15 && s.Money==445000000 && s.CompanyActivityPerformed==true);
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.GenreResearch,new SequenceRandom())==CompanyActivityResult.AlreadyActed);
    s.CompanyActivityPerformed=false;s.Company.OriginalMember90=99;
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.GenreResearch,new SequenceRandom(2))==CompanyActivityResult.Success);
    Check(s.Company.OriginalMember90==100 && s.Money==440000000);
    s.CompanyActivityPerformed=false;s.Company.Fame=99;
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom(2))==CompanyActivityResult.Success && s.Company.Fame==100);
    s.CompanyActivityPerformed=false;
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom(0))==CompanyActivityResult.Success && s.Company.Fame==100 && s.Money==430000000);
});
Test("production expansion preserves rare and ordinary branches without a cap",()=>
{
    foreach (var (roll,growth,expected) in new[]{(1,0,17),(1,7,24),(0,0,4),(11,3,7)})
    {
        var s=PlanningState(5000000);s.Company.ActivityCost=5000000;s.CompanyActivityPerformed=false;s.CompanyActivityTicks=0;s.Company.ProductionCapacity=99;
        var random=new SequenceRandom(roll,growth);
        Check(CompanyActivityService.TryPerform(s,CompanyActivity.ProductionExpansion,random)==CompanyActivityResult.Success);
        Check(s.Money==0 && s.Company.ProductionCapacity==99+expected && random.Calls==2);
    }
});
Test("company activity failures and legacy clocks remain atomic and unknown",()=>
{
    var s=PlanningState(4999999);s.Company.ActivityCost=5000000;s.CompanyActivityPerformed=false;s.CompanyActivityTicks=0;
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom())==CompanyActivityResult.InsufficientFunds && s.Company.Fame==14 && s.CompanyActivityPerformed==false);
    s.Money=5000000;s.Company.Fame=null;
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom())==CompanyActivityResult.StateUnknown && s.Money==5000000);
    s.CompanyActivityPerformed=null;s.CompanyActivityTicks=null;s.Company.Fame=14;
    CompanyActivityService.Tick(s,new SequenceRandom());Check(s.CompanyActivityTicks is null);
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom())==CompanyActivityResult.StateUnknown);
    s.CompanyActivityTicks=200;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    Throws<ArgumentOutOfRangeException>(()=>CompanyActivityService.TryPerform(s,(CompanyActivity)99,new SequenceRandom()));
});
Test("business budget resets at two hundred ticks and automatic expansion respects explicit secretary thresholds",()=>
{
    foreach (var (mode,funds,expands) in new[]{(GameMode.Normal,299999999L,false),(GameMode.Normal,300000000L,true),
        (GameMode.Beginner,99999999L,false),(GameMode.Beginner,100000000L,true)})
    {
        var s=new GameSession().StartNewGame("Synthetic","Tester",mode);s.Money=funds;s.CompanyActivityTicks=158;s.SecretaryPolicies=new() {Production=mode==GameMode.Beginner ? ProductionPolicy.Funds100Million : ProductionPolicy.Funds300Million};
        CompanyActivityService.Tick(s,new SequenceRandom());Check(s.Company.ProductionCapacity==50 && s.CompanyActivityTicks==159);
        CompanyActivityService.Tick(s,expands ? new SequenceRandom(0,0) : new SequenceRandom());
        Check(s.CompanyActivityTicks==160 && s.Company.ProductionCapacity==(expands ? 54 : 50) && s.Money==(expands ? funds-5000000 : funds));
        s.CompanyActivityPerformed=true;s.CompanyActivityTicks=199;
        s=SaveStore.Deserialize(SaveStore.Serialize(s));CompanyActivityService.Tick(s,new SequenceRandom());
        Check(s.CompanyActivityTicks==0 && s.CompanyActivityPerformed==false);
    }
});
Test("genre research unlocks a crossed threshold and public relations feeds next project recognition",()=>
{
    var s=new GameSession().StartNewGame("Synthetic","Tester",GameMode.Normal);var c=CoreCatalog();
    c.Genres[0].RawMembers["125C"]=25;
    Check(!DevelopmentSelectionRules.IsGenreEligible(c.Genres[0],s.Company.OriginalMember90!.Value));
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.GenreResearch,new SequenceRandom(1))==CompanyActivityResult.Success);
    Check(DevelopmentSelectionRules.IsGenreEligible(c.Genres[0],s.Company.OriginalMember90!.Value));
    s.CompanyActivityPerformed=false;
    Check(CompanyActivityService.TryPerform(s,CompanyActivity.PublicRelations,new SequenceRandom(2))==CompanyActivityResult.Success);
    var p=new DevelopmentProjectState {RequiredPoints=100,Progress=50};
    s.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,s.OriginalTypeRosterMap)==DevelopmentSelectionResult.Success && p.Recognition==5);
});
Test("production cost uses floored data-size bands plus packaging",()=>
{
    var h=CoreCatalog().Hardware[0];
    Check(ReleaseService.UnitProductionCost(h,16.99)==500);
    Check(ReleaseService.UnitProductionCost(h,17)==900);
    Check(ReleaseService.UnitProductionCost(h,32.99)==900);
    Check(ReleaseService.UnitProductionCost(h,33)==1400);
    Check(ReleaseService.UnitProductionCost(h,49)==2000);
    h.ProductionBands.Clear();Throws<InvalidDataException>(()=>ReleaseService.UnitProductionCost(h,0));
});
Test("release charges production, snapshots project and frees room exactly once",()=>
{
    var s=PlanningState(100000);var c=CoreCatalog();
    var p=new DevelopmentProjectState {Title="Synthetic Game",RequiredPoints=100,Progress=100,OriginalStage=3,
        GameTypeSourceIndex=0,GenreSourceIndex=0,Quality=new(10,20,30,40),Bugs=70,Recognition=4,DataSize=16.9};
    s.DevelopmentProjects.Add(p);var e=Worker();e.Office!.WorkTicks=12;s.Employees.Add(e);
    Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,100,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    var g=s.ReleasedGames.Single();
    Check(s.Money==50000 && s.DevelopmentProjects.Count==0 && g.Stock==100 && g.Demand==100 && g.MarketFactor==.9);
    Check(g.Quality==p.Quality && g.Bugs==70 && g.ProductionSpending==50000 && e.Office.WorkTicks==0 && e.Office.WorkEngaged);
    p.Quality=new(999,0,0,0);Check(g.Quality.Fun==10);
    Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,100,new SequenceRandom())==ReleaseResult.NotReady && s.Money==50000);
});
Test("release and reprint failure guards are atomic",()=>
{
    var s=PlanningState(49999);var c=CoreCatalog();var p=new DevelopmentProjectState {Title="Synthetic",RequiredPoints=1,Progress=1,
        OriginalStage=3,GameTypeSourceIndex=0,GenreSourceIndex=0};s.DevelopmentProjects.Add(p);
    Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,100,new SequenceRandom())==ReleaseResult.InsufficientFunds);
    Check(s.DevelopmentProjects.Count==1 && s.ReleasedGames.Count==0 && s.Money==49999);
    s.Money=100000;Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,100,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    var g=s.ReleasedGames.Single();
    Check(s.ProductionPerformedThisWeek==true);
    s.ProductionPerformedThisWeek=false; // Authored next-budget-week fixture.
    Check(ReleaseService.TryProduce(s,g,c,101)==ReleaseResult.InsufficientFunds && g.Stock==100 && g.ProductionSpending==50000);
    Check(ReleaseService.TryProduce(s,g,c,100)==ReleaseResult.Success && s.Money==0 && g.Stock==200 && g.ProductionSpending==100000);
    var other=new DevelopmentProjectState {RequiredPoints=1,Progress=1,OriginalStage=3,GameTypeSourceIndex=0,GenreSourceIndex=0};s.DevelopmentProjects.Add(other);
    Check(ReleaseService.TryRelease(s,other,c,"Synthetic",5800,100,new SequenceRandom())==ReleaseResult.DuplicateTitle);
});
Test("weekly sales history records signed units and preserves missing legacy data",()=>
{
    var s=PlanningState(100000);s.SalesTicks=19;s.SalesFee1=s.SalesFee2=1500;
    var g=new ReleasedGameState {Title="History",Stock=10000,Price=5800,MarketFactor=1,Quality=new(100,100,100,100),
        WeeklySalesHistory=SalesHistoryRules.NewHistory()};s.ReleasedGames.Add(g);
    SalesService.Tick(s,CoreCatalog(),new SequenceRandom(0,0));
    Check(g.UnitsSold==3627 && g.WeeklySalesHistory[0]==3627 && g.WeeklySalesHistory.Skip(1).All(x=>x==0));
    s.SalesTicks=19;g.Quality=new();g.Bugs=100;g.WeeklySalesHistory[0]=0;g.UnitsSold=0;g.Stock=10000;
    SalesService.Tick(s,CoreCatalog(),new SequenceRandom(0,0));
    Check(g.LastBatchSold==-844 && g.WeeklySalesHistory[0]==-844 && g.Stock==10844);
    g.WeeklySalesHistory=null;s.SalesTicks=19;g.Quality=new(100,100,100,100);g.Bugs=0;
    SalesService.Tick(s,CoreCatalog(),new SequenceRandom(0,0));
    SalesHistoryRules.AdvanceWeek(s);Check(g.WeeklySalesHistory is null && g.LastBatchSold==3627);
});
Test("week-boundary sales enter the new history cell after shifting and survive reload",()=>
{
    var s=PlanningState(100000);s.SalesTicks=19;s.SalesFee1=s.SalesFee2=1500;s.CompanyActivityTicks=199;
    var g=new ReleasedGameState {Title="Boundary",Stock=10000,Price=5800,MarketFactor=1,Quality=new(100,100,100,100),
        WeeklySalesHistory=SalesHistoryRules.NewHistory()};s.ReleasedGames.Add(g);
    g.WeeklySalesHistory[0]=1234;g.WeeklySalesHistory[1]=567;g.WeeklySalesHistory[12]=99;
    var restored=SaveStore.Deserialize(SaveStore.Serialize(s));var session=new GameSession(new SequenceRandom(0,0));session.Restore(restored);
    session.TickOffice(CoreCatalog());var history=restored.ReleasedGames[0].WeeklySalesHistory!;
    Check(history.Count==13 && history[0]==3627 && history[1]==1234 && history[2]==567 && history[12]==0);
    Check(restored.CompanyActivityTicks==0 && g.WeeklySalesHistory[0]==1234);
    Check(SaveStore.Deserialize(SaveStore.Serialize(restored)).ReleasedGames[0].WeeklySalesHistory!.SequenceEqual(history));
});
Test("sales chart uses original thresholds and omits the oldest cell from scale selection",()=>
{
    var history=SalesHistoryRules.NewHistory();Check(SalesHistoryRules.ChartCeiling(history)==40000);
    history[0]=29999;Check(SalesHistoryRules.ChartCeiling(history)==40000);
    history[0]=30000;Check(SalesHistoryRules.ChartCeiling(history)==80000);
    history[0]=99999;Check(SalesHistoryRules.ChartCeiling(history)==80000);
    history[0]=100000;Check(SalesHistoryRules.ChartCeiling(history)==200000);
    history[0]=long.MaxValue;Check(SalesHistoryRules.ChartCeiling(history)==200000);
    history[0]=0;history[12]=long.MaxValue;Check(SalesHistoryRules.ChartCeiling(history)==40000);
    history[11]=30000;Check(SalesHistoryRules.ChartCeiling(history)==80000);
});
Test("history save validation rejects wrong shapes without filling unknown records",()=>
{
    var s=PlanningState();var g=new ReleasedGameState {Title="History",WeeklySalesHistory=[1]};s.ReleasedGames.Add(g);
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.WeeklySalesHistory=SalesHistoryRules.NewHistory();g.WeeklySalesHistory[0]=-7;
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].WeeklySalesHistory![0]==-7);
    g.WeeklySalesHistory=null;Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].WeeklySalesHistory is null);
    g.WeeklySalesHistory=[1];s.SalesTicks=19;s.SalesFee1=s.SalesFee2=1500;
    Throws<InvalidDataException>(()=>SalesService.Tick(s,CoreCatalog(),new SequenceRandom()));Check(s.SalesTicks==19);
});
Test("production budget is shared across products and resets at exactly two hundred ticks",()=>
{
    var s=PlanningState(200000);var c=CoreCatalog();
    var first=new ReleasedGameState {Title="First",Stock=100,ProductionSpending=50000,Recall=new()};
    var second=new ReleasedGameState {ProductIndex=1,Title="Second",Stock=100,Recall=new()};
    s.ReleasedGames.AddRange([first,second]);
    Check(ReleaseService.TryProduce(s,first,c,100)==ReleaseResult.Success);
    Check(s.Money==150000 && first.Stock==200 && s.ProductionPerformedThisWeek==true);
    Check(ReleaseService.TryProduce(s,second,c,100)==ReleaseResult.AlreadyProducedThisWeek && second.Stock==100 && s.Money==150000);
    for (int i=0;i<199;i++) CompanyActivityService.Tick(s,new SequenceRandom());
    Check(s.CompanyActivityTicks==199 && ReleaseService.CheckProduction(s)==ReleaseResult.AlreadyProducedThisWeek);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));second=s.ReleasedGames[1];
    Check(s.ProductionPerformedThisWeek==true && s.CompanyActivityTicks==199);
    CompanyActivityService.Tick(s,new SequenceRandom());
    Check(s.CompanyActivityTicks==0 && s.ProductionPerformedThisWeek==false);
    Check(ReleaseService.TryProduce(s,second,c,100)==ReleaseResult.Success && second.Stock==200 && s.Money==100000);
});
Test("first shipments set the production flag without using the reprint guard",()=>
{
    var s=PlanningState(100000);s.ProductionPerformedThisWeek=true;var c=CoreCatalog();
    var p=new DevelopmentProjectState {Title="New Shipment",RequiredPoints=1,Progress=1,OriginalStage=3,
        GameTypeSourceIndex=0,GenreSourceIndex=0};s.DevelopmentProjects.Add(p);
    Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,100,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    Check(s.ProductionPerformedThisWeek==true && s.Money==50000);
    Check(ReleaseService.TryProduce(s,s.ReleasedGames[0],c,100)==ReleaseResult.AlreadyProducedThisWeek && s.Money==50000);
});
Test("production unknowns remain unknown and checkpoint modals pause the reset",()=>
{
    var s=PlanningState(100000);var g=new ReleasedGameState {Title="Synthetic",Stock=100,Recall=new()};s.ReleasedGames.Add(g);
    s.ProductionPerformedThisWeek=null;
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames[0];
    Check(s.ProductionPerformedThisWeek is null && ReleaseService.TryProduce(s,g,CoreCatalog(),100)==ReleaseResult.StateUnknown);
    Check(s.Money==100000 && g.Stock==100);
    s.ProductionPerformedThisWeek=true;s.CompanyActivityTicks=199;
    var p=new DevelopmentProjectState {RequiredPoints=100,Progress=50,PendingDecision=DevelopmentMilestone.SelectType};s.DevelopmentProjects.Add(p);
    var session=new GameSession(new SequenceRandom());session.Restore(s);session.TickOffice();
    Check(s.CompanyActivityTicks==199 && s.ProductionPerformedThisWeek==true);
    p.PendingDecision=null;session.TickOffice();
    Check(s.CompanyActivityTicks==0 && s.ProductionPerformedThisWeek==false);
    Check(new GameSession().StartNewGame("New","Tester",GameMode.Normal).ProductionPerformedThisWeek==false);
});
Test("market-factor branches preserve first-two penalty and unreachable graphics tiers",()=>
{
    Check(ReleaseService.MarketFactor(0,new(1000,1000,0,1000),new SequenceRandom(0))==2.4);
    Check(ReleaseService.MarketFactor(1,new(),new SequenceRandom(3))==.4);
    Check(Math.Abs(ReleaseService.MarketFactor(2,new(201,201,201,0),new SequenceRandom(6,0,0,1))-5.1)<1e-12);
    Check(ReleaseService.MarketFactor(2,new(0,201,0,0),new SequenceRandom(3,1))==0);
});
Test("sales formula, audience, recognition, jitter and all price boundaries",()=>
{
    var c=CoreCatalog();c.Audiences[0].RawMembers["14A8"]=1;
    var g=new ReleasedGameState {Quality=new(100,100,100,100),Bugs=10,Recognition=40,MarketFactor=.9,Price=5800};
    // floor(420/2 * 10000/800)=2625; (2625+7)*1.5*2*.9*1*(.7+3/10).
    Check(SalesService.PotentialSales(g,c,7)==7106);
    c.Audiences[0].RawMembers["14A8"]=7;Check(SalesService.PotentialSales(g,c,7)==3553);
    long[] prices=[2999,3000,3999,4000,4999,5000,5999,6000,6999,7000,7999,8000,8999,9000,9999,10000,19999,20000];
    double[] factors=[2,1.5,1.5,1.2,1.2,1,1,.7,.7,.5,.5,.2,.2,.1,.1,.05,.05,0];
    for (int i=0;i<prices.Length;i++) Check(SalesService.PriceMultiplier(prices[i])==factors[i]);
    g.Demand=0;Check(SalesService.PotentialSales(g,c,19)==0);
});
Test("sales batches consume stock, net fees and demand even while out of stock",()=>
{
    var c=CoreCatalog();var s=PlanningState(1000);s.SalesFee1=s.SalesFee2=1500;
    var g=new ReleasedGameState {Title="Synthetic",Quality=new(100,100,100,100),MarketFactor=1,Price=5800,Stock=100};s.ReleasedGames.Add(g);
    for (int i=0;i<19;i++) SalesService.Tick(s,c,new SequenceRandom());
    Check(s.Money==1000 && g.Stock==100 && s.SalesTicks==19);
    SalesService.Tick(s,c,new SequenceRandom(7,1,1));
    Check(g.Stock==0 && g.UnitsSold==100 && s.Money==281000 && g.NetSalesReceipts==280000 && g.Demand==98 && g.LastBatchSold==100);
    for (int i=0;i<19;i++) SalesService.Tick(s,c,new SequenceRandom());
    SalesService.Tick(s,c,new SequenceRandom(1,0));
    Check(g.Stock==0 && g.UnitsSold==100 && g.Demand==100 && g.LastBatchSold==0);
    Check(SalesService.NextDemand(1,1,1)==0 && SalesService.NextDemand(50,0)==50);
});
Test("sales retain original negative-result defect rather than inventing a clamp",()=>
{
    var c=CoreCatalog();var g=new ReleasedGameState {Quality=new(),Bugs=100,MarketFactor=1,Price=5800,Stock=100};
    Check(SalesService.PotentialSales(g,c,0)<0);
});
Test("work cadence and ordinary contribution are persisted mid-task",()=>
{
    var c=CoreCatalog();var s=PlanningState(100000);var e=Worker();s.Employees.Add(e);
    var p=new DevelopmentProjectState {Title="Synthetic",RequiredPoints=1000,Focus=new(100,0,0,0)};s.DevelopmentProjects.Add(p);
    e.Office!.WorkIdleTicks=35;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.WorkTicks==2 && e.Office.WorkTier==1 && e.Experience==3.7);
    var restored=SaveStore.Deserialize(SaveStore.Serialize(s));s=restored;e=s.Employees.Single();p=s.DevelopmentProjects.Single();
    for (int i=0;i<14;i++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Progress==0 && e.Office!.WorkTicks==16);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Progress==2 && p.Quality.Fun==2 && p.DataSize>0 && e.Office!.WorkTicks==0);
});
Test("burst completes after 120 accumulation ticks starting at animation tick 32",()=>
{
    var c=CoreCatalog();var s=PlanningState();var e=Worker();s.Employees.Add(e);
    var p=new DevelopmentProjectState {Title="Synthetic",RequiredPoints=1000,Focus=new(100,0,0,0)};s.DevelopmentProjects.Add(p);
    e.Office!.WorkTier=3;e.Office.WorkTicks=32;e.Office.WorkCategory=DevelopmentWorkCategory.Fun;
    for (int i=0;i<119;i++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Progress==0 && e.Office.BurstTicks==119);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office.WorkTicks==0 && e.Office.BurstTicks==120);
});
Test("development checkpoints award once, pause all clocks and resume after selection",()=>
{
    var c=CoreCatalog();var s=PlanningState(10000);var e=Worker();s.Employees.Add(e);
    s.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);
    s.OriginalAccuracyCells=new(){{99,0}};s.SalesFee1=s.SalesFee2=1500;
    var p=new DevelopmentProjectState {Title="Synthetic",RequiredPoints=100,Progress=50,Focus=new(100,0,0,0)};s.DevelopmentProjects.Add(p);
    var session=new GameSession(new WorkRandom());session.Restore(s);session.TickOffice(c);
    Check(p.PendingDecision==DevelopmentMilestone.SelectType && e.Experience==10);
    int ticks=s.SalesTicks;double stress=e.Stress!.Value;
    for (int i=0;i<100;i++) session.TickOffice(c);
    Check(e.Experience==10 && s.SalesTicks==ticks && e.Stress==stress);
    // Source 0 as chosen type, points indexed through roster[0]=1.
    s.OriginalTypeRosterMap[0]=1;
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,s.OriginalTypeRosterMap)==DevelopmentSelectionResult.Success && p.PendingDecision is null);
    p.Progress=p.RequiredPoints*.81;session.TickOffice(c);Check(p.PendingDecision==DevelopmentMilestone.SelectGenre && e.Experience==30);
    Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success);
    p.Progress=p.RequiredPoints;session.TickOffice(c);
    Check(p.OriginalStage==3 && p.PendingDecision==DevelopmentMilestone.StartDebugging && e.Experience==70);
    var saved=SaveStore.Deserialize(SaveStore.Serialize(s));Check(saved.DevelopmentProjects.Single().PendingDecision==DevelopmentMilestone.StartDebugging);
});
Test("development release sales second production and another project form a repeatable loop",()=>
{
    var c=CoreCatalog();var s=PlanningState(10000000);s.Company.ProductionCapacity=50;s.Company.OriginalMember90=23;
    s.SalesFee1=s.SalesFee2=1500;s.OriginalAccuracyCells=new(){{99,0}};s.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);
    var e=Worker();s.Employees.Add(e);var session=new GameSession(new Random(1234));session.Restore(s);
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Synthetic Loop",new(100,0,0,0))==DevelopmentPlanningResult.Success);
    var p=s.DevelopmentProjects.Single();int tick=0;
    while (p.OriginalStage<3 && tick++<30000)
    {
        session.TickOffice(c);
        if (p.PendingDecision==DevelopmentMilestone.SelectType)
        {
            var roster=DevelopmentSimulation.PrepareTypeCandidates(s,p,c,new Random(2));
            Check(DevelopmentSelectionRules.TrySelectType(s,p,c,roster[0],s.OriginalTypeRosterMap)==DevelopmentSelectionResult.Success);
        }
        if (p.PendingDecision==DevelopmentMilestone.SelectGenre)
            Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success);
        // Lounge management is a player choice; keep this synthetic worker fresh
        // without changing any production gameplay rule.
        e.Stress=0;
    }
    Check(tick<30000 && p.OriginalStage==3 && p.Progress==p.RequiredPoints);
    Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,5000,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    var g=s.ReleasedGames.Single();Check(AcknowledgeReviews(s,g));long before=s.Money!.Value;
    for (int i=0;i<20;i++) session.TickOffice(c);
    Check(g.UnitsSold>0 && g.Stock<5000 && s.Money>before);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));g=s.ReleasedGames.Single();session.Restore(s);
    while (s.ProductionPerformedThisWeek==true) session.TickOffice(c);
    long stock=g.Stock;Check(ReleaseService.TryProduce(s,g,c,1000)==ReleaseResult.Success && g.Stock==stock+1000);
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Synthetic Second",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    Check(s.DevelopmentProjects.Count==1 && s.ReleasedGames.Count==1);
});
Test("core-loop save rejects corrupt stocks, checkpoints, cells and counters",()=>
{
    var s=PlanningState();s.SalesTicks=20;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.SalesTicks=0;
    s.OriginalTypeRosterMap=new(){{0,0}};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.OriginalTypeRosterMap=null;
    var g=new ReleasedGameState {Title="Synthetic",Stock=-1};s.ReleasedGames.Add(g);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.Stock=1;g.MarketFactor=double.NaN;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.ReleasedGames.Clear();
    var e=Worker();s.Employees.Add(e);e.Office!.BurstCount=51;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});

(GameState State,OriginalCatalog Catalog) MangaFixture(long money=1000000)
{
    var s=PlanningState(money);s.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);var c=CoreCatalog();c.Manga=[new() {SourceIndex=0,Title="Synthetic comic",Author="Synthetic author",Publisher="Synthetic publisher",Popularity=10,RightsFee=100}];
    return (s,c);
}
Test("manga rights meeting and quote are read-only; acceptance pays rights exactly once",()=>
{
    var(s,c)=MangaFixture();string before=SaveStore.Serialize(s);
    Check(DevelopmentProjectService.CheckMangaRights(s,c,0)==DevelopmentPlanningResult.Success);
    Check(DevelopmentProjectService.CheckMangaPlan(s,c,0,0,0,0,out var q)==DevelopmentPlanningResult.Success && q==new DevelopmentQuote(130,400));
    Check(SaveStore.Serialize(s)==before);
    Check(DevelopmentProjectService.TryAcceptMangaPlan(s,c,0,0,0,0,"Adaptation",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    var p=s.DevelopmentProjects.Single();Check(p.OriginalMode==2 && p.MangaSourceIndex==0 && p.PredecessorProductIndex is null && p.Quality==new DevelopmentQuality() && p.Recognition==0 && p.DataSize==0);
    Check(s.Money==999870 && p.TypeCandidates is null && p.RequiredPoints==400);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.DevelopmentProjects.Single().MangaSourceIndex==0);
    Check(DevelopmentProjectService.TryAcceptMangaPlan(s,c,1,0,0,0,"Second adaptation",p.Focus!)==DevelopmentPlanningResult.Success);
    Check(s.Money==999740); // The original selector does not consume the title.
});
Test("manga quote includes rights, computer and licensing gates and insufficient funds",()=>
{
    var(s,c)=MangaFixture(100);
    Check(DevelopmentProjectService.CheckMangaRights(s,c,0)==DevelopmentPlanningResult.Success);
    Check(DevelopmentProjectService.CheckMangaPlan(s,c,0,0,0,0,out _)==DevelopmentPlanningResult.InsufficientFunds);
    string before=SaveStore.Serialize(s);
    Check(DevelopmentProjectService.TryAcceptMangaPlan(s,c,0,0,0,0,"Adaptation",new(25,25,25,25))==DevelopmentPlanningResult.InsufficientFunds && SaveStore.Serialize(s)==before);
    Check(DevelopmentProjectService.CheckMangaRights(s,c,99)==DevelopmentPlanningResult.MangaUnavailable);
    s.HardwareLicenses!.Clear();Check(DevelopmentProjectService.CheckMangaPlan(s,c,0,0,0,0,out _)==DevelopmentPlanningResult.LicenseRequired);
    s.HardwareLicenses.Add(0);c.DevelopmentComputers[0].BitWidth=7;Check(DevelopmentProjectService.CheckMangaPlan(s,c,0,0,0,0,out _)==DevelopmentPlanningResult.ComputerTooSmall);
});
Test("manga uses ordinary type and genre choices and can become a sequel parent",()=>
{
    var(s,c)=MangaFixture();DevelopmentProjectService.TryAcceptMangaPlan(s,c,0,0,0,0,"Adaptation",new(25,25,25,25));var p=s.DevelopmentProjects[0];
    p.Progress=p.RequiredPoints*.5;p.PendingDecision=DevelopmentMilestone.SelectType;
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,1,s.OriginalTypeRosterMap!)==DevelopmentSelectionResult.Success);
    p.Progress=p.RequiredPoints*.81;p.PendingDecision=DevelopmentMilestone.SelectGenre;
    Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success);
    p.OriginalStage=3;p.PendingDecision=null;p.Progress=p.RequiredPoints;p.Quality=new(300,300,300,300);
    Check(ReleaseService.TryRelease(s,p,c,"Adapted game",5800,1,new SequenceRandom(6,0,0,1,0,0,0,0))==ReleaseResult.Success);
    var g=s.ReleasedGames[0];Check(g.OriginalMode==2 && g.MangaSourceIndex==0 && g.PredecessorProductIndex is null && g.SeriesBaseTitle==g.Title && g.OriginalSequelCounter==0);
    while (ShipmentDialogueRules.Acknowledge(s,g)) {}
    g.ReviewScores=[8,8,8,8];g.ReviewPending=false;g.HallOfFame=true;
    Check(SequelDevelopmentRules.Candidates(s).Single()==g);
    Check(DevelopmentProjectService.TryAcceptSequelPlan(s,c,0,0,0,0,"Adaptation sequel",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    var sequel=s.DevelopmentProjects.Single();Check(sequel.OriginalMode==1 && sequel.MangaSourceIndex is null && sequel.PredecessorProductIndex==0);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].MangaSourceIndex==0);
});
Test("manga popularity boundaries apply before the market-factor clamp without extra rolls",()=>
{
    int[] popularity=[0,1,2,3,5,6,9,10,14];double[] expected=[-.2,-.2,-.2,.2,.2,.5,.5,.7,.7];
    for (int i=0;i<popularity.Length;i++)
    {
        double modifier=MangaDevelopmentRules.MarketModifier(popularity[i]);Check(modifier==expected[i]);
        var random=new SequenceRandom(6);Check(Math.Abs(ReleaseService.MarketFactor(0,new(),random,modifier)-(.9+modifier))<1e-12 && random.Calls==1);
    }
    // Common novelty penalty makes the pre-modifier factor zero. Applying
    // -.2 before the clamp gives .1, never a negative market factor.
    Check(ReleaseService.MarketFactor(2,new(0,201,0,0),new SequenceRandom(3,1),-.2)==.1);
    Check(ReleaseService.MarketFactor(2,new(0,0,101,0),new SequenceRandom(6,1))==1.5);
    var musicOnly=new SequenceRandom(6);Check(ReleaseService.MarketFactor(2,new(0,0,0,999),musicOnly)==1 && musicOnly.Calls==1);
});
Test("save rejects manga lineage mismatches and missing source references",()=>
{
    var(s,c)=MangaFixture();DevelopmentProjectService.TryAcceptMangaPlan(s,c,0,0,0,0,"Adaptation",new(25,25,25,25));var p=s.DevelopmentProjects[0];
    p.MangaSourceIndex=null;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.MangaSourceIndex=31;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    p.MangaSourceIndex=0;p.OriginalMode=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.OriginalMode=2;p.PredecessorProductIndex=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
(GameState State,OriginalCatalog Catalog,ReleasedGameState Parent) SequelFixture(long money=10_000_000)
{
    var state=PlanningState(money);state.Company.HallOfFameCount=1;state.SalesFee1=state.SalesFee2=1500;
    var parent=new ReleasedGameState {Title="Synthetic Series",HardwareSourceIndex=0,AudienceSourceIndex=0,
        GameTypeSourceIndex=1,GenreSourceIndex=0,Quality=new(101.9,99.9,33.4,6.6),Recognition=99.9,
        DataSize=400,Bugs=90,ReviewScores=[8,8,8,8],HallOfFame=true,MarketFactor=1,Price=5800,
        Recall=new() {OriginalState=1},SalesMilestoneStage=0};
    state.ReleasedGames.Add(parent);return(state,CoreCatalog(),parent);
}
DevelopmentProjectState AcceptSequel(GameState s,OriginalCatalog c,int parent=0,int room=0)
{
    Check(DevelopmentProjectService.TryAcceptSequelPlan(s,c,room,0,0,parent,"Synthetic Sequel",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    return s.DevelopmentProjects.Single(x=>x.RoomIndex==room);
}
void FinishSequel(GameState s,OriginalCatalog c,DevelopmentProjectState p)
{
    var employee=Worker(6+6*p.RoomIndex);
    for (int stage=0;stage<3;stage++)
    {
        p.Progress=stage==0 ? p.RequiredPoints*.5 : stage==1 ? p.RequiredPoints*.81 : p.RequiredPoints;
        DevelopmentSimulation.TickEmployee(s,employee,c,new WorkRandom());
    }
    Check(p.OriginalStage==3 && p.PendingDecision==DevelopmentMilestone.StartDebugging);
}
Test("sequel roster requires Hall of Fame, excludes consumed parents and orders newest first",()=>
{
    var(s,c,parent)=SequelFixture();
    s.ReleasedGames.Add(new() {ProductIndex=1,Title="Not famous",ReviewScores=[7,8,8,8]});
    s.ReleasedGames.Add(new() {ProductIndex=2,Title="Newest famous",ReviewScores=[8,8,8,8],HallOfFame=true});
    Check(SequelDevelopmentRules.Candidates(s).Select(x=>x.ProductIndex).SequenceEqual(new[]{2,0}));
    parent.HasReleasedSequel=true;Check(SequelDevelopmentRules.Candidates(s).Single().ProductIndex==2);
    Check(SequelDevelopmentRules.CheckPredecessor(s,0)==DevelopmentPlanningResult.PredecessorUnavailable);
    Check(SequelDevelopmentRules.CheckPredecessor(s,1)==DevelopmentPlanningResult.PredecessorUnavailable);
    Check(SequelDevelopmentRules.CheckPredecessor(s,99)==DevelopmentPlanningResult.PredecessorUnavailable);
});
Test("sequel previews leave funds and parent eligibility unchanged",()=>
{
    var(s,c,parent)=SequelFixture();string before=SaveStore.Serialize(s);
    Check(DevelopmentProjectService.CheckSequelPlan(s,c,0,0,0,0,out var quote)==DevelopmentPlanningResult.Success);
    Check(quote==DevelopmentPlanningRules.Quote(c.Hardware[0],c.Audiences[0],1));
    Check(SequelDevelopmentRules.SuggestedTitle(parent)=="Synthetic Series2");
    Check(SaveStore.Serialize(s)==before);
});
Test("sequel acceptance floors each inherited value and resets work, bugs and data",()=>
{
    var(s,c,parent)=SequelFixture();long before=s.Money!.Value;var p=AcceptSequel(s,c);
    Check(p.OriginalMode==1 && p.PredecessorProductIndex==0 && p.Quality==new DevelopmentQuality(30,29,10,1));
    Check(p.Recognition==29 && p.Bugs==0 && p.DataSize==0 && p.Progress==0 && p.RequiredPoints==400);
    Check(p.GameTypeSourceIndex is null && p.GenreSourceIndex is null && p.PendingDecision is null && p.OriginalStage==0);
    Check(p.TechnologyApplicationCounts!.All(x=>x.Value==0) && s.Money==before-30 && !parent.HasReleasedSequel);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.DevelopmentProjects[0].Quality==p.Quality);
});
Test("two rooms cannot develop the same predecessor and rejection spends nothing",()=>
{
    var(s,c,parent)=SequelFixture();AcceptSequel(s,c);long before=s.Money!.Value;
    Check(DevelopmentProjectService.TryAcceptSequelPlan(s,c,1,0,0,0,"Duplicate",new(25,25,25,25))==DevelopmentPlanningResult.SequelAlreadyDeveloping);
    Check(s.Money==before && s.DevelopmentProjects.Count==1 && !parent.HasReleasedSequel);
    // The original list still contains the parent; confirmation rejects it.
    Check(SequelDevelopmentRules.Candidates(s).Single()==parent);
});
Test("sequel planning retains hardware license, computer, funds and occupied room gates",()=>
{
    var(s,c,_)=SequelFixture(29);var f=new DevelopmentFocus(25,25,25,25);
    Check(DevelopmentProjectService.TryAcceptSequelPlan(s,c,0,0,0,0,"Sequel",f)==DevelopmentPlanningResult.InsufficientFunds && s.Money==29);
    s.Money=100;s.HardwareLicenses!.Clear();Check(DevelopmentProjectService.CheckSequelPlan(s,c,0,0,0,0,out _)==DevelopmentPlanningResult.LicenseRequired);
    s.HardwareLicenses.Add(0);c.DevelopmentComputers[0].BitWidth=7;Check(DevelopmentProjectService.CheckSequelPlan(s,c,0,0,0,0,out _)==DevelopmentPlanningResult.ComputerTooSmall);
    c.DevelopmentComputers[0].BitWidth=8;DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Other",f);
    Check(DevelopmentProjectService.CheckSequelPlan(s,c,0,0,0,0,out _)==DevelopmentPlanningResult.RoomOccupied);
});
Test("sequel type inherits parent source directly without roster, fee or fame bonus",()=>
{
    var(s,c,parent)=SequelFixture();s.Company.Fame=null;var p=AcceptSequel(s,c);var e=Worker();long before=s.Money!.Value;
    p.Progress=199.99;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(p.OriginalStage==0 && e.Experience==0);
    p.Progress=200;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.GameTypeSourceIndex==1 && p.OriginalStage==1 && p.RequiredPoints==420 && p.PendingDecision is null);
    Check(p.Progress==200 && s.Money==before && e.Experience==10 && p.TypeCandidates is null && s.OriginalTypeRosterMap is null);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Experience==10 && p.RequiredPoints==420);
});
Test("sequel genre inheritance adds only original genre points and no audience bonus",()=>
{
    var(s,c,_)=SequelFixture();var p=AcceptSequel(s,c);var e=Worker();long before=s.Money!.Value;
    p.Progress=200;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());var quality=p.Quality;
    p.Progress=p.RequiredPoints*.81-.001;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(p.OriginalStage==1);
    p.Progress=p.RequiredPoints*.81;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.OriginalStage==2 && p.GenreSourceIndex==0 && p.RequiredPoints==450 && p.PendingDecision is null);
    Check(p.Quality==quality && s.Money==before && e.Experience==30);
    p.Progress=p.RequiredPoints;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.OriginalStage==3 && p.PendingDecision==DevelopmentMilestone.StartDebugging && e.Experience==70);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects[0].PredecessorProductIndex==0);
});
Test("sequel automatic checkpoints recalculate thresholds in one employee pass",()=>
{
    var(s,c,_)=SequelFixture();var p=AcceptSequel(s,c);var e=Worker();p.Progress=p.RequiredPoints;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.OriginalStage==2 && p.RequiredPoints==450 && p.Progress==400 && p.PendingDecision is null && e.Experience==30);
    // Zero-point source catalogs can reach debugging in the same pass.
    var(s2,c2,_)=SequelFixture();c2.GameTypes[1].RawMembers["1308"]=0;c2.Genres[0].RawMembers["1208"]=0;
    var p2=AcceptSequel(s2,c2);var e2=Worker();p2.Progress=p2.RequiredPoints;
    DevelopmentSimulation.TickEmployee(s2,e2,c2,new WorkRandom());
    Check(p2.OriginalStage==3 && p2.PendingDecision==DevelopmentMilestone.StartDebugging && e2.Experience==70);
});
Test("successful sequel shipment consumes parent and preserves root, type and genre",()=>
{
    var(s,c,parent)=SequelFixture();var p=AcceptSequel(s,c);FinishSequel(s,c,p);
    p.Quality=new(300,300,300,300);p.Bugs=0;long before=s.Money!.Value;
    Check(ReleaseService.TryRelease(s,p,c,"Renamed Sequel",5800,100,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    var child=s.ReleasedGames[1];Check(parent.HasReleasedSequel && child.OriginalMode==1 && child.PredecessorProductIndex==0);
    Check(child.SeriesBaseTitle==parent.Title && child.OriginalSequelCounter==1 && !child.HasReleasedSequel && child.ReviewPending);
    Check(s.Money==before-50_000 && s.DevelopmentProjects.Count==0 && SequelDevelopmentRules.Candidates(s).Count()==0);
    Check(AcknowledgeReviews(s,child) && child.HallOfFame);
    Check(SequelDevelopmentRules.Candidates(s).Single()==child && SequelDevelopmentRules.SuggestedTitle(child)=="Synthetic Series3");
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.ReleasedGames[0].HasReleasedSequel && s.ReleasedGames[1].SeriesBaseTitle==parent.Title);
});
Test("sequel lineage counter preserves the fresh-slot increment rather than invented numbering",()=>
{
    var(s,c,parent)=SequelFixture();
    for (int index=0;index<3;index++)
    {
        var p=AcceptSequel(s,c,index);FinishSequel(s,c,p);p.Quality=new(300,300,300,300);
        Check(ReleaseService.TryRelease(s,p,c,"Unique synthetic entry "+index,5800,1,new ConstantRandom(6))==ReleaseResult.Success);
        Check(AcknowledgeReviews(s,s.ReleasedGames[^1]));
        Check(s.ReleasedGames[^1].OriginalSequelCounter==1 && SequelDevelopmentRules.SuggestedTitle(s.ReleasedGames[^1])=="Synthetic Series3");
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
    }
    Check(SequelDevelopmentRules.Candidates(s).Single().ProductIndex==3);
});
Test("failed sequel shipment does not consume eligibility, money or RNG",()=>
{
    var(s,c,parent)=SequelFixture();var p=AcceptSequel(s,c);FinishSequel(s,c,p);var random=new SequenceRandom();
    Check(ReleaseService.TryRelease(s,p,c,parent.Title,5800,1,random)==ReleaseResult.DuplicateTitle);
    s.Money=0;Check(ReleaseService.TryRelease(s,p,c,"New",5800,1,random)==ReleaseResult.InsufficientFunds);
    Check(!parent.HasReleasedSequel && s.DevelopmentProjects.Contains(p) && random.Calls==0);
    parent.HasReleasedSequel=true;s.Money=100000;Check(ReleaseService.TryRelease(s,p,c,"New",5800,1,random)==ReleaseResult.NotReady && random.Calls==0);
});
Test("a low-reviewed sequel ends the chain without restoring the predecessor",()=>
{
    var(s,c,parent)=SequelFixture();var p=AcceptSequel(s,c);FinishSequel(s,c,p);p.Quality=new();
    Check(ReleaseService.TryRelease(s,p,c,"Failed synthetic sequel",5800,1,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    var child=s.ReleasedGames[^1];Check(AcknowledgeReviews(s,child) && !child.HallOfFame);
    Check(parent.HasReleasedSequel && !SequelDevelopmentRules.Candidates(s).Any());
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].HasReleasedSequel);
});
Test("modern saves before sequel support load with known original-game defaults",()=>
{
    var(s,c,_)=SequelFixture();DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Original",new(25,25,25,25));
    var root=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;
    var product=root["State"]!["ReleasedGames"]![0]!.AsObject();
    foreach (var name in new[]{"OriginalMode","PredecessorProductIndex","HasReleasedSequel","OriginalSequelCounter","SeriesBaseTitle"}) product.Remove(name);
    root["State"]!["DevelopmentProjects"]![0]!.AsObject().Remove("PredecessorProductIndex");
    s=SaveStore.Deserialize(root.ToJsonString());Check(s.ReleasedGames[0].OriginalMode==0 && !s.ReleasedGames[0].HasReleasedSequel && s.ReleasedGames[0].OriginalSequelCounter==0);
    Check(s.ReleasedGames[0].SeriesBaseTitle is null && s.DevelopmentProjects[0].PredecessorProductIndex is null);
    Check(SequelDevelopmentRules.SuggestedTitle(s.ReleasedGames[0])=="Synthetic Series2");
});
Test("save validation rejects dangling, duplicate and cyclic sequel references",()=>
{
    var(s,c,_)=SequelFixture();var p=AcceptSequel(s,c);
    p.PredecessorProductIndex=99;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.PredecessorProductIndex=0;
    p.OriginalMode=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.OriginalMode=1;
    s.DevelopmentProjects.Add(new() {RoomIndex=1,OriginalMode=1,PredecessorProductIndex=0,Title="Duplicate",RequiredPoints=400,Focus=p.Focus});
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.DevelopmentProjects.RemoveAt(1);
    FinishSequel(s,c,p);p.Quality=new(300,300,300,300);ReleaseService.TryRelease(s,p,c,"Child",5800,1,new SequenceRandom(6,0,0,0,0));
    AcknowledgeReviews(s,s.ReleasedGames[1]);var child=s.ReleasedGames[1];
    child.PredecessorProductIndex=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));child.PredecessorProductIndex=0;
    s.ReleasedGames[0].HasReleasedSequel=false;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.ReleasedGames[0].HasReleasedSequel=true;
    child.SeriesBaseTitle="Wrong root";Throws<InvalidDataException>(()=>SaveStore.Serialize(s));child.SeriesBaseTitle=s.ReleasedGames[0].Title;
    child.OriginalSequelCounter=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("sequel saves reject manual type and genre selection and mismatched inherited data",()=>
{
    var(s,c,_)=SequelFixture();var p=AcceptSequel(s,c);p.Progress=p.RequiredPoints*.5;p.PendingDecision=DevelopmentMilestone.SelectType;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.PendingDecision=null;
    p.GameTypeSourceIndex=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.GameTypeSourceIndex=null;
    SequelDevelopmentRules.InheritCheckpoint(s,p,c,DevelopmentMilestone.SelectType);
    p.GameTypeSourceIndex=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.GameTypeSourceIndex=1;
    p.GenreSourceIndex=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.GenreSourceIndex=null;
    p.OriginalMode=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("sequel checkpoints reject ordinary selection APIs and malformed inherited points",()=>
{
    var(s,c,_)=SequelFixture();var p=AcceptSequel(s,c);p.Progress=200;
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,new Dictionary<int,int> {{0,0}})==DevelopmentSelectionResult.WrongStage);
    c.GameTypes[1].RawMembers["1308"]=-1;string before=SaveStore.Serialize(s);
    Throws<InvalidDataException>(()=>SequelDevelopmentRules.InheritCheckpoint(s,p,c,DevelopmentMilestone.SelectType));
    Check(SaveStore.Serialize(s)==before);c.GameTypes[1].RawMembers["1308"]=20;
    SequelDevelopmentRules.InheritCheckpoint(s,p,c,DevelopmentMilestone.SelectType);p.Progress=p.RequiredPoints*.81;
    Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.WrongStage);
    c.Genres[0].RawMembers["1208"]=-1;before=SaveStore.Serialize(s);
    Throws<InvalidDataException>(()=>SequelDevelopmentRules.InheritCheckpoint(s,p,c,DevelopmentMilestone.SelectGenre));Check(SaveStore.Serialize(s)==before);
});
Test("two rooms can develop sequels to distinct Hall of Fame games",()=>
{
    var(s,c,parent)=SequelFixture();s.ReleasedGames.Add(new() {ProductIndex=1,Title="Other series",HallOfFame=true,ReviewScores=[8,8,8,8]});
    AcceptSequel(s,c,0,0);AcceptSequel(s,c,1,1);Check(SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects.Count==2);
});

Test("planning acceptance initializes two rooms, charges once and persists",()=>
{
    var state=PlanningState();var catalog=PlanningCatalog();var focus=new DevelopmentFocus(25,25,25,25);
    Check(DevelopmentProjectService.TryAcceptNewPlan(state,catalog,0,0,0," Title ",focus)==DevelopmentPlanningResult.Success);
    var p=state.DevelopmentProjects.Single();
    Check(state.Money==70 && p.RequiredPoints==400 && p.Progress==0 && p.Quality==new DevelopmentQuality());
    Check(p.Title=="Title" && p.Focus==focus && p.GameTypeSourceIndex is null && p.GenreSourceIndex is null && p.DataSize==0 && p.Recognition==0);
    Check(DevelopmentProjectService.TryAcceptNewPlan(state,catalog,0,0,0,"Second",focus)==DevelopmentPlanningResult.RoomOccupied && state.Money==70);
    Check(SaveStore.Deserialize(SaveStore.Serialize(state)).DevelopmentProjects.Single().Focus==focus);
    Check(DevelopmentProjectService.TryAcceptNewPlan(state,catalog,1,0,0,"Other",focus)==DevelopmentPlanningResult.Success && state.DevelopmentProjects.Count==2);
});
Test("planning guards are atomic and equal computer bits pass",()=>
{
    var s=PlanningState(29);var c=PlanningCatalog();var f=new DevelopmentFocus(25,25,25,25);
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Title",f)==DevelopmentPlanningResult.InsufficientFunds && s.Money==29 && s.DevelopmentProjects.Count==0);
    s.Money=null;Check(DevelopmentProjectService.CheckNewPlan(s,c,0,0,0,out var q)==DevelopmentPlanningResult.MoneyUnknown && q.Cost==30);
    s.Money=30;c.DevelopmentComputers[0].BitWidth=7;Check(DevelopmentProjectService.CheckNewPlan(s,c,0,0,0,out _)==DevelopmentPlanningResult.ComputerTooSmall);
    c.DevelopmentComputers[0].BitWidth=8;s.HardwareLicenses!.Clear();Check(DevelopmentProjectService.CheckNewPlan(s,c,0,0,0,out _)==DevelopmentPlanningResult.LicenseRequired);
    s.HardwareLicenses.Add(0);s.AvailableHardware!.Clear();Check(DevelopmentProjectService.CheckNewPlan(s,c,0,0,0,out _)==DevelopmentPlanningResult.HardwareUnavailable);
    s.AvailableHardware.Add(0);
    Throws<ArgumentException>(()=>DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Title",new(0,0,0,0)));
    Throws<ArgumentException>(()=>DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,new string('x',129),f));
    Check(s.Money==30 && s.DevelopmentProjects.Count==0);
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,0,0,"Title",f)==DevelopmentPlanningResult.Success && s.Money==0);
});
Test("type fee and double-index points use different original sources",()=>
{
    var s=PlanningState();var c=PlanningCatalog();var p=new DevelopmentProjectState {RequiredPoints=100,Progress=50};
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,new Dictionary<int,int>{{0,1}})==DevelopmentSelectionResult.Success);
    Check(s.Money==95 && p.RequiredPoints==120 && p.GameTypeSourceIndex==0 && p.OriginalStage==1 && p.Recognition==4);
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,new Dictionary<int,int>{{0,1}})==DevelopmentSelectionResult.WrongStage && s.Money==95);
});
Test("missing roster and insufficient funds cannot partially select type",()=>
{
    var s=PlanningState(4);var c=PlanningCatalog();var p=new DevelopmentProjectState {RequiredPoints=100,Progress=50};
    Throws<InvalidOperationException>(()=>DevelopmentSelectionRules.TrySelectType(s,p,c,0,new Dictionary<int,int>()));
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,new Dictionary<int,int>{{0,1}})==DevelopmentSelectionResult.InsufficientFunds);
    Check(p.GameTypeSourceIndex is null && p.RequiredPoints==100 && p.OriginalStage==0 && s.Money==4);
    s.Money=100;s.Company.Fame=null;
    Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,new Dictionary<int,int>{{0,1}})==DevelopmentSelectionResult.CompanyFameUnknown && s.Money==100);
});
Test("genre affinity uses identifier table and excludes broad audience",()=>
{
    var s=PlanningState();var c=PlanningCatalog();
    var p=new DevelopmentProjectState {RequiredPoints=100,Progress=81,OriginalStage=1,GameTypeSourceIndex=0};
    Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success);
    Check(s.Money==93 && p.RequiredPoints==130 && p.OriginalStage==2 && p.GenreSourceIndex==0 && p.Quality.Fun==20);
    c.Audiences[0].RawMembers["14A8"]=1;p=new() {RequiredPoints=100,Progress=81,OriginalStage=1,GameTypeSourceIndex=0};
    Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success && p.Quality.Fun==0);
    c.Audiences[0].RawMembers["14A8"]=2;p=new() {RequiredPoints=100,Progress=81,OriginalStage=1,GameTypeSourceIndex=1};
    Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success && p.Quality.Fun==0);
    Check(!DevelopmentSelectionRules.IsGenreEligible(c.Genres[0],2) && DevelopmentSelectionRules.IsGenreEligible(c.Genres[0],3));
});
Test("type audience affinity families retain original memberships",()=>
{
    var t=new GameTypeDefinition {RawMembers=new() {{"13B0",8}}};var a=new AudienceDefinition {RawMembers=new() {{"14A8",2}}};
    Check(DevelopmentSelectionRules.MatchesAudience(t,a));a.RawMembers["14A8"]=5;Check(!DevelopmentSelectionRules.MatchesAudience(t,a));
    t.RawMembers["13B0"]=10;Check(DevelopmentSelectionRules.MatchesAudience(t,a));
    t.RawMembers["13B0"]=12;a.RawMembers["14A8"]=7;Check(DevelopmentSelectionRules.MatchesAudience(t,a));
    t.RawMembers["13B0"]=2;Check(!DevelopmentSelectionRules.MatchesAudience(t,a));a.RawMembers["14A8"]=1;Check(DevelopmentSelectionRules.MatchesAudience(t,a));
});
Test("invalid project quality and saves fail before mutation",()=>
{
    var s=PlanningState();DevelopmentProjectService.TryAcceptNewPlan(s,PlanningCatalog(),0,0,0,"Title",new(25,25,25,25));
    var p=s.DevelopmentProjects.Single();p.Quality=new(double.NaN,0,0,0);
    Throws<InvalidDataException>(()=>DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Bugs,DevelopmentWorkTier.Medium));
    Check(p.Progress==0 && p.Bugs==0);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    p.Quality=new();p.OriginalStage=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    p.OriginalStage=0;s.DevelopmentProjects.Add(p);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("type fallback roster preserves original duplicate predicate",()=>
{
    var c=PlanningCatalog();var r=new ConstantRandom(0);
    Check(DevelopmentTypeRosterRules.Build(c,c.Audiences[0],r).SequenceEqual(new[] {0,1,0,1,0}));
    c.Audiences[0].RawMembers["14A8"]=1;
    Check(DevelopmentTypeRosterRules.Build(c,c.Audiences[0],r).SequenceEqual(new[] {0,1,0,1,0}));
    c.GameTypes[1].SourceIndex=2;Throws<InvalidDataException>(()=>DevelopmentTypeRosterRules.Build(c,c.Audiences[0],r));
});
Test("planning profiles distribute their remainder to one hundred",()=>
{
    Check(DevelopmentFocusRules.Generate(new ConstantRandom(0))==new DevelopmentFocus(43,19,19,19));
    var r=new SequenceRandom(5,0,0);Check(DevelopmentFocusRules.Generate(r)==new DevelopmentFocus(35,35,15,15) && r.Calls==3);
    for(int seed=0;seed<100;seed++){var f=DevelopmentFocusRules.Generate(new Random(seed));Check(f.Fun+f.Novelty+f.Graphics+f.Music==100);}
});
Test("work category preserves inclusive original boundaries",()=>
{
    var f=new DevelopmentFocus(25,25,25,25);
    Check(DevelopmentWorkRules.SelectCategory(f,25)==DevelopmentWorkCategory.Fun);
    Check(DevelopmentWorkRules.SelectCategory(f,26)==DevelopmentWorkCategory.Novelty);
    Check(DevelopmentWorkRules.SelectCategory(f,100)==DevelopmentWorkCategory.Music);
    Check(DevelopmentWorkRules.SelectCategory(f,101)==DevelopmentWorkCategory.Bugs);
    Check(DevelopmentWorkRules.SelectCategory(f,110)==DevelopmentWorkCategory.Bugs);
    Check(DevelopmentWorkRules.SelectCategory(f,111)==DevelopmentWorkCategory.Speed);
    Throws<ArgumentException>(()=>DevelopmentWorkRules.SelectCategory(new(20,20,20,20),1));
});
Test("work tiers retain stat eighty discontinuity and distinct debugging",()=>
{
    Check(DevelopmentWorkRules.RequiredIdleTicks(0)==50);Check(Math.Abs(DevelopmentWorkRules.RequiredIdleTicks(99)-20.3)<1e-9);
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Fun,79,0,.5)==DevelopmentWorkTier.Small);
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Fun,80,0,.5)==DevelopmentWorkTier.Medium);
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Fun,0,0,1.99/130)==DevelopmentWorkTier.Burst);
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Fun,0,0,2.01/130)==DevelopmentWorkTier.Medium);
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Debugging,99,99,0)==DevelopmentWorkTier.Medium);
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Debugging,99,99,.99)==DevelopmentWorkTier.Small);
    Throws<InvalidOperationException>(()=>DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Bugs,80,50,.5));
    Throws<ArgumentOutOfRangeException>(()=>DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Music,80,50,double.NaN));
});
Test("burst accumulation stops at fifty or beyond one hundred ticks",()=>
{
    Check(DevelopmentWorkRules.IncreasesBurstCounter(DevelopmentWorkCategory.Fun,99,49,100,.5));
    Check(!DevelopmentWorkRules.IncreasesBurstCounter(DevelopmentWorkCategory.Fun,99,50,100,.5));
    Check(!DevelopmentWorkRules.IncreasesBurstCounter(DevelopmentWorkCategory.Fun,99,49,101,.5));
    Check(DevelopmentWorkRules.IncreasesBurstCounter(DevelopmentWorkCategory.Bugs,0,0,50,.5));
    Check(!DevelopmentWorkRules.IncreasesBurstCounter(DevelopmentWorkCategory.Bugs,99,0,50,.5));
});
Test("contributions preserve fractional progress and separate debugging",()=>
{
    var p=new DevelopmentProjectState {RequiredPoints=100};
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Fun,DevelopmentWorkTier.Small);
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Graphics,DevelopmentWorkTier.Medium);
    Check(p.Quality==new DevelopmentQuality(2,0,4,0) && p.Progress==6);
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Music,DevelopmentWorkTier.Burst,7);
    Check(p.Quality.Music==7 && Math.Abs(p.Progress-(6+7/3.0))<1e-9);
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Bugs,DevelopmentWorkTier.Medium);Check(p.Bugs==4);
    double before=p.Progress;
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Debugging,DevelopmentWorkTier.Medium);
    Check(p.Bugs==2 && p.Progress==before);
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Debugging,DevelopmentWorkTier.Medium);Check(p.Bugs==0);
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Speed,DevelopmentWorkTier.Burst,50);Check(p.Progress==before+50);
    Throws<InvalidOperationException>(()=>DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Fun,DevelopmentWorkTier.Burst));
});
Test("polishing targets clip and original milestone thresholds remain exact",()=>
{
    var p=new DevelopmentProjectState {RequiredPoints=100,Progress=99,OriginalRoomState=2,Quality=new(9,0,0,0),PolishingTargets=new(10,10,10,10)};
    DevelopmentWorkRules.ApplyContribution(p,DevelopmentWorkCategory.Fun,DevelopmentWorkTier.Medium);
    Check(p.Quality.Fun==10 && p.Progress==100 && DevelopmentWorkRules.PendingMilestone(p) is null);
    p.OriginalRoomState=1;p.OriginalStage=0;p.Progress=49.99;Check(DevelopmentWorkRules.PendingMilestone(p) is null);
    p.Progress=50;Check(DevelopmentWorkRules.PendingMilestone(p)==DevelopmentMilestone.SelectType);
    p.OriginalStage=1;p.Progress=80.99;Check(DevelopmentWorkRules.PendingMilestone(p) is null);
    p.Progress=81;Check(DevelopmentWorkRules.PendingMilestone(p)==DevelopmentMilestone.SelectGenre);
    p.OriginalStage=2;p.Progress=100;Check(DevelopmentWorkRules.PendingMilestone(p)==DevelopmentMilestone.StartDebugging);
});
Test("data size uses category jitter and hardware identifier",()=>
{
    Check(Math.Abs(DevelopmentWorkRules.DataSizeGain(DevelopmentWorkCategory.Graphics,2,0)-.32)<1e-9);
    Check(Math.Abs(DevelopmentWorkRules.DataSizeGain(DevelopmentWorkCategory.Fun,0,.999)-.037)<1e-9);
    Check(DevelopmentWorkRules.DataSizeGain(DevelopmentWorkCategory.Debugging,2,.5)==0);
});
Test("isometric detours reach diagonal and axis targets",()=>
{
    Check(OfficeNavigationRules.Direction(0,0,1,1)==3);Check(OfficeNavigationRules.Direction(0,0,-1,-1)==1);
    Check(OfficeNavigationRules.Direction(0,0,1,-1)==2);Check(OfficeNavigationRules.Direction(0,0,-1,1)==4);
    Check(OfficeNavigationRules.Direction(0,0,2,0)==2);Check(OfficeNavigationRules.Direction(0,0,0,2)==4);Check(OfficeNavigationRules.Direction(0,0,0,0)==0);
    for(int x=-4;x<=4;x++)for(int y=-4;y<=4;y++)
    {
        var o=new OfficeEmployeeState {X=0,Y=0};int ticks=0;
        while((o.X/8.0+20!=20+x || o.Y/4.0+20!=20+y) && ticks++<40)OfficeNavigationRules.Step(o,(20+x,20+y));
        Check(ticks<40 && o.X==x*8 && o.Y==y*4);
    }
});
Test("office routes connect every pair of seventeen work destinations",()=>
{
    for(int from=1;from<=17;from++)for(int to=1;to<=17;to++)
    {
        var origin=OfficeNavigationRules.WorkTarget(from);var target=OfficeNavigationRules.WorkTarget(to);
        var o=new OfficeEmployeeState {X=(origin.X-20)*8,Y=(origin.Y-20)*4,RouteNode=OfficeNavigationRules.DestinationNode(from)};
        int destination=OfficeNavigationRules.DestinationNode(to),ticks=0;
        while(o.RouteNode!=destination && ticks++<300)
        {
            var waypoint=OfficeNavigationRules.NextWaypoint(o.RouteNode,destination);
            if(o.X==(waypoint.X-20)*8 && o.Y==(waypoint.Y-20)*4)
                o.RouteNode=OfficeNavigationRules.NodeOnArrival(waypoint,destination) ?? throw new Exception("Missing route transition");
            else OfficeNavigationRules.Step(o,waypoint);
        }
        Check(ticks<300);
        while((o.X!=(target.X-20)*8 || o.Y!=(target.Y-20)*4) && ticks++<400)OfficeNavigationRules.Step(o,target);
        Check(ticks<400 && OfficeNavigationRules.WorkFacing(to) is >=1 and <=4);
    }
});
Test("employees can leave all lounge seats, reach every development desk and return",()=>
{
    for (int lounge=0; lounge<12; lounge++)
    for (int action=6; action<=17; action++)
    {
        var state=State();
        var employee=new EmployeeState {Name="Synthetic",PortraitId=1,Stress=50,Office=new(){LoungeSeat=lounge,Action=18+lounge/4}};
        state.Employees.Add(employee); var office=employee.Office!;
        int ticks=0;
        while (office.MovementState!=6 && ticks++<400) OfficeStaffRules.Tick(employee,new Random(1));
        Check(ticks<400); int x=office.X,y=office.Y;
        Check(OfficeAssignmentService.TryAssignDesk(state,employee,action)==OfficeAssignmentResult.Success);
        Check(office.X==x && office.Y==y && office.OffsetX==0 && office.OffsetY==0 && office.LoungeSeat is null);
        ticks=0;
        while (!office.WorkEngaged && ticks++<500) OfficeStaffRules.Tick(employee,new Random(1));
        Check(ticks<500 && office.MovementState==0 && office.SeatedAction==action);
        var target=OfficeNavigationRules.WorkTarget(action); int facing=OfficeNavigationRules.WorkFacing(action);
        Check(office.X==(target.X-20)*8+(facing is 1 or 4 ? -8 : 8));
        Check(office.Y==(target.Y-20)*4+(facing is 1 or 2 ? -4 : 4));
        Check(office.Frame==13+3*(facing-1) && office.Facing==facing);
        double? stress=employee.Stress;
        for (int i=0;i<100;i++) OfficeStaffRules.Tick(employee,new Random(2));
        Check(employee.Stress==stress); // Desk sitting is not lounge recovery.
        Check(OfficeAssignmentService.TryTakeBreak(state,employee,new SequenceRandom(lounge))==OfficeAssignmentResult.Success);
        Check(!office.WorkEngaged && office.SeatedAction==0);
        ticks=0;
        while (office.MovementState!=6 && ticks++<500) OfficeStaffRules.Tick(employee,new Random(1));
        Check(ticks<500 && office.LoungeSeat==lounge);
    }
});
Test("desk reassignment retains coordinates and frees the old occupied action",()=>
{
    for (int from=6;from<=17;from++)
    for (int to=6;to<=17;to++)
    {
        if (from==to) continue;
        var state=State(); var employee=new EmployeeState {Name="Synthetic",PortraitId=2,Office=new()};
        state.Employees.Add(employee);
        Check(OfficeAssignmentService.TryAssignDesk(state,employee,from)==OfficeAssignmentResult.Success);
        for (int tick=0;tick<500 && !employee.Office!.WorkEngaged;tick++) OfficeStaffRules.Tick(employee,new Random(1));
        Check(employee.Office!.WorkEngaged);
        int x=employee.Office.X,y=employee.Office.Y;
        Check(OfficeAssignmentService.TryAssignDesk(state,employee,to)==OfficeAssignmentResult.Success);
        Check(employee.Office.X==x && employee.Office.Y==y && employee.Office.SeatedAction==0);
        for (int tick=0;tick<500 && !employee.Office.WorkEngaged;tick++) OfficeStaffRules.Tick(employee,new Random(1));
        Check(employee.Office.WorkEngaged && employee.Office.SeatedAction==to);
    }
});
Test("desk and lounge assignment failures are atomic",()=>
{
    var state=State(); var employee=new EmployeeState {Name="Synthetic",PortraitId=1,Office=new(){MovementState=0}};
    var other=new EmployeeState {Name="Other",PortraitId=2,Office=new(){SlotIndex=1}};
    state.Employees.AddRange([employee,other]);
    Check(OfficeAssignmentService.TryAssignDesk(state,other,6)==OfficeAssignmentResult.Success);
    string before=SaveStore.Serialize(state);
    Check(OfficeAssignmentService.TryAssignDesk(state,employee,6)==OfficeAssignmentResult.Occupied);
    Check(OfficeAssignmentService.TryAssignDesk(state,other,6)==OfficeAssignmentResult.AlreadyAssigned);
    Check(SaveStore.Serialize(state)==before);
    state.Employees.Add(new(){Name="Resting",PortraitId=3,Office=new(){SlotIndex=2,LoungeSeat=0,Action=18}});
    before=SaveStore.Serialize(state); var draws=new SequenceRandom(Enumerable.Repeat(0,14).ToArray());
    Check(OfficeAssignmentService.TryTakeBreak(state,other,draws)==OfficeAssignmentResult.NoLoungeSeat);
    Check(draws.Calls==14 && SaveStore.Serialize(state)==before);
});
Test("work-seat transition poses preserve four and six tick offsets",()=>
{
    foreach (int action in new[]{6,8,9,10,12,14,15,16})
    {
        var target=OfficeNavigationRules.WorkTarget(action);
        var office=new OfficeEmployeeState {Action=action,X=(target.X-20)*8,Y=(target.Y-20)*4,
            MovementState=10,RouteNode=OfficeNavigationRules.DestinationNode(action)};
        var employee=new EmployeeState {Name="Synthetic",PortraitId=1,Office=office};
        OfficeStaffRules.Tick(employee,new Random(1));
        Check(office.ActionTicks==1 && office.Frame==1+3*(office.Facing-1));
        for (int i=1;i<=4;i++) OfficeStaffRules.Tick(employee,new Random(1));
        Check(office.ActionTicks==5 && office.Frame==24+office.Facing && !office.WorkEngaged);
        OfficeStaffRules.Tick(employee,new Random(1)); OfficeStaffRules.Tick(employee,new Random(1));
        Check(office.ActionTicks==7);
        Check((office.OffsetX,office.OffsetY)==(office.Facing switch {1=>(-2,-1),2=>(2,-1),3=>(4,2),_=>(-4,2)}));
        OfficeStaffRules.Tick(employee,new Random(1)); OfficeStaffRules.Tick(employee,new Random(1));
        Check(office.WorkEngaged && office.ActionTicks==9);
    }
});
Test("assignment and seated workspaces persist and reject conflicting desks",()=>
{
    var state=State(); var employee=new EmployeeState {Name="Synthetic",PortraitId=1,Office=new()};state.Employees.Add(employee);
    Check(OfficeAssignmentService.TryAssignDesk(state,employee,14)==OfficeAssignmentResult.Success);
    for (int tick=0;tick<500;tick++)
    {
        OfficeStaffRules.Tick(employee,new Random(1));
        var restored=SaveStore.Deserialize(SaveStore.Serialize(state)).Employees.Single().Office!;
        Check(restored.X==employee.Office!.X && restored.SeatedAction==employee.Office.SeatedAction && restored.WorkEngaged==employee.Office.WorkEngaged);
        if (employee.Office.WorkEngaged) break;
    }
    Check(employee.Office!.WorkEngaged);
    state.Employees.Add(new(){Name="Conflict",PortraitId=2,Office=new(){SlotIndex=1,Action=14,MovementState=3}});
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.Employees.RemoveAt(1);employee.Office.WorkEngaged=false;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
});

Test("computer models select corresponding imported office rectangle",()=>
{
    var scene=new OfficeSceneData {Sprites=[new(140,91,96,571,318,36,29,32)]};
    for(int computer=0;computer<4;computer++)
    {
        var sprite=OfficePresentationRules.ForState(scene,new GameState {DevelopmentComputerIndex=computer}).Single();
        Check(sprite.SourceX==571+74*computer && sprite.SourceY==318 && sprite.Width==36 && sprite.Height==29);
    }
    Check(OfficePresentationRules.ForState(scene,new GameState()).Single().SourceX==571);
});
Test("assigned desks display local computer tiles and retain original depth keys",()=>
{
    var scene=new OfficeSceneData();
    for (int action=6;action<=17;action++)
        scene.Sprites.Add(new(93+2*(action-6),-1000,-1000,1,296,46,35,-200));
    var state=State();state.Year=1;state.Month=4;state.Week=1;
    Check(!OfficePresentationRules.ForState(scene,state).Any());
    state.Employees.Add(new(){Name="Synthetic",PortraitId=1,Office=new(){Action=6}});
    for (int computer=0;computer<4;computer++)
    {
        state.DevelopmentComputerIndex=computer;
        for (int action=6;action<=17;action++)
        {
            state.Employees[0].Office!.Action=action;
            var tile=OfficePresentationRules.ForState(scene,state).Single();
            var original=scene.Sprites.Single(x=>x.Id==tile.Id);
            Check(tile.SortKey==original.SortKey && tile.Width==46 && tile.Height==35);
            Check(tile.SourceX==(action is 8 or 9 or 14 or 15 ? 571 : 608)+74*computer);
            Check(tile.SourceY==(action is 6 or 7 or 8 or 12 or 13 or 14 ? 318 : 228));
        }
    }
    state.Employees.Clear();Check(!OfficePresentationRules.ForState(scene,state).Any());
    state.DevelopmentComputerIndex=null;
    state.Employees.Add(new(){Name="Unknown",PortraitId=1,Office=new(){Action=6}});
    Check(!OfficePresentationRules.ForState(scene,state).Any());
});

Test("fresh supported sessions preserve reset accuracy cells while legacy saves remain unknown",()=>
{
    var db=new EmployeeDatabase {SourceHash=EmployeeDatabaseImporter.ExpectedHash,MinimumSourceIndex=35,SourceUpperBound=60};
    for(int source=0;source<60;source++)db.Employees.Add(new EmployeeDefinition
    {
        SourceIndex=source,Name="Synthetic "+source,Level=1,PortraitId=1,ContractFee=100,RawAttributes=[20,30,40,50,60,70,80]
    });
    var s=new GameSession(new Random(1)).StartNewGame("Synthetic","Tester",GameMode.Normal,db);
    Check(s.OriginalAccuracyCells!.Count==60 && s.OriginalAccuracyCells.Keys.Min()==99 && s.OriginalAccuracyCells.Keys.Max()==158 && s.OriginalAccuracyCells.Values.All(x=>x==0));
    Check(DevelopmentWorkRules.Classify(DevelopmentWorkCategory.Bugs,99,0,.5,s.OriginalAccuracyCells[134])==DevelopmentWorkTier.Small);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).OriginalAccuracyCells![134]==0);
    db.SourceHash="Unknown";Check(new GameSession().StartNewGame("Synthetic","Tester",GameMode.Normal,db).OriginalAccuracyCells is null);
    Check(SaveStore.Deserialize(SaveStore.Serialize(State())).OriginalAccuracyCells is null);
});

Test("hired staff enter at original coordinates, rest at all twelve seats and survive save/load", () =>
{
    for (int seat = 0; seat < 12; seat++)
    {
        var state = State(200);
        var candidate = new EmployeeState { SourceIndex = seat, PortraitId = 2, Name = "Synthetic", ContractFee = 100, Stress = 50 };
        Check(HiringService.TryHire(state, candidate, new SequenceRandom(seat)) == HireResult.Success);
        var employee = state.Employees.Single();
        var office = employee.Office!;
        Check(office.X == 128 && office.Y == 56 && office.Frame == 7 && office.LoungeSeat == seat);
        int ticks = 0;
        while (office.MovementState != 6 && ticks++ < 200) OfficeStaffRules.Tick(employee, new Random(1));
        Check(ticks < 200 && office.MovementState == 6);
        var target = OfficeStaffRules.SeatTarget(seat);
        Check(office.X == (target.X-20)*8 && office.Y == (target.Y-20)*4);
        Check(employee.Stress < 50 && employee.Stress >= 49.7);
        // Persist an in-flight action counter; loading may not respawn the worker.
        var restored = SaveStore.Deserialize(SaveStore.Serialize(state)).Employees.Single();
        Check(restored.Office!.X == office.X && restored.Office.ActionTicks == office.ActionTicks && restored.Office.Frame == office.Frame);
        for (int tick = 0; tick < 200; tick++) OfficeStaffRules.Tick(restored, new Random(5));
        Check(restored.Office.Frame is >= 13 and <= 44 && restored.Stress < employee.Stress);
    }
});
Test("lounge sampling retries fourteen draws without forcing an occupied seat", () =>
{
    var state = State();
    state.Employees.Add(new() {Name="Existing", PortraitId=1, Office=new() {SlotIndex=0,LoungeSeat=0,Action=18}});
    var draws = new SequenceRandom(Enumerable.Repeat(0,13).Append(7).ToArray());
    var office = OfficeStaffRules.CreateHire(state, draws);
    Check(draws.Calls == 14 && office.LoungeSeat == 7 && office.SlotIndex == 1 && office.Action == 19);
    draws = new SequenceRandom(Enumerable.Repeat(0,14).ToArray());
    office = OfficeStaffRules.CreateHire(state, draws);
    Check(draws.Calls == 14 && office.LoungeSeat is null && office.Action == 0 && office.MovementState == 0);
});
Test("employee animation uses portrait columns and original feet depth", () =>
{
    var employee = new EmployeeState {Name="Synthetic",PortraitId=3,Office=new() {SlotIndex=2,Frame=7}};
    var sprite = OfficeStaffRules.Sprite(employee);
    Check(sprite.X == 128 && sprite.Y == 56 && sprite.SourceX == 304 && sprite.SourceY == 64 && sprite.SortKey == 84);
    employee.Office.Frame = 44; employee.Office.OffsetX=8;employee.Office.OffsetY=4;
    sprite = OfficeStaffRules.Sprite(employee);
    Check(sprite.SourceX==416 && sprite.SourceY==160 && sprite.X==136 && sprite.Y==60 && sprite.SortKey==84);
    employee.Office.Frame = 0;
    Throws<InvalidDataException>(() => OfficeStaffRules.Sprite(employee));
});
Test("save rejects conflicting or malformed office workspaces", () =>
{
    var state = State();
    state.Employees.Add(new(){Name="Synthetic",PortraitId=1,Office=new(){LoungeSeat=0,Action=18}});
    state.Employees.Add(new(){Name="Other",PortraitId=2,Office=new(){SlotIndex=1,LoungeSeat=0,Action=18}});
    Throws<InvalidDataException>(() => SaveStore.Serialize(state));
    state.Employees.RemoveAt(1); state.Employees[0].Office!.Frame=45;
    Throws<InvalidDataException>(() => SaveStore.Serialize(state));
});

Test("original raster mask preserves background and replaces opaque pixels", () =>
{
    for (int value = 0; value < 256; value++)
    {
        Check(OriginalBitmapCompositor.CompositeChannel((byte)value, 255, 255) == value);
        Check(OriginalBitmapCompositor.CompositeChannel((byte)value, 0, 73) == 73);
    }
    byte[] target = [10,20,30,40,50,60];
    byte[] colors = [1,2,3,91,92,93]; byte[] masks = [0,0,0,0,0,0];
    OriginalBitmapCompositor.Blit(target, 2, 1, colors, masks, 2, 1, new(0,-1,0,0,0,2,1,0));
    Check(target.SequenceEqual(new byte[]{91,92,93,40,50,60}));
    OriginalBitmapCompositor.Blit(target, 2, 1, colors, masks, 2, 1, new(0,0,0,0,0,0,0,0));
    Check(target.SequenceEqual(new byte[]{91,92,93,40,50,60}));
    Throws<ArgumentException>(() => OriginalBitmapCompositor.Blit(target,2,1,colors,masks,2,1,new(0,0,0,2,0,1,1,0)));
});
Test("office draw filtering and calendar glyphs follow original renderer", () =>
{
    var scene = new OfficeSceneData { Sprites = [new(23,-100,0,0,0,162,111,0),new(144,345,23,0,0,28,21,10000),new(145,390,23,0,0,28,21,10000)] };
    var rows = OfficePresentationRules.ForDate(scene,new(1,4,1)).ToList();
    Check(rows.Count == 2 && rows[0].SourceX == 949 && rows[0].SourceY == 294 && rows[1].SourceY == 228);
    Check(OfficePresentationRules.ForDate(scene,new(2,12,4)).First().SourceX == 978);
    Throws<ArgumentOutOfRangeException>(() => OfficePresentationRules.ForDate(scene,new(1,13,1)).ToList());
});
Test("renewal is due once per year at March end", () =>
{
    var due = new List<GameDate>();
    var date = CalendarRules.OpeningDate;
    for (int i = 0; i < 96; i++, date = CalendarRules.AdvanceWeek(date))
        if (ContractRenewalService.IsDueAtEndOfWeek(date)) due.Add(date);
    Check(due.SequenceEqual(new[] {new GameDate(2,3,4), new GameDate(3,3,4)}));
    Throws<ArgumentOutOfRangeException>(() => ContractRenewalService.IsDueAtEndOfWeek(new(1,3,5)));
});
Test("renewal charges selected fees and releases unchecked sources without losing progression", () =>
{
    var state = State(300);
    var retained = Candidate(9, 120); retained.Stress = 87;
    var dismissed = Candidate(2, 50); dismissed.Level = 7; dismissed.Experience = 45.7; dismissed.Stress = 99;
    var source = Row(2, 1); source.Employee.Level = 7; source.Employee.Experience = 45.7; source.Employee.Stress = 99;
    state.Employees.AddRange([retained, dismissed]); state.EmployeeSources.AddRange([Row(9,1), source]);
    Check(ContractRenewalService.TryRenew(state, new HashSet<int> {9}) == ContractRenewalResult.Success);
    Check(state.Money == 180 && state.Employees.SequenceEqual(new[] {retained}) && retained.Stress == 87);
    Check(source.GateValue == 0 && source.Employee.Stress == 0 && dismissed.Stress == 0);
    Check(source.Employee.Level == 7 && source.Employee.Experience == 45.7 && source.Employee.ContractFee == 100);
    Check(state.EmployeeSources[0].GateValue == 1);
});
Test("renewal insufficient funds and unknown fees do not dismiss anyone", () =>
{
    var state = State(99); var first = Candidate(1); var second = Candidate(2, null);
    state.Employees.AddRange([first, second]); state.EmployeeSources.AddRange([Row(1,1), Row(2,1)]);
    Check(ContractRenewalService.TryRenew(state, new HashSet<int> {1}) == ContractRenewalResult.InsufficientFunds);
    Check(ContractRenewalService.TryRenew(state, new HashSet<int> {2}) == ContractRenewalResult.FeeUnknown);
    Check(state.Money == 99 && state.Employees.Count == 2 && state.EmployeeSources.All(x => x.GateValue == 1));
    // An unchecked unknown fee does not enter the total; all unchecked costs zero.
    Check(ContractRenewalService.TryRenew(state, new HashSet<int>()) == ContractRenewalResult.Success);
    Check(state.Money == 99 && state.Employees.Count == 0 && state.EmployeeSources.All(x => x.GateValue == 0));
});
Test("renewal rejects stale identities and overflowing totals atomically", () =>
{
    var state = State(long.MaxValue); state.Employees.AddRange([Candidate(1, long.MaxValue), Candidate(2, 1)]);
    Throws<ArgumentException>(() => ContractRenewalService.TryRenew(state, new HashSet<int> {3}));
    Throws<OverflowException>(() => ContractRenewalService.TryRenew(state, new HashSet<int> {1,2}));
    Check(state.Money == long.MaxValue && state.Employees.Count == 2);
    state.Employees.Add(Candidate(1));
    Throws<InvalidDataException>(() => ContractRenewalService.TryRenew(state, new HashSet<int>()));
    Check(state.Employees.Count == 3);
});

Test("computer upgrade charges full price, rejects downgrade and persists", () =>
{
    var catalog = new OriginalCatalog { DevelopmentComputers = [
        new() {SourceIndex=0, BitWidth=8, PurchasePrice=60},
        new() {SourceIndex=1, BitWidth=16, PurchasePrice=120},
        new() {SourceIndex=2, BitWidth=32, PurchasePrice=250}] };
    var state = State(120); state.DevelopmentComputerIndex = 0;
    Check(DevelopmentComputerService.TryPurchase(state,catalog,0) == ComputerPurchaseResult.AlreadyOwned);
    Check(DevelopmentComputerService.TryPurchase(state,catalog,2) == ComputerPurchaseResult.InsufficientFunds);
    Check(state.Money == 120 && state.DevelopmentComputerIndex == 0);
    Check(DevelopmentComputerService.TryPurchase(state,catalog,1) == ComputerPurchaseResult.Success);
    Check(state.Money == 0 && state.DevelopmentComputerIndex == 1);
    Check(DevelopmentComputerService.TryPurchase(state,catalog,0) == ComputerPurchaseResult.Downgrade);
    Check(SaveStore.Deserialize(SaveStore.Serialize(state)).DevelopmentComputerIndex == 1);
    Check(DevelopmentComputerService.SupportsHardware(16,16) && !DevelopmentComputerService.SupportsHardware(8,16));
    state.DevelopmentComputerIndex = null;
    Check(DevelopmentComputerService.TryPurchase(state,catalog,1) == ComputerPurchaseResult.CurrentComputerUnknown);
});

Test("planning quote adds hardware and audience, mode 2 requires known extra cost", () =>
{
    var hardware = new HardwareDefinition {RawMembers=new() {{"0F00",8},{"0FFC",10_000_000},{"0FE0",100}}};
    var audience = new AudienceDefinition {RawMembers=new() {{"148C",10_000_000},{"1454",300}}};
    Check(DevelopmentPlanningRules.Quote(hardware,audience,0) == new DevelopmentQuote(20_000_000,400));
    Check(DevelopmentPlanningRules.Quote(hardware,audience,1) == new DevelopmentQuote(20_000_000,400));
    Check(DevelopmentPlanningRules.Quote(hardware,audience,2,5_000_000) == new DevelopmentQuote(25_000_000,400));
    Throws<InvalidOperationException>(() => DevelopmentPlanningRules.Quote(hardware,audience,2));
    Throws<ArgumentOutOfRangeException>(() => DevelopmentPlanningRules.Quote(hardware,audience,3));
});

Test("hardware license purchase respects availability, fee and existing ownership", () =>
{
    var catalog = new OriginalCatalog {Hardware = [
        new() {SourceIndex=0, RawMembers=new() {{"0F1C",1},{"0FC4",3}}},
        new() {SourceIndex=1, RawMembers=new() {{"0F1C",0},{"0FC4",10}}},
        new() {SourceIndex=2, RawMembers=new() {{"0F1C",1},{"0FC4",20}}}]};
    var state = new GameSession().StartNewGame("Test","Tester",GameMode.Normal,null,catalog);
    Check(state.HardwareLicenses!.SetEquals(new[] {0}) && state.AvailableHardware!.SetEquals(new[] {0,2}));
    state.Money=20;
    Check(HardwareLicenseService.TryPurchase(state,catalog,0) == LicensePurchaseResult.AlreadyOwned);
    Check(HardwareLicenseService.TryPurchase(state,catalog,1) == LicensePurchaseResult.Unavailable);
    state.Money=19;
    Check(HardwareLicenseService.TryPurchase(state,catalog,2) == LicensePurchaseResult.InsufficientFunds);
    Check(state.Money==19 && state.HardwareLicenses.SetEquals(new[] {0}));
    state.Money=20;
    Check(HardwareLicenseService.TryPurchase(state,catalog,2) == LicensePurchaseResult.Success);
    Check(state.Money==0 && state.HardwareLicenses.SetEquals(new[] {0,2}));
    var restored=SaveStore.Deserialize(SaveStore.Serialize(state));
    Check(restored.HardwareLicenses!.SetEquals(new[] {0,2}) && restored.AvailableHardware!.SetEquals(new[] {0,2}));
});

Test("employee literal-to-semantic map", () =>
{
    var database = new EmployeeDatabase
    {
        Employees = [new EmployeeDefinition { SourceIndex = 0, Name = "Synthetic", Level = 1,
            RawAttributes = [10, 20, 30, 40, 50, 60, 70], ContractFee = 100 }]
    };
    var employee = database.CreateSources(new Random(1))[0].Employee;
    Check(employee.Inspiration == 10 && employee.DevelopmentSpeed == 20 && employee.Accuracy == 30);
    Check(employee.Music == 40 && employee.Graphics == 50 && employee.Novelty == 60 && employee.Fun == 70);
});
Test("level-up gain, super bonus and cap", () =>
{
    Check(EmployeeProgressionRules.SuperBonus(0) == 10);
    Check(EmployeeProgressionRules.SuperBonus(0.099f) == 10);
    Check(EmployeeProgressionRules.SuperBonus(0.1f) == 0);
    Check(EmployeeProgressionRules.AttributeGain(50, 7, 0, 0) == 2);
    Check(EmployeeProgressionRules.AttributeGain(50, 7, 10, 0.5f) == 15);
    Check(EmployeeProgressionRules.AttributeGain(98, 7, 10, 0.9f) == 1);
    Check(EmployeeProgressionRules.AttributeGain(99, 7, 0, 0.9f) == 0);
    Throws<ArgumentOutOfRangeException>(() => EmployeeProgressionRules.SuperBonus(1));
});
Test("calendar opening and rollover", () =>
{
    Check(CalendarRules.OpeningDate == new GameDate(1, 4, 1));
    Check(CalendarRules.AdvanceWeek(new(1, 4, 3)) == new GameDate(1, 4, 4));
    Check(CalendarRules.AdvanceWeek(new(1, 4, 4)) == new GameDate(1, 5, 1));
    Check(CalendarRules.AdvanceWeek(new(1, 12, 4)) == new GameDate(2, 1, 1));
    Throws<ArgumentOutOfRangeException>(() => CalendarRules.AdvanceWeek(new(1, 0, 1)));
});
Test("manual level-up thresholds, reset, fee and source synchronization", () =>
{
    Check(Enumerable.Range(1, 10).Select(EmployeeLevelUpService.RequiredExperience).SequenceEqual(new[] {100,150,200,250,300,350,400,500,600,777}));
    var state = State();
    var employee = new EmployeeState { SourceIndex = 1, Name = "Synthetic", Level = 1, Experience = 125,
        ContractFee = 101, RawParameter3C = 7, Fun = 98, Graphics = 20, Novelty = 30,
        Music = 40, Accuracy = 50, DevelopmentSpeed = 60, Inspiration = 70 };
    state.Employees.Add(employee); state.EmployeeSources.Add(Row(1, 1));
    Check(EmployeeLevelUpService.TryLevelUp(state, 1, new float[] {0,0,0,0,0,0,0,0}) == LevelUpResult.Success);
    Check(employee.Level == 2 && employee.Experience == 0 && employee.ContractFee == 151);
    Check(employee.Fun == 99 && employee.Graphics == 32 && employee.Inspiration == 82);
    Check(state.EmployeeSources[0].Employee.Level == 2 && state.EmployeeSources[0].Employee.ContractFee == 151);
    Check(employee.OriginalAttributes.SequenceEqual(new[] {82,72,62,52,32,42,99}));
});
Test("manual level-up guards and invalid draws are atomic", () =>
{
    var state = State(); var employee = new EmployeeState { SourceIndex = 1, Name = "Synthetic", Level = 9,
        Experience = 599, ContractFee = 100, RawParameter3C = 7, Fun = 10, Graphics = 10, Novelty = 10,
        Music = 10, Accuracy = 10, DevelopmentSpeed = 10, Inspiration = 10 };
    state.Employees.Add(employee);
    Check(EmployeeLevelUpService.TryLevelUp(state, 1, []) == LevelUpResult.InsufficientExperience);
    employee.Experience = 600;
    Throws<ArgumentOutOfRangeException>(() => EmployeeLevelUpService.TryLevelUp(state, 1, new float[] {0,0,0,0,0,0,0,float.NaN}));
    Check(employee.Level == 9 && employee.Experience == 600 && employee.ContractFee == 100 && employee.Fun == 10);
    Check(EmployeeLevelUpService.TryLevelUp(state, 1, new float[] {.5f,0,0,0,0,0,0,0}) == LevelUpResult.Success);
    Check(employee.Level == 10);
    Check(EmployeeLevelUpService.TryLevelUp(state, 1, []) == LevelUpResult.MaximumLevel);
});
Test("48 weeks per year", () =>
{
    var date = CalendarRules.OpeningDate;
    for (int n = 0; n < 48; n++) date = CalendarRules.AdvanceWeek(date);
    Check(date == new GameDate(2, 4, 1));
});
Test("fractional experience is retained and capped", () =>
{
    Check(Math.Abs(EmployeeProgressionRules.AccrueExperience(0, 3.7, 1) - 3.7) < 1e-10);
    Check(EmployeeProgressionRules.AccrueExperience(99, 3.7, 1) == 100);
    Throws<ArgumentOutOfRangeException>(() => EmployeeProgressionRules.AccrueExperience(double.NaN, 10, 1));
});
Test("hot spring rejects without mutation and charges once for all staff", () =>
{
    var state = State(8_000_000); Check(EmployeeCareService.TryHotSpring(state) == EmployeeCareResult.NoEmployees);
    var employee = Candidate(1); employee.Stress = 85.5; state.Employees.Add(employee); state.EmployeeSources.Add(Row(1, 1));
    state.Money = 7_999_999;
    Check(EmployeeCareService.TryHotSpring(state) == EmployeeCareResult.InsufficientFunds && employee.Stress == 85.5);
    state.Money = 8_000_000;
    Check(EmployeeCareService.TryHotSpring(state) == EmployeeCareResult.Success && state.Money == 0 && employee.Stress == 0);
    Check(EmployeeCareService.StressWarningEligible(true, 85) && !EmployeeCareService.StressWarningEligible(true, 84.9));
    Check(!EmployeeCareService.StressWarningEligible(false, 90));
});
Test("mode starting funds", () =>
{
    var session = new GameSession(new Random(1));
    Check(session.StartNewGame(" A ", " B ", GameMode.Normal).Money == 450_000_000);
    Check(session.StartNewGame("A", "B", GameMode.Beginner).Money == 1_200_000_000);
    Check(session.Current!.Company.Fame == 12 && session.Current.Company.Rank == 0);
    Throws<ArgumentException>(() => session.StartNewGame(" ", "B", GameMode.Normal));
});
Test("roster bounds, gates, scan order, repeated passes", () =>
{
    var source = new[] { Row(0), Row(1, 1), Row(2), Row(3) };
    var roster = RecruitmentGenerator.BuildRoster(source, 1, 3, new ConstantRandom(0));
    Check(roster.Count == 5 && roster.All(x => x.SourceIndex == 2));
    roster[0].OriginalAttributes[0] = 99;
    Check(source[2].Employee.OriginalAttributes[0] == 1 && roster[1].OriginalAttributes[0] == 1);
    Check(RecruitmentGenerator.BuildRoster(source, 1, 2, new Random(1)).Count == 0);
});
Test("roster only accepts zero draw", () =>
{
    var roster = RecruitmentGenerator.BuildRoster(new[] { Row(0), Row(1) }, 0, 2, new AlternatingRandom());
    Check(roster.Count == 5 && roster.All(x => x.SourceIndex == 1));
    Throws<InvalidOperationException>(() => RecruitmentGenerator.BuildRoster(new[] { Row(0) }, 0, 1, new ConstantRandom(3)));
});
Test("hire charges once and owns independent state", () =>
{
    var state = State(); var candidate = Candidate(0); state.EmployeeSources.Add(Row(0));
    Check(HiringService.TryHire(state, candidate) == HireResult.Success && state.Money == 0);
    Check(state.Employees.Count == 1 && state.EmployeeSources[0].GateValue == 1);
    candidate.OriginalAttributes[0] = 100;
    Check(state.Employees[0].OriginalAttributes[0] == 1);
    Check(HiringService.TryHire(state, candidate) == HireResult.AlreadyEmployed && state.Money == 0);
});
Test("hire failures do not mutate", () =>
{
    var state = State(99);
    Check(HiringService.TryHire(state, Candidate(1)) == HireResult.InsufficientFunds && state.Money == 99 && state.Employees.Count == 0);
    Check(HiringService.TryHire(state, Candidate(1, null)) == HireResult.ContractFeeUnknown);
    Check(HiringService.TryHire(State(null), Candidate(1)) == HireResult.MoneyUnknown);
    Check(HiringService.TryHire(state, null) == HireResult.NoActiveCandidate);
    Check(HiringService.TryHire(state, Candidate(1, -1)) == HireResult.InvalidContractFee);
});
Test("save restores collections, unknowns, Japanese and gates", () =>
{
    var state = new GameSession().StartNewGame("会社", "社長", GameMode.Normal);
    state.EmployeeSources = [Row(2)]; state.EmployeeSources[0].GateValue = 1;
    state.MinimumSourceIndex = 2; state.SourceUpperBound = 3;
    state.Employees.Add(Candidate(2)); state.Recruitment.Candidates.Add(Candidate(3));
    var restored = SaveStore.Deserialize(SaveStore.Serialize(state));
    Check(restored.Company.Name == "会社" && restored.Employees.Count == 1 && restored.EmployeeSources[0].GateValue == 1);
    Check(restored.Recruitment.Candidates.Count == 1 && restored.Company.Fame == 12 && restored.Employees[0].Fun is null);
});
Test("save rejects invalid or future formats", () =>
{
    Throws<InvalidDataException>(() => SaveStore.Deserialize("{}"));
    Throws<InvalidDataException>(() => SaveStore.Deserialize("{"));
    Throws<InvalidDataException>(() => SaveStore.Deserialize(SaveStore.Serialize(State()).Replace("\"SchemaVersion\": 1", "\"SchemaVersion\": 99")));
    var state = State(); state.Month = 13;
    Throws<InvalidDataException>(() => SaveStore.Serialize(state));
});
Test("atomic save/load", () =>
{
    string dir = Path.Combine(Path.GetTempPath(), "opengds2-" + Guid.NewGuid().ToString("N"));
    try
    {
        string path = Path.Combine(dir, "save.json"); SaveStore.Save(path, State());
        Check(SaveStore.Load(path).Money == 100);
        SaveStore.Save(path, State(200)); Check(SaveStore.Load(path).Money == 200);
        Check(Directory.GetFiles(dir).Length == 1);
    }
    finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
});
Test("initializer rejects unrecognized binary", () => Throws<InvalidDataException>(() => EmployeeDatabaseImporter.Extract(new byte[128])));
Test("catalog rejects unrecognized binary", () => Throws<InvalidDataException>(() => OriginalCatalogImporter.Extract(new byte[128])));
Test("LHA validates data CRC and stored size", () =>
{
    var name = System.Text.Encoding.ASCII.GetBytes("test.txt");
    var payload = System.Text.Encoding.ASCII.GetBytes("123456789");
    int headerLength = 26 + name.Length + 3;
    byte[] raw = new byte[headerLength + payload.Length + 1];
    BitConverter.GetBytes((ushort)headerLength).CopyTo(raw, 0);
    System.Text.Encoding.ASCII.GetBytes("-lh0-").CopyTo(raw, 2);
    BitConverter.GetBytes(payload.Length).CopyTo(raw, 7); BitConverter.GetBytes(payload.Length).CopyTo(raw, 11);
    raw[20] = 2; BitConverter.GetBytes((ushort)0xBB3D).CopyTo(raw, 21);
    BitConverter.GetBytes((ushort)(name.Length + 3)).CopyTo(raw, 24); raw[26] = 1;
    name.CopyTo(raw, 27); payload.CopyTo(raw, headerLength);
    string file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".lzh");
    try
    {
        File.WriteAllBytes(file, raw); var archive = new LhaArchive(file);
        Check(archive.Extract(archive.Entries.Single()).SequenceEqual(payload));
        raw[headerLength] ^= 1; File.WriteAllBytes(file, raw); archive = new LhaArchive(file);
        Throws<InvalidDataException>(() => archive.Extract(archive.Entries.Single()));
    }
    finally { File.Delete(file); }
});
if (args.Length == 2)
{
    Test("optional original import supports development release and real-catalog sales",()=>
    {
        var db=EmployeeDatabaseImporter.Extract(File.ReadAllBytes(args[0]));
        var catalog=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
        Check(catalog.InitialRivalProducts.Count==30 && catalog.StagedRivalProducts.Count==35 && catalog.RivalCompanies.Count==10);
        Check(catalog.StagedRivalProducts.Select(p=>p.TriggerPlatform).Distinct().Order().SequenceEqual(Enumerable.Range(0,8)));
        Check(catalog.StagedRivalProducts.Where(p=>p.TriggerPlatform>=4).All(p=>p.Product.Hardware==0));
        Check(catalog.InitialRivalProducts.All(p=>p.Owner is >=0 and <=9 && p.Demand>=0 && p.SalesPower>=0));
        var session=new GameSession(new Random(123));
        var state=session.StartNewGame("Compatibility test","Tester",GameMode.Beginner,db,catalog);
        Check(state.Competition?.Products.Count==30 && state.Competition.PlatformAges.All(a=>a==0));
        foreach (var candidate in session.OpenRecruitment().Candidates.ToArray().Take(3))
        {
            Check(session.Hire(candidate)==HireResult.Success);
            Check(session.AcknowledgeHireGreeting());
        }
        Check(state.Employees.Count==3);
        // The live desktop enters the staff list on construction. This
        // headless domain fixture must perform the same proven scratch writer.
        Check(OriginalPayrollRules.CaptureStaffList(state));
        var desks=new Dictionary<int,int>();
        for (int i=0;i<3;i++)
        {
            var employee=state.Employees[i];desks[employee.SourceIndex!.Value]=6+i;
            Check(OfficeAssignmentService.TryAssignDesk(state,employee,6+i)==OfficeAssignmentResult.Success);
        }
        Check(DevelopmentProjectService.TryAcceptNewPlan(state,catalog,0,0,0,"Compatibility test game",new(25,25,25,25))==DevelopmentPlanningResult.Success);
        var project=state.DevelopmentProjects.Single();int ticks=0;
        while (project.OriginalStage<3 && ticks++<200000)
        {
            session.TickOffice(catalog);
            if (project.PendingDecision==DevelopmentMilestone.SelectType)
            {
                var roster=DevelopmentSimulation.PrepareTypeCandidates(state,project,catalog,new Random(12));
                Check(DevelopmentSelectionRules.TrySelectType(state,project,catalog,roster[0],state.OriginalTypeRosterMap!)==DevelopmentSelectionResult.Success);
            }
            if (project.PendingDecision==DevelopmentMilestone.SelectGenre)
            {
                var genre=catalog.Genres.First(x=>DevelopmentSelectionRules.IsGenreEligible(x,state.Company.OriginalMember90!.Value));
                Check(DevelopmentSelectionRules.TrySelectGenre(state,project,catalog,genre.SourceIndex)==DevelopmentSelectionResult.Success);
            }
            foreach (var employee in state.Employees)
            {
                if (employee.Office!.Action is >=6 and <=17 && employee.Stress>=80)
                    Check(OfficeAssignmentService.TryTakeBreak(state,employee,new Random(4))==OfficeAssignmentResult.Success);
                else if (employee.Office.MovementState==6 && employee.Stress<=20)
                    Check(OfficeAssignmentService.TryAssignDesk(state,employee,desks[employee.SourceIndex!.Value])==OfficeAssignmentResult.Success);
            }
        }
        Check(ticks<200000 && project.OriginalStage==3 && project.Quality.Fun+project.Quality.Novelty+project.Quality.Graphics+project.Quality.Music>project.Bugs);
        Check(ReleaseService.TryRelease(state,project,catalog,project.Title,5800,5000,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
        Check(AcknowledgeReviews(state,state.ReleasedGames.Single()));
        long funds=state.Money!.Value;
        for (int i=0;i<20;i++) session.TickOffice(catalog);
        Check(state.ReleasedGames.Single().UnitsSold>0 && state.Money>funds);
        Check(SaveStore.Deserialize(SaveStore.Serialize(state)).ReleasedGames.Count==1);
    });
    Test("optional original manga helper and all 31 initializer records match local extraction",()=>
    {
        var c=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
        Check(c.Manga.Count==31 && c.Manga.Select(x=>x.SourceIndex).SequenceEqual(Enumerable.Range(0,31)));
        Check(c.Manga[0].RightsFee==150000000 && c.Manga[1].RightsFee==100000000 && c.Manga[30].RightsFee==30000000);
        Check(c.Manga[0].Popularity==8 && c.Manga[1].Popularity==10 && c.Manga[28].Popularity==14);
        Check(c.Manga.All(x=>!string.IsNullOrWhiteSpace(x.Title) && !string.IsNullOrWhiteSpace(x.Author) && !string.IsNullOrWhiteSpace(x.Publisher)));
    });
    Test("optional original secretary definitions recover local names biographies rank salary fee and portrait",()=>
    {
        var c=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));Check(SecretaryManagementRules.HasCatalog(c));
        Check(c.Secretaries.Select(x=>x.ContractFee).SequenceEqual(new long[]{200000000,200000000,100000000,5000000,2000000}));
        Check(c.Secretaries.Select(x=>x.AdvertisedSalary).SequenceEqual(new long[]{2000000,2000000,1000000,400000,200000}));
        Check(c.Secretaries.Select(x=>x.Rank).SequenceEqual(new[]{"S","S","A","B","C"}));
        Check(c.Secretaries.Select(x=>x.PortraitId).SequenceEqual(new[]{1,2,4,5,6}));
        var d=OriginalDialogueImporter.Extract(File.ReadAllBytes(args[0]));Check(d.Entries["SECRETARY_GREETING"].Contains("{name}") && !string.IsNullOrWhiteSpace(d.Entries["SECRETARY_FAREWELL"]));
    });
    Test("optional original hire dialogue imports locally with ten branches",()=>
    {
        var dialogue=OriginalDialogueImporter.Extract(File.ReadAllBytes(args[0]));
        Check(dialogue.Entries.Count==136 && dialogue.SchemaVersion==7 && dialogue.SourceHash==EmployeeDatabaseImporter.ExpectedHash);
        Check(Enumerable.Range(0,10).All(i=>!string.IsNullOrWhiteSpace(dialogue.Entries[$"HIRE_GREETING_{i}"])));
        Check(dialogue.Entries["HIRE_GREETING_9"].Contains("{company}"));
    });
    Test("optional local original database parity", () =>
    {
        var database = EmployeeDatabaseImporter.Extract(File.ReadAllBytes(args[0]));
        var python = System.Text.Json.JsonSerializer.Deserialize<EmployeeDatabase>(File.ReadAllText(args[1]))!;
        Check(database.Employees.Count == 60 && database.MinimumSourceIndex == 35);
        for (int n = 0; n < 60; n++)
        {
            var a = database.Employees[n]; var b = python.Employees[n];
            Check(a.Name == b.Name && a.GeneratedName == b.GeneratedName && a.ContractFee == b.ContractFee && a.Level == b.Level && a.RawAttributes.SequenceEqual(b.RawAttributes) && a.DefinitionAddress == b.DefinitionAddress &&
                a.SourceIndex == b.SourceIndex && a.PortraitId == b.PortraitId && a.RawParameter34 == b.RawParameter34 && a.RawParameter3C == b.RawParameter3C && a.RawParameter40 == b.RawParameter40 &&
                a.InitialGate == b.InitialGate && a.InitialSpecialFlag == b.InitialSpecialFlag &&
                a.InitialTechnologyLevels!.OrderBy(x=>x.Key).SequenceEqual(b.InitialTechnologyLevels!.OrderBy(x=>x.Key)));
        }
        var catalogPath = Path.Combine(Path.GetDirectoryName(args[1])!, "catalog.json");
        if (File.Exists(catalogPath))
        {
            var catalog = OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
            var expected = System.Text.Json.JsonSerializer.Deserialize<OriginalCatalog>(File.ReadAllText(catalogPath))!;
            Check(catalog.Technologies.Count==8 && expected.Technologies.Count==8);
            for (int n=0;n<8;n++)
            {
                var a=catalog.Technologies[n];var b=expected.Technologies[n];
                Check(a.Name==b.Name && a.Description==b.Description && a.Identifier==b.Identifier && a.SourceIndex==b.SourceIndex &&
                    a.InitialOwnershipLevel==b.InitialOwnershipLevel && a.PurchaseFee==b.PurchaseFee &&
                    a.RequiredTrainingWork.SequenceEqual(b.RequiredTrainingWork) && a.ExperienceCosts.SequenceEqual(b.ExperienceCosts) &&
                    a.RawTable158C.SequenceEqual(b.RawTable158C) && a.RawTable1568.SequenceEqual(b.RawTable1568));
            }
            Check(catalog.Genres.Count == 18);
            for (int n = 0; n < 18; n++)
            {
                var actual = catalog.Genres[n]; var row = expected.Genres[n];
                Check(actual.Name == row.Name && actual.DefinitionAddress == row.DefinitionAddress &&
                    actual.RawMembers.OrderBy(x => x.Key).SequenceEqual(row.RawMembers.OrderBy(x => x.Key)));
            }
            Check(catalog.Audiences.Count == 7 && expected.Audiences.Count == 7);
            for (int n = 0; n < 7; n++)
            {
                var actual = catalog.Audiences[n]; var row = expected.Audiences[n];
                Check(actual.SourceIndex == row.SourceIndex && actual.Name == row.Name &&
                    actual.Description == row.Description && actual.DefinitionAddress == row.DefinitionAddress &&
                    actual.RawMembers.OrderBy(x => x.Key).SequenceEqual(row.RawMembers.OrderBy(x => x.Key)));
            }
            Check(catalog.DevelopmentComputers.Count == 4 && expected.DevelopmentComputers.Count == 4);
            for (int n = 0; n < 4; n++)
            {
                var actual = catalog.DevelopmentComputers[n]; var row = expected.DevelopmentComputers[n];
                Check(actual.SourceIndex == row.SourceIndex && actual.Name == row.Name && actual.Description == row.Description &&
                    actual.BitLabel == row.BitLabel && actual.BitWidth == row.BitWidth && actual.PurchasePrice == row.PurchasePrice);
            }
            Check(catalog.Hardware.Count == 11 && expected.Hardware.Count == 11);
            for (int n = 0; n < 11; n++)
            {
                var actual = catalog.Hardware[n]; var row = expected.Hardware[n];
                Check(actual.SourceIndex == row.SourceIndex && actual.Name == row.Name && actual.Description == row.Description &&
                    actual.RawMembers.OrderBy(x=>x.Key).SequenceEqual(row.RawMembers.OrderBy(x=>x.Key)) &&
                    actual.ProductionBands.Count==4 && actual.ProductionBands.SequenceEqual(row.ProductionBands));
            }
            Check(catalog.GameTypes.Count == 51 && expected.GameTypes.Count == 51);
            for (int n = 0; n < 51; n++)
            {
                var actual = catalog.GameTypes[n]; var row = expected.GameTypes[n];
                Check(actual.SourceIndex == row.SourceIndex && actual.Name == row.Name && actual.ExampleTitles.SequenceEqual(row.ExampleTitles) &&
                    actual.RawTable13CC.Count == 15 && actual.RawUnusedArgument48 == row.RawUnusedArgument48 &&
                    actual.RawTable13CC.SequenceEqual(row.RawTable13CC) && actual.RawMembers.OrderBy(x=>x.Key).SequenceEqual(row.RawMembers.OrderBy(x=>x.Key)));
            }
        }
        var scene = OfficeSceneImporter.Extract(File.ReadAllBytes(args[0]));
        Check(scene.Sprites.Count == 128 && scene.Sprites[0] == new OfficeSpriteDefinition(23,148,64,47,226,11,21,20));
        Check(scene.Sprites[121].Id == 144 && scene.Sprites[122].Id == 145);
        // The final initializer selects secretary record 1 (portrait 2),
        // overriding the earlier portrait-1 tile allocation.
        Check(scene.Sprites.Single(x => x.Id == 141) == new OfficeSpriteDefinition(141,101,79,200,32,16,32,26));
        for (int action=6;action<=17;action++)
        {
            var computer=scene.Sprites.Single(x=>x.Id==93+2*(action-6));
            Check(computer.X==-1000 && computer.Y==-1000 && computer.Width==46 && computer.Height==35 && computer.DepthOffset==-200);
        }
        var archivePath = Path.ChangeExtension(args[0], ".lzh");
        if (File.Exists(archivePath))
        {
            var archive = new LhaArchive(archivePath);
            Check(archive.Entries.Count == 48);
            foreach (var entry in archive.Entries) Check(archive.Extract(entry).Length == entry.OriginalSize);
            var executable = archive.Entries.Single(x => x.Name == "GameDev2DX.exe");
            Check(archive.Extract(executable).SequenceEqual(File.ReadAllBytes(args[0])));
        }
    });
}
Console.WriteLine($"{passed} tests passed");
sealed class ConstantRandom(int value) : Random { public override int Next(int maxValue) => value; }
sealed class UncheckedDoubleRandom(double value) : Random { public override double NextDouble() => value; }
sealed class RecallSalesRandom : Random
{
    public List<int> Bounds {get;}=[];
    public int DoubleCalls {get;private set;}
    public override int Next(int maxValue) {Bounds.Add(maxValue);return 1;}
    public override double NextDouble() {DoubleCalls++;return .99;}
}
sealed class AlternatingRandom : Random { private int n; public override int Next(int maxValue) => n++ % 2 == 0 ? 3 : 0; }
sealed class SequenceRandom(params int[] values) : Random
{
    public int Calls { get; private set; }
    public override int Next(int maxValue)
    {
        if (Calls >= values.Length) throw new Exception("Unexpected random draw");
        int value = values[Calls++];
        if (value < 0 || value >= maxValue) throw new Exception("Invalid synthetic random draw");
        return value;
    }
}

sealed class WorkRandom : Random
{
    public override int Next(int maxValue)=>maxValue switch {130=>1,300=>0,50=>1,_=>0};
    public override double NextDouble()=>.99;
}

sealed class DoubleSequenceRandom(params double[] values) : Random
{
    public int Calls { get; private set; }
    public override double NextDouble()
    {
        if (Calls>=values.Length) throw new Exception("Unexpected recall draw");
        double value=values[Calls++];
        if (value<0 || value>=1) throw new Exception("Invalid synthetic recall draw");
        return value;
    }
}

sealed class GreetingRandom(float value) : Random
{
    public int GreetingCalls {get;private set;}
    public override int Next(int maxValue)=>0;
    public override float NextSingle() {GreetingCalls++;return value;}
}
