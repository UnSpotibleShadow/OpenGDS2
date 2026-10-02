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

    // Contemporary documentation of the original PC release says the company opens with a
    // secretary and one employee. Employee identity/stats still need binary confirmation.
    public bool SecretaryPresent { get; set; } = true;
    public int KnownEmployeeCount { get; set; } = 1;
}
