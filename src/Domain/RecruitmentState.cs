namespace OpenGDS2.Domain;

/// <summary>
/// Reconstructed state for the original fGMSKoyou form.
/// M6 has recovered the roster shape and random-selection test, plus the hire transaction,
/// including the source-index bounds and structural gate test; startup population of the source arrays remains unresolved.
/// </summary>
public sealed class RecruitmentState
{
    public List<EmployeeState> Candidates { get; set; } = [];

    public bool RosterAlgorithmDecoded { get; set; } = true;
    public bool CandidateDatabaseDecoded { get; set; }
    public bool CandidateGeneratorDecoded => RosterAlgorithmDecoded && CandidateDatabaseDecoded;

    public int ApplicantSlotCount => RecruitmentRules.ApplicantSlotCount;
    public int SelectionAcceptanceDenominator => RecruitmentRules.SelectionAcceptanceDenominator;

    // The form-load P-Code rebuilds the roster; the exact source-database eligibility field
    // still needs a semantic name before we reproduce refresh behavior.
    public bool ReopenCanRefreshApplicants { get; set; } = true;
}
