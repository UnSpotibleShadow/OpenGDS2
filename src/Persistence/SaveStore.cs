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
            state.Employees is null || state.EmployeeSources is null || state.Recruitment.Candidates is null ||
            string.IsNullOrWhiteSpace(state.Company.Name) || string.IsNullOrWhiteSpace(state.Company.PresidentName))
            throw new InvalidDataException("Invalid session state.");
        if (state.Year is < 1 || state.Month is < 1 or > 12 || state.Week is < 1 or > 4)
            throw new InvalidDataException("Invalid calendar.");
        if (state.DevelopmentComputerIndex is < 0 or > 3)
            throw new InvalidDataException("Invalid development computer.");
        if ((state.HardwareLicenses?.Any(x => x is < 0 or > 10) ?? false) ||
            (state.AvailableHardware?.Any(x => x is < 0 or > 10) ?? false))
            throw new InvalidDataException("Invalid hardware state.");
        if (state.EmployeeSources.Any(x => x is null || x.Employee is null || x.SourceIndex < 0 ||
                string.IsNullOrWhiteSpace(x.Employee.Name) || x.Employee.OriginalAttributes is null || x.Employee.ContractFee < 0) ||
            state.EmployeeSources.Select(x => x.SourceIndex).Distinct().Count() != state.EmployeeSources.Count ||
            state.Employees.Concat(state.Recruitment.Candidates).Any(x => x is null || string.IsNullOrWhiteSpace(x.Name) || x.OriginalAttributes is null || x.ContractFee < 0 || x.Experience < 0 || x.Stress < 0 || (x.Experience.HasValue && !double.IsFinite(x.Experience.Value)) || (x.Stress.HasValue && !double.IsFinite(x.Stress.Value)) || x.Level is < 1 or > 10) ||
            state.Employees.Where(x => x.SourceIndex.HasValue).Select(x => x.SourceIndex).Distinct().Count() != state.Employees.Count(x => x.SourceIndex.HasValue))
            throw new InvalidDataException("Invalid employee records.");
        if (state.MinimumSourceIndex < 0 || state.SourceUpperBound < state.MinimumSourceIndex ||
            (state.Recruitment.CandidateDatabaseDecoded && (state.MinimumSourceIndex is null || state.SourceUpperBound is null ||
             state.EmployeeSources.Count == 0 || state.EmployeeSources.Any(x => x.SourceIndex >= state.SourceUpperBound))))
            throw new InvalidDataException("Invalid recruitment bounds.");
    }
}
