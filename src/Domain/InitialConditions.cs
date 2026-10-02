namespace OpenGDS2.Domain;

/// <summary>
/// Known opening-state facts. Values which have not yet been recovered remain nullable instead of
/// silently becoming zero and accidentally turning reverse-engineering gaps into game rules.
/// </summary>
public sealed class InitialConditions
{
    public long? StartingMoney { get; set; }
    public int? StartingYear { get; set; }
    public int? StartingMonth { get; set; }

    // A secretary is visible in the imported opening scene. An earlier external
    // description suggested one initial employee; no roster assignment is proven.
    public bool SecretaryPresent { get; set; } = true;
    public int? KnownEmployeeCount { get; set; }
}
