namespace OpenGDS2.Domain;

public sealed class CompanyState
{
    public string Name { get; set; } = string.Empty;
    public string PresidentName { get; set; } = "伊集院 光の助";

    // Fame (company recognition, percent) and rank opening values are statically
    // verified. Development power remains unresolved.
    public int? Fame { get; set; }
    public int? DevelopmentPower { get; set; }
    public int? Rank { get; set; }
}
