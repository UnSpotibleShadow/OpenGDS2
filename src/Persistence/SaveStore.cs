using System.Text.Json;
using OpenGDS2.Domain;
namespace OpenGDS2.Persistence;

/// <summary>Modern save format. Original VB6 save compatibility remains unresolved.</summary>
public static class SaveStore
{
    private const int MaxBytes = 4 * 1024 * 1024;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, MaxDepth = 48 };
    private sealed class Envelope
    {
        public Envelope() { }
        public string? Format { get; set; }
        public int SchemaVersion { get; set; }
        public GameState? State { get; set; }
    }
    public static string Serialize(GameState state)
    {
        Validate(state);
        return JsonSerializer.Serialize(new Envelope { Format = "OpenGDS2", SchemaVersion = 1, State = state }, Options);
    }
    public static GameState Deserialize(string text)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(text) > MaxBytes) throw new InvalidDataException("Save is too large.");
        Envelope save;
        try { save = JsonSerializer.Deserialize<Envelope>(text, Options) ?? throw new InvalidDataException("Empty save."); }
        catch (JsonException ex) { throw new InvalidDataException("Malformed save.", ex); }
        if (save.Format != "OpenGDS2" || save.SchemaVersion != 1 || save.State is null)
            throw new InvalidDataException("Unsupported save format or version.");
        Validate(save.State);
        // Derived mode rules cannot be overwritten by a malformed/stale save.
        save.State.Rules = GameModeRules.For(save.State.Mode);
        return save.State;
    }
    public static void Save(string path, GameState state)
    {
        string text = Serialize(state);
        string absolute = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        string temp = absolute + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text); stream.Write(bytes); stream.Flush(true);
            }
            File.Move(temp, absolute, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public static GameState Load(string path)
    {
        if (new FileInfo(path).Length > MaxBytes) throw new InvalidDataException("Save is too large.");
        return Deserialize(File.ReadAllText(path));
    }
    private static void Validate(GameState state)
    {
        if (!Enum.IsDefined(state.Mode) || state.Company is null || state.Initial is null || state.Recruitment is null ||
            state.Employees is null || state.EmployeeSources is null || state.Recruitment.Candidates is null || state.DevelopmentProjects is null ||
            string.IsNullOrWhiteSpace(state.Company.Name) || string.IsNullOrWhiteSpace(state.Company.PresidentName))
            throw new InvalidDataException("Invalid session state.");
        if (state.Year is < 1 || state.Month is < 1 or > 12 || state.Week is < 1 or > 4)
            throw new InvalidDataException("Invalid calendar.");
        if (state.DevelopmentComputerIndex is < 0 or > 3)
            throw new InvalidDataException("Invalid development computer.");
        if ((state.HardwareLicenses?.Any(x => x is < 0 or > 10) ?? false) ||
            (state.AvailableHardware?.Any(x => x is < 0 or > 10) ?? false))
            throw new InvalidDataException("Invalid hardware state.");
        if (state.OriginalAccuracyCells?.Any(x=>x.Key is <0 or >512 || x.Value is <0 or >99)==true)
            throw new InvalidDataException("Invalid original accuracy cells.");
        if (state.DevelopmentProjects.Count>2 ||
            state.DevelopmentProjects.Select(x=>x?.RoomIndex).Distinct().Count()!=state.DevelopmentProjects.Count)
            throw new InvalidDataException("Invalid development rooms.");
        foreach (var project in state.DevelopmentProjects) ValidateProject(project);
        if (state.EmployeeSources.Any(x => x is null || x.Employee is null || x.SourceIndex < 0 ||
                string.IsNullOrWhiteSpace(x.Employee.Name) || x.Employee.OriginalAttributes is null || x.Employee.ContractFee < 0) ||
            state.EmployeeSources.Select(x => x.SourceIndex).Distinct().Count() != state.EmployeeSources.Count ||
            state.Employees.Concat(state.Recruitment.Candidates).Any(x => x is null || string.IsNullOrWhiteSpace(x.Name) || x.OriginalAttributes is null || x.ContractFee < 0 || x.Experience < 0 || x.Stress < 0 || (x.Experience.HasValue && !double.IsFinite(x.Experience.Value)) || (x.Stress.HasValue && !double.IsFinite(x.Stress.Value)) || x.Level is < 1 or > 10) ||
            state.Employees.Where(x => x.SourceIndex.HasValue).Select(x => x.SourceIndex).Distinct().Count() != state.Employees.Count(x => x.SourceIndex.HasValue))
            throw new InvalidDataException("Invalid employee records.");
        var workspaces = state.Employees.Where(x => x.Office is not null).ToArray();
        if (workspaces.Any(x => x.PortraitId is not (>= 1 and <= 10) || !ValidOffice(x.Office!)) ||
            workspaces.Select(x => x.Office!.SlotIndex).Distinct().Count() != workspaces.Length ||
            workspaces.Where(x => x.Office!.LoungeSeat.HasValue).Select(x => x.Office!.LoungeSeat).Distinct().Count() !=
                workspaces.Count(x => x.Office!.LoungeSeat.HasValue))
            throw new InvalidDataException("Invalid office workspaces.");
        if (state.MinimumSourceIndex < 0 || state.SourceUpperBound < state.MinimumSourceIndex ||
            (state.Recruitment.CandidateDatabaseDecoded && (state.MinimumSourceIndex is null || state.SourceUpperBound is null ||
             state.EmployeeSources.Count == 0 || state.EmployeeSources.Any(x => x.SourceIndex >= state.SourceUpperBound))))
            throw new InvalidDataException("Invalid recruitment bounds.");
    }

    private static void ValidateProject(DevelopmentProjectState project)
    {
        if (project is null || project.RoomIndex is <0 or >1 || project.HardwareSourceIndex is <0 or >10 ||
            project.AudienceSourceIndex is <0 or >6 || project.GameTypeSourceIndex is <0 or >50 ||
            project.GenreSourceIndex is <0 or >17 || project.OriginalMode!=0 ||
            project.OriginalRoomState is not (1 or 2) || project.OriginalStage is <0 or >3 ||
            string.IsNullOrWhiteSpace(project.Title) || project.Title.Length>128 ||
            !double.IsFinite(project.RequiredPoints) || project.RequiredPoints<=0 ||
            !double.IsFinite(project.Progress) || project.Progress<0 || project.Progress>project.RequiredPoints ||
            !double.IsFinite(project.Recognition) || project.Recognition<0 ||
            !double.IsFinite(project.DataSize) || project.DataSize<0 || project.Bugs<0 || project.Focus is null ||
            (project.OriginalStage>=1 && project.GameTypeSourceIndex is null) ||
            (project.OriginalStage>=2 && project.GenreSourceIndex is null))
            throw new InvalidDataException("Invalid development project.");
        DevelopmentWorkRules.ValidateQuality(project.Quality);
        if (project.OriginalRoomState==2 && project.PolishingTargets is null)
            throw new InvalidDataException("Missing polishing targets.");
        if (project.PolishingTargets is not null) DevelopmentWorkRules.ValidateQuality(project.PolishingTargets);
        try {DevelopmentWorkRules.SelectCategory(project.Focus,0);}
        catch (ArgumentException ex) {throw new InvalidDataException("Invalid planning focus.",ex);}
    }

    private static bool ValidOffice(OfficeEmployeeState office)
        => office.SlotIndex >= 0 && office.SlotIndex <= short.MaxValue &&
            office.X is >= short.MinValue and <= short.MaxValue && office.Y is >= short.MinValue and <= short.MaxValue &&
            office.Frame is >= 1 and <= 44 && office.MovementDirection is >= 0 and <= 4 &&
            office.WalkingTicks is >= 0 and <= 7 && office.ActionTicks is >= 0 and <= 100 &&
            office.RouteNode is 1 or 2 or 3 or 4 or 12 or 13 or 14 &&
            office.MovementState is 0 or 3 or 4 or 6 or 10 && office.Facing is >= 0 and <= 4 &&
            office.IdleAnimation is >= 0 and <= 3 && office.OffsetX is >= -6 and <= 8 &&
            office.OffsetY is >= -2 and <= 8 && office.DepthAdjustment is 0 or 2 &&
            (office.LoungeSeat is null ? office.Action == 0 && office.MovementState == 0 :
                office.LoungeSeat is >= 0 and <= 11 && office.Action == 18 + office.LoungeSeat / 4 && office.MovementState != 0);
}
