namespace OpenGDS2.Domain;

/// <summary>
/// Recruitment behavior recovered from the original fGMSKoyou P-Code.
/// Constants here are evidence-backed; the underlying employee database is still being decoded.
/// </summary>
public static class RecruitmentRules
{
    /// <summary>The original hiring window fills five applicant slots.</summary>
    public const int ApplicantSlotCount = 5;

    /// <summary>
    /// A source record whose original gate/flag test passes then passes the roster-selection test when Int(Rnd * 4) == 0.
    /// This is one accepting outcome out of four for each test.
    /// </summary>
    public const int SelectionAcceptanceDenominator = 4;
}
