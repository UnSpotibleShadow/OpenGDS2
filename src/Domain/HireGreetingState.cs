namespace OpenGDS2.Domain;

// Modern persistence of the original modal hire notification, not a hire transaction.
public sealed class HireGreetingState
{
    public int EmployeeSourceIndex { get; set; }
    public int Variant { get; set; }
}

public sealed class OriginalDialogueCatalog
{
    public int SchemaVersion { get; set; } = 1;
    public string SourceHash { get; set; } = string.Empty;
    public Dictionary<string,string> Entries { get; set; } = [];
}
