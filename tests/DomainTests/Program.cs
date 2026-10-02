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
                a.InitialGate == b.InitialGate && a.InitialSpecialFlag == b.InitialSpecialFlag);
        }
        var catalogPath = Path.Combine(Path.GetDirectoryName(args[1])!, "catalog.json");
        if (File.Exists(catalogPath))
        {
            var catalog = OriginalCatalogImporter.Extract(File.ReadAllBytes(args[0]));
            var expected = System.Text.Json.JsonSerializer.Deserialize<OriginalCatalog>(File.ReadAllText(catalogPath))!;
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
                    actual.RawMembers.OrderBy(x=>x.Key).SequenceEqual(row.RawMembers.OrderBy(x=>x.Key)));
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
sealed class AlternatingRandom : Random { private int n; public override int Next(int maxValue) => n++ % 2 == 0 ? 3 : 0; }
