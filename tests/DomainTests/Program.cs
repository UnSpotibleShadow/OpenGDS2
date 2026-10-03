using OpenGDS2.Domain;
using OpenGDS2.Core;
using OpenGDS2.Persistence;
using OpenGDS2.Import;

int passed = 0;
void Test(string name, Action test)
{
    try { test(); passed++; Console.WriteLine("PASS " + name); }
    catch (Exception ex) { Console.Error.WriteLine($"FAIL {name}: {ex}"); Environment.ExitCode = 1; }
}
void Check(bool condition) { if (!condition) throw new Exception("Assertion failed"); }
void Throws<T>(Action action) where T : Exception
{ try { action(); } catch (T) { return; } throw new Exception("Expected " + typeof(T).Name); }
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
Test("timed technology uses the shared 20 then 19 tick cadence and survives reload",()=>
{
    var (s,e,c,p)=UseFixture();Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[0])==TechnologyApplicationResult.Success);
    for (int tick=0;tick<19;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office!.WorkTicks==20 && e.Office.WorkTier==4 && e.Office.TechnologyApplication!.RemainingUnits==2 && p.DataSize==101.9);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];p=s.DevelopmentProjects[0];
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office!.WorkTicks==2 && e.Office.TechnologyApplication!.RemainingUnits==1);
    for (int tick=0;tick<18;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.WorkTicks==20 && p.DataSize==101.9);
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
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office.WorkTier==4 && e.Office.WorkTicks==2);
    for (int tick=0;tick<38;tick++) DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.TechnologyApplication is null && p.Quality.Fun==contributed && e.Experience==95);
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
    Check(ReviewService.Acknowledge(s,g) && g.HallOfFame && !g.ReviewPending && s.Company.HallOfFameCount==1);
    Check(!ReviewService.Acknowledge(s,g) && s.Company.HallOfFameCount==1);
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
    Check(ReviewService.Acknowledge(s,g) && s.Company.HallOfFameCount is null && g.HallOfFame);
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
    Check(ReviewService.Acknowledge(s,g)==false && SalesMilestoneService.Acknowledge(s,g));
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
Test("business budget resets at two hundred ticks and automatic expansion respects mode thresholds",()=>
{
    foreach (var (mode,funds,expands) in new[]{(GameMode.Normal,299999999L,false),(GameMode.Normal,300000000L,true),
        (GameMode.Beginner,99999999L,false),(GameMode.Beginner,100000000L,true)})
    {
        var s=new GameSession().StartNewGame("Synthetic","Tester",mode);s.Money=funds;s.CompanyActivityTicks=158;
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
Test("market-factor branches preserve first-two penalty and unreachable music tiers",()=>
{
    Check(ReleaseService.MarketFactor(0,new(1000,1000,0,1000),new SequenceRandom(0))==2.4);
    Check(ReleaseService.MarketFactor(1,new(),new SequenceRandom(3))==.4);
    Check(Math.Abs(ReleaseService.MarketFactor(2,new(201,201,0,201),new SequenceRandom(6,0,0,1))-5.1)<1e-12);
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
    var g=s.ReleasedGames.Single();Check(ReviewService.Acknowledge(s,g));long before=s.Money!.Value;
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
        var session=new GameSession(new Random(123));
        var state=session.StartNewGame("Compatibility test","Tester",GameMode.Beginner,db,catalog);
        foreach (var candidate in session.OpenRecruitment().Candidates.ToArray().Take(3))
            Check(session.Hire(candidate)==HireResult.Success);
        Check(state.Employees.Count==3);
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
        Check(ReviewService.Acknowledge(state,state.ReleasedGames.Single()));
        long funds=state.Money!.Value;
        for (int i=0;i<20;i++) session.TickOffice(catalog);
        Check(state.ReleasedGames.Single().UnitsSold>0 && state.Money>funds);
        Check(SaveStore.Deserialize(SaveStore.Serialize(state)).ReleasedGames.Count==1);
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
