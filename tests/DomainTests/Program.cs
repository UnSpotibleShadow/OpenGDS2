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
GameState ProgrammingFixture(int index=5)
{
    var actor=new EmployeeState {Name="Actor",SourceIndex=0,PortraitId=1,Level=1,Experience=20,Stress=10,Inspiration=80,DevelopmentSpeed=50,
        Fun=20,Novelty=20,Graphics=20,Music=20,Accuracy=50,
        Office=new() {SlotIndex=0,Action=6,SeatedAction=6,WorkEngaged=true,MovementState=0,WorkTicks=5,ActionTicks=11}};
    return new() {Company=new() {Name="Fixture",PresidentName="Test"},Money=100,ProgrammingActorPasses=0,CompanyActivityTicks=0,
        Employees=[actor],DevelopmentProjects=[new() {Title="Game",RoomIndex=0,RequiredPoints=100,Progress=5,Focus=new(25,25,25,25)}],
        OfficeActorContinuation=new() {ActorSlot=0,EmployeeSourceIndex=0,RoomIndex=0},
        PendingProgrammingChallenge=new() {Inspiration=80,IntroPage=2,Mode=4,Index=index,Outcome=index==1?0:1,QualityIndex=index==1?null:0,CommentVariant=0}};
}
GameState VirusFixture(bool owned=false)
{
    return new() {Company=new() {Name="Fixture",PresidentName="Test",WeeklyNet=0,MonthlyNet=0,YearlyNet=0},Money=200_000_000,BackupDeviceOwned=owned,
        Employees=[new() {Name="Developer",SourceIndex=0,PortraitId=1,Level=1,Experience=0,Stress=90,
            Office=new() {SlotIndex=0,Action=6,SeatedAction=6,WorkEngaged=true,MovementState=0}}],
        DevelopmentProjects=[new() {Title="Project",RoomIndex=0,RequiredPoints=100,Progress=30,Focus=new(25,25,25,25)}]};
}
Test("English title patch preserves the city and mascots, is deterministic and rejects wrong dimensions",()=>
{
    byte[] source=Enumerable.Range(0,558*385*3).Select(i=>(byte)(i%251)).ToArray();var copy=(byte[])source.Clone();
    var patched=LocalTitleArtworkRules.Patch(source);
    Check(source.SequenceEqual(copy)&&!patched.SequenceEqual(source));
    Check(patched.SequenceEqual(LocalTitleArtworkRules.Patch(source)));
    Check(patched.AsSpan(558*165*3).SequenceEqual(source.AsSpan(558*165*3)));
    for(int y=161;y<165;y++)Check(patched.AsSpan((y*558+269)*3,22*3).SequenceEqual(source.AsSpan((y*558+269)*3,22*3)));
    Throws<InvalidDataException>(()=>LocalTitleArtworkRules.Patch(new byte[1]));
});

Test("original number display retains fixed lower Japanese groups and comma thousands",()=>
{
    Check(OriginalNumberFormatter.Format(9999,NumberGrouping.Japanese)=="9999");
    Check(OriginalNumberFormatter.Format(10000,NumberGrouping.Japanese)=="1万0000");
    Check(OriginalNumberFormatter.Format(100000001,NumberGrouping.Japanese)=="1億0000万0001");
    Check(OriginalNumberFormatter.Format(-1234567890123,NumberGrouping.Japanese)=="-1兆2345億6789万0123");
    Check(OriginalNumberFormatter.Format(123456789,NumberGrouping.Thousands)=="123,456,789");
    Check(OriginalNumberFormatter.Format(long.MinValue,NumberGrouping.Thousands)=="-9,223,372,036,854,775,808");
    Throws<ArgumentException>(()=>OriginalNumberFormatter.FormatDigits("1.5",NumberGrouping.Japanese));
});
Test("native office hover areas use strict transformed bounds",()=>
{
    Check(OfficeInteractionRules.TrainingAreaAt(220,60)&&!OfficeInteractionRules.TrainingAreaAt(220,82));
    Check(OfficeInteractionRules.VendingAt(362,130)&&!OfficeInteractionRules.VendingAt(362,121));
    Check(!OfficeInteractionRules.TrainingAreaAt(double.NaN,0)&&!OfficeInteractionRules.VendingAt(0,double.PositiveInfinity));
});
GameState InterviewFixture(double recognition=50)
{
    var state=VirusFixture();var project=state.DevelopmentProjects[0];
    project.OriginalStage=2;project.GameTypeSourceIndex=0;project.GenreSourceIndex=0;
    project.Recognition=recognition;project.InterviewCompleted=false;
    return state;
}
Test("interview trigger consumes eager draws on known ineligible and already interviewed rooms",()=>
{
    foreach(int stage in new[]{0,1,2})
    {
        var s=InterviewFixture(10);var p=s.DevelopmentProjects[0];p.OriginalStage=stage;
        var r=new FailureRandom(.002f);Check(MagazineInterviewService.Periodic(s,p,r)&&r.Calls==1&&s.PendingMagazineInterview is null);
    }
    var state=InterviewFixture();state.DevelopmentProjects[0].InterviewCompleted=true;
    var random=new FailureRandom(.0016f);Check(MagazineInterviewService.Periodic(state,state.DevelopmentProjects[0],random)&&random.Calls==1);
    state.DevelopmentProjects[0].InterviewCompleted=null;
    Check(MagazineInterviewService.Periodic(state,state.DevelopmentProjects[0],new FailureRandom()));
});
Test("interview source portrait precedence and reporter name draws are saved after introduction",()=>
{
    var s=InterviewFixture();var random=new FailureRandom(.0016f,.99f,.85f);
    Check(!MagazineInterviewService.Periodic(s,s.DevelopmentProjects[0],random));
    Check(s.PendingMagazineInterview!.Page==0&&s.DevelopmentProjects[0].InterviewCompleted==true&&random.Calls==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.PendingMagazineInterview!.PortraitId is null);
    Check(MagazineInterviewService.Acknowledge(s,random));
    Check(s.PendingMagazineInterview!.PortraitId==1&&s.PendingMagazineInterview.ReporterNameIndex==4&&random.Calls==3);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.PendingMagazineInterview!.ReporterNameIndex==4);
});
Test("interview applies the capped awareness gain once after all four reporter pages",()=>
{
    foreach(double before in new[]{50d,90d})
    {
        var s=InterviewFixture(before);float trigger=before==50?.0016f:.008f;
        Check(!MagazineInterviewService.Periodic(s,s.DevelopmentProjects[0],new FailureRandom(trigger)));
        MagazineInterviewService.Acknowledge(s,new FailureRandom(.4f,.2f));
        for(int page=1;page<=4;page++)
        {
            Check(s.PendingMagazineInterview!.Page==page&&s.DevelopmentProjects[0].Recognition==before);
            s=SaveStore.Deserialize(SaveStore.Serialize(s));MagazineInterviewService.Acknowledge(s,new FailureRandom());
        }
        double after=Math.Min(before+20,100);Check(s.DevelopmentProjects[0].Recognition==after&&s.PendingMagazineInterview!.Page==5);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.DevelopmentProjects[0].Recognition==after);
        MagazineInterviewService.Acknowledge(s,new FailureRandom());Check(s.PendingMagazineInterview is null&&s.DevelopmentProjects[0].Recognition==after);
        Check(!MagazineInterviewService.Acknowledge(s,new FailureRandom()));
    }
});
Test("company pass resumes at the next interview room without repeating its trigger",()=>
{
    var s=InterviewFixture();s.CompanyEventContinuation=new() {PoachingProcessed=true};
    var other=InterviewFixture().DevelopmentProjects[0];other.RoomIndex=1;s.DevelopmentProjects.Add(other);
    var r=new FailureRandom(.0016f,.6f,.2f,.99f);Check(!CompanyEventService.Resume(s,r));
    Check(s.CompanyEventContinuation!.NextInterviewRoom==1&&r.Calls==1);
    while(s.PendingMagazineInterview is not null)MagazineInterviewService.Acknowledge(s,r);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CompanyEventService.Resume(s,r)&&r.Calls==4&&s.CompanyEventContinuation is null);
    Check(s.DevelopmentProjects[0].Recognition==70&&s.DevelopmentProjects[1].Recognition==50);
});
Test("interview rejects malformed saved phases, overlapping decisions and altered gains",()=>
{
    var s=InterviewFixture();MagazineInterviewService.Periodic(s,s.DevelopmentProjects[0],new FailureRandom(.0016f));
    s.PendingMagazineInterview!.Page=5;s.PendingMagazineInterview.Gain=999;
    Throws<InvalidDataException>(()=>SaveStore.Deserialize(SaveStore.Serialize(s)));
    s=InterviewFixture();MagazineInterviewService.Periodic(s,s.DevelopmentProjects[0],new FailureRandom(.0016f));
    s.PendingBackupDevice=new() {Page=0};Throws<InvalidDataException>(()=>SaveStore.Deserialize(SaveStore.Serialize(s)));
    s.PendingBackupDevice=null;s.DevelopmentProjects[0].OriginalStage=3;s.DevelopmentProjects[0].Progress=100;
    Check(ProjectCancellationService.Check(s,s.DevelopmentProjects[0])==ProjectCancellationResult.DecisionPending);
    Check(ReleaseService.TryRelease(s,s.DevelopmentProjects[0],new OriginalCatalog(),"Blocked",5800,100,new FailureRandom())==ReleaseResult.NotReady);
    s.PendingProjectCancellation=new() {RoomIndex=0};Throws<InvalidDataException>(()=>SaveStore.Deserialize(SaveStore.Serialize(s)));
});
Test("stable user data migration copies imports preferences saves and backups without moving originals",()=>
{
    string parent=Path.Combine(Path.GetTempPath(),"opengds-migrate-"+Guid.NewGuid().ToString("N"));
    try
    {
        string old=Path.Combine(parent,UserDataMigration.LegacyDirectoryName),current=Path.Combine(parent,UserDataMigration.DirectoryName);
        var data=new Dictionary<string,string>{{"interface.cfg","language=ja"},{"audio.cfg","muted=true"},{"rankings.json","authored rankings"},
            {"employee-name-readings.json","authored readings"},{"saves/slot1.json","current save"},{"saves/slot1.json.bak","previous save"},
            {"original/Graphic/nested/art.txt","imported fixture"},{"music/local.ogg","authored music bytes"},{"asset-overrides/Graphic/patch.txt","authored override"}};
        foreach(var (name,text) in data){string path=Path.Combine(old,name);Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,text);}
        Directory.CreateDirectory(Path.Combine(old,"logs"));File.WriteAllText(Path.Combine(old,"logs/godot.log"),"old running log");
        var r=UserDataMigration.Run(current);Check(r.Succeeded&&r.CopiedFiles==data.Count&&File.Exists(Path.Combine(current,UserDataMigration.CompletionFile)));
        foreach(var (name,text) in data)Check(File.ReadAllText(Path.Combine(current,name))==text&&File.ReadAllText(Path.Combine(old,name))==text);
        Check(!Directory.Exists(Path.Combine(current,"logs")));
        File.Delete(Path.Combine(current,"saves/slot1.json"));
        Check(UserDataMigration.Run(current).Succeeded&&!File.Exists(Path.Combine(current,"saves/slot1.json")));
        Check(!Directory.EnumerateFiles(current,".migration-*.tmp",SearchOption.AllDirectories).Any());
    }
    finally{if(Directory.Exists(parent))Directory.Delete(parent,true);}
});
Test("migration retains destination save families and resumes identical partially copied saves",()=>
{
    string parent=Path.Combine(Path.GetTempPath(),"opengds-migrate-"+Guid.NewGuid().ToString("N"));
    try
    {
        string old=Path.Combine(parent,UserDataMigration.LegacyDirectoryName),current=Path.Combine(parent,UserDataMigration.DirectoryName);
        Directory.CreateDirectory(Path.Combine(old,"saves"));Directory.CreateDirectory(Path.Combine(current,"saves"));
        for(int i=1;i<=3;i++){File.WriteAllText(Path.Combine(old,$"saves/slot{i}.json"),"old "+i);File.WriteAllText(Path.Combine(old,$"saves/slot{i}.json.bak"),"backup "+i);}
        File.WriteAllText(Path.Combine(old,"interface.cfg"),"old language");File.WriteAllText(Path.Combine(current,"interface.cfg"),"new language");
        File.WriteAllText(Path.Combine(current,"saves/slot1.json"),"existing current");
        File.WriteAllText(Path.Combine(current,"saves/slot2.json.bak"),"existing backup");
        File.WriteAllText(Path.Combine(current,"saves/slot3.json"),"old 3");
        var r=UserDataMigration.Run(current);Check(r.Succeeded);
        Check(File.ReadAllText(Path.Combine(current,"interface.cfg"))=="new language");
        Check(File.ReadAllText(Path.Combine(current,"saves/slot1.json"))=="existing current"&&!File.Exists(Path.Combine(current,"saves/slot1.json.bak")));
        Check(File.ReadAllText(Path.Combine(current,"saves/slot2.json.bak"))=="existing backup"&&!File.Exists(Path.Combine(current,"saves/slot2.json")));
        Check(File.ReadAllText(Path.Combine(current,"saves/slot3.json.bak"))=="backup 3");
    }
    finally{if(Directory.Exists(parent))Directory.Delete(parent,true);}
});
Test("failed data migration is retryable and never marks a partial copy complete",()=>
{
    string parent=Path.Combine(Path.GetTempPath(),"opengds-migrate-"+Guid.NewGuid().ToString("N"));
    try
    {
        string old=Path.Combine(parent,UserDataMigration.LegacyDirectoryName),current=Path.Combine(parent,UserDataMigration.DirectoryName);
        Directory.CreateDirectory(Path.Combine(old,"original"));Directory.CreateDirectory(current);
        File.WriteAllText(Path.Combine(old,"interface.cfg"),"original preference");File.WriteAllText(Path.Combine(old,"original/import.txt"),"import bytes");
        File.WriteAllText(Path.Combine(current,"original"),"destination obstacle");
        Check(!UserDataMigration.Run(current).Succeeded&&!File.Exists(Path.Combine(current,UserDataMigration.CompletionFile)));
        Check(File.ReadAllText(Path.Combine(current,"original"))=="destination obstacle");
        File.Delete(Path.Combine(current,"original"));File.WriteAllText(Path.Combine(current,"interface.cfg"),"preserve partial destination");
        Check(UserDataMigration.Run(current).Succeeded);
        Check(File.ReadAllText(Path.Combine(current,"original/import.txt"))=="import bytes"&&File.ReadAllText(Path.Combine(current,"interface.cfg"))=="preserve partial destination");
    }
    finally{if(Directory.Exists(parent))Directory.Delete(parent,true);}
});
Test("fresh profiles and custom data roots do not repeatedly ingest legacy data",()=>
{
    string parent=Path.Combine(Path.GetTempPath(),"opengds-migrate-"+Guid.NewGuid().ToString("N"));
    try
    {
        string current=Path.Combine(parent,UserDataMigration.DirectoryName),old=Path.Combine(parent,UserDataMigration.LegacyDirectoryName);
        Check(UserDataMigration.Run(current).Succeeded);Directory.CreateDirectory(old);File.WriteAllText(Path.Combine(old,"interface.cfg"),"late old settings");
        Check(UserDataMigration.Run(current).Succeeded&&!File.Exists(Path.Combine(current,"interface.cfg")));
        string custom=Path.Combine(parent,"CustomProfile");Check(UserDataMigration.Run(custom).Succeeded&&!Directory.Exists(custom));
        File.WriteAllText(Path.Combine(current,UserDataMigration.CompletionFile),"damaged marker");Check(!UserDataMigration.Run(current).Succeeded);
    }
    finally{if(Directory.Exists(parent))Directory.Delete(parent,true);}
});
if(!OperatingSystem.IsWindows())Test("automatic migration refuses symbolic links and preserves their targets",()=>
{
    string parent=Path.Combine(Path.GetTempPath(),"opengds-migrate-"+Guid.NewGuid().ToString("N"));
    try
    {
        string old=Path.Combine(parent,UserDataMigration.LegacyDirectoryName),current=Path.Combine(parent,UserDataMigration.DirectoryName);
        Directory.CreateDirectory(old);string target=Path.Combine(parent,"outside.txt");File.WriteAllText(target,"must remain intact");
        File.CreateSymbolicLink(Path.Combine(old,"interface.cfg"),target);
        Check(!UserDataMigration.Run(current).Succeeded&&!File.Exists(Path.Combine(current,"interface.cfg")));
        Check(File.ReadAllText(target)=="must remain intact");
    }
    finally{if(Directory.Exists(parent))Directory.Delete(parent,true);}
});
Test("game-title aliases preserve player names and require a matching imported rival",()=>
{
    var rival=new RivalProduct {Title="Authored rival",Owner=2};
    var catalog=new OriginalCatalog {SourceHash=EmployeeDatabaseImporter.ExpectedHash,InitialRivalProducts=[rival.Copy()],
        StagedRivalProducts=[new() {Product=new() {Title="Authored sequel",Owner=2}}],RivalCompanies=Enumerable.Range(0,10).Select(i=>$"Authored publisher {i}").ToList()};
    string Translate(string key)=>key switch
    {"CATALOG_RIVAL_INITIAL_TITLE_0"=>"English rival","CATALOG_RIVAL_STAGED_TITLE_0"=>"English sequel","CATALOG_RIVAL_COMPANY_2"=>"English publisher",_=>key};
    Check(GameNameLocalization.RivalTitle(rival,catalog,true,Translate)=="English rival");
    var saved=rival.Copy();saved.Demand=0;saved.UnitsSold=987654;
    Check(GameNameLocalization.RivalTitle(saved,catalog,true,Translate)=="English rival");
    Check(GameNameLocalization.RivalTitle(catalog.StagedRivalProducts[0].Product,catalog,true,Translate)=="English sequel");
    Check(GameNameLocalization.RivalTitle(rival,catalog,false,Translate)==rival.Title);
    Check(GameNameLocalization.RivalTitle(rival,catalog,true,key=>key)==rival.Title);
    Check(GameNameLocalization.RivalTitle(rival,catalog,true,_=>" ")==rival.Title);
    var own=rival.Copy();own.Owner=-1;own.OwnProduct=0;
    Check(GameNameLocalization.RivalTitle(own,catalog,true,Translate)==rival.Title);
    saved.Owner=3;Check(GameNameLocalization.RivalTitle(saved,catalog,true,Translate)==rival.Title);
    saved.Owner=2;saved.Title="Custom title";Check(GameNameLocalization.RivalTitle(saved,catalog,true,Translate)==saved.Title);
    Check(GameNameLocalization.RivalCompany(2,catalog,true,Translate,"Unknown")=="English publisher");
    Check(GameNameLocalization.RivalCompany(2,catalog,false,Translate,"Unknown")==catalog.RivalCompanies[2]);
    Check(GameNameLocalization.RivalCompany(99,catalog,true,Translate,"Unknown")=="Unknown");
    catalog.SourceHash="other";
    Check(GameNameLocalization.RivalTitle(rival,catalog,true,Translate)==rival.Title);
    Check(GameNameLocalization.RivalCompany(2,catalog,true,Translate,"Unknown")==catalog.RivalCompanies[2]);
    Check(GameNameLocalization.RivalTitle(rival,null,true,Translate)==rival.Title);
    Check(rival.Title=="Authored rival"&&catalog.InitialRivalProducts[0].Title==rival.Title);
});
Test("example-title aliases keep unknown, Japanese and custom catalog text",()=>
{
    var type=new GameTypeDefinition {SourceIndex=7,ExampleTitles=["Authored game","Authored game two","Authored magazine"]};
    var catalog=new OriginalCatalog {SourceHash=EmployeeDatabaseImporter.ExpectedHash,GameTypes=Enumerable.Range(0,51).Select(i=>new GameTypeDefinition {SourceIndex=i}).ToList()};
    string Translate(string key)=>key=="CATALOG_TYPE_EXAMPLE_7_0"?"English game":key;
    Check(GameNameLocalization.Examples(type,catalog,true,Translate).SequenceEqual(new[]{"English game","Authored game two","Authored magazine"}));
    Check(GameNameLocalization.Examples(type,catalog,false,Translate).SequenceEqual(type.ExampleTitles));
    catalog.GameTypes.Clear();Check(GameNameLocalization.Examples(type,catalog,true,Translate).SequenceEqual(type.ExampleTitles));
    Check(type.ExampleTitles[0]=="Authored game");
});
Test("secretary badge English patch preserves the face and all mouth frames",()=>
{
    byte[] source=Enumerable.Range(0,64*64*3).Select(i=>(byte)(i%251)).ToArray();var copy=(byte[])source.Clone();
    var patch=LocalSceneCaptionRules.PatchSecretary(source);Check(source.SequenceEqual(copy)&&!patch.SequenceEqual(copy));
    for(int y=0;y<64;y++)for(int x=0;x<64;x++)if(y<52||y>=62||x<18||x>=48)
        for(int c=0;c<3;c++)Check(patch[(y*64+x)*3+c]==copy[(y*64+x)*3+c]);
});
Test("television English patch stays inside the original CRT glass",()=>
{
    byte[] pixels=Enumerable.Range(0,256*152*3).Select(i=>(byte)(i%251)).ToArray();var copy=(byte[])pixels.Clone();
    var patch=LocalSceneCaptionRules.PatchTelevision(pixels);Check(pixels.SequenceEqual(copy)&&!patch.SequenceEqual(copy));
    // Independent half-plane bounds from the original glass corners, one pixel
    // outside the authored inset. This catches the old paint below the CRT.
    (int X,int Y)[] glass=[(84,31),(178,38),(174,94),(87,85)];
    for(int y=0;y<152;y++)for(int x=0;x<256;x++)
    {
        bool inside=true;
        for(int edge=0;edge<4;edge++)
        {
            var a=glass[edge];var b=glass[(edge+1)%4];
            if((b.X-a.X)*(y-a.Y)-(b.Y-a.Y)*(x-a.X)<0)inside=false;
        }
        if(!inside)for(int c=0;c<3;c++)Check(patch[(y*256+x)*3+c]==copy[(y*256+x)*3+c]);
    }
    foreach(var (x,y) in new[]{(85,100),(120,105),(170,102),(84,50),(177,75),(130,95)})
        for(int c=0;c<3;c++)Check(patch[(y*256+x)*3+c]==copy[(y*256+x)*3+c]);
    Throws<InvalidDataException>(()=>LocalSceneCaptionRules.PatchTelevision(new byte[256*160*3]));
});
GameState PoachingFixture(bool defensive=false)
{
    var s=VirusFixture();s.DevelopmentProjects.Clear();s.Employees.Clear();
    var employee=new EmployeeState {SourceIndex=49,Name="Rival developer",PortraitId=1,ContractFee=10_000_000,
        Level=1,Experience=0,Stress=10,SpecialFlag=1,TechnologyLevels=new(),OriginalAttributes=[50,50,50,50,50,50,50],
        Fun=50,Novelty=50,Graphics=50,Music=50,Accuracy=50,DevelopmentSpeed=50,Inspiration=50};
    s.EmployeeSources=[new() {SourceIndex=49,GateValue=defensive?1:0,Employee=employee}];
    if(defensive){employee.Office=new() {SlotIndex=0,Action=0,MovementState=0};s.Employees.Add(employee);}
    return s;
}
void StartPoaching(GameState s)
{
    Check(PoachingService.Begin(s,49));Check(PoachingService.Acknowledge(s,new FailureRandom()));
    Check(PoachingService.Confirm(s));
}
Test("poaching candidate filter and confirmation never hire or charge on cancel",()=>
{
    var s=PoachingFixture();Check(PoachingService.Candidates(s).Single().SourceIndex==49);
    Check(PoachingService.Begin(s,49));Check(!BackupDeviceService.Begin(s));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(PoachingService.Cancel(s) && s.Employees.Count==0 && s.Money==200_000_000);
    s.EmployeeSources[0].GateValue=1;Check(!PoachingService.Begin(s,49));
});
Test("poaching donations debit every accumulator while contract raises are free until outcome",()=>
{
    var s=PoachingFixture();StartPoaching(s);
    Check(PoachingService.Donate(s,5_000_000)==PoachingActionResult.Success && s.Money==195_000_000 && s.Company.WeeklyNet==-5_000_000);
    Check(PoachingService.RaiseOffer(s,100_000_000)==PoachingActionResult.Success && s.Money==195_000_000 && s.PendingPoaching!.PlayerOffer==110_000_000);
    Check(PoachingService.Donate(s,1)==PoachingActionResult.InvalidAmount);
    s.Money=1;Check(PoachingService.Donate(s,5_000_000)==PoachingActionResult.InsufficientFunds && s.PendingPoaching.PlayerDonations==5_000_000);
    s.Money=195_000_000;Check(PoachingService.Cancel(s) && s.Money==195_000_000 && s.Employees.Count==0);
});
Test("poaching donation acknowledgment survives reload before its one-time debit",()=>
{
    var s=PoachingFixture();StartPoaching(s);Check(PoachingService.QueueDonation(s,5_000_000)==PoachingActionResult.Success);
    Check(s.Money==200_000_000 && s.PendingPoaching!.PlayerDonations==0 && !PoachingService.Tick(s,new FailureRandom()));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(PoachingService.Acknowledge(s,new FailureRandom()));
    Check(s.Money==195_000_000 && s.PendingPoaching!.PlayerDonations==5_000_000 && s.PendingPoaching.PendingDonation is null);
    Check(!PoachingService.Acknowledge(s,new FailureRandom()) && s.Money==195_000_000);
});
Test("poaching timed offers retain all four draws across save load and conditional final response",()=>
{
    var s=PoachingFixture();StartPoaching(s);PoachingService.RaiseOffer(s,100_000_000);
    var first=new FailureRandom(0,.34f);for(int i=0;i<=80;i++)PoachingService.Tick(s,first);
    Check(first.Calls==2 && s.PendingPoaching!.RivalDonations==1_000_000 && s.PendingPoaching.RivalOffer==30_000_000);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));var last=new FailureRandom(.99f,.34f);
    for(int i=81;i<=200;i++)PoachingService.Tick(s,last);
    Check(last.Calls==2 && s.PendingPoaching!.RivalDonations==16_000_000 && s.PendingPoaching.RivalOffer==125_000_000 && s.PendingPoaching.TimerCallbacks==201);
    Check(s.PendingPoaching.Phase==PoachingPhase.DecisionIntroduction && !PoachingService.Tick(s,new FailureRandom()));
    var noResponse=PoachingFixture();StartPoaching(noResponse);var three=new FailureRandom(0,0,0);
    for(int i=0;i<=200;i++)PoachingService.Tick(noResponse,three);
    Check(three.Calls==3 && noResponse.PendingPoaching!.RivalOffer==20_000_000);
});
Test("winning poach charges the previous contract fee once after the final secretary result",()=>
{
    var s=PoachingFixture();StartPoaching(s);PoachingService.Donate(s,50_000_000);PoachingService.RaiseOffer(s,100_000_000);
    var roll=new FailureRandom(0,.34f,.99f,.34f);for(int i=0;i<=200;i++)PoachingService.Tick(s,roll);
    Check(PoachingService.Acknowledge(s,new FailureRandom()) && s.PendingPoaching!.PlayerWins==true);
    Check(PoachingService.Acknowledge(s,new FailureRandom()) && PoachingService.Acknowledge(s,new FailureRandom()));
    Check(s.Employees.Count==0 && s.Money==150_000_000);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(PoachingService.Acknowledge(s,new Random(1)));
    Check(s.Employees.Single().ContractFee==110_000_000 && s.Money==140_000_000 && s.EmployeeSources[0].GateValue==1 && s.PendingPoaching is null);
    Check(!PoachingService.Acknowledge(s,new FailureRandom()) && s.Money==140_000_000);
});
Test("negotiation ties favour the player and defensive retention raises only future renewal fee",()=>
{
    var s=PoachingFixture(true);s.PoachingOpportunityTicks=30000;
    Check(!PoachingService.Periodic(s,new FailureRandom(.00015f,0)) && s.OriginalPayrollScratchIndex==1);
    PoachingService.Acknowledge(s,new FailureRandom());PoachingService.Acknowledge(s,new FailureRandom());
    PoachingService.RaiseOffer(s,10_000_000);var scene=s.PendingPoaching!;scene.RivalOffer=scene.PlayerOffer;scene.TimerCallbacks=200;
    PoachingService.Tick(s,new FailureRandom());PoachingService.Acknowledge(s,new FailureRandom());Check(scene.PlayerWins==true);
    while(PoachingService.Acknowledge(s,new Random(2))){}
    Check(s.Money==200_000_000 && s.Employees.Single().ContractFee==20_000_000 && s.EmployeeSources[0].Employee.ContractFee==20_000_000);
});
Test("losing defensive negotiation dismisses after its result and keeps paid donations spent",()=>
{
    var s=PoachingFixture(true);s.PoachingOpportunityTicks=30000;PoachingService.Periodic(s,new FailureRandom(.00015f,0));
    PoachingService.Acknowledge(s,new FailureRandom());PoachingService.Acknowledge(s,new FailureRandom());PoachingService.Donate(s,5_000_000);
    s.PendingPoaching!.RivalOffer=100_000_000;s.PendingPoaching.TimerCallbacks=200;PoachingService.Tick(s,new FailureRandom());
    for(int i=0;i<3;i++)PoachingService.Acknowledge(s,new FailureRandom());
    Check(s.Employees.Count==1 && s.EmployeeSources[0].GateValue==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));PoachingService.Acknowledge(s,new FailureRandom());
    Check(s.Employees.Count==0 && s.EmployeeSources[0].GateValue==0 && s.EmployeeSources[0].Employee.Stress==0 && s.Money==195_000_000);
});
Test("periodic poaching uses eager opportunity draw and resumes before virus without rerolling",()=>
{
    var s=PoachingFixture(true);s.PoachingOpportunityTicks=29999;var below=new FailureRandom(.00015f);
    Check(CompanyEventService.Resume(s,below) && below.Calls==1 && s.PoachingOpportunityTicks==30000);
    var eligible=new FailureRandom(.00015f,0);Check(!CompanyEventService.Resume(s,eligible) && eligible.Calls==2 && s.CompanyEventContinuation!.PoachingProcessed);
    PoachingService.Acknowledge(s,new FailureRandom());PoachingService.Acknowledge(s,new FailureRandom());Check(PoachingService.Cancel(s));
    Check(CompanyEventService.Resume(s,new FailureRandom()) && s.PoachingOpportunityTicks==0 && s.CompanyEventContinuation is null);
    s.Employees.Clear();s.EmployeeSources.Clear();s.PoachingOpportunityTicks=30000;
    Check(CompanyEventService.Resume(s,new FailureRandom(.00015f,.9f)) && s.PoachingEmptyRosterObserved && s.PendingPoaching is null);
});
Test("poaching overflow and malformed saved outcomes cannot partially mutate a transaction",()=>
{
    var s=PoachingFixture();StartPoaching(s);s.Company.MonthlyNet=long.MinValue;
    Throws<OverflowException>(()=>PoachingService.Donate(s,5_000_000));Check(s.Money==200_000_000 && s.PendingPoaching!.PlayerDonations==0);
    s.Company.MonthlyNet=0;s.PendingPoaching!.PlayerWins=true;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingPoaching.PlayerWins=null;s.PendingPoaching.RivalOffer=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});

Test("backup purchase charges once, updates every financial period and resumes its saved receipt",()=>
{
    var s=VirusFixture();Check(BackupDeviceService.Begin(s));Check(BackupDeviceService.Acknowledge(s));
    Check(BackupDeviceService.Confirm(s)==BackupPurchaseResult.Success && s.BackupDeviceOwned && s.Money==100_000_000);
    Check(s.Company.WeeklyNet==-100_000_000 && s.Company.MonthlyNet==-100_000_000 && s.Company.YearlyNet==-100_000_000);
    var restored=SaveStore.Deserialize(SaveStore.Serialize(s));Check(restored.BackupDeviceOwned && restored.PendingBackupDevice?.Result==BackupPurchaseResult.Success);
    Check(BackupDeviceService.Confirm(restored) is null && restored.Money==100_000_000);
    Check(BackupDeviceService.Acknowledge(restored) && BackupDeviceService.Begin(restored));
    Check(restored.PendingBackupDevice?.Result==BackupPurchaseResult.AlreadyOwned && restored.Money==100_000_000);
});
Test("backup cancellation and insufficient funds do not charge or grant ownership",()=>
{
    var s=VirusFixture();Check(BackupDeviceService.Begin(s) && BackupDeviceService.Acknowledge(s) && BackupDeviceService.Cancel(s));
    Check(!s.BackupDeviceOwned && s.Money==200_000_000 && s.PendingBackupDevice is null);
    s.Money=BackupDeviceService.Price-1;Check(BackupDeviceService.Begin(s) && BackupDeviceService.Acknowledge(s));
    Check(BackupDeviceService.Confirm(s)==BackupPurchaseResult.InsufficientFunds && !s.BackupDeviceOwned && s.Money==BackupDeviceService.Price-1);
});
Test("backup purchase prepares overflow and modal guards before mutation",()=>
{
    var s=VirusFixture();s.Company.WeeklyNet=long.MinValue;
    Check(BackupDeviceService.Begin(s) && FacilityService.HasPendingDecision(s));Check(!BackupDeviceService.Begin(s));BackupDeviceService.Acknowledge(s);
    Throws<OverflowException>(()=>BackupDeviceService.Confirm(s));Check(!s.BackupDeviceOwned && s.Money==200_000_000 && s.PendingBackupDevice?.Page==1);
});
Test("unprotected virus applies stress only after each reaction and discards only after final acknowledgment",()=>
{
    var s=VirusFixture();var random=new FailureRandom(.2f);CompanyEventService.BeginVirus(s,s.DevelopmentProjects[0]);
    Check(s.Employees[0].Stress==90 && s.DevelopmentProjects.Count==1 && random.Calls==0);
    Check(CompanyEventService.AcknowledgeVirus(s,random) && s.PendingVirusEvent?.Page==2 && random.Calls==1 && s.Employees[0].Stress==90);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CompanyEventService.AcknowledgeVirus(s,random) && s.Employees[0].Stress==99 && s.PendingVirusEvent?.Page==3);
    Check(s.DevelopmentProjects.Count==1);CompanyEventService.AcknowledgeVirus(s,random);Check(s.PendingVirusEvent?.Page==4 && s.DevelopmentProjects.Count==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CompanyEventService.AcknowledgeVirus(s,random) && s.DevelopmentProjects.Count==0 && s.PendingVirusEvent is null);
    Check(!CompanyEventService.AcknowledgeVirus(s,random) && s.Employees[0].Stress==99 && random.Calls==1);
});
Test("owned backup preserves project and stress throughout virus dialogue and save reload",()=>
{
    var s=VirusFixture(true);s.Employees[0].Stress=100;var random=new FailureRandom(.9f);CompanyEventService.BeginVirus(s,s.DevelopmentProjects[0]);
    Check(CompanyEventService.AcknowledgeVirus(s,random) && s.PendingVirusEvent?.Page==1 && random.Calls==0);
    Check(CompanyEventService.AcknowledgeVirus(s,random) && s.PendingVirusEvent?.Page==2 && random.Calls==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));CompanyEventService.AcknowledgeVirus(s,random);
    Check(s.PendingVirusEvent is null && s.Employees[0].Stress==100 && s.DevelopmentProjects.Count==1 && s.BackupDeviceOwned);
});
Test("virus eligibility consumes eager room draws after five releases and resumes at the next room",()=>
{
    var s=VirusFixture();var firstRoll=new FailureRandom(.5f);Check(CompanyEventService.Resume(s,firstRoll) && firstRoll.Calls==1);
    s.ReleasedGames=Enumerable.Range(0,5).Select(i=>new ReleasedGameState {ProductIndex=i,Title="Game"+i}).ToList();
    var trigger=new FailureRandom(.5f,.000067f);Check(!CompanyEventService.Resume(s,trigger) && trigger.Calls==2 && s.CompanyEventContinuation?.NextVirusRoom==1);
    var reaction=new FailureRandom(.1f);while(CompanyEventService.AcknowledgeVirus(s,reaction)){}
    var remaining=new FailureRandom(.5f);Check(CompanyEventService.Resume(s,remaining) && remaining.Calls==1 && s.CompanyEventContinuation is null);
    var emptyRooms=new FailureRandom(.5f,.2f,.3f);Check(CompanyEventService.Resume(s,emptyRooms) && emptyRooms.Calls==3);
});
Test("backup equipment uses the original hidden tile and retained two-frame animation",()=>
{
    var s=VirusFixture();var tile=new OfficeSpriteDefinition(142,-100,79,379,357,24,32,26);
    Check(CompanyEventService.ProjectBackup(tile,s)==tile);s.BackupDeviceOwned=true;
    var rolls=new FailureRandom(Enumerable.Repeat(.5f,29).ToArray());for(int i=0;i<19;i++)CompanyEventService.Resume(s,rolls);
    var visible=CompanyEventService.ProjectBackup(tile,s);Check(visible.X==18 && visible.Y==109 && visible.SourceX==403 && s.BackupDeviceAnimationTicks==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));for(int i=0;i<10;i++)CompanyEventService.Resume(s,rolls);
    Check(CompanyEventService.ProjectBackup(tile,s).SourceX==379 && rolls.Calls==29);
});
Test("malformed virus and backup continuation states are rejected before loading",()=>
{
    var s=VirusFixture();CompanyEventService.BeginVirus(s,s.DevelopmentProjects[0]);s.PendingVirusEvent!.Protected=true;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.PendingVirusEvent.Protected=false;s.PendingVirusEvent.Page=2;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.PendingVirusEvent=null;s.BackupDeviceAnimationTicks=19;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.BackupDeviceAnimationTicks=0;s.PendingBackupDevice=new() {Page=2};
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});

Test("English office date units preserve the blue frame and every pixel outside their labels",()=>
{
    byte[] source=Enumerable.Range(0,466*290*3).Select(i=>(byte)(i%251)).ToArray();var original=(byte[])source.Clone();
    var patched=LocalBitmapLetteringRules.PatchOfficeDateLabels(source,466,290);
    Check(source.SequenceEqual(original) && !source.SequenceEqual(patched));
    for(int y=0;y<290;y++)for(int x=0;x<466;x++)
        if(!LocalBitmapLetteringRules.OfficeDateLabelRegions.Any(r=>x>=r.X && x<r.X+r.Width && y>=r.Y && y<r.Y+r.Height))
            Check(source.AsSpan((y*466+x)*3,3).SequenceEqual(patched.AsSpan((y*466+x)*3,3)));
    Throws<InvalidDataException>(()=>LocalBitmapLetteringRules.PatchOfficeDateLabels(source,465,290));
});
Test("English Help covers all five original pages and 91 caption blocks",()=>
{
    int[] counts=[16,34,20,2,19];Check(counts.Sum()==OriginalHelpEnglish.BlockCount);
    for(int topic=0;topic<5;topic++){var blocks=OriginalHelpEnglish.Page(topic);Check(blocks.Count==counts[topic] && blocks.All(x=>!string.IsNullOrWhiteSpace(x)));}
    Check(OriginalHelpEnglish.Page(0).Any(x=>x.Contains("experience points")));
    Check(OriginalHelpEnglish.Page(1).Any(x=>x.Contains("poach")));
    Check(OriginalHelpEnglish.Page(4).Any(x=>x.Contains("Hall of Fame")));
    Check(OriginalHelpEnglish.OriginalWebsite=="http://www.bj.wakwak.com/~gamedev2/");
});
Test("company activity English captions preserve arrows and every pixel outside the three labels",()=>
{
    byte[] source=Enumerable.Range(0,256*160*3).Select(i=>(byte)(i%251)).ToArray();var original=(byte[])source.Clone();
    var patched=LocalSceneCaptionRules.PatchActivity(source);
    Check(source.SequenceEqual(original) && !source.SequenceEqual(patched));
    for(int y=0;y<160;y++)for(int x=0;x<256;x++)
        if(!LocalSceneCaptionRules.ActivityRegions.Any(r=>x>=r.X && x<r.X+r.Width && y>=r.Y && y<r.Y+r.Height))
            Check(source.AsSpan((y*256+x)*3,3).SequenceEqual(patched.AsSpan((y*256+x)*3,3)));
    Throws<InvalidDataException>(()=>LocalSceneCaptionRules.PatchActivity(new byte[8]));
});
Test("local scene captions change only their declared area and preserve source pixels",()=>
{
    byte[] source=Enumerable.Range(0,256*160*3).Select(i=>(byte)(i%251)).ToArray();byte[] original=(byte[])source.Clone();
    foreach(bool hall in new[]{true,false})
    {
        byte[] patched=LocalSceneCaptionRules.Patch(source,hall);var r=hall?LocalSceneCaptionRules.HallRegion:LocalSceneCaptionRules.ExhibitionRegion;
        Check(source.SequenceEqual(original) && !source.SequenceEqual(patched));
        Check(patched.SequenceEqual(LocalSceneCaptionRules.Patch(source,hall)));
        for(int y=0;y<160;y++)for(int x=0;x<256;x++)if(x<r.X||x>=r.X+r.Width||y<r.Y||y>=r.Y+r.Height)
        {int i=(y*256+x)*3;Check(source.AsSpan(i,3).SequenceEqual(patched.AsSpan(i,3)));}
    }
    Throws<InvalidDataException>(()=>LocalSceneCaptionRules.Patch(new byte[1],true));
});

Test("award caption preserves its source and every pixel outside the title plate",()=>
{
    byte[] source=Enumerable.Range(0,256*160*3).Select(i=>(byte)(i%251)).ToArray(),copy=(byte[])source.Clone();
    byte[] patch=LocalSceneCaptionRules.PatchAwards(source);var r=LocalSceneCaptionRules.AwardsRegion;
    Check(source.SequenceEqual(copy) && !patch.SequenceEqual(source) && patch.SequenceEqual(LocalSceneCaptionRules.PatchAwards(source)));
    for(int y=0;y<160;y++)for(int x=0;x<256;x++)if(x<r.X||x>=r.X+r.Width||y<r.Y||y>=r.Y+r.Height)
    {int i=(y*256+x)*3;Check(source.AsSpan(i,3).SequenceEqual(patch.AsSpan(i,3)));}
    Throws<InvalidDataException>(()=>LocalSceneCaptionRules.PatchAwards(new byte[1]));
});

Test("English reviewer role labels preserve every face pixel",()=>
{
    byte[] source=Enumerable.Range(0,64*64*3).Select(i=>(byte)(i%251)).ToArray(),copy=(byte[])source.Clone();
    for(int row=0;row<4;row++)
    {
        var patch=LocalSceneCaptionRules.PatchReviewer(source,row);Check(source.SequenceEqual(copy));
        Check(patch.AsSpan(0,48*64*3).SequenceEqual(source.AsSpan(0,48*64*3)) && !patch.SequenceEqual(source));
        Check(patch.SequenceEqual(LocalSceneCaptionRules.PatchReviewer(source,row)));
    }
    Throws<InvalidDataException>(()=>LocalSceneCaptionRules.PatchReviewer(source,4));
});
Test("independent native review comment draws allow repeated comments",()=>
{
    var product=new ReleasedGameState {Title="Fixture",ReviewScores=[5,5,5,5]};var random=new GreetingRandom(.4f);
    ReviewPresentationRules.Prepare(product,random);
    Check(random.GreetingCalls==4 && product.ReviewCommentVariants!.SequenceEqual(new[]{4,4,4,4}));
});

Test("office cadence retains nominal wall time without replaying stalled iterations",()=>
{
    double remainder=0;int ticks=0;
    for(int frame=0;frame<600;frame++)if(OfficeTickScheduler.Advance(ref remainder,1.0/60))ticks++;
    Check(ticks==250);
    Check(OfficeTickScheduler.Advance(ref remainder,1.0));
    Check(!OfficeTickScheduler.Advance(ref remainder,.01));
    Check(!OfficeTickScheduler.Advance(ref remainder,.01));
    Check(!OfficeTickScheduler.Advance(ref remainder,.01));
    Check(OfficeTickScheduler.Advance(ref remainder,.01));
    Check(OfficeTickScheduler.Advance(ref remainder,.16));
    Check(!OfficeTickScheduler.Advance(ref remainder,.01));
});

Test("programming result saves between quality and experience and resumes same actor without prefix replay",()=>
{
    var s=ProgrammingFixture();ProgrammingChallengeService.AcknowledgeResult(s);ProgrammingChallengeService.AcknowledgeResult(s);
    Check(s.DevelopmentProjects[0].Quality.Fun==40 && s.Employees[0].Experience==20);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));ProgrammingChallengeService.AcknowledgeResult(s);
    Check(s.Employees[0].Experience==50 && s.PendingProgrammingChallenge is null && s.OfficeActorContinuation is not null);
    var session=new GameSession(new FailureRandom());session.Restore(s);session.TickOffice(CoreCatalog());
    Check(s.OfficeActorContinuation is null && s.Employees[0].Office!.ActionTicks==11 && s.Employees[0].Stress==10 &&
        s.Employees[0].Office!.WorkTicks==6 && s.CompanyActivityTicks==0 && s.ProgrammingActorPasses==0);
});
Test("programming failure adds bugs and size exactly once after final message",()=>
{
    var s=ProgrammingFixture(1);for(int i=0;i<3;i++)ProgrammingChallengeService.AcknowledgeResult(s);
    Check(s.DevelopmentProjects[0].Bugs==60 && s.DevelopmentProjects[0].DataSize==6 && s.Employees[0].Experience==20);
    Check(!ProgrammingChallengeService.AcknowledgeResult(s));
});
Test("programming reel stop consumes one draw and final result choices survive reload",()=>
{
    var s=ProgrammingFixture();s.PendingProgrammingChallenge=new() {Inspiration=80,IntroPage=2};var rng=new FailureRandom(0,.5f,.7f);
    Check(ProgrammingChallengeService.Command(s,rng) && rng.Calls==0);Check(ProgrammingChallengeService.Command(s,rng));
    for(int n=0;n<15;n++)ProgrammingChallengeService.Advance(s,rng);
    Check(s.PendingProgrammingChallenge.Mode==3 && s.PendingProgrammingChallenge.Index==5);
    for(int n=0;n<13;n++)ProgrammingChallengeService.Advance(s,rng);
    Check(s.PendingProgrammingChallenge.Outcome==1 && s.PendingProgrammingChallenge.QualityIndex==2 && rng.Calls==3);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(!ProgrammingChallengeService.Advance(s,rng) && rng.Calls==3);
});
Test("programming trigger draws eagerly and captures current actor rather than selection",()=>
{
    var s=ProgrammingFixture();s.PendingProgrammingChallenge=null;s.OfficeActorContinuation=null;var rng=new FailureRandom(0,.00006f);
    Check(!ProgrammingChallengeService.TryTrigger(s,s.Employees[0],s.DevelopmentProjects[0],rng) && rng.Calls==1 && s.ProgrammingActorPasses==1);
    s.ProgrammingActorPasses=50000;Check(ProgrammingChallengeService.TryTrigger(s,s.Employees[0],s.DevelopmentProjects[0],rng) && s.ProgrammingActorPasses==0);
    Check(s.OfficeActorContinuation!.EmployeeSourceIndex==0 && s.OfficeActorContinuation.ActorSlot==0);
    s.OfficeActorContinuation.ActorSlot=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("activity confirmation charges once and persists reported before and after values",()=>
{
    var s=State();s.CompanyActivityTicks=0;s.CompanyActivityPerformed=false;s.Company.ActivityCost=5;s.Company.Fame=12;
    s.PendingCompanyActivity=new() {Activity=CompanyActivity.PublicRelations};
    Check(CompanyActivityPresentation.Confirm(s,null,new ConstantRandom(1))==CompanyActivityResult.Success && s.Money==95);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.PendingCompanyActivity!.Before==12 && s.PendingCompanyActivity.After==14);
    Check(CompanyActivityPresentation.Confirm(s,null,new FailureRandom())==CompanyActivityResult.AlreadyActed && s.Money==95);
});
Test("held shipment controls use recovered acceleration thresholds",()=>
{
    Check(HeldShipmentRules.Step(10,true)==100 && HeldShipmentRules.Step(11,true)==1000 && HeldShipmentRules.Step(31,true)==30000 &&
        HeldShipmentRules.Step(61,true)==1000000 && HeldShipmentRules.Step(20,false)==10 && HeldShipmentRules.Step(21,false)==100);
});
Test("VB-family random recurrence agrees with framework reference and shared stream resumes from saves",()=>
{
    float first=Microsoft.VisualBasic.VBMath.Rnd(-1f);var rng=new VbSimulationRandom((uint)(first*16777216f));
    for(int i=0;i<100;i++)Check(rng.NextSingle()==Microsoft.VisualBasic.VBMath.Rnd());
    var session=new GameSession(rng);var s=session.StartNewGame("Company","Test",GameMode.Normal);
    session.Random.NextSingle();string saved=SaveStore.Serialize(s);float expected=session.Random.NextSingle();session.Restore(SaveStore.Deserialize(saved));
    Check(session.Random.NextSingle()==expected);
});
Test("numbered save preview distinguishes damaged files and preserves backup metadata",()=>
{
    string root=Path.Combine(Path.GetTempPath(),"opengds-slots-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
    try
    {
        var s=State();SaveStore.Save(SaveSlotStore.PathFor(root,1),s);s.Money=200;SaveStore.Save(SaveSlotStore.PathFor(root,1),s);
        File.WriteAllText(SaveSlotStore.PathFor(root,2),"damaged");var slots=SaveSlotStore.Read(root,true);
        Check(slots.Count==3 && slots[0].State!.Money==200 && slots[0].BackupAvailable && slots[1].Error is not null && slots[2].Slot==3);
        Throws<ArgumentOutOfRangeException>(()=>SaveSlotStore.PathFor(root,0));
    }
    finally{Directory.Delete(root,true);}
});
Test("Hall celebration is queued once by new qualifying review acknowledgment",()=>
{
    var p=new ReleasedGameState {ProductIndex=0,Title="Test",ReviewScores=[8,8,8,8],ReviewPending=true};
    var s=new GameState {ReleasedGames=[p],Company=new() {Name="Test",PresidentName="Test",HallOfFameCount=0}};
    Check(ReviewService.Acknowledge(s,p) && p.HallCelebration is not null && s.Company.HallOfFameCount==1);
    Check(!ReviewService.Acknowledge(s,p) && s.Company.HallOfFameCount==1);
    for(int n=0;n<3;n++)HallCelebrationRules.Advance(s,p);
    Check(p.HallCelebration!.Ticks==3 && p.HallCelebration.Frame==0);
    HallCelebrationRules.Advance(s,p);Check(p.HallCelebration.Frame==1);
    HallCelebrationRules.Advance(s,p);HallCelebrationRules.Advance(s,p);
    Check(p.HallCelebration.Frame==2 && p.HallCelebration.Ticks==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.ReleasedGames[0];
    Check(p.HallCelebration!.Frame==2 && p.HallCelebration.Ticks==1 && s.Company.HallOfFameCount==1);
    Check(HallCelebrationRules.Acknowledge(p) && !HallCelebrationRules.Acknowledge(p) && s.Company.HallOfFameCount==1);
    var next=new ReleasedGameState {ProductIndex=1,Title="Next",ReviewScores=[8,8,8,8],ReviewPending=true};s.ReleasedGames.Add(next);
    Check(ReviewService.Acknowledge(s,next) && next.HallCelebration!.Ticks==1 && next.HallCelebration.Frame==0 && s.Company.HallOfFameCount==2);
});
Test("legacy Hall status does not invent a pending celebration and malformed frames reject",()=>
{
    var p=new ReleasedGameState {ProductIndex=0,Title="Test",HallOfFame=true,ReviewScores=[8,8,8,8]};
    var s=new GameState {Company=new() {Name="Test",PresidentName="Test"},ReleasedGames=[p]};
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].HallCelebration is null);
    p.HallCelebration=new() {Frame=3};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("promotion delays transaction until callback 27 and resumes without duplicate draws",()=>
{
    var p=new DevelopmentProjectState {RoomIndex=0,Title="Test",OriginalStage=1,GameTypeSourceIndex=0,PromotionUseCount=0,Recognition=90,RequiredPoints=100,Focus=new(25,25,25,25)};
    var s=new GameState {Company=new() {Name="Test",PresidentName="Test",WeeklyNet=0,MonthlyNet=0,YearlyNet=0},Money=30_000_000,DevelopmentProjects=[p]};
    var rng=new FailureRandom(.99f);
    Check(PromotionService.Start(s,p,0)==PromotionResult.Success && s.Money==30_000_000);
    for(int i=0;i<26;i++)PromotionService.Advance(s,rng);
    Check(rng.Calls==0 && p.Recognition==90 && !PromotionService.Acknowledge(s));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects[0];
    PromotionService.Advance(s,rng);
    Check(p.Recognition==99 && p.PromotionUseCount==1 && rng.Calls==1 && s.Money==25_000_000 && s.Company.WeeklyNet==-5_000_000);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(!PromotionService.Advance(s,rng) && rng.Calls==1);
    Check(PromotionService.Acknowledge(s) && !PromotionService.Acknowledge(s));
});
Test("promotion use count is per target across media and retains negative cap",()=>
{
    var p=new DevelopmentProjectState {OriginalStage=1,PromotionUseCount=1};
    var s=new GameState {Money=100_000_000,DevelopmentProjects=[p]};var rng=new FailureRandom(0,0,0);
    Check(PromotionService.Start(s,p,5)==PromotionResult.Success);
    for(int i=0;i<27;i++)PromotionService.Advance(s,rng);
    Check(p.Recognition==6 && p.PromotionUseCount==2);PromotionService.Acknowledge(s);
    PromotionService.Start(s,p,2);for(int i=0;i<27;i++)PromotionService.Advance(s,rng);
    Check(p.Recognition==6 && p.PromotionUseCount==3);PromotionService.Acknowledge(s);
    p.Recognition=105;PromotionService.Start(s,p,0);for(int i=0;i<27;i++)PromotionService.Advance(s,rng);
    Check(p.Recognition==99 && s.PendingPromotion!.Gain==-6 && s.Money==50_000_000);
});
Test("promotion refuses pre-alpha, unknown legacy history and unaffordable media without mutation",()=>
{
    var p=new DevelopmentProjectState {PromotionUseCount=0};var s=new GameState {Money=0,DevelopmentProjects=[p]};
    Check(PromotionService.Start(s,p,0)==PromotionResult.AlphaRequired);p.OriginalStage=1;p.PromotionUseCount=null;
    Check(PromotionService.Start(s,p,0)==PromotionResult.HistoryUnknown);p.PromotionUseCount=0;
    Check(PromotionService.Start(s,p,0)==PromotionResult.InsufficientFunds && s.PendingPromotion is null && s.Money==0);
});
Test("promotion malformed pending save rejects before serialization",()=>
{
    var p=new DevelopmentProjectState {RoomIndex=0,Title="Test",OriginalStage=1,GameTypeSourceIndex=0,PromotionUseCount=0,RequiredPoints=100,Focus=new(25,25,25,25)};
    var s=new GameState {Company=new() {Name="Test",PresidentName="Test"},Money=10_000_000,DevelopmentProjects=[p]};
    PromotionService.Start(s,p,0);s.PendingPromotion!.Gain=1;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("room panel follows native counter thresholds and is independent of employees",()=>
{
    var s=new GameState {DevelopmentProjects=[new() {RoomIndex=0,RequiredPoints=100,Focus=new(25,25,25,25)}]};
    for(int n=0;n<12;n++) OfficeRoomPresentationRules.Tick(s);
    Check(s.OfficeRoomPresentation.PanelTicks[0]==12 && s.OfficeRoomPresentation.PanelSourceX[0]==259);
    OfficeRoomPresentationRules.Tick(s);
    Check(s.OfficeRoomPresentation.PanelTicks[0]==1 && s.OfficeRoomPresentation.PanelSourceX[0]==321);
    s.DevelopmentProjects[0].OriginalRoomPaused=true;
    OfficeRoomPresentationRules.Tick(s);Check(s.OfficeRoomPresentation.PanelTicks[0]==1);
    Check(s.OfficeRoomPresentation.PanelTicks[1]==0);
});
Test("room projections hide empty rooms and preserve native panel depth",()=>
{
    var s=new GameState();var panel=new OfficeSpriteDefinition(91,-1000,131,259,226,62,31,26);
    Check(OfficeRoomPresentationRules.Project(panel,s).Y==0);
    s.DevelopmentProjects.Add(new() {RoomIndex=0,RequiredPoints=100,Progress=49.99,Focus=new(25,25,25,25)});
    Check(OfficeRoomPresentationRules.Project(panel,s)==panel with {X=162});
    var digit=OfficeRoomPresentationRules.Project(new(88,-70,129,0,0,17,12,255),s);
    Check(digit.SourceX==383+17*4 && digit.SourceY==226+12*9);
    s.DevelopmentProjects[0].OriginalStage=3;
    Check(OfficeRoomPresentationRules.Project(digit,s).Y==0);
});
Test("completion labels share native phase across rooms",()=>
{
    var s=new GameState {DevelopmentProjects=[new() {RoomIndex=1,OriginalStage=3,RequiredPoints=100,Focus=new(25,25,25,25)},new() {RoomIndex=0,OriginalStage=3,RequiredPoints=100,Focus=new(25,25,25,25)}]};
    for(int n=0;n<4;n++) OfficeRoomPresentationRules.Tick(s);
    Check(s.OfficeRoomPresentation.CompletionTicks==8 && s.OfficeRoomPresentation.CompletionSourceX[1]==1005 && s.OfficeRoomPresentation.CompletionSourceX[0]==948);
});
Test("room phase persists and malformed phase rejects before save",()=>
{
    var s=new GameState {Company=new() {Name="Synthetic company",PresidentName="Tester"}};s.OfficeRoomPresentation.PanelTicks[1]=11;s.OfficeRoomPresentation.PanelSourceX[1]=321;
    var loaded=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(loaded.OfficeRoomPresentation.PanelTicks[1]==11 && loaded.OfficeRoomPresentation.PanelSourceX[1]==321);
    s.OfficeRoomPresentation.PanelTicks[0]=13;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("desk targets use distinct native strict diamonds",()=>
{
    var centers=new (int Action,double X,double Y)[] {(6,200,133.5),(7,232,149.5),(8,169,141),(9,217,164.5),(10,152,157.5),(11,184,173.5),(12,296,181.5),(13,328,197.5),(14,265,189),(15,313,212.5),(16,248,205.5),(17,280,221.5)};
    foreach(var d in centers) Check(OfficeInteractionRules.DeskAt(d.X,d.Y)==d.Action);
    Check(OfficeInteractionRules.DeskAt(double.NaN,0)==null && OfficeInteractionRules.DeskAt(0,0)==null);
    Check(OfficeInteractionRules.DeskAt(200,112)==null);
});
Test("right-click assignment resets timer before occupied rejection without clearing burst units",()=>
{
    var e=new EmployeeState {Name="Synthetic selected",Office=new() {Action=6,SeatedAction=6,WorkTicks=17,SharedWorkUnits=37,BurstCount=37}};
    var other=new EmployeeState {Name="Synthetic occupant",Office=new() {Action=7}};
    var s=new GameState {Employees=[e,other]};
    Check(OfficeAssignmentService.TryAssignDeskFromOffice(s,e,7)==OfficeAssignmentResult.Occupied);
    Check(e.Office.WorkTicks==0 && e.Office.SharedWorkUnits==37 && e.Office.Action==6);
    Check(OfficeAssignmentService.TryAssignDeskFromOffice(s,e,12)==OfficeAssignmentResult.Success);
    Check(e.Office.Action==12 && e.Office.MovementState==3 && e.Office.SeatedAction==0);
});
Test("own-console catalog composition preserves locally imported race definitions",()=>
{
    var s=new GameState();s.OwnHardware.Launched=new HardwareDefinition {SourceIndex=11};
    var c=new OriginalCatalog {Horses=[new HorseDefinition {SourceIndex=0,Name="Synthetic runner",Strength=.13}],Jockeys=[new JockeyDefinition {SourceIndex=0,Name="Synthetic rider"}]};
    var combined=OwnHardwareService.Catalog(s,c);Check(ReferenceEquals(combined.Horses,c.Horses) && ReferenceEquals(combined.Jockeys,c.Jockeys));
    var legacy=System.Text.Json.JsonSerializer.Deserialize<OriginalCatalog>("{}")!;Check(legacy.Horses.Count==0 && legacy.Jockeys.Count==0);
});
OriginalCatalog HorseCatalog()=>new() {Horses=Enumerable.Range(0,44).Select(i=>new HorseDefinition {SourceIndex=i,Name=$"Synthetic runner {i}",RawParameter=13,Strength=i+.13}).ToList(),Jockeys=Enumerable.Range(0,10).Select(i=>new JockeyDefinition {SourceIndex=i,Name=$"Synthetic rider {i}"}).ToList()};
float[] HorseSetupDraws()=>new[]{0f,0f}.Concat(Enumerable.Range(0,8).Select(i=>(i+.01f)/44)).Concat(Enumerable.Range(0,8).Select(i=>(i+.01f)/10)).Concat(Enumerable.Repeat(0f,40)).ToArray();
GameState HorseFixture(long money=10000)
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=4;s.Money=money;
    Check(CasinoHorseService.Open(s,HorseCatalog(),new FailureRandom(HorseSetupDraws()))==CasinoResult.Success);return s;
}
void HorseFinished(GameState s)
{
    var r=s.Casino.Horse;r.Stage=1;r.Tick=361;r.RacePhase=2;r.Camera=-1010;r.Performance=Enumerable.Repeat(0d,8).ToList();r.FinishCoordinates=Enumerable.Repeat(-1919d,8).ToList();
    CasinoHorseService.TickRace(s,new FailureRandom());Check(r.Stage==2 && r.Arrival.SequenceEqual(Enumerable.Range(0,8)));
}
Test("horse racing requires complete recovered records but not casino coins",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=4;s.Money=10000;s.Casino.Coins=0;
    Check(CasinoHorseService.Open(s,new OriginalCatalog(),new FailureRandom())==CasinoResult.CatalogMissing);
    Check(CasinoHorseService.Open(s,HorseCatalog(),new FailureRandom(HorseSetupDraws()))==CasinoResult.Success && s.Casino.Coins==0);CasinoPokerService.Validate(s);
    Check(CasinoPokerService.Open(s)==CasinoResult.EventPending);Check(CasinoHorseService.Close(s));
});
Test("horse setup preserves unused draws, unique source selections, forecasts and cosmetics",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=4;s.Money=10000;
    var random=new FailureRandom(HorseSetupDraws());CasinoHorseService.Open(s,HorseCatalog(),random);var r=s.Casino.Horse;
    Check(random.Calls==58 && r.Runners.Select(h=>h.SourceIndex).SequenceEqual(Enumerable.Range(0,8)) && r.Runners.Select(h=>h.JockeySourceIndex).Distinct().Count()==8);
    Check(r.ForecastRanks.All(row=>row.SequenceEqual(Enumerable.Range(0,8).Reverse())) && r.Runners.All(h=>h.WeightChange==-1));
    Check(r.WinOdds.SequenceEqual(CasinoHorseService.Odds.Reverse()) && r.PairOdds.All(row=>row[7]==0));
});
Test("horse wager drafts close without committing and start debits funds without accounting helper",()=>
{
    var s=HorseFixture();var weekly=s.Company!.WeeklyNet;CasinoHorseService.AdjustWager(s,1);Check(s.Money==10000 && s.Casino.Horse.DraftCash==9900);
    CasinoHorseService.Close(s);Check(s.Money==10000 && s.CompanyActivityPerformed==true);
    CasinoHorseService.Open(s,HorseCatalog(),new FailureRandom(HorseSetupDraws()));CasinoHorseService.AdjustWager(s,1);
    var random=new FailureRandom(new[]{0f}.Concat(Enumerable.Repeat(.1f,8)).ToArray());CasinoHorseService.Start(s,random);
    Check(s.Money==9900 && s.Company.WeeklyNet==weekly && random.Calls==9);CasinoHorseService.Close(s);Check(s.Money==9900);
});
Test("horse positive cash guard can overspend the final fraction of a wager",()=>
{
    var s=HorseFixture(1);Check(CasinoHorseService.AdjustWager(s,1)==CasinoResult.Success && s.Casino.Horse.DraftCash==-99);
    Check(CasinoHorseService.AdjustWager(s,1)==CasinoResult.InvalidAction && s.Casino.Horse.SingleStakes[0]==100);
});
Test("horse single cap retains the debit after clamping a hundred-thousand stake",()=>
{
    var s=HorseFixture(10000);s.Casino.Horse.SingleStakes[0]=99900;
    Check(CasinoHorseService.AdjustWager(s,1)==CasinoResult.Success && s.Casino.Horse.SingleStakes[0]==99900 && s.Casino.Horse.DraftCash==9900);
});
Test("horse pair odds display is compressed but payout reads raw arrival columns",()=>
{
    var s=HorseFixture();var r=s.Casino.Horse;r.Arrival=[0,1,2,3,4,5,6,7];r.PairStakes[0][1]=100;
    Check(r.PairOdds[0][0]==r.WinOdds[0]*r.WinOdds[1]/2.4 && r.PairOdds[0][1]==r.WinOdds[0]*r.WinOdds[2]/2.4);
    Check(CasinoHorseService.Award(r)==100*(decimal)Math.Round(r.PairOdds[0][1],1,MidpointRounding.ToEven));
    r.Arrival=[0,7,1,2,3,4,5,6];r.PairStakes[0][7]=100;Check(CasinoHorseService.Award(r)==0);
});
Test("horse pair diagonal typo and compressed removal guard preserve wrong-cell behavior",()=>
{
    var s=HorseFixture();var r=s.Casino.Horse;r.PairSelected=true;r.SelectedFirst=4;r.SelectedSecond=3;
    CasinoHorseService.AdjustWager(s,1);Check(r.PairStakes[4][4]==100 && r.PairStakes[4][3]==0 && r.DraftCash==9900);
    Check(CasinoHorseService.AdjustWager(s,-1)==CasinoResult.InvalidAction);
    r.SelectedSecond=5;Check(CasinoHorseService.AdjustWager(s,-1)==CasinoResult.Success && r.PairStakes[4][5]==-100 && r.DraftCash==10000 && r.PairStakes[4][4]==100);
    Check(CasinoHorseService.AdjustWager(s,-1)==CasinoResult.Success && r.PairStakes[4][5]==-200 && r.DraftCash==10100);CasinoPokerService.Validate(s);
});
Test("horse pair cap can clamp an adjacent column instead of the updated stake",()=>
{
    var s=HorseFixture();var r=s.Casino.Horse;r.PairSelected=true;r.SelectedFirst=0;r.SelectedSecond=1;r.PairStakes[0][1]=99900;
    CasinoHorseService.AdjustWager(s,1);Check(r.PairStakes[0][1]==100000 && r.PairStakes[0][0]==0 && r.DraftCash==9900);
});
Test("horse performance uses successive random tests and consumes one to three draws",()=>
{
    var s=HorseFixture();var draws=new List<float>{0f,.1f,0f,.2f,0f,0f,.9f};draws.AddRange(Enumerable.Repeat(.1f,5));var random=new FailureRandom(draws.ToArray());
    CasinoHorseService.Start(s,random);var p=s.Casino.Horse.Performance;
    Check(random.Calls==12 && p[0]==.13/1.2+6 && p[1]==1.13/1.2+4 && p[2]==2.13/1.2+3);
});
Test("horse phase-one windows and mode-specific signed divisions match recovered vectors",()=>
{
    Check(CasinoHorseService.PhaseOneDelta(64,0,0,72)==0 && CasinoHorseService.PhaseOneDelta(65,0,0,72)==-14.4);
    Check(CasinoHorseService.PhaseOneDelta(80,0,4,72)==-18 && CasinoHorseService.PhaseOneDelta(81,0,4,72)==0);
    Check(CasinoHorseService.PhaseOneDelta(110,1,7,72)==28.8 && CasinoHorseService.PhaseOneDelta(140,1,1,70)==-10);
});
Test("horse overlapping tick two-ninety executes both movement blocks and camera steps",()=>
{
    var a=CasinoHorseService.PhaseTwoDelta(289,0,0,72,0);var b=CasinoHorseService.PhaseTwoDelta(290,0,0,72,0);var c=CasinoHorseService.PhaseTwoDelta(291,0,0,72,0);
    Check(a==(-8d,-18d) && b==(-26d,-46d) && c==(-18d,-28d));
    Check(CasinoHorseService.PhaseTwoDelta(290,0,0,72,10)==(-46d,-46d));
    Check(CasinoHorseService.PhaseTwoDelta(231,1,6,72,0)==(-6d,-16d));
});
Test("horse transitions select mode at forty-five and preserve unique-maximum tie behavior",()=>
{
    Check(CasinoHorseService.UniqueMaximum(new[]{1d,2d,2d}) is null && CasinoHorseService.UniqueMaximum(new[]{1d,3d,2d})==1);
    var s=HorseFixture();var random=new FailureRandom(new[]{0f}.Concat(Enumerable.Repeat(.1f,8)).Concat(new[]{.9f}).ToArray());CasinoHorseService.Start(s,random);
    for(int i=0;i<45;i++) CasinoHorseService.TickRace(s,random);Check(s.Casino.Horse.Mode==1 && s.Casino.Horse.RacePhase==1 && random.Calls==10);
    for(int i=45;i<200;i++) CasinoHorseService.TickRace(s,random);
    var r=s.Casino.Horse;Check(r.RacePhase==2 && r.Camera==700 && r.FinishCoordinates.Min()==0 && r.Positions.SequenceEqual(r.FinishCoordinates));CasinoPokerService.Validate(s);
});
Test("horse finish ties follow ascending runner visitation rather than an outcome sort",()=>
{
    var s=HorseFixture();HorseFinished(s);Check(s.Casino.Horse.Arrival.SequenceEqual(Enumerable.Range(0,8)));CasinoPokerService.Validate(s);
});
Test("horse result timer waits one hundred callbacks and source settlement discards winnings",()=>
{
    var s=HorseFixture();CasinoHorseService.AdjustWager(s,1);CasinoHorseService.Start(s,new FailureRandom(new[]{0f}.Concat(Enumerable.Repeat(.1f,8)).ToArray()));HorseFinished(s);
    var r=s.Casino.Horse;for(int i=0;i<99;i++) CasinoHorseService.TickResult(s,HorseCatalog(),new FailureRandom());Check(r.Stage==2);
    CasinoHorseService.TickResult(s,HorseCatalog(),new FailureRandom());Check(r.Stage==3 && r.DisplayedAward==4770 && s.Money==9900);CasinoPokerService.Validate(s);
    var random=new FailureRandom(HorseSetupDraws().Skip(1).ToArray());Check(CasinoHorseService.AcceptPayout(s,HorseCatalog(),random));
    Check(s.Money==9900 && s.Casino.Horse.DraftCash==9900 && s.Casino.Horse.TemporaryBalance==9900 && s.Casino.Horse.RaceCounter==1 && random.Calls==57);
    Check(!CasinoHorseService.AcceptPayout(s,HorseCatalog(),new FailureRandom()));
});
Test("horse losing result opens the next card without inventing a payout",()=>
{
    var s=HorseFixture();HorseFinished(s);for(int i=0;i<99;i++) CasinoHorseService.TickResult(s,HorseCatalog(),new FailureRandom());
    CasinoHorseService.TickResult(s,HorseCatalog(),new FailureRandom(HorseSetupDraws().Skip(1).ToArray()));Check(s.Casino.Horse.Stage==0 && s.Money==10000);
});
Test("horse save continuity does not resume held physical input and rejects malformed runner data",()=>
{
    var s=HorseFixture();s.Casino.Horse.HeldAdjustment=1;var copy=System.Text.Json.JsonSerializer.Deserialize<GameState>(System.Text.Json.JsonSerializer.Serialize(s))!;
    CasinoPokerService.Validate(copy);Check(copy.Casino.Horse.HeldAdjustment==0 && copy.Casino.Horse.Runners[0].Name==s.Casino.Horse.Runners[0].Name);
    copy.Casino.Horse.Runners[1].SourceIndex=copy.Casino.Horse.Runners[0].SourceIndex;Throws<InvalidDataException>(()=>CasinoPokerService.Validate(copy));
});
Test("horse missing future catalog suspends next-card setup and resumes after recovery",()=>
{
    var s=HorseFixture();HorseFinished(s);for(int i=0;i<100;i++) CasinoHorseService.TickResult(s,null,new FailureRandom());
    Check(s.Casino.Horse.CatalogBoundary && s.Casino.Horse.Stage==2);CasinoPokerService.Validate(s);
    Check(CasinoHorseService.ResumeCatalogBoundary(s,HorseCatalog(),new FailureRandom(HorseSetupDraws().Skip(1).ToArray())) && s.Casino.Horse.Stage==0);
});
Test("horse numeric guards preserve ledger when wrong-cell refunds reach integer limits",()=>
{
    var s=HorseFixture();var r=s.Casino.Horse;r.PairSelected=true;r.SelectedFirst=4;r.SelectedSecond=5;r.PairStakes[4][4]=100;r.DraftCash=long.MaxValue;
    Check(CasinoHorseService.AdjustWager(s,-1)==CasinoResult.NumericLimit && r.PairStakes[4][5]==0 && r.DraftCash==long.MaxValue);
});
Test("horse activity blocks office updates and conflicting facility actions",()=>
{
    var s=HorseFixture();var session=new GameSession(new FailureRandom());session.Restore(s);var before=s.CompanyActivityTicks;session.TickOffice();
    Check(s.CompanyActivityTicks==before && FacilityService.HasPendingDecision(s) && CasinoCardChainService.Open(s)==CasinoResult.EventPending);
});
Test("horse ordinary full races finish with eight distinct arrivals under both modes",()=>
{
    foreach(float mode in new[]{0f,.9f})
    {
        var s=HorseFixture();var random=new FailureRandom(new[]{0f}.Concat(Enumerable.Repeat(.1f,8)).Concat(new[]{mode}).ToArray());CasinoHorseService.Start(s,random);
        for(int tick=0;tick<1200 && s.Casino.Horse.Stage==1;tick++) CasinoHorseService.TickRace(s,random);
        Check(s.Casino.Horse.Stage==2 && s.Casino.Horse.Arrival.Distinct().Count()==8);CasinoPokerService.Validate(s);
    }
});

Test("horse ordinary pair odds agree with bounded decimal vectors away from midpoints",()=>
{
    foreach(double a in CasinoHorseService.Odds) foreach(double b in CasinoHorseService.Odds)
    {
        decimal exact=(decimal)a*(decimal)b/2.4m;decimal scaled=exact*10;
        Check(Math.Abs((scaled-decimal.Floor(scaled))-.5m)>.0001m);
        Check((decimal)Math.Round(a*b/2.4,1,MidpointRounding.ToEven)==decimal.Round(exact,1));
    }
});
GameState CardChainFixture()
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=4;s.Casino.Coins=100;
    Check(CasinoCardChainService.Open(s)==CasinoResult.Success);return s;
}
PokerRoundState ChainHand(GameState s,params CasinoCard[] cards)
{
    var p=s.Casino.CardChain.Round;p.Cards=cards.ToList();p.UsedCards=cards.Select(c=>c.Identifier).ToHashSet();p.Phase=1;p.Revealed=[true,true,true,true,true];s.Casino.Bet=1;
    return p;
}
Test("card-chain range bands match all recovered payout boundaries",()=>
{
    int[] values=[1,2,3,4,6,8,10,13,17,20,25,30,40,50,55,60,70,80,100];Check(CasinoCardChainService.Multipliers.SequenceEqual(values));
    int[] lower=[10,14,17,20,23,26,28,30,32,34,36,38,40,42,44,46,48,50,52];
    Check(CasinoCardChainService.ChainMultiplier(9)==0);
    for(int i=0;i<lower.Length;i++) {Check(CasinoCardChainService.ChainMultiplier(lower[i])==values[i]);if(i+1<lower.Length) Check(CasinoCardChainService.ChainMultiplier(lower[i+1]-1)==values[i]);}
});
Test("card-chain strongest hand bonuses include royal under straight flush",()=>
{
    Check(CasinoCardChainService.BonusMultiplier([new(0,0),new(0,2),new(0,4),new(0,7),new(0,11)])==1);
    Check(CasinoCardChainService.BonusMultiplier([new(0,1),new(1,1),new(2,1),new(0,2),new(1,2)])==2);
    Check(CasinoCardChainService.BonusMultiplier([new(0,1),new(1,1),new(2,1),new(3,1),new(0,2)])==5);
    Check(CasinoCardChainService.BonusMultiplier([new(0,0),new(0,9),new(0,10),new(0,11),new(0,12)])==20);
});
Test("card-chain load retains shared amounts, accepted betting resets only bonus",()=>
{
    var s=CardChainFixture();s.Casino.Bet=17;s.Casino.PendingPayout=73;s.Casino.ChainBonus=19;
    CasinoCardChainService.Close(s);CasinoCardChainService.Open(s);
    Check(s.Casino.Bet==17 && s.Casino.PendingPayout==73 && s.Casino.ChainBonus==19);
    CasinoCardChainService.Command(s,0,new FailureRandom(0));Check(s.Casino.Bet==18 && s.Casino.Coins==99 && s.Casino.ChainBonus==0 && s.Casino.PendingPayout==73);
});
Test("card-chain initial deal evaluates bonus before any replacement",()=>
{
    var s=CardChainFixture();CasinoCardChainService.Command(s,0,new FailureRandom(0));
    CasinoCard[] cards=[new(0,0),new(0,9),new(0,10),new(0,11),new(0,12)];var r=new FailureRandom(new[]{0f}.Concat(CardDraws(cards)).ToArray());
    CasinoCardChainService.Command(s,1,r);Check(s.Casino.ChainBonus==20 && s.Casino.CardChain.ChainCount==0 && r.Calls==11);
    CasinoPokerService.Validate(s);
});
Test("card-chain first selection is free and later choices match removed suit or rank",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(1,1),new(0,2),new(2,0),new(3,7));
    Check(CasinoCardChainService.SelectCard(s,0));var r=new FailureRandom(CardDraws(new CasinoCard(0,9)));
    CasinoCardChainService.Tick(s,r);Check(s.Casino.CardChain.PreviousCard==new CasinoCard(0,0) && r.Calls==0);
    CasinoCardChainService.Tick(s,r);CasinoCardChainService.Tick(s,r);
    Check(s.Casino.CardChain.ChainCount==1 && r.Calls==2);
    Check(CasinoCardChainService.CanSelect(s,0) && !CasinoCardChainService.CanSelect(s,1) && CasinoCardChainService.CanSelect(s,2) && CasinoCardChainService.CanSelect(s,3));CasinoPokerService.Validate(s);
});
Test("card-chain short bonus award adds retained pending instead of clearing it",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));
    s.Casino.ChainBonus=2;s.Casino.PendingPayout=73;CasinoCardChainService.SelectCard(s,0);
    var r=new FailureRandom(CardDraws(new CasinoCard(2,9)));for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);
    Check(s.Casino.CardChain.Round.Phase==3 && s.Casino.PendingPayout==75 && s.Casino.CardChain.ChainCount==1);CasinoPokerService.Validate(s);
});
Test("card-chain ten replacement award replaces retained pending before adding bonus",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));
    s.Casino.CardChain.ChainCount=9;s.Casino.CardChain.PreviousCard=new(0,0);s.Casino.PendingPayout=73;s.Casino.ChainBonus=2;
    CasinoCardChainService.SelectCard(s,0);var r=new FailureRandom(CardDraws(new CasinoCard(2,9)));for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);
    Check(s.Casino.PendingPayout==3 && s.Casino.CardChain.Round.Phase==3);
});
Test("card-chain no bonus short loss retains storage but clears active bet",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));s.Casino.PendingPayout=73;
    CasinoCardChainService.SelectCard(s,0);var r=new FailureRandom(CardDraws(new CasinoCard(2,9)));for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);
    Check(s.Casino.CardChain.Round.Phase==0 && s.Casino.Bet==0 && s.Casino.PendingPayout==73);
});
Test("card-chain bonuses accumulate once for each replacement evaluation",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(0,2),new(0,4),new(0,7),new(0,11));
    var r=new FailureRandom(CardDraws(new CasinoCard(0,9),new CasinoCard(0,10)));
    for(int k=0;k<2;k++) {CasinoCardChainService.SelectCard(s,0);for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);}
    Check(s.Casino.ChainBonus==2 && s.Casino.CardChain.ChainCount==2);
});
Test("card-chain collection switches from ten to one below one hundred fifty",()=>
{
    var s=CardChainFixture();var p=ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));p.Phase=3;s.Casino.PendingPayout=150;
    CasinoCardChainService.Command(s,0,new FailureRandom(0));for(int i=0;i<2;i++) CasinoCardChainService.Tick(s,new FailureRandom());
    Check(s.Casino.Coins==110 && s.Casino.PendingPayout==140);
    for(int i=0;i<2;i++) CasinoCardChainService.Tick(s,new FailureRandom());Check(s.Casino.Coins==111 && s.Casino.PendingPayout==139);
    CasinoCardChainService.Close(s);Check(s.Casino.Bet==1 && s.Casino.PendingPayout==139 && s.CompanyActivityPerformed==true);
});
Test("card-chain copied keyboard replacement consumes five plus one cards but one chain step",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));
    CasinoCard[] drawn=[new(0,1),new(0,2),new(0,3),new(0,4),new(0,5),new(0,6)];
    var r=new FailureRandom(new[]{0f,0f}.Concat(CardDraws(drawn)).ToArray());
    Check(CasinoCardChainService.Command(s,1,r)==CasinoResult.InvalidAction && r.Calls==1);
    Check(CasinoCardChainService.Command(s,1,r,direct:true)==CasinoResult.Success && r.Calls==12);
    for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);
    Check(r.Calls==14 && s.Casino.CardChain.ChainCount==1 && s.Casino.CardChain.Round.UsedCards.Count==11);
});
Test("card-chain keyboard can start without a bet while the deal button is disabled",()=>
{
    var s=CardChainFixture();Check(!CasinoCardChainService.CanCommand(s,1));
    var r=new FailureRandom(new[]{0f}.Concat(CardDraws(BlackjackDeck().Take(5).ToArray())).ToArray());
    Check(CasinoCardChainService.Command(s,1,r,direct:true)==CasinoResult.Success && s.Casino.Bet==0);
    CasinoPokerService.Validate(s);
});
Test("card-chain exhausted deck pauses without extra draws, recycled cards or invented award",()=>
{
    var s=CardChainFixture();var p=ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));p.UsedCards=Enumerable.Range(0,52).ToHashSet();
    s.Casino.CardChain.ChainCount=47;s.Casino.CardChain.PreviousCard=new(0,0);CasinoCardChainService.SelectCard(s,0);
    var r=new FailureRandom();for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);
    Check(s.Casino.CardChain.DeckExhausted && r.Calls==0 && s.Casino.PendingPayout==0 && s.Casino.Bet==1);
    CasinoPokerService.Validate(s);Check(CasinoCardChainService.Close(s));
});
Test("card-chain double-up follows fresh-deck timing and high-card comparison",()=>
{
    var s=CardChainFixture();var p=ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));p.Phase=3;s.Casino.PendingPayout=4;
    CasinoCardChainService.Command(s,1,new FailureRandom(0));var r=new FailureRandom(CardDraws(new CasinoCard(0,1),new CasinoCard(1,0),new CasinoCard(2,2),new CasinoCard(3,3),new CasinoCard(0,4)));
    for(int i=0;i<18;i++) CasinoCardChainService.Tick(s,r);Check(r.Calls==10 && p.Revealed[0]);
    CasinoCardChainService.SelectCard(s,1);for(int i=0;i<28;i++) CasinoCardChainService.Tick(s,new FailureRandom());
    Check(s.Casino.PendingPayout==8 && p.Phase==3);CasinoPokerService.Validate(s);
});
Test("card-chain keyboard dealer choice and early double selection preserve their stalled paths",()=>
{
    var s=CardChainFixture();var p=ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));p.Phase=3;s.Casino.PendingPayout=4;
    CasinoCardChainService.Command(s,1,new FailureRandom(0));Check(CasinoCardChainService.SelectCard(s,0,direct:true));
    for(int i=0;i<50;i++) CasinoCardChainService.Tick(s,new FailureRandom());
    Check(p.Tick==28 && p.Phase==6 && s.Casino.PendingPayout==4 && s.Casino.CardChain.EarlyDoubleSelection);
    CasinoPokerService.Validate(s);CasinoCardChainService.Close(s);
});
Test("card-chain saved replacement restores the same removed reference and used deck",()=>
{
    var s=CardChainFixture();ChainHand(s,new(0,0),new(1,1),new(1,3),new(2,5),new(3,7));CasinoCardChainService.SelectCard(s,0);CasinoCardChainService.Tick(s,new FailureRandom());
    var copy=System.Text.Json.JsonSerializer.Deserialize<GameState>(System.Text.Json.JsonSerializer.Serialize(s))!;CasinoPokerService.Validate(copy);
    Check(copy.Casino.CardChain.PreviousCard==new CasinoCard(0,0) && copy.Casino.CardChain.Round.Tick==1);
    copy.Casino.BlackjackOpen=true;Throws<InvalidDataException>(()=>CasinoPokerService.Validate(copy));
});
Test("card-chain overflow refuses financial commands and bounds bonus addition",()=>
{
    var s=CardChainFixture();var p=ChainHand(s,new(0,0),new(0,2),new(0,4),new(0,7),new(0,11));p.Phase=3;s.Casino.PendingPayout=long.MaxValue;
    Check(CasinoCardChainService.Command(s,0,new FailureRandom(0))==CasinoResult.NumericLimit);
    Check(CasinoCardChainService.Command(s,1,new FailureRandom(0))==CasinoResult.NumericLimit);
    p.Phase=1;s.Casino.ChainBonus=long.MaxValue;CasinoCardChainService.SelectCard(s,0);var r=new FailureRandom(CardDraws(new CasinoCard(0,9)));
    for(int i=0;i<3;i++) CasinoCardChainService.Tick(s,r);Check(s.Casino.CardChain.NumericBoundary && s.Casino.ChainBonus==long.MaxValue);
});

Test("card-chain early keyboard double-up can resolve using retained cards without a new draw",()=>
{
    var s=CardChainFixture();var p=ChainHand(s,new(0,1),new(1,0),new(1,3),new(2,5),new(3,7));p.Phase=3;s.Casino.PendingPayout=4;
    CasinoCardChainService.Command(s,1,new FailureRandom(0));CasinoCardChainService.SelectCard(s,1,direct:true);
    var r=new FailureRandom();for(int i=0;i<28;i++) CasinoCardChainService.Tick(s,r);
    Check(r.Calls==0 && p.Phase==3 && s.Casino.PendingPayout==8 && s.Casino.CardChain.EarlyDoubleSelection);CasinoPokerService.Validate(s);
});
Test("card-chain session pauses progression and blocks simultaneous card tables",()=>
{
    var s=CardChainFixture();var session=new GameSession(new FailureRandom());session.Restore(s);var before=s.CompanyActivityTicks;session.TickOffice();
    Check(s.CompanyActivityTicks==before && CasinoPokerService.Open(s)==CasinoResult.EventPending && CasinoBlackjackService.Open(s)==CasinoResult.EventPending);
    Check(FacilityService.HasPendingDecision(s));CasinoCardChainService.Close(s);Check(CasinoPokerService.Open(s)==CasinoResult.Success);
});
GameState BlackjackFixture()
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=4;s.Casino.Coins=100;
    Check(CasinoBlackjackService.Open(s)==CasinoResult.Success);return s;
}
CasinoCard[] BlackjackDeck()=>Enumerable.Range(0,18).Select(i=>new CasinoCard(i/13,i%13)).ToArray();
BlackjackRoundState BlackjackDecision(GameState s,int count=2)
{
    var b=s.Casino.Blackjack;s.Casino.Bet=1;b.Cards=BlackjackDeck().ToList();b.UsedCards=b.Cards.Select(c=>c.Identifier).ToHashSet();
    b.PlayerCount=count;b.DealerCount=1;b.PlayerTotal=17;b.Phase=2;return b;
}
Test("blackjack source ace retries differ from optimal scoring",()=>
{
    Check(CasinoBlackjackService.InitialTotal(new(0,0),new(1,0))==12);
    Check(CasinoBlackjackService.InitialTotal(new(0,9),new(1,0))==21);
    Check(CasinoBlackjackService.Score([new(0,0),new(1,0),new(2,8)])==11);
    Check(CasinoBlackjackService.Score([new(0,0),new(1,5)])==17);
    Check(CasinoBlackjackService.Score([new(0,9),new(1,12),new(2,5)])==26);
});
Test("blackjack dealer compares before requesting a draw to seventeen",()=>
{
    Check(CasinoBlackjackService.DealerOutcome(14,15)==-1);
    Check(CasinoBlackjackService.DealerOutcome(15,15)==0);
    Check(CasinoBlackjackService.DealerOutcome(20,16)==-2);
    Check(CasinoBlackjackService.DealerOutcome(20,17)==1);
    Check(CasinoBlackjackService.DealerOutcome(20,22)==1);
});
Test("blackjack stay-triggered awards and pay table match the source",()=>
{
    Check(CasinoBlackjackService.Multipliers.SequenceEqual(new[]{1,2,4,6,20,25,100}));
    for (int n=2;n<=9;n++)
    {
        int expected=n switch {5=>2,6=>3,7=>4,>=8=>6,_=>-1};
        Check(CasinoBlackjackService.StayAward(BlackjackDeck().Take(n).ToArray())==expected);
    }
    Check(CasinoBlackjackService.StayAward([new(0,5),new(1,5),new(2,5)])==5);
});
Test("blackjack opening resets shared bet but retains pending payout and bonus",()=>
{
    var s=PokerFixture();CasinoPokerService.Command(s,0,new FailureRandom(0));s.Casino.PendingPayout=37;s.Casino.ChainBonus=19;
    Check(CasinoBlackjackService.Open(s)==CasinoResult.EventPending);CasinoPokerService.Close(s);
    Check(CasinoBlackjackService.Open(s)==CasinoResult.Success && s.Casino.Bet==0 && s.Casino.PendingPayout==37 && s.Casino.ChainBonus==19);
    CasinoPokerService.Validate(s);Check(CasinoPokerService.Open(s)==CasinoResult.EventPending);
    Check(CasinoBlackjackService.Close(s) && s.CompanyActivityPerformed==true);
});
Test("blackjack bets debit immediately, cap at fifty and consume no unused random draw",()=>
{
    var s=BlackjackFixture();var r=new FailureRandom();
    Check(CasinoBlackjackService.Command(s,1,r)==CasinoResult.InvalidAction && r.Calls==0);
    for (int i=0;i<50;i++) Check(CasinoBlackjackService.Command(s,2,r)==CasinoResult.Success);
    Check(s.Casino.Coins==50 && s.Casino.Bet==50 && r.Calls==0);
    Check(CasinoBlackjackService.Command(s,2,r)==CasinoResult.InvalidAction);
    CasinoBlackjackService.Close(s);CasinoBlackjackService.Open(s);Check(s.Casino.Coins==50 && s.Casino.Bet==0);
});
Test("blackjack predeals eighteen cards in side order and uses source reveal callbacks",()=>
{
    var s=BlackjackFixture();CasinoBlackjackService.Command(s,2,new FailureRandom());var deck=BlackjackDeck();
    var r=new FailureRandom(CardDraws(deck));CasinoBlackjackService.Command(s,1,r);var b=s.Casino.Blackjack;
    Check(r.Calls==36 && b.Cards.SequenceEqual(deck) && b.UsedCards.Count==18);
    for (int i=1;i<=75;i++)
    {
        CasinoBlackjackService.Tick(s,new FailureRandom());
        Check(b.PlayerCount==(i<18 ? 0 : i<36 ? 1 : 2));Check(b.DealerCount==(i<54 ? 0 : 1));CasinoPokerService.Validate(s);
    }
    Check(b.Phase==2 && b.PlayerTotal==13 && CasinoBlackjackService.CanCommand(s,1));
});
Test("blackjack natural waits for callback seventy-five and awards two times",()=>
{
    var s=BlackjackFixture();CasinoBlackjackService.Command(s,2,new FailureRandom());
    var deck=BlackjackDeck();(deck[1],deck[9])=(deck[9],deck[1]);CasinoBlackjackService.Command(s,1,new FailureRandom(CardDraws(deck)));
    for (int i=0;i<74;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    Check(s.Casino.Blackjack.Natural && !s.Casino.Blackjack.Win);
    CasinoBlackjackService.Tick(s,new FailureRandom());Check(s.Casino.PendingPayout==2 && s.Casino.Blackjack.AwardTick==2);
    for (int i=0;i<28;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    Check(s.Casino.Blackjack.AwardReady);CasinoPokerService.Validate(s);
});
Test("blackjack more uses a predealt card with no random draws",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s);var r=new FailureRandom();CasinoBlackjackService.Command(s,1,r);
    for (int i=0;i<9;i++) CasinoBlackjackService.Tick(s,r);Check(b.PlayerCount==2);
    CasinoBlackjackService.Tick(s,r);Check(b.PlayerCount==3 && b.PlayerTotal==16 && r.Calls==0 && b.Phase==2);
});
Test("blackjack bust discards active stake without refunding or erasing retained pending",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s);b.Cards[0]=new(2,9);b.Cards[1]=new(2,10);b.Cards[2]=new(2,11);
    b.UsedCards=b.Cards.Select(c=>c.Identifier).ToHashSet();s.Casino.PendingPayout=71;
    CasinoBlackjackService.Command(s,1,new FailureRandom());for(int i=0;i<10;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    Check(b.Phase==0 && s.Casino.Bet==0 && s.Casino.PendingPayout==71 && s.Casino.Coins==100);CasinoPokerService.Validate(s);
});
Test("blackjack stay bonuses are not automatically awarded on hitting the count",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);Check(!b.Win);
    CasinoBlackjackService.Command(s,2,new FailureRandom());Check(b.Win && s.Casino.PendingPayout==4 && !b.AwardReady);
    for(int i=0;i<29;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(b.AwardReady);
    CasinoBlackjackService.Command(s,2,new FailureRandom());CasinoBlackjackService.Tick(s,new FailureRandom());Check(s.Casino.Coins==100);
    for(int i=0;i<7;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(s.Casino.Coins==104 && s.Casino.PendingPayout==0 && s.Casino.Bet==0);
});
Test("blackjack dealer redraw delay retains negative-nine timer restart",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s);b.PlayerTotal=20;
    b.Cards[9]=new(2,4);b.Cards[10]=new(2,5);b.Cards[11]=new(2,6);b.UsedCards=b.Cards.Select(c=>c.Identifier).ToHashSet();
    CasinoBlackjackService.Command(s,2,new FailureRandom());for(int i=0;i<10;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    Check(b.DealerCount==2 && b.DealerTotal==11 && b.Tick==-9);
    for(int i=0;i<18;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(b.DealerCount==2);
    CasinoBlackjackService.Tick(s,new FailureRandom());Check(b.DealerCount==3 && b.DealerTotal==18 && b.Win && s.Casino.PendingPayout==2);
});
Test("blackjack double-up predeals five cards and compares the two choices",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);CasinoBlackjackService.Command(s,2,new FailureRandom());
    for(int i=0;i<29;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    CasinoCard[] cards=[new(0,2),new(1,3),new(2,7),new(3,9),new(0,0)];var r=new FailureRandom(CardDraws(cards));
    CasinoBlackjackService.Command(s,1,r);Check(r.Calls==10 && b.Phase==5);
    Check(CasinoBlackjackService.SelectCard(s,0) && b.Phase==6);Check(CasinoBlackjackService.SelectCard(s,4));
    for(int i=0;i<37;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(s.Casino.PendingPayout==4);
    CasinoBlackjackService.Tick(s,new FailureRandom());Check(s.Casino.PendingPayout==8 && b.AwardReady);CasinoPokerService.Validate(s);
});
Test("blackjack double-up tie redeals and direct same-index selection remains handler-reachable",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);b.Win=true;b.AwardReady=true;b.AwardTick=30;s.Casino.PendingPayout=4;
    CasinoBlackjackService.Command(s,1,new FailureRandom(CardDraws(BlackjackDeck().Take(5).ToArray())));
    Check(CasinoBlackjackService.SelectCard(s,0) && CasinoBlackjackService.SelectCard(s,0));
    for(int i=0;i<38;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(b.TieTick==2 && s.Casino.PendingPayout==4);
    for(int i=0;i<27;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(b.Phase==7);
    var r=new FailureRandom(CardDraws(BlackjackDeck().Skip(5).Take(5).ToArray()));CasinoBlackjackService.Tick(s,r);
    Check(b.Phase==5 && r.Calls==10 && s.Casino.PendingPayout==4);CasinoPokerService.Validate(s);
});
Test("blackjack double-up loss retains inactive payout storage",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);b.Win=true;b.AwardReady=true;s.Casino.PendingPayout=4;
    CasinoBlackjackService.Command(s,1,new FailureRandom(CardDraws(BlackjackDeck().Take(5).ToArray())));
    CasinoBlackjackService.SelectCard(s,0);CasinoBlackjackService.SelectCard(s,1);
    for(int i=0;i<38;i++) CasinoBlackjackService.Tick(s,new FailureRandom());Check(b.Phase==0 && s.Casino.Bet==0 && s.Casino.PendingPayout==4);
    CasinoPokerService.Validate(s);
});
Test("blackjack refuses overflowing awards before mutation and closes without draining",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);b.Win=true;b.AwardReady=true;s.Casino.PendingPayout=long.MaxValue;
    Check(CasinoBlackjackService.Command(s,1,new FailureRandom())==CasinoResult.NumericLimit && b.Phase==2);
    Check(CasinoBlackjackService.Command(s,2,new FailureRandom())==CasinoResult.NumericLimit && s.Casino.Coins==100);
    CasinoBlackjackService.Close(s);Check(s.Casino.Bet==1 && s.Casino.PendingPayout==long.MaxValue);
});
Test("blackjack active saves roundtrip predealt cards and reject conflicting casino forms",()=>
{
    var s=BlackjackFixture();CasinoBlackjackService.Command(s,2,new FailureRandom());CasinoBlackjackService.Command(s,1,new FailureRandom(CardDraws(BlackjackDeck())));
    for(int i=0;i<36;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    var json=System.Text.Json.JsonSerializer.Serialize(s);var copy=System.Text.Json.JsonSerializer.Deserialize<GameState>(json)!;
    CasinoPokerService.Validate(copy);Check(copy.Casino.Blackjack.Tick==36 && copy.Casino.Blackjack.Cards.SequenceEqual(s.Casino.Blackjack.Cards));
    copy.Casino.PokerOpen=true;Throws<InvalidDataException>(()=>CasinoPokerService.Validate(copy));
    copy.Casino.PokerOpen=false;copy.Casino.Blackjack.Cards.RemoveAt(0);Throws<InvalidDataException>(()=>CasinoPokerService.Validate(copy));
});
Test("blackjack reaching the predealt-array boundary stops without inventing a card or award",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,9);CasinoBlackjackService.Command(s,1,new FailureRandom());
    Check(b.BoundaryReached && s.Casino.PendingPayout==0 && s.Casino.Bet==1);CasinoPokerService.Validate(s);
    Check(CasinoBlackjackService.Command(s,0,new FailureRandom())==CasinoResult.Success);
});

Test("blackjack reopen retains its win gate and allows collection with a reset stake",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);CasinoBlackjackService.Command(s,2,new FailureRandom());
    for(int i=0;i<29;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    CasinoBlackjackService.Close(s);CasinoBlackjackService.Open(s);b=s.Casino.Blackjack;
    Check(b.Win && b.ReopenedWin && s.Casino.Bet==0 && !CasinoBlackjackService.CanCommand(s,1) && CasinoBlackjackService.CanCommand(s,2));
    CasinoPokerService.Validate(s);CasinoBlackjackService.Command(s,2,new FailureRandom());
    for(int i=0;i<8;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    Check(s.Casino.Coins==104 && !b.Win && s.Casino.PendingPayout==0);CasinoPokerService.Validate(s);
});
Test("blackjack retained zero payout remains safely closable after another game's collection",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);b.Win=true;b.AwardTick=30;b.AwardReady=true;s.Casino.PendingPayout=4;
    CasinoBlackjackService.Close(s);Check(CasinoPokerService.Open(s)==CasinoResult.Success);
    CasinoPokerService.Close(s);s.Casino.PendingPayout=0;
    CasinoBlackjackService.Open(s);Check(s.Casino.Blackjack.ReopenedWin && !CasinoBlackjackService.CanCommand(s,1));
    CasinoBlackjackService.Command(s,2,new FailureRandom());
    for(int i=0;i<100;i++) CasinoBlackjackService.Tick(s,new FailureRandom());
    Check(s.Casino.Blackjack.Phase==4 && s.Casino.Coins==100);CasinoPokerService.Validate(s);
    CasinoBlackjackService.Close(s);
});
Test("blackjack pauses session progression and blocks conflicting company actions",()=>
{
    var s=BlackjackFixture();var session=new GameSession(new FailureRandom());session.Restore(s);
    var tick=s.CompanyActivityTicks;session.TickOffice();Check(s.CompanyActivityTicks==tick);
    Check(FacilityService.HasPendingDecision(s) && !FacilityService.HasPendingDecision(s,ignoreCasino:true));
    s.Casino.Blackjack.Win=true;s.Casino.Blackjack.AwardTick=30;s.Casino.PendingPayout=7;
    var next=session.StartNewGame("Next","Owner",GameMode.Normal);
    Check(next.Casino.Coins==0 && next.Casino.PendingPayout==7 && next.Casino.Blackjack.Win && !next.Casino.CardGameOpen);
});
Test("blackjack save validation refuses impossible predealt maps and overflowing collection",()=>
{
    var s=BlackjackFixture();var b=BlackjackDecision(s,5);b.Win=true;b.AwardReady=true;s.Casino.PendingPayout=4;
    CasinoBlackjackService.Command(s,2,new FailureRandom());s.Casino.Coins=long.MaxValue;
    Throws<InvalidDataException>(()=>CasinoPokerService.Validate(s));
    s.Casino.Coins=100;b.UsedCards.Remove(b.Cards[0].Identifier);Throws<InvalidDataException>(()=>CasinoPokerService.Validate(s));
});
GameState PokerFixture(long coins=100)
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=(int)FacilityKind.Casino;s.Casino.Coins=coins;
    Check(CasinoPokerService.Open(s)==CasinoResult.Success);return s;
}
float[] CardDraws(params CasinoCard[] cards)=>cards.SelectMany(c=>new[]{(c.Suit+.01f)/4,(c.Rank+.01f)/13}).ToArray();
PokerRoundState ReadyPoker(GameState s,params CasinoCard[] cards)
{
    var p=s.Casino.Poker;p.Cards=cards.ToList();p.UsedCards=cards.Select(x=>x.Identifier).ToHashSet();
    p.Revealed=[true,true,true,true,true];p.Phase=1;p.Tick=45;s.Casino.Bet=1;
    return p;
}
Test("work sound cues occur once at medium or burst start and never on restore",()=>
{
    foreach(var (draw,cue) in new[]{(.2,(OriginalSoundCue?)OriginalSoundCue.Up),(0d,(OriginalSoundCue?)OriginalSoundCue.Fire),(.99,(OriginalSoundCue?)null)})
    {
        var s=PlanningState(200000000);var e=Worker();e.Office!.WorkIdleTicks=100;s.Employees=[e];s.DevelopmentProjects=[CancellationProject()];
        var session=new GameSession(new AudioWorkRandom(draw));var cues=new List<OriginalSoundCue>();int resets=0;session.SoundRequested+=cues.Add;session.SoundResetRequested+=()=>resets++;
        session.Restore(s);Check(resets==1 && cues.Count==0);session.TickOffice(CoreCatalog());Check(e.Office.WorkTicks==2);
        Check(cue is null ? cues.Count==0 : cues.SequenceEqual(new[]{cue.Value}));session.TickOffice(CoreCatalog());Check(cues.Count==(cue is null?0:1));
    }
});
Test("original audio notifications follow casino transactions and silent native branches",()=>
{
    var s=PokerFixture();var session=new GameSession(new GreetingRandom(0));session.Restore(s);var cues=new List<OriginalSoundCue>();session.SoundRequested+=cues.Add;
    Check(session.PokerCommand(0)==CasinoResult.Success && cues.SequenceEqual(new[]{OriginalSoundCue.Gold}));
    var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));p.Tick=0;p.Revealed=[false,false,false,false,false];cues.Clear();
    for(int i=0;i<45;i++)session.TickPoker();Check(cues.Count==5 && cues.All(x=>x==OriginalSoundCue.Up));
    p.Phase=2;p.Tick=0;p.RevealOrder=[0];p.Revealed[0]=false;cues.Clear();for(int i=0;i<9;i++)session.TickPoker();Check(cues.Count==0);
    p.Phase=4;p.Tick=0;s.Casino.PendingPayout=2;session.TickPoker();Check(cues.Count==0);session.TickPoker();Check(cues.SequenceEqual(new[]{OriginalSoundCue.Gold}));
    cues.Clear();session.Restore(s);Check(cues.Count==0); // Loading/rendering is not a cue.
});
Test("original poker double comparison cues exclude its dealer-only reveal",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,4),new(1,5),new(2,6),new(3,7),new(0,8));p.Phase=5;p.Tick=18;p.Revealed=[true,false,false,false,false];s.Casino.PendingPayout=2;
    var session=new GameSession(new FailureRandom());session.Restore(s);var cues=new List<OriginalSoundCue>();session.SoundRequested+=cues.Add;
    session.TickPoker();Check(cues.Count==0);Check(CasinoPokerService.SelectCard(s,1));
    for(int i=0;i<28;i++)session.TickPoker();Check(cues.Count==4 && cues.All(x=>x==OriginalSoundCue.Up));
});
Test("card-chain selection and blackjack collection use their own native audio calls",()=>
{
    var s=CardChainFixture();var p=s.Casino.CardChain.Round;p.Phase=1;p.Cards=[new(0,0),new(1,1),new(2,2),new(3,3),new(0,4)];p.UsedCards=p.Cards.Select(x=>x.Identifier).ToHashSet();p.Revealed=[true,true,true,true,true];
    var session=new GameSession(new FailureRandom());session.Restore(s);var cues=new List<OriginalSoundCue>();session.SoundRequested+=cues.Add;
    Check(session.SelectChainCard(0));Check(cues.SequenceEqual(new[]{OriginalSoundCue.Up}));session.TickCardChain();Check(cues.Count==1);
    s=BlackjackFixture();session.Restore(s);cues.Clear();Check(session.BlackjackCommand(2)==CasinoResult.Success && cues.Single()==OriginalSoundCue.Gold);
    var b=s.Casino.Blackjack;b.Phase=1;b.Tick=17;b.Win=false;cues.Clear();session.TickBlackjack();Check(cues.Count==0);
    b.Phase=4;b.CollectTick=0;s.Casino.PendingPayout=3;session.TickBlackjack();session.TickBlackjack();Check(cues.SequenceEqual(new[]{OriginalSoundCue.Gold}));
});
Test("poker uses the recovered nine categories, total-return multipliers and high-pair threshold",()=>
{
    var hands=new (int Outcome,CasinoCard[] Cards)[] {
        (0,[new(0,1),new(1,1),new(2,4),new(3,7),new(0,11)]),
        (1,[new(0,9),new(1,9),new(2,4),new(3,7),new(0,11)]),
        (1,[new(0,0),new(1,0),new(2,4),new(3,7),new(0,11)]),
        (2,[new(0,1),new(1,1),new(2,4),new(3,4),new(0,11)]),
        (3,[new(0,1),new(1,1),new(2,1),new(3,4),new(0,11)]),
        (4,[new(0,0),new(1,1),new(2,2),new(3,3),new(0,4)]),
        (4,[new(0,8),new(1,9),new(2,10),new(3,11),new(0,12)]),
        (4,[new(0,0),new(1,9),new(2,10),new(3,11),new(0,12)]),
        (5,[new(0,0),new(0,2),new(0,4),new(0,7),new(0,12)]),
        (6,[new(0,1),new(1,1),new(2,1),new(3,4),new(0,4)]),
        (7,[new(0,1),new(1,1),new(2,1),new(3,1),new(0,11)]),
        (8,[new(2,0),new(2,1),new(2,2),new(2,3),new(2,4)]),
        (9,[new(3,0),new(3,9),new(3,10),new(3,11),new(3,12)])};
    foreach (var (outcome,cards) in hands) Check(CasinoPokerService.Classify(cards)==outcome);
    Check(CasinoPokerService.Multipliers.SequenceEqual(new[]{0,1,2,3,10,15,20,100,500,1000}));
    Check(CasinoPokerService.Classify([new(0,0),new(1,1),new(2,10),new(3,11),new(0,12)])==0);
    Throws<ArgumentException>(()=>CasinoPokerService.Classify([new(0,0),new(0,0),new(0,1),new(0,2),new(0,3)]));
});
Test("coin purchase debits the financial helper without an affordability gate or redemption",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=4;s.Money=1;s.Company.WeeklyNet=10;s.Company.MonthlyNet=20;s.Company.YearlyNet=30;
    Check(CasinoPokerService.BuyCoins(s)==CasinoResult.Success && s.Casino.Coins==50 && s.Money==-9999999);
    Check(s.Company.WeeklyNet==-9999990 && s.Company.MonthlyNet==-9999980 && s.Company.YearlyNet==-9999970);
    Check(CasinoPokerService.Open(s)==CasinoResult.Success);Check(CasinoPokerService.BuyCoins(s)==CasinoResult.EventPending && s.Casino.Coins==50);
});
Test("poker entry checks coins and ownership and resets form phase while retaining shared amounts",()=>
{
    var (s,_)=CalendarFixture();Check(CasinoPokerService.Open(s)==CasinoResult.NotOwned);
    s.Calendar!.OriginalSlots0444[0]=4;Check(CasinoPokerService.Open(s)==CasinoResult.NoCoins);
    s.Casino.Coins=50;s.Casino.Bet=17;s.Casino.PendingPayout=100;s.Casino.ChainBonus=31;
    Check(CasinoPokerService.Open(s)==CasinoResult.Success && !CasinoPokerService.CanCommand(s,1));
    Check(s.Casino.Bet==17 && s.Casino.PendingPayout==100 && s.Casino.ChainBonus==31 && s.Casino.Poker.Phase==0);
});
Test("poker spends one coin per accepted increment with native fifty cap and failed-command entry draw",()=>
{
    var s=PokerFixture(55);var random=new GreetingRandom(0);
    Check(CasinoPokerService.Command(s,1,random)==CasinoResult.InvalidAction && random.GreetingCalls==1);
    for (int i=0;i<50;i++) Check(CasinoPokerService.Command(s,0,random)==CasinoResult.Success);
    Check(s.Casino.Coins==5 && s.Casino.Bet==50 && CasinoPokerService.CanCommand(s,1));
    Check(CasinoPokerService.Command(s,0,random)==CasinoResult.InvalidAction && random.GreetingCalls==52 && s.Casino.Coins==5);
    CasinoPokerService.Close(s);CasinoPokerService.Open(s);Check(!CasinoPokerService.CanCommand(s,0) && !CasinoPokerService.CanCommand(s,1));
});
Test("poker rejection sampling consumes both draws on duplicates and marks source card identity",()=>
{
    var used=new HashSet<int>{0};var random=new FailureRandom(0,0,0,.1f);
    var card=CasinoPokerService.Draw(used,random);Check(card==new CasinoCard(0,1) && random.Calls==4 && used.SetEquals(new[]{0,1}));
    Throws<InvalidOperationException>(()=>CasinoPokerService.Draw(Enumerable.Range(0,52).ToHashSet(),new FailureRandom()));
});
Test("initial poker deal is generated once and reveals at nine-event intervals through forty-five",()=>
{
    var s=PokerFixture();CasinoPokerService.Command(s,0,new FailureRandom(0));
    CasinoCard[] cards=[new(0,0),new(1,2),new(2,4),new(3,6),new(0,8)];
    var random=new FailureRandom(new[]{0f}.Concat(CardDraws(cards)).ToArray());Check(CasinoPokerService.Command(s,1,random)==CasinoResult.Success && random.Calls==11);
    for (int tick=1;tick<=45;tick++)
    {
        CasinoPokerService.Tick(s,new FailureRandom());Check(s.Casino.Poker.Revealed.Count(x=>x)==tick/9);
        Check(CasinoPokerService.CanCommand(s,1)==(tick==45));
    }
    Check(s.Casino.Poker.Cards.SequenceEqual(cards) && s.Casino.Coins==99);
});
Test("poker replaces only unheld cards from the existing used map in source index order",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,0),new(1,2),new(2,4),new(3,6),new(0,8));
    foreach (int i in new[]{0,2,4}) Check(CasinoPokerService.SelectCard(s,i));
    var random=new FailureRandom(new[]{0f}.Concat(CardDraws(new(1,9),new(2,11))).ToArray());
    Check(CasinoPokerService.Command(s,1,random)==CasinoResult.Success && random.Calls==5);
    Check(p.Cards.SequenceEqual(new CasinoCard[]{new(0,0),new(1,9),new(2,4),new(2,11),new(0,8)}) && p.UsedCards.Count==7);
    for (int i=0;i<18;i++) CasinoPokerService.Tick(s,new FailureRandom());
    Check(p.Phase==0 && s.Casino.Bet==0 && p.Revealed.All(x=>x));
});
Test("holding all cards enters the award after the source result delay and excludes the returned stake",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));s.Casino.Bet=7;
    foreach (int i in Enumerable.Range(0,5)) CasinoPokerService.SelectCard(s,i);
    CasinoPokerService.Command(s,1,new FailureRandom(0));CasinoPokerService.Tick(s,new FailureRandom());
    Check(p.Phase==3 && p.Outcome==1 && !p.AwardReady);
    for (int i=0;i<32;i++) CasinoPokerService.Tick(s,new FailureRandom());Check(!p.AwardReady);
    CasinoPokerService.Tick(s,new FailureRandom());Check(p.AwardReady && s.Casino.PendingPayout==7 && s.Casino.Coins==100);
});
Test("poker collection credits exactly one coin per two callbacks and close does not drain or refund",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));p.Phase=3;p.Outcome=1;p.AwardReady=true;s.Casino.PendingPayout=3;
    Check(CasinoPokerService.Command(s,0,new FailureRandom(0))==CasinoResult.Success);
    CasinoPokerService.Tick(s,new FailureRandom());Check(s.Casino.Coins==100 && s.Casino.PendingPayout==3);
    CasinoPokerService.Tick(s,new FailureRandom());Check(s.Casino.Coins==101 && s.Casino.PendingPayout==2);
    Check(CasinoPokerService.Close(s) && s.CompanyActivityPerformed==true && s.Casino.Bet==1 && s.Casino.PendingPayout==2);
    CasinoPokerService.Tick(s,new FailureRandom());Check(s.Casino.Coins==101 && s.Casino.PendingPayout==2);
    Check(!CasinoPokerService.Close(s));
});
Test("poker complete collection clears phase and stake after the last coin",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));p.Phase=3;p.Outcome=1;p.AwardReady=true;s.Casino.PendingPayout=1;
    CasinoPokerService.Command(s,0,new FailureRandom(0));CasinoPokerService.Tick(s,new FailureRandom());CasinoPokerService.Tick(s,new FailureRandom());
    Check(p.Phase==0 && s.Casino.Coins==101 && s.Casino.Bet==0 && s.Casino.PendingPayout==0 && !p.DealEnabled);
});
Test("poker double-up delays generation until callback eighteen and uses a fresh rejection map",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));p.Phase=3;p.Outcome=1;p.AwardReady=true;s.Casino.PendingPayout=3;
    var entry=new FailureRandom(0);CasinoPokerService.Command(s,1,entry);Check(entry.Calls==1 && p.UsedCards.Count==0 && p.Phase==5);
    for (int i=0;i<17;i++) CasinoPokerService.Tick(s,new FailureRandom());
    Check(!CasinoPokerService.SelectCard(s,1));
    var draws=new FailureRandom(CardDraws(new(0,4),new(1,5),new(2,6),new(3,7),new(0,8)));CasinoPokerService.Tick(s,draws);
    Check(draws.Calls==10 && p.Revealed[0] && p.Revealed.Count(x=>x)==1 && p.UsedCards.Count==5 && s.Casino.Coins==100 && s.Casino.PendingPayout==3);
});
Test("double-up compares aces high, doubles only pending winnings and retains storage after loss",()=>
{
    foreach (bool win in new[]{false,true})
    {
        var s=PokerFixture();var p=ReadyPoker(s,new(0,12),new(1,0),new(2,2),new(3,3),new(0,4));p.Phase=5;p.Tick=18;p.Revealed=[true,false,false,false,false];
        if (!win) p.Cards=[new(0,0),new(1,12),new(2,2),new(3,3),new(0,4)];
        s.Casino.PendingPayout=7;Check(!CasinoPokerService.SelectCard(s,0) && CasinoPokerService.SelectCard(s,1));
        for (int i=0;i<28;i++) CasinoPokerService.Tick(s,new FailureRandom());
        Check(s.Casino.Coins==100 && s.Casino.PendingPayout==(win ? 14 : 7) && p.Phase==(win ? 3 : 0));
        Check(s.Casino.Bet==(win ? 1 : 0));
    }
});
Test("equal double-up ranks retain the award then redeal after the native tie delay",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,4),new(1,4),new(2,6),new(3,7),new(0,8));p.Phase=5;p.Tick=18;s.Casino.PendingPayout=9;
    CasinoPokerService.SelectCard(s,1);for (int i=0;i<28;i++) CasinoPokerService.Tick(s,new FailureRandom());
    Check(p.Phase==6 && p.TieRedeal && p.TieTick==2 && s.Casino.PendingPayout==9);
    for (int i=0;i<27;i++) CasinoPokerService.Tick(s,new FailureRandom());Check(p.Phase==6);
    CasinoPokerService.Tick(s,new FailureRandom());Check(p.Phase==5 && p.Tick==0 && p.UsedCards.Count==0 && s.Casino.PendingPayout==9);
});
Test("modern poker saves resume reveal and partial payout without replaying draws or credits",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));p.Phase=3;p.Outcome=1;p.AwardReady=true;s.Casino.PendingPayout=4;
    CasinoPokerService.Command(s,0,new FailureRandom(0));CasinoPokerService.Tick(s,new FailureRandom());
    s=SaveStore.Deserialize(SaveStore.Serialize(s));CasinoPokerService.Tick(s,new FailureRandom());
    Check(s.Casino.Coins==101 && s.Casino.PendingPayout==3);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.Casino.Coins==101 && s.Casino.Poker.Phase==4);
});
Test("casino pauses office and blocks other modal transactions while preserving accounting",()=>
{
    var s=PokerFixture();var session=new GameSession(new FailureRandom());session.Restore(s);string before=SaveStore.Serialize(s);
    session.TickOffice();Check(SaveStore.Serialize(s)==before);
    Check(FacilityService.Close(s,0,FacilityKind.Casino)==FacilityResult.EventPending && !FinancialFailureService.Begin(s));
});
Test("casino numeric boundary rejects an unrepresentable action before changing coins or pending payout",()=>
{
    var s=PokerFixture();var p=ReadyPoker(s,new(0,9),new(1,9),new(2,4),new(3,7),new(0,11));
    p.Phase=3;p.Outcome=1;p.AwardReady=true;s.Casino.PendingPayout=long.MaxValue;
    var random=new FailureRandom(0,0);Check(CasinoPokerService.Command(s,0,random)==CasinoResult.NumericLimit);
    Check(CasinoPokerService.Command(s,1,random)==CasinoResult.NumericLimit && random.Calls==2);
    Check(s.Casino.Coins==100 && s.Casino.PendingPayout==long.MaxValue && p.Phase==3);
});
Test("new company resets original coins while preserving native shared casino scratch storage",()=>
{
    var s=PokerFixture();s.Casino.Bet=17;s.Casino.PendingPayout=37;s.Casino.ChainBonus=5;
    var session=new GameSession();session.Restore(s);var fresh=session.StartNewGame("New","Owner",GameMode.Normal);
    Check(fresh.Casino.Coins==0 && fresh.Casino.Bet==17 && fresh.Casino.PendingPayout==37 && fresh.Casino.ChainBonus==5 && !fresh.Casino.PokerOpen);
});
Test("casino malformed phases cards maps and active overlaps are rejected",()=>
{
    var mutations=new Action<GameState>[] {s=>s.Casino.Coins=-1,s=>s.Casino.Bet=51,s=>s.Casino.PendingPayout=-1,
        s=>s.Casino.Poker.Phase=7,s=>s.Casino.Poker.Holds.RemoveAt(0),s=>s.Casino.Poker.UsedCards.Add(52),
        s=>s.Calendar!.OriginalSlots0444[0]=-1,s=>s.Casino.Poker.RevealOrder=[0,0],s=>s.Casino.Poker.Phase=1};
    foreach (var mutate in mutations) {var s=PokerFixture();mutate(s);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));}
});
GameState FailureFixture(long? money=-1)
{
    var (s,_)=CalendarFixture();s.Money=money;s.SourceUpperBound=3;s.OriginalPayrollScratchIndex=2;
    s.EmployeeSources=Enumerable.Range(0,3).Select(i=>new RecruitmentSourceRecord {SourceIndex=i,GateValue=i==1 ? 0 : 1,
        Employee=new() {Name="Synthetic "+i,SourceIndex=i,PortraitId=i+1,Stress=9,RawParameter40=100}}).Reverse().ToList();
    return s;
}
Test("financial failure triggers strictly below zero in either mode without cash clamping",()=>
{
    foreach (var mode in Enum.GetValues<GameMode>()) foreach (long? cash in new long?[]{null,0,1,-1,-3})
    {
        var s=FailureFixture(cash);s.Mode=mode;
        Check(FinancialFailureService.Begin(s)==(cash<0));Check(s.Money==cash);
        Check(!FinancialFailureService.Begin(s));
    }
});
Test("failure consumes every source gate draw and only selected employed reaction draws in source order",()=>
{
    var s=FailureFixture();var random=new FailureRandom(.9f,.0f,.9f,.9f,.999f);
    Check(FinancialFailureService.Begin(s));Check(FinancialFailureService.Acknowledge(s,random) && random.Calls==0);
    Check(FinancialFailureService.Acknowledge(s,random) && random.Calls==2 && s.FinancialFailure!.Reaction!.SourceIndex==0 && s.FinancialFailure.Reaction.Variant==0);
    Check(FinancialFailureService.Acknowledge(s,random) && random.Calls==5 && s.FinancialFailure!.Reaction!.SourceIndex==2 && s.FinancialFailure.Reaction.Variant==7);
    Check(FinancialFailureService.Acknowledge(s,random) && random.Calls==5 && s.FinancialFailure!.Phase==3);
    Check(FinancialFailureService.Acknowledge(s,random) && FinancialFailureService.IsComplete(s));
    Check(!FinancialFailureService.Acknowledge(s,random) && s.OriginalPayrollScratchIndex==2);
});
Test("all eight failure reactions resume without repeating their selection draws",()=>
{
    for (int variant=0;variant<8;variant++)
    {
        var s=FailureFixture();s.SourceUpperBound=1;s.EmployeeSources=s.EmployeeSources.Where(x=>x.SourceIndex==0).ToList();
        var r=new FailureRandom(.5f,(variant+.01f)/8);FinancialFailureService.Begin(s);
        FinancialFailureService.Acknowledge(s,r);s=SaveStore.Deserialize(SaveStore.Serialize(s));
        FinancialFailureService.Acknowledge(s,r);Check(s.FinancialFailure!.Reaction!.Variant==variant && r.Calls==2);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));var empty=new FailureRandom();
        FinancialFailureService.Acknowledge(s,empty);Check(s.FinancialFailure!.Phase==3 && empty.Calls==0);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));FinancialFailureService.Acknowledge(s,empty);Check(FinancialFailureService.IsComplete(s));
    }
});
Test("failure skips unselected employed reactions and still scans unemployed slots",()=>
{
    var s=FailureFixture();var r=new FailureRandom(0,.999f,.499f);FinancialFailureService.Begin(s);
    FinancialFailureService.Acknowledge(s,r);FinancialFailureService.Acknowledge(s,r);
    Check(r.Calls==3 && s.FinancialFailure!.Phase==3 && s.FinancialFailure.NextSource==3);
});
Test("unknown legacy failure roster suspends without fabricated reactions or random draws",()=>
{
    var s=FailureFixture();s.SourceUpperBound=null;var r=new FailureRandom();FinancialFailureService.Begin(s);
    FinancialFailureService.Acknowledge(s,r);FinancialFailureService.Acknowledge(s,r);
    Check(s.FinancialFailure!.Phase==2 && s.FinancialFailure.Slots is null && r.Calls==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(!FinancialFailureService.Acknowledge(s,r));
    var session=new GameSession(r);session.Restore(s);string before=SaveStore.Serialize(s);session.TickOffice();Check(SaveStore.Serialize(s)==before);
});
Test("failure preserves company gameplay cells and stops office work and new transactions",()=>
{
    var s=FailureFixture();var p=CancellationProject();s.DevelopmentProjects.Add(p);s.Company.WeeklyNet=17;s.MysteryLab.Progress=18;
    var r=new FailureRandom(0,0,0);var session=new GameSession(r);session.Restore(s);FinancialFailureService.Begin(s);
    string before=SaveStore.Serialize(s);session.TickOffice();Check(SaveStore.Serialize(s)==before && ReferenceEquals(s.DevelopmentProjects[0],p));
    Check(FacilityService.HasPendingDecision(s) && BankLoanService.CheckBorrow(s)==BankLoanResult.EventPending);
    Check(s.Money==-1 && s.Company.WeeklyNet==17 && s.MysteryLab.Progress==18);
    Check(s.EmployeeSources.All(x=>x.Employee.Stress==9) && s.OriginalPayrollScratchIndex==2);
});
Test("failure waits for earlier modal receipts and resumes at the late office-loop trigger",()=>
{
    var s=FailureFixture();s.PendingBankLoanReceipt=new() {Slot=0,OriginalType=1,BorrowedOn=new(3,5,1)};
    Check(!FinancialFailureService.Begin(s));s.PendingBankLoanReceipt=null;
    var session=new GameSession(new FailureRandom(.5f));session.Restore(s);session.TickOffice();Check(s.FinancialFailure?.Phase==0);
});
Test("invalid failure cursors reactions snapshots and overlaps are rejected",()=>
{
    var mutations=new Action<GameState>[] {s=>s.FinancialFailure!.Phase=5,s=>s.FinancialFailure!.NextSource=-1,
        s=>s.FinancialFailure!.Reaction=new() {SourceIndex=0,Variant=0},s=>s.Money=0,
        s=>s.FinancialFailure!.Slots![0].SourceIndex=3,s=>s.FinancialFailure!.Slots![0].PortraitId=11,
        s=>s.FinancialFailure!.Slots![1].Employed=true,s=>s.FinancialFailure!.Phase=3,
        s=>s.PendingHireGreeting=new() {EmployeeSourceIndex=0,Variant=0}};
    foreach (var mutate in mutations) {var s=FailureFixture();FinancialFailureService.Begin(s);mutate(s);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));}
});
Test("failure continuation rejects missing required phase and reaction fields",()=>
{
    var s=FailureFixture();FinancialFailureService.Begin(s);
    string json=SaveStore.Serialize(s);Throws<InvalidDataException>(()=>SaveStore.Deserialize(json.Replace("\"Phase\": 0,","")));
    var r=new FailureRandom(.9f,.3f);FinancialFailureService.Acknowledge(s,r);FinancialFailureService.Acknowledge(s,r);
    json=SaveStore.Serialize(s);Throws<InvalidDataException>(()=>SaveStore.Deserialize(json.Replace("\"Variant\": 2","\"Unused\": 2")));
});
Test("terminal failure disables save replacement and new company starts independently",()=>
{
    var s=FailureFixture();var r=new FailureRandom(0,0,0);FinancialFailureService.Begin(s);
    while (FinancialFailureService.Acknowledge(s,r)) {}
    var path=Path.Combine(Path.GetTempPath(),"opengds-failure-"+Guid.NewGuid()+".json");
    try {File.WriteAllText(path,"preserved");Throws<InvalidOperationException>(()=>SaveStore.Save(path,s));Check(File.ReadAllText(path)=="preserved");}
    finally {File.Delete(path);}
    var session=new GameSession();session.Restore(s);var fresh=session.StartNewGame("New","Owner",GameMode.Normal);
    Check(fresh.FinancialFailure is null && fresh.Money==450000000 && s.Money==-1);
});
DevelopmentProjectState CancellationProject(int room=0)=>new()
    {RoomIndex=room,Title="Synthetic cancellation",RequiredPoints=1000,Focus=new(100,0,0,0)};
Test("cancellation selects all six assigned desks in either room and clamps stress to 99",()=>
{
    foreach (int room in new[]{0,1})
    foreach (int desk in Enumerable.Range(0,6))
    foreach (double stress in new[]{0d,78.5,79,99,100})
    {
        var s=PlanningState();var p=CancellationProject(room);s.DevelopmentProjects.Add(p);
        var e=Worker(6+6*room+desk);e.Stress=stress;
        // Assigned but not yet sitting: still penalized in the original.
        e.Office!.SeatedAction=0;e.Office.WorkEngaged=false;e.Office.MovementState=3;
        var other=Worker(6+6*(1-room),1);other.Stress=7;s.Employees=[e,other];
        Check(ProjectCancellationService.Check(s,p)==ProjectCancellationResult.Success && e.Stress==stress);
        Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.Success);
        Check(e.Stress==Math.Min(99,stress+20) && other.Stress==7);
        Check(e.Office.Action==6+6*room+desk && e.Office.SeatedAction==0 && s.DevelopmentProjects.Count==0);
        Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.ProjectUnavailable && e.Stress==Math.Min(99,stress+20));
    }
});
Test("cancellation has no refund shipment sequel or source-gate side effects across all own-project modes",()=>
{
    foreach (int mode in new[]{0,1,2})
    {
        var s=PlanningState(321);s.Company.WeeklyNet=3;s.Company.MonthlyNet=4;s.Company.YearlyNet=5;
        var p=CancellationProject();p.OriginalMode=mode;s.DevelopmentProjects.Add(p);
        var other=CancellationProject(1);s.DevelopmentProjects.Add(other);var e=Worker(source:4);e.Stress=12;s.Employees.Add(e);
        var source=new RecruitmentSourceRecord {SourceIndex=4,GateValue=1,Employee=Worker(source:4)};s.EmployeeSources.Add(source);
        Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.Success);
        Check(e.Stress==32 && source.Employee.Stress==32 && source.GateValue==1);
        Check(s.Money==321 && s.Company.WeeklyNet==3 && s.Company.MonthlyNet==4 && s.Company.YearlyNet==5);
        Check(s.DevelopmentProjects.SequenceEqual(new[]{other}) && s.ReleasedGames.Count==0 && s.ProductionPerformedThisWeek==false);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
        Check(s.DevelopmentProjects.Single().RoomIndex==1 && s.Employees.Single().Stress==32 && s.EmployeeSources.Single().Employee.Stress==32);
    }
});
Test("cancellation rejects unknown legacy staff data and pending decisions atomically",()=>
{
    var s=PlanningState();var p=CancellationProject();s.DevelopmentProjects.Add(p);
    var known=Worker();known.Stress=10;var unknown=Worker(7,1);unknown.Stress=null;s.Employees=[known,unknown];
    Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.StateUnknown && known.Stress==10 && s.DevelopmentProjects.Contains(p));
    unknown.Stress=0;unknown.Office=null;
    Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.StateUnknown && known.Stress==10);
    unknown.Office=Worker(12).Office;unknown.Stress=null;
    p.PendingDecision=DevelopmentMilestone.SelectType;
    Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.DecisionPending && known.Stress==10);
    p.PendingDecision=null;p.OriginalRoomState=2;
    Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.ProjectUnavailable && known.Stress==10);
    p.OriginalRoomState=1;
    // Unknown stress in the unaffected room does not block a known transaction.
    Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.Success && known.Stress==30 && unknown.Stress is null);
});
Test("cancelled bursts and technology cannot leak into an immediately reused room or touch the other room",()=>
{
    var s=PlanningState();var p=CancellationProject();var other=CancellationProject(1);s.DevelopmentProjects=[p,other];
    var e=Worker();e.Office!.WorkTicks=40;e.Office.WorkTier=3;e.Office.BurstTicks=4;e.Office.BurstCount=7;
    e.Office.TechnologyApplication=new() {TechnologySourceIndex=0,RemainingUnits=3};
    var second=Worker(12,1);second.Office!.SlotIndex=1;second.Office.WorkTicks=40;second.Office.WorkTier=3;second.Office.BurstTicks=4;
    s.Employees=[e,second];
    Check(ProjectCancellationService.TryCancel(s,p)==ProjectCancellationResult.Success);
    Check(e.Office.WorkTicks==0 && e.Office.WorkTier==0 && e.Office.BurstTicks==0 && e.Office.TechnologyApplication is null);
    Check(e.Office.Action==6 && e.Office.SeatedAction==6 && e.Office.WorkEngaged);
    Check(second.Office.WorkTicks==40 && second.Office.WorkTier==3 && second.Office.BurstTicks==4 && second.Stress==0);
    while (ProjectCancellationDialogueRules.Acknowledge(s)) {}
    var next=CancellationProject();s.DevelopmentProjects.Add(next);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];next=s.DevelopmentProjects.Single(x=>x.RoomIndex==0);
    DevelopmentSimulation.TickEmployee(s,e,CoreCatalog(),new WorkRandom());
    Check(next.Progress==0 && next.Quality==new DevelopmentQuality() && next.Bugs==0);
    Check(e.Office!.TechnologyApplication is null && e.Office.WorkTier==0);
});
Test("cancellation resolves corrupt source rows before changing stress or room state",()=>
{
    var s=PlanningState();var p=CancellationProject();s.DevelopmentProjects.Add(p);var e=Worker();s.Employees.Add(e);
    s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=Worker()},new() {SourceIndex=0,GateValue=1,Employee=Worker()}];
    Throws<InvalidOperationException>(()=>ProjectCancellationService.TryCancel(s,p));
    Check(e.Stress==0 && s.DevelopmentProjects.Contains(p));
});
Test("cancellation dialogue selects each original reaction once in employed order",()=>
{
    for (int variant=0;variant<7;variant++)
    {
        var s=PlanningState();var p=CancellationProject(1);s.DevelopmentProjects.Add(p);
        var a=Worker(12,8);var b=Worker(17,3);b.Office!.SlotIndex=1;b.Office.SeatedAction=0;b.Office.WorkEngaged=false;b.Office.MovementState=3;
        s.Employees=[a,Worker(6,9),b];var random=new GreetingRandom((variant+.25f)/7);
        Check(ProjectCancellationService.TryCancel(s,p,random)==ProjectCancellationResult.Success);
        var pending=s.PendingProjectCancellation!;
        Check(random.GreetingCalls==2 && pending.RoomIndex==1 && pending.MessageIndex==0);
        Check(pending.Staff.Select(x=>x.SourceIndex).SequenceEqual(new int?[]{8,3}));
        Check(pending.Staff.All(x=>x.Variant==variant && x.PortraitId==1));
        Check(s.Employees[0].Stress==20 && s.Employees[1].Stress==0 && s.Employees[2].Stress==20);
    }
});
Test("cancellation dialogue cursor survives every page reload without rerolls or penalties",()=>
{
    var s=PlanningState();var p=CancellationProject();var other=CancellationProject(1);s.DevelopmentProjects=[p,other];
    var e=Worker();s.Employees.Add(e);var random=new GreetingRandom(.95f);
    Check(ProjectCancellationService.TryCancel(s,p,random)==ProjectCancellationResult.Success);
    Check(ProjectCancellationService.TryCancel(s,other,random)==ProjectCancellationResult.DecisionPending);
    for (int page=0;page<3;page++)
    {
        s=SaveStore.Deserialize(SaveStore.Serialize(s));var pending=s.PendingProjectCancellation!;
        Check(pending.MessageIndex==page && pending.Staff[0].Variant==6 && pending.Staff[0].Name==e.Name);
        Check(s.Employees[0].Stress==20 && s.Money==100 && s.DevelopmentProjects.Single().RoomIndex==1);
        Check(ProjectCancellationDialogueRules.Acknowledge(s));
    }
    Check(s.PendingProjectCancellation is null && !ProjectCancellationDialogueRules.Acknowledge(s) && random.GreetingCalls==1);
});
Test("unoccupied cancellation has two summaries and no random employee draw",()=>
{
    var s=PlanningState();var p=CancellationProject();s.DevelopmentProjects.Add(p);var random=new GreetingRandom(.5f);
    Check(ProjectCancellationService.TryCancel(s,p,random)==ProjectCancellationResult.Success);
    Check(s.PendingProjectCancellation!.Staff.Count==0 && random.GreetingCalls==0);
    Check(ProjectCancellationDialogueRules.Acknowledge(s) && s.PendingProjectCancellation!.MessageIndex==1);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(ProjectCancellationDialogueRules.Acknowledge(s) && s.PendingProjectCancellation is null);
});
Test("bad cancellation random values reject before changing stress and project",()=>
{
    foreach (float roll in new[]{-1f,1f,float.NaN,float.PositiveInfinity})
    {
        var s=PlanningState();var p=CancellationProject();s.DevelopmentProjects.Add(p);s.Employees.Add(Worker());
        Throws<ArgumentOutOfRangeException>(()=>ProjectCancellationService.TryCancel(s,p,new GreetingRandom(roll)));
        Check(s.Employees[0].Stress==0 && s.PendingProjectCancellation is null && s.DevelopmentProjects.Contains(p));
    }
});
Test("corrupt cancellation dialogue saves reject and old saves default to no pending sequence",()=>
{
    Action<ProjectCancellationDialogueState>[] corruptions=[p=>p.RoomIndex=2,p=>p.MessageIndex=-1,p=>p.MessageIndex=3,
        p=>p.Staff=null!,p=>p.Staff[0].Variant=7,p=>p.Staff[0].Name="",p=>p.Staff[0].PortraitId=11,
        p=>p.Staff.Add(p.Staff[0])];
    foreach (var corrupt in corruptions)
    {
        var s=PlanningState();var p=CancellationProject();s.DevelopmentProjects.Add(p);s.Employees.Add(Worker());
        Check(ProjectCancellationService.TryCancel(s,p,new GreetingRandom(0))==ProjectCancellationResult.Success);
        corrupt(s.PendingProjectCancellation!);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    }
    var old=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(PlanningState()))!;
    Check(old["State"]!.AsObject().Remove("PendingProjectCancellation"));
    Check(SaveStore.Deserialize(old.ToJsonString()).PendingProjectCancellation is null);
});
Test("pending cancellation suspends the session clock recruitment and direct development work",()=>
{
    var s=PlanningState();var p=CancellationProject();s.DevelopmentProjects.Add(p);s.Employees.Add(Worker());
    var session=new GameSession(new GreetingRandom(0));session.Restore(s);
    Check(session.CancelProject(p)==ProjectCancellationResult.Success);
    string before=SaveStore.Serialize(s);session.TickOffice(CoreCatalog());
    DevelopmentSimulation.TickEmployee(s,s.Employees[0],CoreCatalog(),new WorkRandom());
    Check(SaveStore.Serialize(s)==before);
    Throws<InvalidOperationException>(()=>session.OpenRecruitment());
    Check(session.Hire(Worker())==HireResult.ConversationPending && session.AskSecretary(CoreCatalog())==SecretaryAdviceResult.EventPending);
    while (ProjectCancellationDialogueRules.Acknowledge(s)) {}
    session.TickOffice(CoreCatalog());Check(s.CompanyActivityTicks==1);
});
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
void AddDialogueSources(GameState s)
{
    s.SourceUpperBound=3;s.MinimumSourceIndex=0;s.MaximumSourceIndex=2;
    s.EmployeeSources=Enumerable.Range(0,3).Select(i=>new RecruitmentSourceRecord
        {SourceIndex=i,GateValue=i==1 ? 0 : 1,Employee=new() {Name=$"Synthetic speaker {i}",SourceIndex=i,PortraitId=i+1,RawParameter40=1000+100*i}}).ToList();
    s.Employees=s.EmployeeSources.Where(x=>x.GateValue==1).Select(x=>x.Employee).ToList();
}
Test("random dialogue portraits rebuild payroll scratch and use source-ordered employed gates",()=>
{
    var(s,p,c)=CheckpointFixture();AddDialogueSources(s);s.OriginalPayrollScratchIndex=19;
    var r=new DialogueSequenceRandom([], .9f);var speaker=RandomDialogueSpeakerRules.Pick(s,r);
    Check(speaker==new RandomDialogueSpeaker(2,3) && r.SingleCalls==1 && s.OriginalPayrollScratchIndex==2);
    Check(OriginalPayrollRules.MonthlyDebit(s)==2400 && OriginalPayrollRules.StaffSalaryTotal(s.EmployeeSources)==2200 && s.Money==100000);
});
Test("random dialogue protects unknown and empty maps without inventing a speaker",()=>
{
    var(s,p,c)=CheckpointFixture();s.OriginalPayrollScratchIndex=19;var r=new DialogueSequenceRandom([]);
    Check(RandomDialogueSpeakerRules.Pick(s,r) is null && s.OriginalPayrollScratchIndex is null && r.SingleCalls==0);
    AddDialogueSources(s);foreach(var row in s.EmployeeSources)row.GateValue=0;
    Check(RandomDialogueSpeakerRules.Pick(s,r) is null && s.OriginalPayrollScratchIndex==0 && r.SingleCalls==0);
});
Test("checkpoint speaker draws interleave with candidate remarks in original order",()=>
{
    var(s,p,c)=CheckpointFixture();AddDialogueSources(s);
    var r=new DialogueSequenceRandom([0,0,0,1,2,2,2],.9f,0);CheckpointMeetingRules.Prepare(s,p,c,r);
    Check(p.CheckpointMeeting!.Lines.Select(x=>x.EmployeeSourceIndex).SequenceEqual(new int?[]{2,0}));
    Check(r.Order.SequenceEqual(new[]{"N3","N6","S","N3","N6","S","N3","N3","N3"}) && s.OriginalPayrollScratchIndex==2);
});
Test("beta meeting selects a portrait for every employee call before showing the saved queue",()=>
{
    var(s,p,c)=CheckpointFixture();AddDialogueSources(s);p.OriginalStage=1;p.GameTypeSourceIndex=0;p.Progress=81;p.PendingDecision=DevelopmentMilestone.SelectGenre;
    var r=new DialogueSequenceRandom([],0,.9f,0,.9f);CheckpointMeetingRules.Prepare(s,p,c,r);
    Check(r.SingleCalls==4 && p.CheckpointMeeting!.Lines.Select(x=>x.EmployeeSourceIndex).SequenceEqual(new int?[]{0,2,0,2}) && s.OriginalPayrollScratchIndex==2);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects[0].CheckpointMeeting!.Lines.SequenceEqual(p.CheckpointMeeting.Lines));
});
Test("checkpoint selected speakers and payroll scratch survive load without another draw",()=>
{
    var(s,p,c)=CheckpointFixture();AddDialogueSources(s);
    CheckpointMeetingRules.Prepare(s,p,c,new DialogueSequenceRandom([0,0,2,2,2,2],.9f));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects[0];var r=new DialogueSequenceRandom([]);
    CheckpointMeetingRules.Prepare(s,p,c,r);
    Check(p.CheckpointMeeting!.Lines[0] is {EmployeeSourceIndex:2,EmployeePortraitId:3} && s.OriginalPayrollScratchIndex==2 && r.SingleCalls==0);
});
Test("type confirmation chooses one employee and publicity uses the secretary bypass",()=>
{
    var(s,p,c)=CheckpointFixture();AddDialogueSources(s);p.CheckpointMeeting=new() {Milestone=DevelopmentMilestone.SelectType};
    var r=new DialogueSequenceRandom([],.9f);
    Check(CheckpointMeetingRules.TrySelectType(s,p,c,0,r)==DevelopmentSelectionResult.Success && r.SingleCalls==1);
    Check(p.CheckpointMeeting!.Lines[0] is {EmployeeSourceIndex:2,EmployeePortraitId:3} && p.CheckpointMeeting.Lines[1].EmployeeSourceIndex is null);
    Check(CheckpointMeetingRules.IsSecretary(CheckpointMeetingMessage.Publicity) && CheckpointMeetingRules.IsSecretary(CheckpointMeetingMessage.FunBonus));
    Check(s.OriginalPayrollScratchIndex==2);
});
Test("saved checkpoint speakers reject incomplete identities and secretary employee overrides",()=>
{
    var(s,p,c)=CheckpointFixture();p.CheckpointMeeting=new() {Milestone=DevelopmentMilestone.SelectType,Lines=[new(CheckpointMeetingMessage.TypeSuggestion,0,0,null)]};
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    p.CheckpointMeeting.Lines=[new(CheckpointMeetingMessage.TypeSuggestion,0,-1,1)];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    p.CheckpointMeeting.Lines=[new(CheckpointMeetingMessage.Publicity,null,0,1)];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
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
Test("burst flames preserve seat clocks, exact frame boundaries and saved phase",()=>
{
    var s=PlanningState();var e=Worker();s.Employees.Add(e);e.Office!.WorkTier=3;
    var tile=new OfficeSpriteDefinition(64,-100,0,319,359,25,25,23);
    for (int tick=0;tick<12;tick++) OfficeBurstFlameRules.Tick(s,e);
    Check(s.OfficeBurstFlames.Ticks[0]==12 && s.OfficeBurstFlames.SourceX[0]==319);
    OfficeBurstFlameRules.Tick(s,e);Check(s.OfficeBurstFlames.Ticks[0]==1 && s.OfficeBurstFlames.SourceX[0]==346);
    var sprite=OfficeBurstFlameRules.Project(tile,s);Check(sprite.X==200 && sprite.Y==102 && sprite.SourceX==346 && sprite.SortKey==140);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];
    for (int tick=0;tick<5;tick++) OfficeBurstFlameRules.Tick(s,e);
    Check(s.OfficeBurstFlames.SourceX[0]==346 && s.OfficeBurstFlames.Ticks[0]==6);
    OfficeBurstFlameRules.Tick(s,e);Check(s.OfficeBurstFlames.SourceX[0]==319 && s.OfficeBurstFlames.Ticks[0]==7);
    e.Office!.WorkTier=2;OfficeBurstFlameRules.Tick(s,e);Check(s.OfficeBurstFlames.Ticks[0]==7 && OfficeBurstFlameRules.Project(tile,s).X==-100);
    e.Office.WorkTier=3;e.Office.SeatedAction=7;OfficeBurstFlameRules.Tick(s,e);
    Check(s.OfficeBurstFlames.Ticks[0]==7 && s.OfficeBurstFlames.Ticks[1]==1);
    e.Office.SeatedAction=6;Check(OfficeBurstFlameRules.Project(tile,s).SourceX==319);
});
Test("burst flame projection covers every original desk and hides unrelated seats",()=>
{
    var s=PlanningState();var e=Worker();s.Employees.Add(e);e.Office!.WorkTier=3;
    (int X,int Y,int Sy)[] expected=[(200,102,359),(232,118,359),(151,105,384),(210,147,359),(125,134,384),(157,150,384),(296,150,359),(328,166,359),(247,153,384),(306,195,359),(221,182,384),(253,198,384)];
    for(int i=0;i<12;i++)
    {
        e.Office.SeatedAction=i+6;var tile=new OfficeSpriteDefinition(64+i,-100,0,319,359,25,25,23);
        var p=OfficeBurstFlameRules.Project(tile,s);Check((p.X,p.Y,p.SourceY)==expected[i] && p.Width==25 && p.Height==25 && p.SortKey==p.Y+38);
        var scene=new OfficeSceneData {Sprites=[tile]};Check(OfficePresentationRules.ForState(scene,s).Single()==p);
        e.Office.WorkEngaged=false;Check(!OfficePresentationRules.ForState(scene,s).Any());e.Office.WorkEngaged=true;
    }
    var unrelated=new OfficeSpriteDefinition(38,-100,0,319,359,25,25,23);Check(OfficeBurstFlameRules.Project(unrelated,s)==unrelated);
});
Test("burst flame snapshots reject malformed cells and older saves restart presentation only",()=>
{
    var s=PlanningState();string old=SaveStore.Serialize(s);var node=System.Text.Json.Nodes.JsonNode.Parse(old)!;node["State"]!.AsObject().Remove("OfficeBurstFlames");
    var restored=SaveStore.Deserialize(node.ToJsonString());Check(restored.OfficeBurstFlames.Ticks.All(x=>x==0) && restored.OfficeBurstFlames.SourceX.All(x=>x==319));
    s.OfficeBurstFlames.Ticks[0]=13;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.OfficeBurstFlames.Ticks[0]=0;
    s.OfficeBurstFlames.SourceX[0]=320;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.OfficeBurstFlames.SourceX[0]=319;
    s.OfficeBurstFlames.Ticks=new int[11];Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.OfficeBurstFlames.Ticks=new int[12];
    s.OfficeBurstFlames.SourceX=null!;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("occupied facility artwork uses saved slots and original geometry without buying unsupported types",()=>
{
    var (s,_)=CalendarFixture();s.Money=SchoolService.PurchasePrice;
    for(int slot=0;slot<3;slot++)
    {
        var tile=new OfficeSpriteDefinition(146+slot,-100,0,428,357,162,111,500);
        Check(OfficeFacilityPresentationRules.Project(tile,s).X==-1000);
        Check(SchoolService.Open(s,slot)==SchoolResult.Success);
        var p=OfficeFacilityPresentationRules.Project(tile,s);Check(p.X==(slot==0 ? -16 : slot==1 ? 64 : 320) && p.Y==(slot==0 ? 130 : 170) && p.SourceX==1 && p.SourceY==409 && p.Width==162 && p.Height==111 && p.SortKey==500);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(OfficeFacilityPresentationRules.Project(tile,s)==p);
        Check(SchoolService.Close(s,slot)==SchoolResult.Success && OfficeFacilityPresentationRules.Project(tile,s).X==-1000);s.Money=SchoolService.PurchasePrice;
    }
    int[] columns=[1,163,325,487,649,811,973,1297,1135];
    for(int type=0;type<9;type++) {s.Calendar!.OriginalSlots0444[0]=type;Check(OfficeFacilityPresentationRules.Project(new(146,-100,0,428,357,162,111,500),s).SourceX==columns[type]);}
    s.Calendar=null;Check(OfficeFacilityPresentationRules.Project(new(146,-100,0,428,357,162,111,500),s).X==-1000);
});
Test("facility lettering changes only three sign interiors and rejects unsupported captions",()=>
{
    var source=Enumerable.Repeat((byte)81,466*290*3).ToArray();
    var patched=LocalBitmapLetteringRules.PatchFacilitySigns(source,466,290,"FOR RENT");
    Check(source.All(x=>x==81));
    for(int pixel=0;pixel<466*290;pixel++)
    {
        int x=pixel%466,y=pixel/466;bool inside=LocalBitmapLetteringRules.FacilitySignInteriors.Any(r=>x>=r.X && x<r.X+r.Width && y>=r.Y && y<r.Y+r.Height);
        for(int c=0;c<3;c++) Check(inside ? patched[pixel*3+c] is 0 or 220 : patched[pixel*3+c]==81);
    }
    Check(LocalBitmapLetteringRules.FacilitySignInteriors.All(r=>Enumerable.Range(r.Y,r.Height).Any(y=>Enumerable.Range(r.X,r.Width).Any(x=>patched[(y*466+x)*3]==0))));
    Throws<InvalidDataException>(()=>LocalBitmapLetteringRules.PatchFacilitySigns(source,466,290,"TOO LONG FOR THIS SIGN"));
    Throws<InvalidDataException>(()=>LocalBitmapLetteringRules.PatchFacilitySigns(source,466,290,"テナント"));
    Throws<InvalidDataException>(()=>LocalBitmapLetteringRules.PatchFacilitySigns(source,465,290,"FOR RENT"));
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
GameState LawyerState(long funds=15_000_000,long fee=1500)
{
    var (state,_)=CalendarFixture(money:funds);state.Calendar!.OriginalSlots0444[0]=5;
    state.SalesFee1=state.SalesFee2=fee;return state;
}
Test("lawyer consultations preserve original affordability-only cash behavior and target independent fees",()=>
{
    foreach (var target in Enum.GetValues<FeeNegotiationTarget>())
    {
        var s=LawyerState();var random=new GreetingRandom(.75f);
        Check(FeeNegotiationService.Begin(s,0,target,random)==FacilityResult.Success);
        Check(s.Money==15_000_000 && s.Company.WeeklyNet==11 && s.Company.MonthlyNet==22 && s.Company.YearlyNet==33);
        Check(FeeNegotiationService.CurrentFee(s,target)==1300 && FeeNegotiationService.CurrentFee(s,target==FeeNegotiationTarget.Transport ? FeeNegotiationTarget.Retail : FeeNegotiationTarget.Transport)==1500);
        Check(random.GreetingCalls==0 && s.PendingFeeNegotiation!.MessageIndex==0);
    }
});
Test("lawyer lower-fee outcome uses exactly one Single draw and original shrinking chances",()=>
{
    foreach (long fee in new[]{1300L,1100,900,700}) foreach (float roll in new[]{0f,.99f})
    {
        var s=LawyerState(fee:fee);var random=new GreetingRandom(roll);
        Check(FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Retail,random)==FacilityResult.Success);
        Check(s.SalesFee1==(roll==0 ? fee-200 : fee) && random.GreetingCalls==1);
        while (FeeNegotiationService.Acknowledge(s)) {}
        Check(s.Money==15_000_000 && !FeeNegotiationService.Acknowledge(s));
    }
});
Test("lawyer fee floor and unmatched fee branches preserve original draw and message counts",()=>
{
    foreach (long fee in new[]{500L,600})
    {
        var s=LawyerState(fee:fee);var random=new GreetingRandom(0);
        Check(FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Transport,random)==FacilityResult.Success);
        Check(s.SalesFee2==fee && random.GreetingCalls==0 && FeeNegotiationService.MessageCount(s.PendingFeeNegotiation!)==(fee==500 ? 5 : 1));
    }
});
Test("lawyer rejected entry cannot change cash fees or RNG",()=>
{
    var s=LawyerState(14_999_999);string before=SaveStore.Serialize(s);var random=new GreetingRandom(0);
    Check(FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Transport,random)==FacilityResult.InsufficientFunds && SaveStore.Serialize(s)==before);
    s.Money=15_000_000;s.SalesFee2=null;
    Check(FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Transport,random)==FacilityResult.StateUnknown);
    s.Calendar!.OriginalSlots0444[0]=0;
    Check(FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Transport,random)==FacilityResult.NotOwned && random.GreetingCalls==0);
});
Test("lawyer outcome snapshots resume every dialogue cursor without rerolls and block duplicate consultation",()=>
{
    var s=LawyerState(fee:1300);var random=new GreetingRandom(0);var session=new GameSession(random);session.Restore(s);
    Check(session.BeginFeeNegotiation(0,FeeNegotiationTarget.Transport)==FacilityResult.Success);
    for(int i=0;i<5;i++)
    {
        s=SaveStore.Deserialize(SaveStore.Serialize(s));session.Restore(s);
        Check(s.PendingFeeNegotiation!.MessageIndex==i && s.SalesFee2==1100);
        Check(FeeNegotiationService.ImportKeys.Contains(FeeNegotiationService.MessageKey(s.PendingFeeNegotiation)));
        Check(session.BeginFeeNegotiation(0,FeeNegotiationTarget.Retail)==FacilityResult.EventPending);
        Check(FacilityService.Close(s,0,FacilityKind.Lawyer)==FacilityResult.EventPending);
        int? clock=s.CompanyActivityTicks;session.TickOffice();Check(s.CompanyActivityTicks==clock);
        Check(FeeNegotiationService.Acknowledge(s));
    }
    Check(s.PendingFeeNegotiation is null && random.GreetingCalls==1 && s.Money==15_000_000);
});
Test("lawyer invalid Single draw leaves state unchanged",()=>
{
    foreach (float roll in new[]{float.NaN,float.PositiveInfinity,-.1f,1f})
    {
        var s=LawyerState(fee:1300);string before=SaveStore.Serialize(s);
        Throws<ArgumentOutOfRangeException>(()=>FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Retail,new GreetingRandom(roll)));
        Check(SaveStore.Serialize(s)==before);
    }
});
Test("lawyer save validation rejects tampered fee results cursors and missing ownership",()=>
{
    var s=LawyerState();FeeNegotiationService.Begin(s,0,FeeNegotiationTarget.Retail,new GreetingRandom(0));
    s.PendingFeeNegotiation!.After=1100;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingFeeNegotiation.After=1300;s.PendingFeeNegotiation.MessageIndex=5;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingFeeNegotiation.MessageIndex=0;s.SalesFee1=1500;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.SalesFee1=1300;s.Calendar!.OriginalSlots0444[0]=-1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    Check(FeeNegotiationService.ImportKeys.Count==18 && FeeNegotiationService.ImportKeys.Distinct().Count()==18);
});
Test("all facility types purchase exact prices once in any plot without financial accumulators",()=>
{
    long[] prices=[600_000_000,60_000_000,80_000_000,100_000_000,500_000_000,250_000_000,300_000_000,150_000_000,1_000_000_000];
    foreach (FacilityKind kind in Enum.GetValues<FacilityKind>()) foreach (int slot in Enumerable.Range(0,3))
    {
        var (s,_)=CalendarFixture(money:prices[(int)kind]*2);s.Company.Rank=null;
        Check(FacilityService.PurchasePrice(kind)==prices[(int)kind]);
        Check(FacilityService.Open(s,slot,kind)==FacilityResult.Success && s.Money==prices[(int)kind]);
        Check(s.Company.WeeklyNet==11 && s.Company.MonthlyNet==22 && s.Company.YearlyNet==33);
        Check(FacilityService.Open(s,(slot+1)%3,kind)==FacilityResult.AlreadyOwned);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
        Check(s.Calendar!.OriginalSlots0444[slot]==(int)kind);
        Check(FacilityService.Close(s,slot,kind)==FacilityResult.Success && s.Money==prices[(int)kind] && s.Calendar.OriginalSlots0444[slot]==-1);
        Check(FacilityService.Close(s,slot,kind)==FacilityResult.NotOwned);
    }
});
Test("facility purchase rejection preserves state and checks affordability before duplicates",()=>
{
    var (s,_)=CalendarFixture(money:59_999_999);s.Calendar!.OriginalSlots0444[1]=1;
    string before=SaveStore.Serialize(s);
    Check(FacilityService.Open(s,0,FacilityKind.HotSpring)==FacilityResult.InsufficientFunds && SaveStore.Serialize(s)==before);
    s.Money=100_000_000;s.Calendar.OriginalSlots0444[0]=0;
    Check(FacilityService.Open(s,0,FacilityKind.Recycling)==FacilityResult.SlotOccupied);
    Check(FacilityService.Open(s,-1,FacilityKind.School)==FacilityResult.InvalidSlot);
    Check(FacilityService.Close(s,0,FacilityKind.HotSpring)==FacilityResult.NotOwned);
    Throws<ArgumentOutOfRangeException>(()=>FacilityService.Open(s,0,(FacilityKind)99));
});
Test("hot spring ownership receipt and reload prevent repeated spending and clear source stress",()=>
{
    var (s,_)=CalendarFixture(money:8_000_000);s.Calendar!.OriginalSlots0444[0]=1;
    var e=Worker(source:0);e.Stress=99;s.Employees=[e];
    var source=Worker(source:0);source.Stress=99;s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=source}];
    Check(FacilityService.HotSpring(s,1)==FacilityResult.NotOwned && s.Money==8_000_000);
    Check(FacilityService.HotSpring(s,0)==FacilityResult.Success && s.Money==0 && e.Stress==99 && source.Stress==99);
    Check(s.Company.WeeklyNet==11-8_000_000 && s.PendingFacilityReceipt!.Kind==FacilityKind.HotSpring);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));string before=SaveStore.Serialize(s);
    Check(FacilityService.HotSpring(s,0)==FacilityResult.EventPending && SaveStore.Serialize(s)==before);
    var session=new GameSession();session.Restore(s);session.TickOffice();Check(SaveStore.Serialize(s)==before);
    Check(FacilityService.AcknowledgeReceipt(s) && s.Employees[0].Stress==0 && s.EmployeeSources[0].Employee.Stress==0);
    Check(FacilityService.HotSpring(s,0)==FacilityResult.InsufficientFunds);
});
Test("hot spring chain consumes eager source draws and saves each named reaction without repeat payments",()=>
{
    var (state,_)=CalendarFixture(money:100_000_000);state.Calendar!.OriginalSlots0444[0]=1;
    state.SourceUpperBound=6;state.EmployeeSources=[];state.Employees=[];
    for(int i=0;i<6;i++)
    {
        var source=Worker(source:i);source.Stress=80;
        state.EmployeeSources.Add(new(){SourceIndex=i,GateValue=i%3==0?1:0,Employee=source});
        if(i%3==0){var worker=Worker(source:i);worker.Stress=80;worker.Office=null;state.Employees.Add(worker);}
    }
    var random=new GreetingRandom(.2f);
    Check(FacilityService.HotSpring(state,0,random)==FacilityResult.Success && random.GreetingCalls==8);
    Check(state.PendingFacilityReceipt!.Reactions!.Select(r=>r.SourceIndex).SequenceEqual(new[]{0,3}) && state.PendingFacilityReceipt.Reactions!.All(r=>r.Variant==1));
    Check(state.Money==92_000_000 && state.Employees.All(e=>e.Stress==80));
    for(int page=0;page<3;page++)
    {
        state=SaveStore.Deserialize(SaveStore.Serialize(state));Check(state.PendingFacilityReceipt!.PageIndex==page && state.Money==92_000_000);
        Check(FacilityService.HotSpring(state,0,random)==FacilityResult.EventPending && random.GreetingCalls==8);
        Check(FacilityService.AcknowledgeReceipt(state));
        Check(state.Employees.All(e=>e.Stress==0) && state.EmployeeSources.Where(x=>x.GateValue==1).All(x=>x.Employee.Stress==0));
    }
    Check(state.PendingFacilityReceipt is null && !FacilityService.AcknowledgeReceipt(state) && state.Money==92_000_000);
});
Test("hot spring large staff has no four-speaker cap and incomplete source history does not invent draws",()=>
{
    foreach(float draw in new[]{0f,.9f})
    {
        var (state,_)=CalendarFixture(money:100_000_000);state.Calendar!.OriginalSlots0444[0]=1;state.SourceUpperBound=6;state.EmployeeSources=[];state.Employees=[];
        for(int i=0;i<6;i++){state.EmployeeSources.Add(new(){SourceIndex=i,GateValue=1,Employee=Worker(source:i)});state.Employees.Add(Worker(source:i));}
        var random=new GreetingRandom(draw);Check(FacilityService.HotSpring(state,0,random)==FacilityResult.Success);
        Check(state.PendingFacilityReceipt!.Reactions!.Count==(draw==0?6:0) && random.GreetingCalls==(draw==0?12:6));
        state.PendingFacilityReceipt=null;state.SourceUpperBound=7;long money=state.Money!.Value;int calls=random.GreetingCalls;
        Check(FacilityService.HotSpring(state,0,random)==FacilityResult.StateUnknown && random.GreetingCalls==calls && state.Money==money);
    }
});
Test("facility chain snapshots reject invalid pages speakers owner slots and changed capacity",()=>
{
    var (state,_)=CalendarFixture();state.Calendar!.OriginalSlots0444[0]=1;
    state.PendingFacilityReceipt=new(){Kind=FacilityKind.HotSpring,Amount=8_000_000,SourceSlot=0,Reactions=[new(0,"Authored staff",1,2)],PageIndex=2};
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));state.PendingFacilityReceipt.PageIndex=1;
    Check(SaveStore.Deserialize(SaveStore.Serialize(state)).PendingFacilityReceipt!.PageIndex==1);
    state.PendingFacilityReceipt.Reactions.Add(new(0,"Duplicate",1,0));Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.PendingFacilityReceipt.Reactions=[];state.PendingFacilityReceipt.PageIndex=0;state.PendingFacilityReceipt.SourceSlot=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.Calendar.OriginalSlots0444[0]=7;state.Company.ProductionCapacity=100;
    state.PendingFacilityReceipt=new(){Kind=FacilityKind.ProductionLab,Amount=10_000_000,Gain=15,CapacityBefore=84,SourceSlot=0};Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.PendingFacilityReceipt.CapacityBefore=85;Check(SaveStore.Deserialize(SaveStore.Serialize(state)).PendingFacilityReceipt!.CapacityBefore==85);
    state.PendingFacilityReceipt=new(){Kind=FacilityKind.HotSpring,Amount=8_000_000};
    state=SaveStore.Deserialize(SaveStore.Serialize(state));Check(state.PendingFacilityReceipt!.Reactions is null);FacilityService.AcknowledgeReceipt(state);Check(state.PendingFacilityReceipt is null);
});
Test("recycling zeroes stock and pays exactly once without changing sales or production history",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=2;
    var p=new ReleasedGameState {Title="Recycle A",Stock=7,UnitsSold=9,ProductionSpending=123,NetSalesReceipts=456};
    var q=new ReleasedGameState {Title="Recycle B",ProductIndex=1,Stock=3};s.ReleasedGames=[p,q];
    long before=s.Money!.Value;
    Check(FacilityService.Recycle(s,0)==FacilityResult.Success && s.Money==before && p.Stock==0 && q.Stock==0);
    Check(s.Company.WeeklyNet==11 && p.UnitsSold==9 && p.ProductionSpending==123 && p.NetSalesReceipts==456);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(FacilityService.Recycle(s,0)==FacilityResult.EventPending && s.PendingFacilityReceipt!.Amount==5000);
    Check(s.Money==before && s.PendingFacilityReceipt.EffectsPending);
    Check(FacilityService.AcknowledgeReceipt(s) && s.Money==before+5000 && s.Company.WeeklyNet==5011);
    Check(!FacilityService.AcknowledgeReceipt(s) && s.Money==before+5000);
    Check(FacilityService.Recycle(s,0)==FacilityResult.Success && s.Money==before+5000);
});
Test("recycling overflow is atomic before stock disposal",()=>
{
    var (s,_)=CalendarFixture(money:long.MaxValue);s.Calendar!.OriginalSlots0444[0]=2;
    var p=new ReleasedGameState {Stock=1};s.ReleasedGames=[p];
    Throws<OverflowException>(()=>FacilityService.Recycle(s,0));Check(p.Stock==1 && s.Money==long.MaxValue && s.PendingFacilityReceipt is null);
    s.Money=0;p.Stock=long.MaxValue;Throws<OverflowException>(()=>FacilityService.Recycle(s,0));Check(p.Stock==long.MaxValue && s.Money==0);
});
Test("technology selector uses source order and its shared payroll scratch writer even when cancelled",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=3;s.OriginalPayrollScratchIndex=17;s.SourceUpperBound=2;
    var a=new EmployeeState {Name="A",SourceIndex=1,RawParameter34=2};var b=new EmployeeState {Name="B",SourceIndex=0,RawParameter34=3};s.Employees=[a,b];
    s.EmployeeSources=[new() {SourceIndex=1,GateValue=1,Employee=a},new() {SourceIndex=0,GateValue=1,Employee=b}];
    Check(FacilityService.OpenTechnologyRoster(s,0)!.SequenceEqual(new[]{b,a}) && s.OriginalPayrollScratchIndex==2);
    Check(s.Money==2_000_000_000 && a.MaximumTechnologies==2 && b.MaximumTechnologies==3);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.OriginalPayrollScratchIndex==2);
});
Test("technology study adds one capacity and synchronizes source without changing skills experience or assignments",()=>
{
    var (s,_)=CalendarFixture(money:40_000_000);s.Calendar!.OriginalSlots0444[0]=3;
    var e=new EmployeeState {Name="Study",PortraitId=1,SourceIndex=0,RawParameter34=8,Experience=17,TechnologyLevels=new() {{0,1}},Office=Worker().Office};
    var source=new EmployeeState {Name="Study",SourceIndex=0,RawParameter34=8};s.Employees=[e];
    s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=source}];
    Check(FacilityService.Study(s,0,e)==FacilityResult.Success && e.MaximumTechnologies==9 && source.MaximumTechnologies==9 && s.Money==0);
    Check(e.Experience==17 && e.TechnologyLevels![0]==1 && e.Office!.Action==6 && s.Company.WeeklyNet==11-40_000_000);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.Employees[0].MaximumTechnologies==9 && s.EmployeeSources[0].Employee.MaximumTechnologies==9);
    Check(FacilityService.Study(s,0,s.Employees[0])==FacilityResult.EventPending);
    s.PendingFacilityReceipt=null;Check(FacilityService.Study(s,0,s.Employees[0])==FacilityResult.InsufficientFunds);
});
Test("technology study rejects unknown source capacity and overflow without spending",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=3;
    var e=new EmployeeState {Name="Unknown",SourceIndex=0};s.Employees=[e];
    Check(FacilityService.Study(s,0,e)==FacilityResult.StateUnknown && FacilityService.OpenTechnologyRoster(s,0) is null);
    e.RawParameter34=int.MaxValue;s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=e}];
    Throws<OverflowException>(()=>FacilityService.Study(s,0,e));Check(s.Money==2_000_000_000 && e.MaximumTechnologies==int.MaxValue && s.PendingFacilityReceipt is null);
});
Test("production facility preserves single draw 15 to 19 gain negative cash and untouched weekly flags",()=>
{
    foreach (var (roll,gain) in new[]{(0f,15),(.2f,16),(.4f,17),(.6f,18),(float.BitDecrement(1f),19)})
    {
        var (s,_)=CalendarFixture(money:0);s.Calendar!.OriginalSlots0444[0]=7;s.Company.ProductionCapacity=50;s.CompanyActivityPerformed=true;
        var random=new GreetingRandom(roll);
        Check(FacilityService.ExpandProduction(s,0,random)==FacilityResult.Success && s.Money==0 && s.Company.ProductionCapacity==50 && random.GreetingCalls==1);
        Check(s.Company.WeeklyNet==11 && s.Company.MonthlyNet==22 && s.Company.YearlyNet==33 && s.CompanyActivityPerformed==true);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
        Check(FacilityService.ExpandProduction(s,0,random)==FacilityResult.EventPending && random.GreetingCalls==1 && s.PendingFacilityReceipt!.Gain==gain);
        Check(s.Money==0 && s.Company.ProductionCapacity==50 && s.PendingFacilityReceipt.EffectsPending);
        Check(FacilityService.AcknowledgeReceipt(s) && s.Money==-10_000_000 && s.Company.ProductionCapacity==50+gain);
        Check(!FacilityService.AcknowledgeReceipt(s) && random.GreetingCalls==1 && s.Company.WeeklyNet==11);

    }
});
Test("production expansion invalid draw and overflow are atomic",()=>
{
    foreach (float roll in new[]{-1f,1f,float.NaN})
    {
        var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=7;s.Company.ProductionCapacity=50;
        Throws<ArgumentOutOfRangeException>(()=>FacilityService.ExpandProduction(s,0,new GreetingRandom(roll)));
        Check(s.Money==2_000_000_000 && s.Company.ProductionCapacity==50 && s.PendingFacilityReceipt is null);
    }
    var (t,_c)=CalendarFixture(money:long.MinValue);t.Calendar!.OriginalSlots0444[0]=7;t.Company.ProductionCapacity=50;
    Throws<OverflowException>(()=>FacilityService.ExpandProduction(t,0,new GreetingRandom(0)));Check(t.Company.ProductionCapacity==50 && t.Money==long.MinValue);
});
Test("legacy already-applied facility receipts never replay effects",()=>
{
    foreach(var kind in new[]{FacilityKind.Recycling,FacilityKind.ProductionLab})
    {
        var (state,_)=CalendarFixture();state.Calendar!.OriginalSlots0444[0]=(int)kind;
        state.Company.ProductionCapacity=65;
        state.PendingFacilityReceipt=new(){Kind=kind,Amount=kind==FacilityKind.Recycling?500:10_000_000,
            SourceSlot=0,Gain=kind==FacilityKind.ProductionLab?15:null,CapacityBefore=kind==FacilityKind.ProductionLab?50:null};
        var legacy=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(state))!;
        legacy["State"]!["PendingFacilityReceipt"]!.AsObject().Remove("EffectsPending");
        state=SaveStore.Deserialize(legacy.ToJsonString());long? money=state.Money;
        Check(!state.PendingFacilityReceipt!.EffectsPending && FacilityService.AcknowledgeReceipt(state));
        Check(state.Money==money && state.Company.ProductionCapacity==65 && !FacilityService.AcknowledgeReceipt(state));
    }
});
Test("pending facility snapshots reject unappliable or advanced effects",()=>
{
    var (state,_)=CalendarFixture();state.Calendar!.OriginalSlots0444[0]=7;state.Company.ProductionCapacity=50;
    FacilityService.ExpandProduction(state,0,new GreetingRandom(0));state.Company.ProductionCapacity=65;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));state.Company.ProductionCapacity=50;
    state.PendingFacilityReceipt!.PageIndex=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
    state.PendingFacilityReceipt.PageIndex=0;state.Money=long.MinValue;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(state));
});
Test("facility changes are blocked during other pending game decisions",()=>
{
    var (s,_)=CalendarFixture();s.Calendar!.OriginalSlots0444[0]=7;s.Company.ProductionCapacity=50;
    s.PendingHireGreeting=new() {EmployeeSourceIndex=0,Variant=0};
    Check(FacilityService.ExpandProduction(s,0,new GreetingRandom(0))==FacilityResult.EventPending);
    Check(FacilityService.Close(s,0,FacilityKind.ProductionLab)==FacilityResult.EventPending);
    Check(FacilityService.Open(s,1,FacilityKind.HotSpring)==FacilityResult.EventPending);
});
Test("facility receipt validation rejects unsupported missing or malformed result snapshots",()=>
{
    var (s,_)=CalendarFixture();s.PendingFacilityReceipt=new() {Kind=FacilityKind.Casino,Amount=0};
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingFacilityReceipt=new() {Kind=FacilityKind.ProductionLab,Amount=10_000_000,Gain=20};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingFacilityReceipt=new() {Kind=FacilityKind.TechnologySchool,Amount=40_000_000,Gain=2};Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingFacilityReceipt=null;s.Calendar!.OriginalSlots0444[0]=9;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("overlapping tenant strips expose both exact strict hits without choosing priority",()=>
{
    foreach (double sum in new[]{270.5,272,273.5})
        Check(OfficeInteractionRules.TenantSlotsAt(sum-160,(sum+160)/2).SequenceEqual(new[]{0,1}));
    Check(OfficeInteractionRules.TenantSlotsAt(110,215).SequenceEqual(new[]{0})); // sum270
    Check(OfficeInteractionRules.TenantSlotsAt(114,217).SequenceEqual(new[]{1})); // sum274
    Check(OfficeInteractionRules.TenantSlotsAt(double.NaN,0).Count==0);
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
Test("beginner ending matches the original exact date rather than three elapsed years",()=>
{
    Check(CalendarRules.OpeningDate==new GameDate(1,4,1));
    Check(GameModeRules.For(GameMode.Beginner).EndingClosingDate==new GameDate(2,12,4));
    Check(GameModeRules.For(GameMode.Beginner).TimeLimitYears is null && GameModeRules.For(GameMode.Normal).EndingClosingDate is null);
    foreach(var date in new[] {new GameDate(1,12,4),new GameDate(2,11,4),new GameDate(2,12,3),new GameDate(2,12,4),new GameDate(3,12,4)})
    {
        var (s,c)=CalendarFixture(date,1000);s.Mode=GameMode.Beginner;s.Rules=GameModeRules.For(s.Mode);
        CalendarService.BeginWeek(s,c,new SequenceRandom());
        Check((s.Calendar!.Pending?.PendingEvent==CalendarEvent.BeginnerEnding)==(date==new GameDate(2,12,4)));
        Check(!CalendarRules.IsBeginnerEnding(GameMode.Normal,date));
    }
});
Test("old three-year mode metadata cannot change the verified ending after save reload",()=>
{
    var (s,c)=CalendarFixture(new(2,12,4),1000);s.Mode=GameMode.Beginner;
    s.Rules=new GameModeRules(true,3,true);
    var loaded=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(loaded.Rules.TimeLimitYears is null && loaded.Rules.EndingClosingDate==new GameDate(2,12,4));
    Check(!CalendarService.BeginWeek(loaded,c,new SequenceRandom()) && loaded.Calendar!.Pending!.PendingEvent==CalendarEvent.BeginnerEnding);
    var again=SaveStore.Deserialize(SaveStore.Serialize(loaded));
    Check(again.Calendar!.Pending!.ClosingDate==new GameDate(2,12,4) && !CalendarService.Resume(again,c,new SequenceRandom()));
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
Test("bank credits all four source principals and shared finance periods before the saved receipt",()=>
{
    foreach (int type in Enumerable.Range(1,4))
    {
        var (s,c)=CalendarFixture(new(3,5,2),-100);s.Company.Rank=null;
        long amount=CalendarDialogueRules.LoanPrincipal(type);
        Check(BankLoanService.TryBorrow(s,type)==BankLoanResult.Success);
        Check(s.Money==amount-100 && s.Company.WeeklyNet==amount+11 && s.Company.MonthlyNet==amount+22 && s.Company.YearlyNet==amount+33);
        Check(s.Calendar!.Loans[0].OriginalType==type && s.Calendar.Loans[0].BorrowedOn==new GameDate(3,5,2));
        Check(BankLoanService.DueOn(s.PendingBankLoanReceipt!.BorrowedOn)==new GameDate(4,5,2));
        s=SaveStore.Deserialize(SaveStore.Serialize(s));string before=SaveStore.Serialize(s);
        Check(BankLoanService.TryBorrow(s,type)==BankLoanResult.EventPending && SaveStore.Serialize(s)==before);
        Check(BankLoanService.Acknowledge(s) && !BankLoanService.Acknowledge(s) && s.Money==amount-100);
    }
});
Test("bank fills the first free slot refuses three loans and reuses repaid slots",()=>
{
    var (s,c)=CalendarFixture();
    for (int type=1;type<=3;type++) {Check(BankLoanService.TryBorrow(s,type)==BankLoanResult.Success);Check(s.PendingBankLoanReceipt!.Slot==type-1);Check(BankLoanService.Acknowledge(s));}
    string before=SaveStore.Serialize(s);Check(BankLoanService.TryBorrow(s,4)==BankLoanResult.AllSlotsOccupied && SaveStore.Serialize(s)==before);
    s.Calendar!.Loans[1].OriginalType=0;
    Check(BankLoanService.TryBorrow(s,4)==BankLoanResult.Success && s.PendingBankLoanReceipt!.Slot==1 && s.Calendar.Loans[2].OriginalType==3);
});
Test("new borrowing reaches the existing exact one-year due-date dialogue and debit once",()=>
{
    foreach (int type in Enumerable.Range(1,4))
    {
        var (s,c)=CalendarFixture(new(3,5,2),0);Check(BankLoanService.TryBorrow(s,type)==BankLoanResult.Success);BankLoanService.Acknowledge(s);
        s.Year=4;s.Week=1;
        Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Calendar!.Pending!.PendingEvent==CalendarEvent.LoanRepayment);
        long amount=CalendarDialogueRules.LoanPrincipal(type);Check(s.Money==amount && s.Calendar!.Pending!.Amount==CalendarService.LoanRepayment(type));
        s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(CalendarDialogueRules.Acknowledge(s));
        Check(s.Money==amount-CalendarService.LoanRepayment(type) && s.Calendar!.Loans[0].OriginalType==0);
        Check(!CalendarDialogueRules.Acknowledge(s));Check(CalendarService.Resume(s,c,new SequenceRandom()));
        Check(BankLoanService.TryBorrow(s,1)==BankLoanResult.Success && s.PendingBankLoanReceipt!.Slot==0);
    }
});
Test("bank rejects unknown states invalid terms and pending transactions without mutation",()=>
{
    var(s,c)=CalendarFixture();string before=SaveStore.Serialize(s);
    Throws<ArgumentOutOfRangeException>(()=>BankLoanService.TryBorrow(s,0));Throws<ArgumentOutOfRangeException>(()=>BankLoanService.TryBorrow(s,5));Check(SaveStore.Serialize(s)==before);
    s.Money=null;Check(BankLoanService.TryBorrow(s,1)==BankLoanResult.MoneyUnknown);
    s.Money=100;s.Year=null;Check(BankLoanService.TryBorrow(s,1)==BankLoanResult.DateUnknown);
    s.Year=3;s.PendingSecretaryAdvice=new();Check(BankLoanService.TryBorrow(s,1)==BankLoanResult.EventPending);s.PendingSecretaryAdvice=null;
    s.Calendar=null;Check(BankLoanService.TryBorrow(s,1)==BankLoanResult.CalendarUnknown);
});
Test("bank financial and date overflow are atomic",()=>
{
    var(s,c)=CalendarFixture(money:long.MaxValue);string before=SaveStore.Serialize(s);
    Throws<OverflowException>(()=>BankLoanService.TryBorrow(s,1));Check(SaveStore.Serialize(s)==before);
    s.Money=0;s.Company.MonthlyNet=long.MaxValue;before=SaveStore.Serialize(s);
    Throws<OverflowException>(()=>BankLoanService.TryBorrow(s,1));Check(SaveStore.Serialize(s)==before);
    s.Company.MonthlyNet=0;s.Year=int.MaxValue;before=SaveStore.Serialize(s);
    Throws<OverflowException>(()=>BankLoanService.TryBorrow(s,1));Check(SaveStore.Serialize(s)==before);
});
Test("bank receipt freezes clocks and rejects forged or missing receipt fields",()=>
{
    var(s,c)=CalendarFixture();BankLoanService.TryBorrow(s,1);var session=new GameSession(new SequenceRandom());session.Restore(s);
    string before=SaveStore.Serialize(s);session.TickOffice(c);Check(SaveStore.Serialize(s)==before);
    Throws<InvalidOperationException>(()=>session.OpenRecruitment());Check(session.Hire(new() {Name="Synthetic candidate"})==HireResult.ConversationPending);
    Check(session.AskSecretary(c)==SecretaryAdviceResult.EventPending && SaveStore.Serialize(s)==before);
    string json=SaveStore.Serialize(s);Throws<InvalidDataException>(()=>SaveStore.Deserialize(json.Replace("\"Slot\": 0,","")));
    s.PendingBankLoanReceipt!.Slot=1;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.PendingBankLoanReceipt.Slot=0;
    s.Calendar!.Loans[0].OriginalType=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.Calendar.Loans[0].OriginalType=1;
    s.Week=3;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
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
    Check(CalendarDialogueRules.Acknowledge(s) && s.Money==187000000 && s.Calendar!.Loans[0].OriginalType==0);
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
        string sourcePng=Path.Combine(original,"Graphic","Native.png");File.WriteAllBytes(sourcePng,[1]);
        string replacementPng=Path.Combine(overlay,"Graphic","Native.png");File.WriteAllBytes(replacementPng,[2]);
        Check(LocalAssetOverlay.Resolve(original,overlay,"Graphic/Native.png")==replacementPng);
        File.Delete(sourcePng);Check(LocalAssetOverlay.Resolve(original,overlay,"Graphic/Native.png")==sourcePng);
        File.WriteAllBytes(Path.Combine(overlay,"Graphic","Hidden.png"),[3]);
        Check(LocalAssetOverlay.Resolve(original,overlay,"Graphic/Hidden.png")==Path.Combine(original,"Graphic","Hidden.png"));
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
EmployeeState Worker(int action=6,int source=0)=>new()
{
    Name="Synthetic Worker",SourceIndex=source,PortraitId=1,Level=1,Experience=0,Stress=0,
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
Test("all six work technologies apply after actor ticks and survive halfway reload at every learned level",()=>
{
    foreach(int id in TechnologyApplicationService.ProjectTechnologyIdentifiers)
    foreach(int level in new[]{1,2,3})
    {
        var (s,e,c,p)=UseFixture();e.TechnologyLevels![id]=level;
        Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[id])==TechnologyApplicationResult.Success);
        Check(e.Experience==100-5*level && s.Money==100);
        for(int tick=0;tick<11;tick++)DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
        Check(p.Quality==new DevelopmentQuality(100,100,100,100) && p.DataSize==101.9 && p.Bugs==101);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];p=s.DevelopmentProjects[0];
        for(int tick=0;tick<100 && e.Office!.TechnologyApplication is not null;tick++)DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
        double factor=(10+10*level)/100.0;
        Check(e.Office!.TechnologyApplication is null && p.TechnologyApplicationCounts![id]==1);
        Check(p.DataSize==(id==0?Math.Floor(101.9*(1-factor)):101.9));
        Check(p.Bugs==(id==2?(int)Math.Floor(101*(1-factor)):101));
        Check(p.Quality.Fun==(id==6?Math.Floor(100*(1+factor)):100));
        Check(p.Quality.Novelty==(id==7?Math.Floor(100*(1+factor)):100));
        Check(p.Quality.Graphics==(id==1?Math.Floor(100*(1+factor)):100));
        Check(p.Quality.Music==(id==3?Math.Floor(100*(1+factor)):100));
        Check(e.Experience==100-5*level);
    }
});
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
Test("stress relief floors selected lower levels and preserves the slot-zero top-level target",()=>
{
    var (s,e,c,p)=UseFixture();e.Stress=99.9;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success && e.Stress==49 && e.Experience==95);
    e.TechnologyLevels![5]=2;e.Stress=99.9;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success && e.Stress==19 && e.Experience==85);
    e.TechnologyLevels[5]=3;e.Stress=100;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success && e.Stress==0 && e.Experience==70);
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[4])==TechnologyApplicationResult.StateUnknown && e.Experience==70);
});
(GameState State,EmployeeState Employee,OriginalCatalog Catalog,DevelopmentProjectState Project) SpeedFixture(int level=1)
{
    var(s,e,c,p)=UseFixture();s.RetainedDevelopmentEligibility=0;e.Level=10;e.TechnologyLevels![4]=level;
    p.OriginalStage=0;p.GameTypeSourceIndex=null;p.GenreSourceIndex=null;p.RequiredPoints=10000;
    return(s,e,c,p);
}
Test("speed sets the separate level timer and preserves the shared unit cell",()=>
{
    for(int level=1;level<=3;level++)
    {
        var(s,e,c,p)=SpeedFixture(level);e.Office!.BurstCount=17;c.Technologies[4].RawTable158C=[];
        Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[4])==TechnologyApplicationResult.Success);
        Check(e.Experience==100-5*level && e.Office.SpeedBoostTicks==50*level && e.Office.TechnologyApplication is {IsSpeedBoost:true,RemainingUnits:17});
        Check(p.Quality==new DevelopmentQuality(100,100,100,100) && p.Progress==0 && s.Money==100);
    }
});
Test("speed requires established frame eligibility without consuming experience",()=>
{
    var(s,e,c,p)=SpeedFixture();s.RetainedDevelopmentEligibility=null;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[4])==TechnologyApplicationResult.StateUnknown && e.Experience==100 && e.Office!.SpeedBoostTicks==0);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(s.RetainedDevelopmentEligibility==0 && TechnologyApplicationService.TryUse(s,e,c.Technologies[4])==TechnologyApplicationResult.Success);
});
Test("speed substitutes the idle comparison and starts ordinary work without tier four",()=>
{
    var(s,e,c,p)=SpeedFixture();DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office!.WorkTicks==0 && e.Office.WorkIdleTicks==1);
    TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.WorkTicks==2 && e.Office.WorkTier==1 && e.Office.WorkCategory==DevelopmentWorkCategory.Fun);
    Check(e.Office.TechnologyApplication is {IsSpeedBoost:true,RemainingUnits:0} && e.DevelopmentSpeed==50 && e.Office.SpeedBoostTicks==50);
});
Test("speed retains eligibility one and skips ordinary technology completion",()=>
{
    var(s,e,c,p)=SpeedFixture();s.RetainedDevelopmentEligibility=1;
    TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    for(int i=0;i<50;i++){DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());TechnologyApplicationService.TickSpeed(e);}
    Check(s.RetainedDevelopmentEligibility==1 && e.Office is {SpeedBoostTicks:0,TechnologyApplication:null,WorkTier:4,WorkTicks:51});
    Check(p.Progress==0 && p.TechnologyApplicationCounts!.Values.All(x=>x==0));
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office.WorkTicks==52 && s.RetainedDevelopmentEligibility==0);
});
Test("speed replaces in-flight technology without resetting its work counters",()=>
{
    var(s,e,c,p)=SpeedFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office!.WorkTier==4 && s.RetainedDevelopmentEligibility==1);
    int tick=e.Office.WorkTicks;TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    Check(e.Office.TechnologyApplication is {IsSpeedBoost:true,TechnologySourceIndex:4,RemainingUnits:2} && e.Office.WorkTicks==tick && e.Experience==90);
    for(int i=0;i<50;i++){DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());TechnologyApplicationService.TickSpeed(e);}
    Check(e.Office.TechnologyApplication is null && e.Office.WorkTier==4 && e.Office.WorkTicks==tick+50 && p.TechnologyApplicationCounts![0]==0);
    Check(OfficeAssignmentService.TryAssignStudySeat(s,e,1)==OfficeAssignmentResult.Success && e.Office.WorkTicks==0 && e.Office.WorkTier==0);
});
Test("speed does not accelerate debugging but its separate timer still expires",()=>
{
    var(s,e,c,p)=SpeedFixture();p.OriginalStage=3;p.GameTypeSourceIndex=p.GenreSourceIndex=0;
    TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    for(int i=0;i<50;i++){DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());TechnologyApplicationService.TickSpeed(e);}
    Check(p.Bugs==101 && e.Office is {TechnologyApplication:null,SpeedBoostTicks:0,WorkTier:4});
});
Test("speed timer consumes actor passes while travelling and resting without random draws",()=>
{
    var(s,e,c,p)=SpeedFixture(3);TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    OfficeAssignmentService.TryAssignStudySeat(s,e,1);var session=new GameSession(new WorkRandom());session.Restore(s);
    session.TickOffice(c);Check(e.Office!.SpeedBoostTicks==149);
    for(int i=0;i<149;i++)TechnologyApplicationService.TickSpeed(e);
    Check(e.Office.SpeedBoostTicks==0 && e.Office.TechnologyApplication is null);
});
Test("speed pauses with pending decisions and resumes on the next actor pass",()=>
{
    var(s,e,c,p)=SpeedFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    var session=new GameSession(new WorkRandom());session.Restore(s);p.PendingDecision=DevelopmentMilestone.SelectType;
    session.TickOffice(c);Check(e.Office!.SpeedBoostTicks==50);
    p.PendingDecision=null;session.TickOffice(c);Check(e.Office.SpeedBoostTicks==49);
});
Test("normal technology replacement retains the unused speed timer",()=>
{
    var(s,e,c,p)=SpeedFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);TechnologyApplicationService.TickSpeed(e);
    TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);TechnologyApplicationService.TickSpeed(e);
    Check(e.Office!.SpeedBoostTicks==49 && e.Office.TechnologyApplication is {IsSpeedBoost:false,RemainingUnits:2,TechnologySourceIndex:0});
    TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);Check(e.Office.SpeedBoostTicks==50 && e.Office.TechnologyApplication!.RemainingUnits==2);
});
Test("empty development room clears the selected technology but retains its separate timer",()=>
{
    var(s,e,c,p)=SpeedFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);s.DevelopmentProjects.Clear();
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());TechnologyApplicationService.TickSpeed(e);
    Check(e.Office is {TechnologyApplication:null,SpeedBoostTicks:50,WorkTicks:0});
});
Test("speed ordinary bursts retain shared units until the next work start",()=>
{
    var(s,e,c,p)=SpeedFixture();e.Office!.WorkTier=3;e.Office.WorkTicks=32;e.Office.WorkCategory=DevelopmentWorkCategory.Fun;
    e.Office.BurstCount=37;e.Office.BurstTicks=119;TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Quality.Fun==137 && Math.Abs(p.Progress-37/3.0)<1e-10 && e.Office.TechnologyApplication is {IsSpeedBoost:true,RemainingUnits:37});
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.TechnologyApplication is {IsSpeedBoost:true,RemainingUnits:0} && e.Office.WorkTicks==2);
});
Test("actor slot order determines retained eligibility even after a reused hire slot",()=>
{
    foreach(bool boostedFirst in new[]{false,true})
    {
        var(s,e,c,p)=SpeedFixture();var previous=Worker(7,1);previous.Level=10;previous.Experience=100;
        previous.TechnologyLevels=Enumerable.Range(0,8).ToDictionary(x=>x,_=>1);
        previous.Office!.SlotIndex=boostedFirst ? 1 : 0;e.Office!.SlotIndex=boostedFirst ? 0 : 1;
        s.Employees.Add(previous);TechnologyApplicationService.TryUse(s,previous,c.Technologies[0]);TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
        // List order is deliberately opposite the actor order in one case.
        var session=new GameSession(new WorkRandom());session.Restore(s);session.TickOffice(c);
        Check(e.Office.WorkTier==(boostedFirst ? 1 : 4) && e.Office.SpeedBoostTicks==49 && s.RetainedDevelopmentEligibility==1);
    }
});
Test("retained eligibility survives modern load and starting another company",()=>
{
    var(s,e,c,p)=SpeedFixture();s.RetainedDevelopmentEligibility=1;TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    e.Office!.SpeedBoostTicks=23;s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(s.RetainedDevelopmentEligibility==1 && s.Employees[0].Office is {SpeedBoostTicks:23,TechnologyApplication.IsSpeedBoost:true});
    var session=new GameSession();Check(session.StartNewGame("First","Owner",GameMode.Normal).RetainedDevelopmentEligibility==0);
    session.Restore(s);Check(session.StartNewGame("Next","Owner",GameMode.Normal).RetainedDevelopmentEligibility==1);
});
Test("speed save guards reject invalid counters while retaining zero shared speed units",()=>
{
    var(s,e,c,p)=SpeedFixture();TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    Check(SaveStore.Deserialize(SaveStore.Serialize(s)).Employees[0].Office!.TechnologyApplication!.RemainingUnits==0);
    e.Office!.SpeedBoostTicks=151;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    e.Office.SpeedBoostTicks=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    e.Office.SpeedBoostTicks=50;s.RetainedDevelopmentEligibility=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.RetainedDevelopmentEligibility=0;e.Office.TechnologyApplication!.IsSpeedBoost=false;
    Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("speed expiry retains the shared unit cell for a later reactivation",()=>
{
    var(s,e,c,p)=SpeedFixture();e.Office!.WorkTier=4;e.Office.WorkTicks=3;
    TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    for(int i=0;i<50;i++)TechnologyApplicationService.TickSpeed(e);
    Check(e.Office.SharedWorkUnits==2 && e.Office.TechnologyApplication is null);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees[0];
    TechnologyApplicationService.TryUse(s,e,c.Technologies[4]);
    Check(e.Office!.TechnologyApplication!.RemainingUnits==2 && e.Office.SpeedBoostTicks==50);
});
Test("a generic technology selected mid-burst replaces the same native accumulation cell",()=>
{
    var(s,e,c,p)=SpeedFixture();e.Office!.WorkTier=3;e.Office.WorkTicks=32;e.Office.WorkCategory=DevelopmentWorkCategory.Fun;
    e.Office.BurstCount=37;e.Office.BurstTicks=119;TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Quality.Fun==102 && Math.Abs(p.Progress-2/3.0)<1e-10 && e.Office.SharedWorkUnits==2 && e.Office.TechnologyApplication!.RemainingUnits==2);
    for(int i=0;i<21;i++)DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(e.Office.SharedWorkUnits==0 && e.Office.TechnologyApplication is null && p.TechnologyApplicationCounts![0]==1);
});
Test("shared burst units update a selected application and survive ordinary completion",()=>
{
    var(s,e,c,p)=SpeedFixture();e.Office!.WorkTier=3;e.Office.WorkTicks=32;e.Office.WorkCategory=DevelopmentWorkCategory.Fun;
    e.Office.BurstCount=37;e.Office.BurstTicks=0;TechnologyApplicationService.TryUse(s,e,c.Technologies[0]);
    DevelopmentSimulation.TickEmployee(s,e,c,new BurstRandom());
    Check(e.Office.BurstCount==3 && e.Office.SharedWorkUnits==3 && e.Office.TechnologyApplication!.RemainingUnits==3 && e.Office.BurstDisplayCount==2);
    e.Office.BurstTicks=119;DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Quality.Fun==103 && e.Office.SharedWorkUnits==3 && e.Office.TechnologyApplication!.RemainingUnits==3);
});
Test("top-level stress relief spends selected experience but clears a different slot-zero employee",()=>
{
    var (s,e,c,p)=UseFixture();e.Office!.SlotIndex=1;e.TechnologyLevels![5]=3;e.Stress=77.7;
    var first=Worker(18,7);first.Office!.SlotIndex=0;first.Stress=88;s.Employees.Add(first);
    var firstSource=Worker(18,7);firstSource.Stress=88;s.EmployeeSources.Add(new() {SourceIndex=7,GateValue=1,Employee=firstSource});
    var selectedSource=Worker(6,0);selectedSource.Experience=100;selectedSource.Stress=77.7;s.EmployeeSources.Add(new() {SourceIndex=0,GateValue=1,Employee=selectedSource});
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success);
    Check(first.Stress==0 && firstSource.Stress==0 && e.Stress==77.7 && e.Experience==85 && selectedSource.Experience==85 && selectedSource.Stress==77.7);
    Check(e.Office.TechnologyApplication is null && first.Office.TechnologyApplication is null);
});
Test("top-level stress target follows actor slot zero rather than source index or list order",()=>
{
    var (s,e,c,p)=UseFixture();e.Office!.SlotIndex=2;e.TechnologyLevels![5]=3;
    var first=Worker(18,15);first.Office!.SlotIndex=0;first.Stress=70;
    var other=Worker(18,3);other.Office!.SlotIndex=1;other.Stress=80;s.Employees.Add(other);s.Employees.Add(first);
    Check(ReferenceEquals(TechnologyApplicationService.LevelThreeStressTarget(s),first));
    TechnologyApplicationService.TryUse(s,e,c.Technologies[5]);Check(first.Stress==0 && other.Stress==80);
});
Test("top-level stress relief disables unknown slot mapping before consuming experience",()=>
{
    var (s,e,c,p)=UseFixture();e.Office!.SlotIndex=1;e.TechnologyLevels![5]=3;
    Check(TechnologyApplicationService.CheckUse(s,e,c.Technologies[5])==TechnologyApplicationResult.StateUnknown);
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.StateUnknown && e.Experience==100);
    var unknown=new EmployeeState {Name="Legacy synthetic",SourceIndex=null,Office=new(){SlotIndex=0}};s.Employees.Add(unknown);
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.StateUnknown && e.Experience==100);
});
Test("top-level direct stress writer resolves an unknown target stress without borrowing selected stress",()=>
{
    var (s,e,c,p)=UseFixture();e.TechnologyLevels![5]=3;e.Stress=null;
    Check(TechnologyApplicationService.TryUse(s,e,c.Technologies[5])==TechnologyApplicationResult.Success && e.Stress==0 && e.Experience==85);
});
Test("top-level stress effects and selected costs survive modern save reconstruction",()=>
{
    var (s,e,c,p)=UseFixture();e.Office!.SlotIndex=1;e.TechnologyLevels![5]=3;e.Stress=75;
    var first=Worker(18,7);first.Office!.SlotIndex=0;first.Stress=80;s.Employees.Add(first);TechnologyApplicationService.TryUse(s,e,c.Technologies[5]);
    var copy=System.Text.Json.JsonSerializer.Deserialize<GameState>(System.Text.Json.JsonSerializer.Serialize(s))!;
    Check(copy.Employees.Single(x=>x.Office!.SlotIndex==0).Stress==0 && copy.Employees.Single(x=>x.Office!.SlotIndex==1).Stress==75 && copy.Employees.Single(x=>x.Office!.SlotIndex==1).Experience==85);
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
    session.TickOffice(CoreCatalog());Check(s.SalesTicks==0 && g.HallCelebration is not null);
    Check(HallCelebrationRules.Acknowledge(g));session.TickOffice(CoreCatalog());Check(s.SalesTicks==1);
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
    HallCelebrationRules.Acknowledge(g);g.HallOfFame=false;Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].ReviewScores!.Sum()==4);
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
(GameState State,OriginalCatalog Catalog) HardwareFixture(long money=1_000_000_000)
{
    var (s,c)=CalendarFixture(money:money);s.Calendar!.OriginalSlots0444[0]=8;
    // The synthetic calendar catalog intentionally omits software planning columns.
    foreach (var hardware in c.Hardware)
    {hardware.RawMembers["0F00"]=64;hardware.RawMembers["0FFC"]=0;hardware.RawMembers["0FE0"]=400;hardware.RawMembers["0FC4"]=0;}
    return (s,c);
}
DevelopmentProjectState StartHardware(GameState s,OriginalCatalog c,int room=0,int design=0,int medium=0)
{
    Check(OwnHardwareService.Start(s,c,0,room,design,medium,"Synthetic console development")==OwnHardwareResult.Success);
    return s.DevelopmentProjects.Single(p=>p.OriginalRoomState==2);
}
void ReadyHardware(DevelopmentProjectState p)
{p.Progress=2500;p.Quality=new(800,800,800,800);foreach (int id in OwnHardwareService.RequiredTechnologies) p.HardwareDesign!.TechnologyFlags[id]=1;}
Test("hardware quotes all design combinations and accepts equal funds with accounting",()=>
{
    for (int d=0;d<2;d++) for (int m=0;m<2;m++) for (int room=0;room<2;room++)
    {
        long cost=(d==0 ? 200_000_000L : 100_000_000L)+(m==0 ? 200_000_000L : 100_000_000L);
        var (s,c)=HardwareFixture(cost);Check(OwnHardwareService.Quote(d,m)==cost);var p=StartHardware(s,c,room,d,m);
        Check(s.Money==0 && s.Company.WeeklyNet==11-cost && s.Company.MonthlyNet==22-cost && s.Company.YearlyNet==33-cost);
        Check(p.OriginalRoomState==2 && p.OriginalMode==1 && p.OriginalStage==0 && p.RequiredPoints==2500 && p.Progress==0);
        Check(p.Quality==new DevelopmentQuality() && p.PolishingTargets==new DevelopmentQuality(800,800,800,800));
        Check(p.HardwareDesign!.Design==d && p.HardwareDesign.Medium==m && p.HardwareDesign.TechnologyFlags.Values.All(x=>x==0));
        Check(OwnHardwareService.CheckStart(s,c,0,1-room,d,m)==OwnHardwareResult.ProjectRunning);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.DevelopmentProjects.Single().HardwareDesign!.Design==d);
    }
});
Test("hardware rejects insufficient funds occupied room incomplete catalog and duplicate ownership atomically",()=>
{
    var (s,c)=HardwareFixture(199_999_999);string before=SaveStore.Serialize(s);
    Check(OwnHardwareService.Start(s,c,0,0,1,1,"Hardware")==OwnHardwareResult.InsufficientFunds && SaveStore.Serialize(s)==before);
    s.Money=1_000_000_000;c.Hardware.RemoveAt(10);before=SaveStore.Serialize(s);
    Check(OwnHardwareService.Start(s,c,0,0,1,1,"Hardware")==OwnHardwareResult.StateUnknown && SaveStore.Serialize(s)==before);
    (s,c)=HardwareFixture();var p=StartHardware(s,c);ReadyHardware(p);Check(OwnHardwareService.Launch(s,p,"Console")==OwnHardwareResult.Success);
    before=SaveStore.Serialize(s);Check(OwnHardwareService.Start(s,c,0,1,0,0,"Second")==OwnHardwareResult.AlreadyLaunched && SaveStore.Serialize(s)==before);
    (s,c)=HardwareFixture();var software=new DevelopmentProjectState {RoomIndex=0,Title="Synthetic software",RequiredPoints=100,Focus=new(25,25,25,25)};
    s.DevelopmentProjects.Add(software);before=SaveStore.Serialize(s);
    Check(OwnHardwareService.Start(s,c,0,0,1,1,"Hardware")==OwnHardwareResult.RoomOccupied && SaveStore.Serialize(s)==before);
    s.Company.WeeklyNet=long.MinValue;Throws<OverflowException>(()=>OwnHardwareService.Start(s,c,0,1,1,1,"Hardware"));
    Check(s.Money==1_000_000_000 && s.DevelopmentProjects.Count==1);
});
Test("hardware work preserves inclusive interval boundaries and idles at full targets",()=>
{
    var (s,c)=HardwareFixture();var p=StartHardware(s,c);
    foreach (int roll in Enumerable.Range(0,400))
    {
        var category=roll<=50 ? DevelopmentWorkCategory.Fun : roll<=100 ? DevelopmentWorkCategory.Novelty :
            roll<=150 ? DevelopmentWorkCategory.Graphics : roll<=200 ? DevelopmentWorkCategory.Music : DevelopmentWorkCategory.Speed;
        Check(OwnHardwareService.SelectCategory(p,roll)==category);
    }
    p.Quality=new(800,0,0,0);Check(OwnHardwareService.SelectCategory(p,50) is null && OwnHardwareService.SelectCategory(p,51)==DevelopmentWorkCategory.Novelty);
    p.Quality=new(800,800,800,800);p.Progress=2500;
    Check(Enumerable.Range(0,400).All(roll=>OwnHardwareService.SelectCategory(p,roll) is null));
    Check(DevelopmentWorkRules.PendingMilestone(p) is null);Throws<ArgumentOutOfRangeException>(()=>OwnHardwareService.SelectCategory(p,400));
});
Test("hardware ordinary work uses real worker scheduling caps qualities and avoids software checkpoints",()=>
{
    var (s,c)=HardwareFixture();var p=StartHardware(s,c);var e=Worker();s.Employees=[e];
    p.Quality=new(799,800,800,800);p.Progress=2499;e.Office!.WorkCategory=DevelopmentWorkCategory.Fun;e.Office.WorkTier=2;e.Office.WorkTicks=16;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());
    Check(p.Quality==new DevelopmentQuality(800,800,800,800) && p.Progress==2500 && p.PendingDecision is null && p.OriginalStage==0);
    Check(!OwnHardwareService.Ready(p));
    // Full-target selection must leave work idle, rather than reassign an unrelated category.
    e.Office.WorkTicks=0;e.Office.WorkIdleTicks=100;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(e.Office.WorkTicks==0);
});
Test("hardware demolition and reload leave worker progress alive without the facility",()=>
{
    var (s,c)=HardwareFixture();var p=StartHardware(s,c);s.Employees=[Worker()];
    Check(FacilityService.Close(s,0,FacilityKind.HardwareLab)==FacilityResult.Success);long money=s.Money!.Value;
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects.Single();var e=s.Employees.Single();
    e.Office!.WorkCategory=DevelopmentWorkCategory.Speed;e.Office.WorkTier=2;e.Office.WorkTicks=16;
    DevelopmentSimulation.TickEmployee(s,e,c,new WorkRandom());Check(p.Progress==4 && s.Money==money && s.Calendar!.OriginalSlots0444[0]==-1);
    Check(ProjectCancellationService.Check(s,p)==ProjectCancellationResult.ProjectUnavailable);
});
Test("hardware completion requires all work qualities and four flags exactly",()=>
{
    var (s,c)=HardwareFixture();var p=StartHardware(s,c);ReadyHardware(p);Check(OwnHardwareService.Ready(p));
    p.Progress=2499;Check(!OwnHardwareService.Ready(p));p.Progress=2500;
    foreach (int id in OwnHardwareService.RequiredTechnologies)
    {p.HardwareDesign!.TechnologyFlags[id]=0;Check(!OwnHardwareService.Ready(p));p.HardwareDesign.TechnologyFlags[id]=1;}
    p.Quality=new(800,799,800,800);Check(!OwnHardwareService.Ready(p));p.Quality=new(800,800,800,800);
    p.HardwareDesign!.PendingTechnology=1;Check(!OwnHardwareService.Ready(p));p.HardwareDesign.PendingTechnology=null;
    p.HardwareDesign.TechnologyFlags[1]=2;Check(!OwnHardwareService.Ready(p));Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("hardware technologies use saved messages and set flags after acknowledgment without percentage RNG",()=>
{
    foreach (int id in OwnHardwareService.RequiredTechnologies)
    {
        var (s,c)=HardwareFixture();var p=StartHardware(s,c);var e=Worker();e.Experience=100;e.TechnologyLevels=new() {{id,1}};s.Employees=[e];
        var t=new TechnologyDefinition {SourceIndex=id,Identifier=id,RawTable1568=[5,10,15],RawTable158C=[1,2,3]};c.Technologies=[t];
        Check(TechnologyApplicationService.TryUse(s,e,t)==TechnologyApplicationResult.Success && e.Experience==95);
        e.Office!.WorkTier=4;e.Office.WorkTicks=20;var noRandom=new SequenceRandom();
        TechnologyApplicationService.TickWork(p,e,c,noRandom);
        Check(noRandom.Calls==0 && p.HardwareDesign!.PendingTechnology==id && p.HardwareDesign.TechnologyFlags[id]==0 && p.Quality==new DevelopmentQuality());
        Check(e.Office.TechnologyApplication is null && OwnHardwareService.HasPending(s));
        s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects.Single();
        Check(p.HardwareDesign!.PendingTechnology==id && OwnHardwareService.AcknowledgeTechnology(s,p));
        Check(p.HardwareDesign.TechnologyFlags[id]==1 && s.OwnHardware.RetainedRoomFlags[0][id]==1 && !OwnHardwareService.AcknowledgeTechnology(s,p));
        Check(TechnologyApplicationService.TryUse(s,s.Employees.Single(),t)==TechnologyApplicationResult.Success);
    }
});
Test("hardware start preserves retained native flags rather than resetting them",()=>
{
    var (s,c)=HardwareFixture();s.OwnHardware.RetainedRoomFlags[1][3]=1;var p=StartHardware(s,c,1);
    Check(p.HardwareDesign!.TechnologyFlags[3]==1 && p.HardwareDesign.TechnologyFlags[1]==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.OwnHardware.RetainedRoomFlags[1][3]==1 && s.DevelopmentProjects.Single().HardwareDesign!.TechnologyFlags[3]==1);
});
Test("hardware launch appends exact cartridge CD records bands license and earliest ranking without another debit",()=>
{
    for (int medium=0;medium<2;medium++)
    {
        var (s,c)=HardwareFixture();var p=StartHardware(s,c,medium:medium);ReadyHardware(p);
        long cash=s.Money!.Value;long? weekly=s.Company.WeeklyNet;int catalogCount=c.Hardware.Count;
        Check(OwnHardwareService.Launch(s,p,"Synthetic Console")==OwnHardwareResult.Success && s.Money==cash && s.Company.WeeklyNet==weekly);
        var h=s.OwnHardware.Launched!;Check(h.SourceIndex==11 && h.Identifier==11 && h.BitWidth==64 && h.LicenseFee==0 && h.PlanningCost==0);
        Check(h.PlanningPoints==(medium==0 ? 400 : 600) && h.RawMembers["0F54"]==(medium==0 ? 50 : 36));
        Check(h.RawMembers["0FA8"]==(medium==0 ? 3 : 4) && h.RawMembers["0F70"]==400000);
        Check(h.ProductionBands.SequenceEqual(medium==0 ? new ProductionBand[] {new(64,600),new(128,1000),new(256,1500),new(512,2000)} :
            new ProductionBand[] {new(20,700),new(40,1000),new(60,1300),new(80,1500)}));
        var market=s.Calendar!.Hardware.Single(x=>x.SourceIndex==11);
        Check(market.Availability==1 && market.Market==(medium==0 ? 0 : 10) && market.GrowthWeeks==240 && market.GrowthBoost==10 && market.LaunchWeeks==0 && market.RetirementWeeks==0);
        Check(s.HardwareLicenses!.Contains(11) && s.AvailableHardware!.Contains(11) && s.DevelopmentProjects.Count==0);
        Check(s.PendingRankings.Single().Kind==RankingKind.OwnHardware && c.Hardware.Count==catalogCount && OwnHardwareService.Catalog(s,c).Hardware.Count==12);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));string before=SaveStore.Serialize(s);
        Check(OwnHardwareService.Launch(s,p,"Again")==OwnHardwareResult.NotReady && SaveStore.Serialize(s)==before);
    }
});
Test("own console enters ordinary software planning and weekly market growth after reload",()=>
{
    var (s,c)=HardwareFixture();var p=StartHardware(s,c,medium:1);ReadyHardware(p);Check(OwnHardwareService.Launch(s,p,"CD Console")==OwnHardwareResult.Success);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));c.DevelopmentComputers[0].BitWidth=64;
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,11,0,"Own platform game",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    Check(s.DevelopmentProjects.Single().HardwareSourceIndex==11);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));
    Check(ReleaseService.UnitProductionCost(OwnHardwareService.Catalog(s,c).Hardware.Single(h=>h.SourceIndex==11),20)==900);
    var market=s.Calendar!.Hardware.Single(x=>x.SourceIndex==11);double expected=HardwareCalendarRules.NextMarket(10,240,10,36);
    Check(CalendarService.BeginWeek(s,c,new Random(10)));Check(Math.Abs(market.Market-expected)<.0000001 && market.GrowthWeeks==239 && market.GrowthBoost==9);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(Math.Abs(s.Calendar!.Hardware.Single(x=>x.SourceIndex==11).Market-expected)<.0000001);
});
Test("own platform supports shipment sales and reprints through the unchanged catalog entry points",()=>
{
    var (s,c)=HardwareFixture();var hardware=StartHardware(s,c,medium:1);ReadyHardware(hardware);
    Check(OwnHardwareService.Launch(s,hardware,"Synthetic CD")==OwnHardwareResult.Success);c.DevelopmentComputers[0].BitWidth=64;
    Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,11,0,"Own software",new(25,25,25,25))==DevelopmentPlanningResult.Success);
    var p=s.DevelopmentProjects.Single();p.OriginalStage=3;p.Progress=p.RequiredPoints;p.GameTypeSourceIndex=0;p.GenreSourceIndex=0;p.Quality=new(100,100,100,100);p.DataSize=20;
    s.SalesFee1=s.SalesFee2=1500;
    long cash=s.Money!.Value;Check(ReleaseService.TryRelease(s,p,c,"Own software",5800,10,new Random(5))==ReleaseResult.Success);
    var product=s.ReleasedGames.Single();Check(product.HardwareSourceIndex==11 && s.Money==cash-9000);
    while (ShipmentDialogueRules.HasPending(product)) Check(ShipmentDialogueRules.Acknowledge(s,product));
    Check(ReviewService.Acknowledge(s,product));s.ProductionPerformedThisWeek=false;cash=s.Money!.Value;
    Check(ReleaseService.TryProduce(s,product,c,5)==ReleaseResult.Success && product.Stock==15 && s.Money==cash-4500);
    s.SalesTicks=SalesService.TicksPerBatch-1;SalesService.Tick(s,c,new Random(10));
    Check(product.UnitsSold>0 && product.Stock<15);s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.ReleasedGames.Single().HardwareSourceIndex==11);
});
Test("hardware saved pending messages and launch gate suspend session and other workflows",()=>
{
    var (s,c)=HardwareFixture();var p=StartHardware(s,c);p.HardwareDesign!.PendingTechnology=1;
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects.Single();var session=new GameSession();session.Restore(s);
    string before=SaveStore.Serialize(s);session.TickOffice(c);Check(SaveStore.Serialize(s)==before);
    Throws<InvalidOperationException>(()=>session.OpenRecruitment());Check(session.AskSecretary(c)==SecretaryAdviceResult.EventPending);
    Check(FacilityService.Close(s,0,FacilityKind.HardwareLab)==FacilityResult.EventPending);
    Check(BankLoanService.CheckBorrow(s)==BankLoanResult.EventPending);
    Check(OwnHardwareService.AcknowledgeTechnology(s,p));ReadyHardware(p);before=SaveStore.Serialize(s);
    session.TickOffice(c);Check(SaveStore.Serialize(s)==before && FacilityService.HasPendingDecision(s));
    Check(OwnHardwareService.Launch(s,p," ")==OwnHardwareResult.InvalidName && SaveStore.Serialize(s)==before);
});
Test("hardware rejects fabricated own platform references flags targets and launch records",()=>
{
    var (s,c)=HardwareFixture();s.HardwareLicenses!.Add(11);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.HardwareLicenses.Remove(11);
    var p=StartHardware(s,c);p.HardwareDesign!.TechnologyFlags.Remove(1);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.HardwareDesign.TechnologyFlags[1]=0;
    p.PolishingTargets=new(900,800,800,800);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));p.PolishingTargets=new(800,800,800,800);
    ReadyHardware(p);Check(OwnHardwareService.Launch(s,p,"Valid")==OwnHardwareResult.Success);
    s.OwnHardware.Launched!.RawMembers["0F54"]=50; // A cartridge already has 50.
    s.OwnHardware.Launched.RawMembers["0F54"]=51;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));s.OwnHardware.Launched.RawMembers["0F54"]=50;
    s.OwnHardware.Launched.ProductionBands[0]=new(64,601);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    (s,c)=HardwareFixture();var old=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;old["State"]!.AsObject().Remove("OwnHardware");
    Check(SaveStore.Deserialize(old.ToJsonString()).OwnHardware.Launched is null);
});

(GameState State,OriginalCatalog Catalog) LabFixture(long money=100_000_000)
{
    var (s,c)=CalendarFixture(money:money);s.Calendar!.OriginalSlots0444[0]=6;s.SourceUpperBound=3;
    var e=Worker(source:1);e.Stress=10;s.Employees=[e];
    s.EmployeeSources=Enumerable.Range(0,3).Select(i=>new RecruitmentSourceRecord {SourceIndex=i,GateValue=i==1 ? 1 : 0,
        Employee=new() {Name=$"Synthetic lab {i}",SourceIndex=i,Stress=i*10}}).ToList();
    s.Company.Fame=12;s.Company.ProductionCapacity=50;return (s,c);
}
void CompleteLab(GameState s,int outcome)
{
    var rng=new GreetingRandom((outcome+.25f)/7f);
    Check(MysteryLabService.Start(s,0,rng)==FacilityResult.Success && rng.GreetingCalls==1);
    for (int i=0;i<494;i++) Check(!MysteryLabService.Tick(s));
    Check(s.MysteryLab.Progress<=100 && MysteryLabService.Tick(s));
    Check(s.PendingMysteryLabResult!.Outcome==outcome && s.MysteryLab.Progress>100);
}
Test("lab selects seven single-draw outcomes and applies exactly the recovered effects without accounting",()=>
{
    for (int outcome=0;outcome<7;outcome++)
    {
        var (s,_)=LabFixture();CompleteLab(s,outcome);
        Check(s.Money==(outcome==0 ? 112_500_000 : outcome==1 ? 37_500_000 : 75_000_000));
        Check(s.Company.ProductionCapacity==(outcome==2 ? 100 : outcome==3 ? 40 : 50));
        Check(s.Company.Fame==(outcome==4 ? 80 : 12));
        Check(s.Company.WeeklyNet==11 && s.Company.MonthlyNet==22 && s.Company.YearlyNet==33);
        Check(s.Employees[0].Stress==(outcome==5 ? 0 : outcome==6 ? 90 : 10));
        Check(s.EmployeeSources[0].Employee.Stress==0 && s.EmployeeSources[2].Employee.Stress==20);
        Check(s.EmployeeSources[1].Employee.Stress==s.Employees[0].Stress);
        string before=SaveStore.Serialize(s);Check(!MysteryLabService.Tick(s) && SaveStore.Serialize(s)==before);
    }
});
Test("lab has no affordability gate and negative odd cash uses floor rather than truncation",()=>
{
    foreach (long afterFee in new long[] {-3,7})
    foreach (int outcome in new[] {0,1})
    {
        var (s,_)=LabFixture(MysteryLabService.ExperimentCost+afterFee);CompleteLab(s,outcome);
        Check(s.Money==(afterFee==-3 ? (outcome==0 ? -5 : -2) : (outcome==0 ? 10 : 3)));
    }
});
Test("lab fame threshold and production flooring are evaluated at completion",()=>
{
    foreach (int before in new[]{0,79,80,99,100})
    {
        var (s,_)=LabFixture();s.Company.Fame=before;CompleteLab(s,4);
        Check(s.Company.Fame==(before<80 ? 80 : 99) && s.PendingMysteryLabResult!.Fame99==(before>=80));
        Check(MysteryLabService.Acknowledge(s));
        Check(MysteryLabService.MessageKey(s.PendingMysteryLabResult!)==(before<80 ? "MYSTERY_LAB_RESULT_4" : "MYSTERY_LAB_RESULT_4_HIGH"));
    }
    var (state,_)=LabFixture();state.Company.ProductionCapacity=7;CompleteLab(state,3);Check(state.Company.ProductionCapacity==5);
    (state,_)=LabFixture();Check(MysteryLabService.Start(state,0,new GreetingRandom(0))==FacilityResult.Success);
    state.Money=9;state.MysteryLab.Progress=99.9;Check(MysteryLabService.Tick(state) && state.Money==13);
});
Test("lab demolition suspends work and rebuilding preserves progress and selected outcome without refunds",()=>
{
    var (s,_)=LabFixture(1_000_000_000);Check(MysteryLabService.Start(s,0,new GreetingRandom(.5f))==FacilityResult.Success);
    for (int i=0;i<10;i++) Check(!MysteryLabService.Tick(s));
    double progress=s.MysteryLab.Progress;int outcome=s.MysteryLab.Outcome;
    Check(FacilityService.Close(s,0,FacilityKind.MysteryLab)==FacilityResult.Success && s.Money==975_000_000);
    for (int i=0;i<600;i++) Check(!MysteryLabService.Tick(s));
    s=SaveStore.Deserialize(SaveStore.Serialize(s));Check(s.MysteryLab.Progress==progress && s.MysteryLab.Outcome==outcome);
    Check(FacilityService.Open(s,2,FacilityKind.MysteryLab)==FacilityResult.Success && s.Money==675_000_000);
    Check(s.MysteryLab.Progress==progress && !MysteryLabService.Tick(s) && s.MysteryLab.Progress==progress+.2);
});
Test("lab saved intro and result pages never reroll reapply or clear progress before modal return",()=>
{
    var (s,_)=LabFixture();CompleteLab(s,2);
    for (int page=0;page<2;page++)
    {
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
        Check(s.PendingMysteryLabResult!.MessageIndex==page && s.Company.ProductionCapacity==100 && s.MysteryLab.Progress>100);
        Check(!MysteryLabService.Tick(s));Check(MysteryLabService.Acknowledge(s));
    }
    Check(s.PendingMysteryLabResult is null && s.MysteryLab.Progress==0 && s.MysteryLab.Outcome==2);
    Check(!MysteryLabService.Acknowledge(s));
    Check(MysteryLabService.Start(s,0,new GreetingRandom(0))==FacilityResult.Success);
});
Test("lab active and modal guards reject repeat starts and pause session recruitment and direct work",()=>
{
    var (s,c)=LabFixture();CompleteLab(s,6);var rng=new GreetingRandom(0);var session=new GameSession(rng);session.Restore(s);
    string snapshot=SaveStore.Serialize(s);session.TickOffice(c);CompanyActivityService.Tick(s,rng,c);
    Check(SaveStore.Serialize(s)==snapshot && rng.GreetingCalls==0);
    Check(MysteryLabService.Start(s,0,rng)==FacilityResult.EventPending);
    Check(FacilityService.Close(s,0,FacilityKind.MysteryLab)==FacilityResult.EventPending);
    Check(session.Hire(Worker())==HireResult.ConversationPending);Throws<InvalidOperationException>(()=>session.OpenRecruitment());
    Check(session.AskSecretary(c)==SecretaryAdviceResult.EventPending);
    Check(SecretaryAdviceRules.Prepare(s,c,rng)==SecretaryAdviceResult.EventPending);
    Check(!SecretaryPolicyRules.TrySet(s,new()));
    MysteryLabService.Acknowledge(s);MysteryLabService.Acknowledge(s);
    Check(MysteryLabService.Start(s,0,rng)==FacilityResult.Success && rng.GreetingCalls==1);
    Check(MysteryLabService.Start(s,0,rng)==FacilityResult.ExperimentRunning && rng.GreetingCalls==1);
});
Test("lab stress outcomes affect current gate-one staff including employees hired during the experiment",()=>
{
    var (s,_)=LabFixture();Check(MysteryLabService.Start(s,0,new GreetingRandom(.8f))==FacilityResult.Success);
    s.EmployeeSources[1].GateValue=0;s.EmployeeSources[2].GateValue=1;
    var hired=Worker(source:2);hired.Stress=88;s.Employees=[hired];s.MysteryLab.Progress=99.9;
    Check(MysteryLabService.Tick(s) && hired.Stress==0 && s.EmployeeSources[2].Employee.Stress==0 && s.EmployeeSources[1].Employee.Stress==10);
});
Test("lab unknown-data rejected starts and overflow completion are atomic",()=>
{
    var (s,_)=LabFixture();var rng=new GreetingRandom(0);s.Company.Fame=null;
    Check(MysteryLabService.Start(s,0,rng)==FacilityResult.StateUnknown && s.Money==100_000_000 && rng.GreetingCalls==0);
    (s,_)=LabFixture(long.MinValue);Throws<OverflowException>(()=>MysteryLabService.Start(s,0,rng));Check(rng.GreetingCalls==0 && s.MysteryLab.Progress==0);
    (s,_)=LabFixture();s.Company.ProductionCapacity=int.MaxValue;Check(MysteryLabService.Start(s,0,new GreetingRandom(.3f))==FacilityResult.Success);
    s.MysteryLab.Progress=99.9;string before=SaveStore.Serialize(s);Throws<OverflowException>(()=>MysteryLabService.Tick(s));Check(SaveStore.Serialize(s)==before);
    (s,_)=LabFixture();Throws<ArgumentOutOfRangeException>(()=>MysteryLabService.Start(s,0,new GreetingRandom(float.NaN)));Check(s.Money==100_000_000 && s.MysteryLab.Progress==0);
});
Test("lab saves reject latent result seven invalid cursors and missing state while old modern saves migrate idle",()=>
{
    var (s,_)=LabFixture();var old=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;
    old["State"]!.AsObject().Remove("MysteryLab");old["State"]!.AsObject().Remove("PendingMysteryLabResult");
    Check(SaveStore.Deserialize(old.ToJsonString()).MysteryLab.Progress==0);
    foreach (Action<GameState> corrupt in new Action<GameState>[] {x=>x.MysteryLab.Outcome=7,x=>x.MysteryLab.Progress=-1,
        x=>x.MysteryLab.Progress=100.1,x=>x.MysteryLab=null!})
    {var (bad,_)=LabFixture();corrupt(bad);Throws<InvalidDataException>(()=>SaveStore.Serialize(bad));}
    CompleteLab(s,4);s.PendingMysteryLabResult!.MessageIndex=2;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    s.PendingMysteryLabResult.MessageIndex=0;s.PendingMysteryLabResult.Outcome=0;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("technology actions retain payroll through reload and charge month end exactly once",()=>
{
    for (int action=0;action<3;action++)
    {
        var (s,c)=CalendarFixture(new(3,5,4),1000);
        var e=Worker(action==1 ? 1 : 6);e.Experience=100;e.RawParameter34=1;
        e.TechnologyLevels=new() {{0,action==2 ? 1 : 0}};s.Employees=[e];
        s.SourceUpperBound=2;s.EmployeeSources=[new() {SourceIndex=0,GateValue=1,Employee=e},
            new() {SourceIndex=1,GateValue=0,Employee=new() {Name="Synthetic salary",SourceIndex=1,RawParameter40=120}}];
        s.TechnologyOwnershipLevels=new() {{0,action==0 ? 0 : 3}};
        Check(OriginalPayrollRules.CaptureStaffList(s) && s.OriginalPayrollScratchIndex==1);
        if (action==0) Check(TechnologyService.TryPurchase(s,TrainingTechnology())==TechnologyPurchaseResult.Success);
        else if (action==1) Check(TrainingService.TryStart(s,e,TrainingTechnology())==TrainingResult.Success);
        else
        {
            s.DevelopmentProjects.Add(new() {Title="Synthetic payroll project",RequiredPoints=100,Focus=new(25,25,25,25),
                TechnologyApplicationCounts=TechnologyApplicationService.NewProjectCounters()});
            Check(TechnologyApplicationService.TryUse(s,e,UseTechnology())==TechnologyApplicationResult.Success);
        }
        Check(s.OriginalPayrollScratchIndex==1);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));
        Check(CalendarService.BeginWeek(s,c,new SequenceRandom()) && s.Month==6 && s.Week==1);
        Check(s.Money==(action==0 ? 830 : 880) && s.Company.MonthlyNet==-120);
        Check(CalendarService.Resume(s,c,new SequenceRandom()) && s.Money==(action==0 ? 830 : 880));
    }
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
Test("numbered bursts commit the displayed gain once to all six work categories",()=>
{
    foreach (var category in Enum.GetValues<DevelopmentWorkCategory>().Where(x=>x!=DevelopmentWorkCategory.Debugging))
    {
        var c=CoreCatalog();var s=PlanningState();var e=Worker();s.Employees.Add(e);
        var p=new DevelopmentProjectState {Title="Synthetic burst",RequiredPoints=10000,Focus=new(100,0,0,0)};
        s.DevelopmentProjects.Add(p);
        e.Office!.WorkTier=3;e.Office.WorkTicks=32;e.Office.WorkCategory=category;
        for (int i=0;i<119;i++) DevelopmentSimulation.TickEmployee(s,e,c,new BurstRandom());
        Check(e.Office.BurstDisplayCount==50 && e.Office.BurstCount==50 && e.Office.BurstTicks==119);
        var digits=OfficeVisualRules.Sprites(e).Single(x=>x.Id==201);
        Check(digits.SourceX==468 && digits.SourceY==226); // Atlas number 50.
        Check(p.Quality==new DevelopmentQuality() && p.Progress==0 && p.Bugs==0);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees.Single();p=s.DevelopmentProjects.Single();
        DevelopmentSimulation.TickEmployee(s,e,c,new BurstRandom());
        var expected=category switch
        {
            DevelopmentWorkCategory.Fun=>new DevelopmentQuality(50,0,0,0),
            DevelopmentWorkCategory.Novelty=>new DevelopmentQuality(0,50,0,0),
            DevelopmentWorkCategory.Graphics=>new DevelopmentQuality(0,0,50,0),
            DevelopmentWorkCategory.Music=>new DevelopmentQuality(0,0,0,50),
            _=>new DevelopmentQuality()
        };
        Check(p.Quality==expected && p.Bugs==(category==DevelopmentWorkCategory.Bugs ? 50 : 0));
        Check(Math.Abs(p.Progress-(category==DevelopmentWorkCategory.Speed ? 50 : 50/3.0))<1e-9);
        Check(e.Office!.WorkTicks==0 && !OfficeVisualRules.Sprites(e).Any(x=>x.Id==201));
        double progress=p.Progress;var quality=p.Quality;int bugs=p.Bugs;
        s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees.Single();p=s.DevelopmentProjects.Single();
        DevelopmentSimulation.TickEmployee(s,e,c,new BurstRandom());
        Check(p.Progress==progress && p.Quality==quality && p.Bugs==bugs);
    }
});
Test("live office burst survives modal pause and mid-animation reload then reaches project stats",()=>
{
    var c=CoreCatalog();var s=PlanningState();var e=Worker();s.Employees.Add(e);
    var p=new DevelopmentProjectState {Title="Synthetic live burst",RequiredPoints=10000,Focus=new(100,0,0,0)};
    s.DevelopmentProjects.Add(p);
    e.Office!.WorkTier=3;e.Office.WorkTicks=32;e.Office.WorkCategory=DevelopmentWorkCategory.Fun;
    var session=new GameSession(new BurstRandom());session.Restore(s);
    for(int i=0;i<30;i++) session.TickOffice(c);
    Check(e.Office.BurstTicks==30 && e.Office.BurstCount==30 && e.Office.BurstDisplayCount==29);
    s.PendingHireGreeting=new() {EmployeeSourceIndex=0,Variant=0};
    for(int i=0;i<200;i++) session.TickOffice(c);
    Check(e.Office.BurstTicks==30 && p.Progress==0 && p.Quality.Fun==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Employees.Single();p=s.DevelopmentProjects.Single();
    session.Restore(s);Check(session.AcknowledgeHireGreeting());
    for(int i=30;i<120;i++) session.TickOffice(c);
    Check(p.Quality.Fun==50 && Math.Abs(p.Progress-50/3.0)<1e-9 && e.Office!.WorkTicks==0);
    var saved=SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects.Single();
    Check(saved.Quality==p.Quality && saved.Progress==p.Progress);
});
Test("burst gain is retained when it triggers a checkpoint and progress clips independently",()=>
{
    var c=CoreCatalog();var s=PlanningState();var e=Worker();s.Employees.Add(e);
    var p=new DevelopmentProjectState {Title="Synthetic crossing",RequiredPoints=100,Progress=49,Focus=new(100,0,0,0)};
    s.DevelopmentProjects.Add(p);
    e.Office!.WorkTier=3;e.Office.WorkTicks=151;e.Office.BurstTicks=119;e.Office.BurstCount=50;
    e.Office.WorkCategory=DevelopmentWorkCategory.Graphics;
    DevelopmentSimulation.TickEmployee(s,e,c,new BurstRandom());
    Check(p.Quality.Graphics==50 && Math.Abs(p.Progress-(49+50/3.0))<1e-9 && p.PendingDecision==DevelopmentMilestone.SelectType);
    var saved=SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects.Single();
    Check(saved.Quality.Graphics==50 && saved.PendingDecision==DevelopmentMilestone.SelectType);
    p.PendingDecision=null;p.OriginalStage=2;p.Progress=99;
    e.Office.WorkTicks=151;e.Office.BurstTicks=119;e.Office.BurstCount=50;
    DevelopmentSimulation.TickEmployee(s,e,c,new BurstRandom());
    Check(p.Progress==100 && p.Quality.Graphics==100 && p.PendingDecision==DevelopmentMilestone.StartDebugging);
});
Test("another employee's unfinished burst survives all three phase changes and checkpoint reload",()=>
{
    foreach (int stage in new[]{0,1,2})
    {
        var c=CoreCatalog();var s=PlanningState(10000000);
        s.OriginalTypeRosterMap=Enumerable.Range(0,65).ToDictionary(x=>x,_=>0);
        var lead=Worker();var burst=Worker(7,1);burst.Office!.SlotIndex=1;
        s.Employees.AddRange([lead,burst]);
        var p=new DevelopmentProjectState {Title="Synthetic phase boundary",RequiredPoints=100,
            Progress=stage switch {0=>49,1=>80,_=>99},OriginalStage=stage,Focus=new(100,0,0,0),
            GameTypeSourceIndex=stage>0 ? 0 : null,GenreSourceIndex=stage==2 ? 0 : null};
        s.DevelopmentProjects.Add(p);
        lead.Office!.WorkTier=1;lead.Office.WorkTicks=16;lead.Office.WorkCategory=DevelopmentWorkCategory.Speed;
        burst.Office.WorkTier=3;burst.Office.WorkTicks=151;burst.Office.BurstTicks=119;
        burst.Office.BurstCount=37;burst.Office.BurstDisplayCount=37;burst.Office.WorkCategory=DevelopmentWorkCategory.Graphics;
        var session=new GameSession(new WorkRandom());session.Restore(s);session.TickOffice(c);
        var milestone=stage switch {0=>DevelopmentMilestone.SelectType,1=>DevelopmentMilestone.SelectGenre,_=>DevelopmentMilestone.StartDebugging};
        Check(p.PendingDecision==milestone && p.Quality.Graphics==0);
        Check(burst.Office.BurstCount==37 && burst.Office.BurstTicks==119 && burst.Office.WorkTicks==151);
        for(int i=0;i<150;i++) session.TickOffice(c);
        Check(burst.Office.BurstTicks==119 && p.Quality.Graphics==0);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects.Single();burst=s.Employees[1];
        session.Restore(s);
        if(stage==0) Check(DevelopmentSelectionRules.TrySelectType(s,p,c,0,s.OriginalTypeRosterMap!)==DevelopmentSelectionResult.Success);
        else if(stage==1) Check(DevelopmentSelectionRules.TrySelectGenre(s,p,c,0)==DevelopmentSelectionResult.Success);
        else p.PendingDecision=null; // Actual Continue Debugging operation.
        double previous=p.Progress;session.TickOffice(c);
        Check(p.Quality.Graphics==37 && burst.Office!.WorkTicks==0);
        Check(Math.Abs(p.Progress-Math.Min(p.RequiredPoints,previous+37/3.0))<1e-9);
        var saved=SaveStore.Deserialize(SaveStore.Serialize(s)).DevelopmentProjects.Single();
        Check(saved.Quality.Graphics==37 && saved.Progress==p.Progress);
    }
});
Test("immediate shipment snapshots completed gains and discards another employee's pending burst",()=>
{
    var c=CoreCatalog();var s=PlanningState(10000000);s.SalesFee1=s.SalesFee2=1500;
    var lead=Worker();var burst=Worker(7,1);burst.Office!.SlotIndex=1;
    s.Employees.AddRange([lead,burst]);
    var p=new DevelopmentProjectState {Title="Synthetic immediate shipment",RequiredPoints=100,Progress=99,
        OriginalStage=2,GameTypeSourceIndex=0,GenreSourceIndex=0,Focus=new(100,0,0,0),Quality=new(10,20,30,40)};
    s.DevelopmentProjects.Add(p);
    lead.Office!.WorkTier=1;lead.Office.WorkTicks=16;lead.Office.WorkCategory=DevelopmentWorkCategory.Speed;
    burst.Office.WorkTier=3;burst.Office.WorkTicks=151;burst.Office.BurstTicks=119;
    burst.Office.BurstCount=37;burst.Office.WorkCategory=DevelopmentWorkCategory.Graphics;
    var session=new GameSession(new WorkRandom());session.Restore(s);session.TickOffice(c);
    Check(p.PendingDecision==DevelopmentMilestone.StartDebugging && p.Quality.Graphics==30 && burst.Office.BurstTicks==119);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));p=s.DevelopmentProjects.Single();burst=s.Employees[1];
    Check(ReleaseService.TryRelease(s,p,c,p.Title,5800,1,new SequenceRandom(6,0,0,0,0))==ReleaseResult.Success);
    var released=s.ReleasedGames.Single();Check(released.Quality==new DevelopmentQuality(10,20,30,40));
    Check(burst.Office!.WorkTicks==0 && burst.Office.WorkTier==0 && burst.Office.BurstTicks==0);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));burst=s.Employees[1];released=s.ReleasedGames.Single();
    var next=new DevelopmentProjectState {Title="Synthetic next project",RequiredPoints=1000,Focus=new(100,0,0,0)};
    s.DevelopmentProjects.Add(next);
    DevelopmentSimulation.TickEmployee(s,burst,c,new WorkRandom());
    Check(next.Quality==new DevelopmentQuality() && next.Progress==0 && released.Quality.Graphics==30);
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
Test("assigned desks use the computer array with tight crops and depth above desks",()=>
{
    var scene=new OfficeSceneData();
    for (int action=6;action<=17;action++)
        scene.Sprites.Add(new(50+(action-6),-100,110,162,308,36,29,33));
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
            Check(tile.SortKey==original.SortKey && tile.Width==36 && tile.Height==29);
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
    var future=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(State()))!;future["SchemaVersion"]=99;
    Throws<InvalidDataException>(() => SaveStore.Deserialize(future.ToJsonString()));
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
        Check(Directory.GetFiles(dir).Length == 2 && SaveStore.LoadBackup(path).Money == 100);
    }
    finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
});
Test("previous valid save survives damaged current slot and rejected writes", () =>
{
    string dir = Path.Combine(Path.GetTempPath(), "opengds2-backup-" + Guid.NewGuid());
    try
    {
        string path = Path.Combine(dir, "save.json");
        SaveStore.Save(path, State(100));
        Check(!File.Exists(SaveStore.BackupPath(path)));
        SaveStore.Save(path, State(200));
        Check(SaveStore.Load(path).Money == 200 && SaveStore.LoadBackup(path).Money == 100);
        File.WriteAllText(path, "{");
        Throws<InvalidDataException>(() => SaveStore.Load(path));
        Check(SaveStore.LoadBackup(path).Money == 100 && File.ReadAllText(path) == "{");
        SaveStore.Save(path, State(300));
        Check(SaveStore.Load(path).Money == 300 && SaveStore.LoadBackup(path).Money == 100);
        var invalid = State(400); invalid.Week = 5;
        Throws<InvalidDataException>(() => SaveStore.Save(path, invalid));
        Check(SaveStore.Load(path).Money == 300 && SaveStore.LoadBackup(path).Money == 100);
        SaveStore.Save(path, State(500));
        Check(SaveStore.Load(path).Money == 500 && SaveStore.LoadBackup(path).Money == 300);
        Check(!Directory.EnumerateFiles(dir, "*.tmp").Any());
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
Test("native horse raster keeps masks clipping and even rounding",()=>
{
    byte[] background=Enumerable.Repeat((byte)100,421*173*3).ToArray(),colors=new byte[233*316*3],masks=Enumerable.Repeat((byte)255,233*316*3).ToArray();
    void Pixel(int x,int y,byte r,byte g,byte b){int at=(y*233+x)*3;colors[at]=r;colors[at+1]=g;colors[at+2]=b;masks[at]=masks[at+1]=masks[at+2]=0;}
    Pixel(29,0,20,30,40);var race=new HorseRaceState {Tick=2,Positions=[2.5,-999,-999,-999,-999,-999,-999,-999]};var image=HorseRasterRules.Compose(background,colors,masks,race);
    Check(image.AsSpan((38*421+2)*3,3).SequenceEqual(new byte[]{20,30,40}));Check(image[(38*421+3)*3]==100);
    Pixel(31,0,10,20,30);int maskAt=31*3;masks[maskAt]=15;masks[maskAt+1]=240;masks[maskAt+2]=255;race.Positions[0]=-1.5;image=HorseRasterRules.Compose(background,colors,masks,race);Check(image.AsSpan(38*421*3,3).SequenceEqual(new byte[]{14,116,122}));
    Pixel(116,0,7,8,9);race.Tick=47;race.RacePhase=1;race.Positions[0]=100;image=HorseRasterRules.Compose(background,colors,masks,race);Check(image.AsSpan((130*421+15)*3,3).SequenceEqual(new byte[]{7,8,9}));
    Pixel(58,0,18,19,20);race.Tick=201;race.RacePhase=2;race.Positions[0]=-414;image=HorseRasterRules.Compose(background,colors,masks,race);Check(image.AsSpan((38*421+1)*3,3).SequenceEqual(new byte[]{18,19,20}));
});
Test("horse drawing retains the preceding scene and frame at native transitions",()=>
{
    var background=Enumerable.Repeat((byte)100,421*173*3).ToArray();var colors=new byte[233*316*3];var masks=Enumerable.Repeat((byte)255,colors.Length).ToArray();
    void Pixel(int x,byte value){for(int c=0;c<3;c++){colors[x*3+c]=value;masks[x*3+c]=0;}}
    Pixel(0,11);Pixel(177,22);Pixel(58,33);
    var race=new HorseRaceState {Tick=45,RacePhase=1,Positions=[10,-999,-999,-999,-999,-999,-999,-999]};
    Check(HorseRasterRules.Compose(background,colors,masks,race)[(38*421+10)*3]==11);
    race.Tick=200;race.RacePhase=2;race.FrontDistance=100;race.Positions=[0,20,20,20,20,20,20,20];
    Check(HorseRasterRules.Compose(background,colors,masks,race)[(130*421+15)*3]==22);
    race.Tick=201;race.Positions=[-405,-999,-999,-999,-999,-999,-999,-999];
    Check(HorseRasterRules.Compose(background,colors,masks,race)[(38*421+10)*3]==33);
});
Test("horse scenery matches sequential native scrolls across phases and reloads",()=>
{
    byte[] Scene(int height,int tint)
    {
        var data=new byte[421*height*3];
        for(int y=0;y<height;y++)for(int x=0;x<421;x++)
        {int at=(y*421+x)*3;data[at]=(byte)x;data[at+1]=(byte)(x>>8);data[at+2]=(byte)(y+tint);}
        return data;
    }
    // Independent oracle: execute each overlapping self-copy, then read the
    // resulting tail for the second copy, as the two native BitBlts do.
    void NativeScroll(byte[] data,int y,int height,int shift)
    {
        var before=(byte[])data.Clone();
        for(int row=y;row<y+height;row++)
            Array.Copy(before,row*421*3,data,(row*421+shift)*3,(421-shift)*3);
        var shifted=(byte[])data.Clone();
        for(int row=y;row<y+height;row++)
            Array.Copy(shifted,(row*421+421-shift)*3,data,row*421*3,shift*3);
    }
    var start=Scene(173,0);var closeup=Scene(217,20);var finish=Scene(173,40);
    var colors=new byte[233*316*3];var masks=Enumerable.Repeat((byte)255,colors.Length).ToArray();
    foreach(int tick in new[]{0,44,45,46,47,100,199,200,201,202,270,370,371,372,400})
    {
        var race=new HorseRaceState {Tick=tick,RacePhase=tick<45?0:tick<200?1:2,
            Camera=700-Math.Min(Math.Max(tick-200,0),171)*10,Positions=Enumerable.Repeat(-10000d,8).ToList()};
        int presented=tick<=45?0:tick<=200?1:2;
        var expected=(presented==0?start:presented==1?closeup:finish).AsSpan(0,start.Length).ToArray();
        if(presented==1)for(int i=46;i<tick;i++){NativeScroll(expected,0,77,2);NativeScroll(expected,82,91,6);}
        if(presented==2)for(int i=0;i<Math.Min(tick-201,171);i++)NativeScroll(expected,0,173,10);
        string saved=System.Text.Json.JsonSerializer.Serialize(race);
        var actual=HorseRasterRules.Compose(start,colors,masks,race,closeup,finish);
        if(!actual.SequenceEqual(expected))throw new Exception($"Scenery differs at tick {tick}");
        Check(HorseRasterRules.Compose(start,colors,masks,race,closeup,finish).SequenceEqual(actual));
        Check(System.Text.Json.JsonSerializer.Serialize(race)==saved);
        Check(HorseRasterRules.Compose(start,colors,masks,System.Text.Json.JsonSerializer.Deserialize<HorseRaceState>(saved)!,closeup,finish).SequenceEqual(actual));
    }
    Check(start.SequenceEqual(Scene(173,0))&&closeup.SequenceEqual(Scene(217,20))&&finish.SequenceEqual(Scene(173,40)));
});
Test("horse finish post follows camera with mask clipping and draws behind runners",()=>
{
    byte[] background=Enumerable.Repeat((byte)100,421*173*3).ToArray(),colors=new byte[233*316*3],masks=Enumerable.Repeat((byte)255,233*316*3).ToArray();
    // Mark both edges of the recovered 12x147 finish tile at sheet (0,169).
    void Pixel(int x,int y,byte color,byte mask=0)
    {int at=(y*233+x)*3;for(int c=0;c<3;c++){colors[at+c]=color;masks[at+c]=mask;}}
    Pixel(0,169,7,15);Pixel(11,315,31);
    var race=new HorseRaceState {Tick=280,RacePhase=2,Camera=-800,Positions=Enumerable.Repeat(-10000d,8).ToList()};
    var image=HorseRasterRules.Compose(background,colors,masks,race);
    Check(image[(10*421)*3]==3&&image[(156*421+11)*3]==31);
    race.Camera=-810;image=HorseRasterRules.Compose(background,colors,masks,race);
    Check(image[(10*421+10)*3]==3&&image[(156*421+21)*3]==31&&image[(10*421)*3]==100);
    race.Camera=-790;image=HorseRasterRules.Compose(background,colors,masks,race);
    Check(image[(156*421+1)*3]==31&&image[(10*421)*3]==100);
    race.Camera=-1220;image=HorseRasterRules.Compose(background,colors,masks,race);
    Check(image[(10*421+420)*3]==3);
    race.Camera=-1230;Check(HorseRasterRules.Compose(background,colors,masks,race).SequenceEqual(background));
    Pixel(0,197,12);Pixel(58,0,22);race.Tick=281;race.Camera=-800;race.Positions[0]=-415;
    image=HorseRasterRules.Compose(background,colors,masks,race);Check(image[(38*421)*3]==22);
});
if (args.Length == 2)
{
    Test("imported game-start icon matches the separate original application resource",()=>
    {
        var exe=File.ReadAllBytes(args[0]);const int offset=0x2301E2;
        var icon=NativePresentationAssetImporter.Icon(exe.AsSpan(offset,BitConverter.ToInt32(exe,offset-4)).ToArray());
        var pixels=icon.Rgba;for(int i=0;i<pixels.Length;i+=4)if(pixels[i+3]==0){pixels[i]=pixels[i+1]=pixels[i+2]=0;}
        Check(icon.Width==16&&icon.Height==16);
        Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)).ToLowerInvariant()=="c4d94d7c2ab04f612c22c920b3f136e41990e9934224262fee6b4f87474ce719");
    });
    Test("optional original native window icons match independent decoded pixels",()=>
    {
        var exe=File.ReadAllBytes(args[0]);
        (int Offset,string Hash)[] cases=[(0x3B574B,"266501a9e11edbb992eca2cb90e29f8842a99b16906a475815369416024ad142"),(0x297CB2,"8b68d7723299348a3326cf989a7813f6ceae15293fd9d4b6e6178e1082bafb82"),(0x25127F,"cfd2518a264aae014654812d40733e3d2aaff65ede481819e1bcf918cefa2a7f"),(0x274C52,"c4d94d7c2ab04f612c22c920b3f136e41990e9934224262fee6b4f87474ce719")];
        foreach(var (offset,hash) in cases)
        {
            int size=BitConverter.ToInt32(exe,offset-4);var raster=NativePresentationAssetImporter.Icon(exe.AsSpan(offset,size).ToArray());
            Check(raster.Width==16&&raster.Height==16);
            var pixels=raster.Rgba;for(int i=0;i<pixels.Length;i+=4)if(pixels[i+3]==0){pixels[i]=pixels[i+1]=pixels[i+2]=0;}
            Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)).ToLowerInvariant()==hash);
        }
        string root=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(root);
        try{NativePresentationAssetImporter.Extract(exe,root);Check(Directory.GetFiles(root,"WindowIcon-*.png").Length==82);Check(Directory.GetFiles(root,"Facility-*").Length==9);Check(!File.Exists(Path.Combine(root,"WindowIcon-MainForm3.png")));}
        finally{Directory.Delete(root,true);}
    });
    Test("optional original scene patches preserve animation and scenery outside captions",()=>
    {
        byte[] exe=File.ReadAllBytes(args[0]);var patches=new List<byte[]>();
        foreach(int offset in new[]{0x2FD305,0x2F2EA1})
        {
            int start=BitConverter.ToInt32(exe,offset+10);byte[] rgb=new byte[256*160*3];
            for(int y=0;y<160;y++)for(int x=0;x<256;x++)
            {int color=exe[offset+start+(159-y)*256+x],pal=offset+54+color*4,i=(y*256+x)*3;rgb[i]=exe[pal+2];rgb[i+1]=exe[pal+1];rgb[i+2]=exe[pal];}
            var patch=LocalSceneCaptionRules.Patch(rgb,true);Check(!patch.SequenceEqual(rgb));patches.Add(patch);
            var r=LocalSceneCaptionRules.HallRegion;
            for(int y=0;y<160;y++)for(int x=0;x<256;x++)if(x<r.X||x>=r.X+r.Width||y<r.Y||y>=r.Y+r.Height)
            {int i=(y*256+x)*3;Check(rgb.AsSpan(i,3).SequenceEqual(patch.AsSpan(i,3)));}
        }
        Check(!patches[0].SequenceEqual(patches[1]));
        var booth=LocalRasterDecoder.Gif(exe.AsSpan(0x11243,21264).ToArray(),256,160);
        byte[] original=booth.Rgba.Where((v,i)=>i%4!=3).ToArray(),translated=LocalSceneCaptionRules.Patch(original,false);
        Check(!translated.SequenceEqual(original));
        for(int y=22;y<160;y++)Check(original.AsSpan(y*256*3,256*3).SequenceEqual(translated.AsSpan(y*256*3,256*3)));
    });
    Test("optional original sales and Help captions preserve surrounding pixels",()=>
    {
        byte[] exe=File.ReadAllBytes(args[0]);(int Offset,int W,int H,int Stage)[] cases=[(0x380C40,256,160,1),(0x3767DE,256,160,2),(0x36C37C,256,160,3),(0x361F1A,256,160,4),(0x33F306,250,171,0)];
        foreach(var (offset,w,h,stage) in cases)
        {
            int bits=BitConverter.ToUInt16(exe,offset+28),start=BitConverter.ToInt32(exe,offset+10),stride=((w*bits+31)/32)*4;byte[] rgb=new byte[w*h*3];Check(bits is 8 or 24);
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){int data=offset+start+(h-1-y)*stride+(bits==8?x:x*3),color=bits==8?offset+54+exe[data]*4:data,i=(y*w+x)*3;rgb[i]=exe[color+2];rgb[i+1]=exe[color+1];rgb[i+2]=exe[color];}
            byte[] patch=stage==0?LocalSceneCaptionRules.PatchHelp(rgb):LocalSceneCaptionRules.PatchSales(rgb,stage);Check(!patch.SequenceEqual(rgb));var area=stage==0?LocalSceneCaptionRules.HelpRegion:LocalSceneCaptionRules.SalesRegions[stage-1];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(x<area.X||x>=area.X+area.Width||y<area.Y||y>=area.Y+area.Height)Check(rgb.AsSpan((y*w+x)*3,3).SequenceEqual(patch.AsSpan((y*w+x)*3,3)));
        }
    });
    Test("optional original awards caption fingerprint and scenery match",()=>
    {
        byte[] exe=File.ReadAllBytes(args[0]);var scene=LocalRasterDecoder.Gif(exe.AsSpan(0x1367D8,19520).ToArray(),256,160);
        byte[] rgb=scene.Rgba.Where((v,i)=>i%4!=3).ToArray();
        Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rgb)).ToLowerInvariant()=="d81ae783132d1866c722c3aa4e4ada3de3bd1cf43cfa6bd15108fbc75ea06463");
        var patch=LocalSceneCaptionRules.PatchAwards(rgb);Check(!patch.SequenceEqual(rgb));var r=LocalSceneCaptionRules.AwardsRegion;
        for(int y=0;y<160;y++)for(int x=0;x<256;x++)if(x<r.X||x>=r.X+r.Width||y<r.Y||y>=r.Y+r.Height)
        {int i=(y*256+x)*3;Check(rgb.AsSpan(i,3).SequenceEqual(patch.AsSpan(i,3)));}
    });
    Test("optional original button skins and arrows are recovered exactly",()=>
    {
        byte[] exe=File.ReadAllBytes(args[0]);string root=Path.Combine(Path.GetTempPath(),"opengds-buttons-"+Guid.NewGuid().ToString("N"));
        try
        {
            EmbeddedGraphicsImporter.Extract(exe,root);Check(Directory.GetFiles(root,"Button-*.bmp").Length>=40);
            Check(Directory.GetFiles(root,"CardRank*.bmp").Length==26&&Directory.GetFiles(root,"HorseMarker*.bmp").Length==24);
            Check(File.ReadAllBytes(Path.Combine(root,"CardBack.bmp")).SequenceEqual(exe.AsSpan(0x2BABBE,BitConverter.ToInt32(exe,0x2BABBE+2)).ToArray()));
            Check(File.ReadAllBytes(Path.Combine(root,"CompanyActivity.bmp")).SequenceEqual(exe.AsSpan(0x16585,BitConverter.ToInt32(exe,0x16585+2)).ToArray()));
            Check(File.ReadAllBytes(Path.Combine(root,"HardwareLaunchScene.bmp")).SequenceEqual(exe.AsSpan(0x28D66E,42038).ToArray()));
            var bitmap=exe.AsSpan(0x28D66E,42038).ToArray();int pixelStart=BitConverter.ToInt32(bitmap,10);byte[] rgb=new byte[256*160*3];
            for(int y=0;y<160;y++)for(int x=0;x<256;x++){int color=bitmap[pixelStart+(159-y)*256+x],pal=54+color*4,i=(y*256+x)*3;rgb[i]=bitmap[pal+2];rgb[i+1]=bitmap[pal+1];rgb[i+2]=bitmap[pal];}
            Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rgb)).ToLowerInvariant()=="c8ac9409990d3b897fc7ef49204b99df3e47d0a3d9c20d5dae8cc478497c57c0");
            var patched=LocalSceneCaptionRules.PatchHardware(rgb);Check(!patched.SequenceEqual(rgb));var area=LocalSceneCaptionRules.HardwareRegion;
            for(int y=0;y<160;y++)for(int x=0;x<256;x++)if(x<area.X||x>=area.X+area.Width||y<area.Y||y>=area.Y+area.Height)Check(rgb.AsSpan((y*256+x)*3,3).SequenceEqual(patched.AsSpan((y*256+x)*3,3)));
            foreach(int offset in new[]{0x28814D,0x289EF9,0x28A8DD,0x27DC18,0x270A71,0x2631DA,0x265C47,0x2661A9})
            {int size=BitConverter.ToInt32(exe,offset+2);Check(File.ReadAllBytes(Path.Combine(root,$"Button-{offset:X6}.bmp")).SequenceEqual(exe.AsSpan(offset,size).ToArray()));}
        }
        finally{if(Directory.Exists(root))Directory.Delete(root,true);}
    });
    Test("optional original all eight technology definitions charge and produce their real effect",()=>
    {
        var native=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
        foreach(var technology in native.Technologies)
        {
            var (state,employee,catalog,project)=UseFixture();catalog.Technologies=native.Technologies;
            employee.Experience=10000;employee.Stress=77;state.RetainedDevelopmentEligibility=0;
            int id=technology.Identifier;
            Check(TechnologyApplicationService.TryUse(state,employee,technology)==TechnologyApplicationResult.Success);
            Check(employee.Experience==10000-technology.RawTable1568[0]);
            if(id==5){Check(employee.Stress==38 && employee.Office!.TechnologyApplication is null);continue;}
            if(id==4)
            {
                Check(employee.Office!.SpeedBoostTicks==50);
                for(int i=0;i<50;i++)TechnologyApplicationService.TickSpeed(employee);
                Check(employee.Office.TechnologyApplication is null && employee.Office.SpeedBoostTicks==0);continue;
            }
            Check(employee.Office!.TechnologyApplication!.RemainingUnits==technology.RawTable158C[0]);
            for(int tick=0;tick<2000 && employee.Office.TechnologyApplication is not null;tick++)DevelopmentSimulation.TickEmployee(state,employee,catalog,new WorkRandom());
            Check(employee.Office.TechnologyApplication is null && project.TechnologyApplicationCounts![id]==1);
            Check(id switch {0=>project.DataSize<101.9,1=>project.Quality.Graphics>100,2=>project.Bugs<101,3=>project.Quality.Music>100,6=>project.Quality.Fun>100,7=>project.Quality.Novelty>100,_=>false});
        }
    });
    Test("optional original help and every bounded serialized artwork resource are recovered locally",()=>
    {
        byte[] original=File.ReadAllBytes(args[0]);var help=OriginalHelpImporter.Extract(original);
        Check(help.Paragraphs.Count==91 && help.Paragraphs.Select(p=>p.Topic).Distinct().Count()==5);
        for(int topic=0;topic<5;topic++)Check(help.Paragraphs.Count(p=>p.Topic==topic)==OriginalHelpEnglish.Page(topic).Count);
        string root=Path.Combine(Path.GetTempPath(),"opengds-art-"+Guid.NewGuid().ToString("N"));
        try{var art=SerializedArtworkImporter.Extract(original,root);Check(art.Count==422 && art.Count(a=>a.Format=="GIF")==22);}
        finally{if(Directory.Exists(root))Directory.Delete(root,true);}
    });

    Test("optional original raster pixels match independent decoded references",()=>
    {
        byte[] source=File.ReadAllBytes(args[0]);
        (string Name,int Offset,int Length,int Width,int Height,string Hash)[] resources=[
            ("exhibition",0x11243,21264,256,160,"409f289681a426dd8402885ff27d3b9283aefb5dbd4e03fb912cfd5f121d9e67"),
            ("awards",0x1367d8,19520,256,160,"4610a16cb67c2d5568c405eee08841fe703fa40488e32476561c48ed797a1f20"),
            ("hall",0x845d,862,40,40,"e202c0cf84a7cd41ca821495e47ca8ebdc68b32b8c6f4ea1724e6b64fcda46c2"),
            ("score1",0x312826,1998,64,64,"587ab427e571c9350134764ef832ab945ac5fdfffd706ffdd8d46e0bbfb004a2"),
            ("score2",0x311f4a,2222,64,64,"17b7d43bf6fa1aef81dd2b95c3d63ca85b58036c5a0e1003286ba591b1b9dc2d"),
            ("score3",0x3115ce,2382,64,64,"d95eb7b228eaf6e2bd6e1083cfa975376c06986eae1649977cfc5d84bed29b83"),
            ("score4",0x310cf2,2222,64,64,"766436e815ec6a85ae12f73453f4010e7b156bfe6056187b162e8844c5c09e13"),
            ("score5",0x3103f6,2254,64,64,"238978b7da3bf77fb80db3689e4a629de35812c50d0509b2ded8629272cbf8c7"),
            ("score6",0x30fa3a,2446,64,64,"a39037f36fde4b7e1d46d15978ecebf2487e6205000bd0c16ab7f857eb022151"),
            ("score7",0x30f21e,2030,64,64,"1efbc3c677422f5533cb3f18983b2d516c8471c1100f920aa7956cad6256dfd6"),
            ("score8",0x30e862,2446,64,64,"66b82b9c43a8c09622c5332fae5a9d65ae6db61bcef36cc88e2d4dbf23fdabbf"),
            ("score9",0x30de86,2478,64,64,"f0e0f99fb4792ae7101bcf2483049da7a0585461a0b06644d3b436c6b9498b2f"),
            ("score10",0x30d1ca,3214,64,64,"31f9b078370558139ec37f7ecd0185cf171f5eee35669397c781da865d0912b5")
        ];
        foreach(var r in resources)
        {
            byte[] bytes=source.AsSpan(r.Offset,r.Length).ToArray();
            var image=r.Name.StartsWith("score") ? LocalRasterDecoder.Rle8(bytes,r.Width,r.Height) : LocalRasterDecoder.Gif(bytes,r.Width,r.Height);
            Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(image.Rgba)).ToLowerInvariant()==r.Hash);
        }
        var dialogue=OriginalDialogueImporter.Extract(source);
        for(int band=0;band<6;band++)for(int v=0;v<ReviewPresentationRules.CommentCounts[band];v++)
            Check(!string.IsNullOrWhiteSpace(dialogue.Entries[$"REVIEW_COMMENT_{band}_{v}"]));
        Check(dialogue.Entries["REVIEW_COMMENT_0_1"].Contains("{genre}") && dialogue.Entries["REVIEW_COMMENT_4_4"].Contains("{company}"));
    });
    Test("optional original proposal import keeps the separate serialized document image",()=>
    {
        string directory=Path.Combine(Path.GetTempPath(),"opengds2-proposal-"+Guid.NewGuid().ToString("N"));
        try
        {
            EmbeddedGraphicsImporter.Extract(File.ReadAllBytes(args[0]),directory);
            byte[] proposal=File.ReadAllBytes(Path.Combine(directory,"PlanningProposal.bmp"));
            Check(proposal.Length==42036 && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(proposal)).ToLowerInvariant()==
                "68b9527b0fca162503e367eabe8f03b0114e92857c1926160ffa994de8073447");
            Check(!proposal.SequenceEqual(File.ReadAllBytes(Path.Combine(directory,"PlanningMeeting.bmp"))));
        }
        finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
    });
    Test("optional original horse and jockey append tables recover all source records locally",()=>
    {
        var c=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
        Check(c.Horses.Count==44 && c.Jockeys.Count==10 && CasinoHorseService.HasCatalog(c));
        Check(c.Horses.Select(h=>h.SourceIndex).SequenceEqual(Enumerable.Range(0,44)));
        Check(c.Jockeys.Select(j=>j.SourceIndex).SequenceEqual(Enumerable.Range(0,10)));
        Check(c.Horses.All(h=>!string.IsNullOrWhiteSpace(h.Name) && h.Strength==h.SourceIndex+h.RawParameter/100));
        Check(c.Jockeys.All(j=>!string.IsNullOrWhiteSpace(j.Name)));
        Check(c.Horses[5].RawParameter==15.4 && c.Horses[5].Strength==5.154);
    });

    Test("optional original catalog supports own-console launch and subsequent software planning",()=>
    {
        var db=EmployeeDatabaseImporter.Extract(File.ReadAllBytes(args[0]));var c=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
        var session=new GameSession();var s=session.StartNewGame("Synthetic company","Tester",GameMode.Normal,db,c);
        s.Money=2_000_000_000;Check(FacilityService.Open(s,0,FacilityKind.HardwareLab)==FacilityResult.Success);
        var hardware=StartHardware(s,c,design:1,medium:1);ReadyHardware(hardware);
        Check(OwnHardwareService.Launch(s,hardware,"Original-backed console")==OwnHardwareResult.Success);
        s=SaveStore.Deserialize(SaveStore.Serialize(s));s.DevelopmentComputerIndex=c.DevelopmentComputers.Single(x=>x.BitWidth==64).SourceIndex;
        Check(DevelopmentProjectService.TryAcceptNewPlan(s,c,0,11,0,"Original-backed software",new(25,25,25,25))==DevelopmentPlanningResult.Success);
        Check(s.DevelopmentProjects.Single().HardwareSourceIndex==11 && OwnHardwareService.Catalog(s,c).Hardware.Count==12 && c.Hardware.Count==11);
        SaveStore.Deserialize(SaveStore.Serialize(s));
    });
    Test("optional original game names have complete display-only English coverage",()=>
    {
        var catalog=OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
        var entries=System.Text.RegularExpressions.Regex.Matches(File.ReadAllText("localization/en.po"),"(?m)^msgid \"([^\"\\r\\n]+)\"\\r?\\nmsgstr \"([^\"\\r\\n]*)\"$")
            .ToDictionary(m=>m.Groups[1].Value,m=>m.Groups[2].Value);
        string Translate(string key)=>entries.GetValueOrDefault(key)??key;
        string before=System.Text.Json.JsonSerializer.Serialize(catalog);
        var rivals=catalog.InitialRivalProducts.Concat(catalog.StagedRivalProducts.Select(p=>p.Product)).ToArray();
        Check(rivals.Length==65&&catalog.GameTypes.Count==51);
        foreach(var rival in rivals)
        {
            string title=GameNameLocalization.RivalTitle(rival.Copy(),catalog,true,Translate);
            Check(!string.IsNullOrWhiteSpace(title)&&title.All(c=>c<128)&&!title.StartsWith("CATALOG_"));
            Check(GameNameLocalization.RivalTitle(rival,catalog,false,Translate)==rival.Title);
            var own=rival.Copy();own.Owner=-1;own.OwnProduct=0;
            Check(GameNameLocalization.RivalTitle(own,catalog,true,Translate)==rival.Title);
        }
        for(int owner=0;owner<10;owner++)Check(GameNameLocalization.RivalCompany(owner,catalog,true,Translate,"Unknown").All(c=>c<128));
        foreach(var type in catalog.GameTypes)
        {
            var examples=GameNameLocalization.Examples(type,catalog,true,Translate);
            Check(examples.Count==3&&examples.All(t=>!string.IsNullOrWhiteSpace(t)&&t.All(c=>c<128)&&!t.StartsWith("CATALOG_")));
            Check(GameNameLocalization.Examples(type,catalog,false,Translate).SequenceEqual(type.ExampleTitles));
        }
        Check(before==System.Text.Json.JsonSerializer.Serialize(catalog));
    });
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
        Check(dialogue.Entries.Count==374 && dialogue.SchemaVersion==18 && dialogue.SourceHash==EmployeeDatabaseImporter.ExpectedHash);
        Check(FinancialFailureService.ImportKeys.All(k=>dialogue.Entries.ContainsKey(k)) && dialogue.Entries["FINANCIAL_FAILURE_NAMED"].Contains("{name}"));
        Check(Enumerable.Range(0,10).All(i=>!string.IsNullOrWhiteSpace(dialogue.Entries[$"HIRE_GREETING_{i}"])));
        Check(dialogue.Entries["HIRE_GREETING_9"].Contains("{company}"));
        Check(ProjectCancellationDialogueRules.ImportKeys.All(key=>!string.IsNullOrWhiteSpace(dialogue.Entries[key])));
        Check(MysteryLabService.ImportKeys.All(key=>!string.IsNullOrWhiteSpace(dialogue.Entries[key])));
        Check(dialogue.Entries["CANCELLATION_EMPLOYEE_NAMED"].Contains("{name}") && dialogue.Entries["CANCELLATION_STRESS_SUMMARY"].Contains("{room}"));
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
        Check(scene.Sprites.Single(x=>x.Id==142)==new OfficeSpriteDefinition(142,-100,79,379,357,24,32,26));
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
Test("compact product chart preserves its five distinct native scale boundaries",()=>
{
    var history=SalesHistoryRules.NewHistory();history[12]=9999999;
    foreach(var item in new[]{(0L,20000L),(19999L,20000L),(20000L,40000L),(39999L,40000L),(40000L,80000L),(99999L,80000L),(100000L,200000L),(199999L,200000L),(200000L,400000L)})
    {history[0]=item.Item1;Check(SalesHistoryRules.CompactChartCeiling(history)==item.Item2);}
    history[0]=30000;Check(SalesHistoryRules.ChartCeiling(history)==80000 && SalesHistoryRules.CompactChartCeiling(history)==40000);
    Throws<InvalidDataException>(()=>SalesHistoryRules.CompactChartCeiling(new long[12]));
});

Test("review comments and callback stages preserve score bands and one-time draws",()=>
{
    var g=new ReleasedGameState {ReviewScores=[2,4,6,10],ReviewPending=true};
    var rng=new DialogueSequenceRandom([],0,.999f,0,.999f);ReviewPresentationRules.Prepare(g,rng);
    Check(g.ReviewCommentVariants!.SequenceEqual(new[]{0,7,0,3}) && rng.SingleCalls==4);
    for(int i=0;i<10;i++)Check(ReviewPresentationRules.Advance(g));
    Check(ReviewPresentationRules.Previewed(g,0) && !ReviewPresentationRules.Revealed(g,0));
    for(int i=10;i<20;i++)ReviewPresentationRules.Advance(g);
    Check(ReviewPresentationRules.Revealed(g,0) && !ReviewPresentationRules.Previewed(g,1));
    while(ReviewPresentationRules.Advance(g)){}
    Check(g.ReviewDisplayCallbacks==90 && ReviewPresentationRules.Revealed(g,3) && ReviewPresentationRules.Finished(g) && rng.SingleCalls==4);
    Check(ReviewPresentationRules.Reaction(9) is null && ReviewPresentationRules.Reaction(10)==7);
    Throws<InvalidOperationException>(()=>ReviewPresentationRules.Prepare(g,rng));
});
Test("review presentation saves partial progress and rejects out-of-band comments",()=>
{
    var s=State();var g=new ReleasedGameState {Title="Synthetic",ReviewScores=[2,4,6,10],ReviewPending=true,ReviewCommentVariants=[8,7,10,3],ReviewDisplayCallbacks=39};s.ReleasedGames.Add(g);
    var copy=SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0];
    Check(copy.ReviewDisplayCallbacks==39 && copy.ReviewCommentVariants!.SequenceEqual(g.ReviewCommentVariants!));
    g.ReviewCommentVariants[0]=9;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));g.ReviewCommentVariants[0]=8;
    g.ReviewDisplayCallbacks=91;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
    g.ReviewDisplayCallbacks=null;g.ReviewCommentVariants=null;Check(SaveStore.Deserialize(SaveStore.Serialize(s)).ReleasedGames[0].ReviewDisplayCallbacks is null);
});
Test("exhibition display replays saved visitor increments without repeating its transaction",()=>
{
    var (s,c)=CalendarFixture(new(3,9,4),100000000);s.Calendar!.ExhibitionVisitorRecord=123;
    s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic"}];Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));
    var rng=new SequenceRandom(Enumerable.Repeat(0,22).ToArray());Check(ExhibitionService.Participate(s,new HashSet<int>{0},rng)==ExhibitionResult.Success);
    var e=s.Calendar!.Pending!.Exhibition!;Check(e.PreviousVisitorRecord==123 && ExhibitionService.DisplayVisitors(e)==0);
    for(int i=0;i<23;i++)ExhibitionService.AdvanceDisplay(e);
    Check(ExhibitionService.DisplayVisitors(e)==2250 && s.Money==90000000 && rng.Calls==22);
    s=SaveStore.Deserialize(SaveStore.Serialize(s));e=s.Calendar!.Pending!.Exhibition!;
    Check(e.DisplayCallbacks==23 && e.VisitorSteps!.Count==22);
    while(ExhibitionService.AdvanceDisplay(e)){}
    Check(ExhibitionService.DisplayVisitors(e)==9900 && s.Money==90000000);
    Check(ExhibitionService.Participate(s,new HashSet<int>{0},new SequenceRandom())==ExhibitionResult.NotPending);
    e.VisitorSteps![0]=449;Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("award row history survives marker overwrites without repeating prizes",()=>
{
    var (s,c)=CalendarFixture(new(3,12,3),1000);
    s.ReleasedGames=[new() {ProductIndex=0,Title="Synthetic",ReleasedOn=new(3,6,1),Quality=new(30,140,70,70),OriginalAwardCode=0,UnitsSold=1000000}];
    Check(!CalendarService.BeginWeek(s,c,new SequenceRandom()));Check(AwardService.Reveal(s,new SequenceRandom(1)));Check(AwardService.Continue(s));
    Check(AwardService.Reveal(s,new SequenceRandom(1)));long money=s.Money!.Value;
    s=SaveStore.Deserialize(SaveStore.Serialize(s));var a=s.Calendar!.Pending!.Awards!;
    Check(a.Outcomes!.Select(o=>o.Category).SequenceEqual(new[]{GameAward.Music,GameAward.Graphics}) && a.Outcomes!.All(o=>o.ProductIndex==0));
    Check(!AwardService.Reveal(s,new SequenceRandom()) && s.Money==money);
    a.Outcomes.Add(a.Outcomes[0]);Throws<InvalidDataException>(()=>SaveStore.Serialize(s));
});
Test("portable local GIF RLE8 and PNG decoding preserves authored pixels and rejects truncation",()=>
{
    byte[] gif=[71,73,70,56,55,97,1,0,1,0,128,0,0,255,255,255,0,0,0,44,0,0,0,0,1,0,1,0,0,2,2,68,1,0,59];
    var raster=LocalRasterDecoder.Gif(gif,1,1);Check(raster.Rgba.SequenceEqual(new byte[]{255,255,255,255}));
    byte[] png=LocalRasterDecoder.Png(raster);Check(png.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10}));
    int pos=8;byte[]? zipped=null;
    while(pos<png.Length){int n=System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(pos));if(System.Text.Encoding.ASCII.GetString(png,pos+4,4)=="IDAT")zipped=png.AsSpan(pos+8,n).ToArray();pos+=12+n;}
    using(var zip=new System.IO.Compression.ZLibStream(new MemoryStream(zipped!),System.IO.Compression.CompressionMode.Decompress))
    {using var output=new MemoryStream();zip.CopyTo(output);Check(output.ToArray().SequenceEqual(new byte[]{0,255,255,255,255}));}
    Throws<InvalidDataException>(()=>LocalRasterDecoder.Gif(gif[..^1],1,1));
    Throws<InvalidDataException>(()=>LocalRasterDecoder.Gif(gif,2,1));
    var bmp=new byte[72];bmp[0]=66;bmp[1]=77;
    void I32(int at,int value)=>System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(at),value);
    I32(2,72);I32(10,62);I32(14,40);I32(18,2);I32(22,2);bmp[26]=1;bmp[28]=8;I32(30,1);I32(46,2);
    bmp[54]=bmp[55]=bmp[56]=255;new byte[]{2,1,0,0,2,0,0,0,0,1}.CopyTo(bmp,62);
    var rle=LocalRasterDecoder.Rle8(bmp,2,2);Check(rle.Rgba.Take(8).All(v=>v==255) && rle.Rgba.Skip(8).Where((v,i)=>i%4!=3).All(v=>v==0));
    Throws<InvalidDataException>(()=>LocalRasterDecoder.Rle8(bmp[..^1],2,2));
});

Test("schema-one award markers migrate once while preserving legacy meanings and funds",()=>
{
    var s=State();s.Money=123;s.ReleasedGames=Enumerable.Range(0,6).Select(i=>new ReleasedGameState {ProductIndex=i,Title="Synthetic "+i,OriginalAwardCode=i}).ToList();
    var json=System.Text.Json.Nodes.JsonNode.Parse(SaveStore.Serialize(s))!;json["SchemaVersion"]=1;
    var copy=SaveStore.Deserialize(json.ToJsonString());
    Check(copy.ReleasedGames.Select(p=>p.OriginalAwardCode).SequenceEqual(new int?[]{0,5,4,3,2,1}) && copy.Money==123);
    copy=SaveStore.Deserialize(SaveStore.Serialize(copy));
    Check(copy.ReleasedGames.Select(p=>p.OriginalAwardCode).SequenceEqual(new int?[]{0,5,4,3,2,1}));
    Check(AwardService.OriginalCode(GameAward.GrandPrize)==1 && AwardService.OriginalCode(GameAward.Design)==2 && AwardService.OriginalCode(GameAward.Graphics)==4 && AwardService.OriginalCode(GameAward.Worst)==5);
    Check(s.ReleasedGames[1].OriginalAwardCode==1); // Serialization does not mutate the caller.
});

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

sealed class DialogueSequenceRandom(int[] integers,params float[] singles) : Random
{
    private int integerIndex;
    public int SingleCalls {get;private set;}
    public List<string> Order {get;}=[];
    public override int Next(int maxValue)
    {
        if(integerIndex>=integers.Length)throw new Exception("Unexpected dialogue integer draw");
        int value=integers[integerIndex++];
        if(value<0 || value>=maxValue)throw new Exception("Invalid dialogue integer draw");
        Order.Add("N"+maxValue);return value;
    }
    public override float NextSingle()
    {
        if(SingleCalls>=singles.Length)throw new Exception("Unexpected dialogue portrait draw");
        float value=singles[SingleCalls++];
        if(value<0 || value>=1)throw new Exception("Invalid dialogue portrait draw");
        Order.Add("S");return value;
    }
}

sealed class BurstRandom : Random
{
    public override int Next(int maxValue)=>maxValue switch {130=>1,300=>0,50=>1,_=>0};
    public override double NextDouble()=>0;
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

sealed class FailureRandom(params float[] values) : Random
{
    public int Calls {get;private set;}
    public override float NextSingle()=>Calls<values.Length ? values[Calls++] : throw new Exception("Unexpected failure draw");
}

sealed class AudioWorkRandom(double draw) : Random
{
    public override int Next(int maxValue)=>maxValue==130?1:0;
    public override double NextDouble()=>draw;
    public override float NextSingle()=>.5f;
}
