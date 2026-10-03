namespace OpenGDS2.Domain;

/// <summary>
/// Reimplementation of the fGMSKoyou roster loop recovered from VB6 P-Code.
/// It scans [minimumSourceIndex, sourceUpperBound), accepts only GateValue == 0,
/// then accepts a record when the random 0..3 result is zero. Passes repeat until
/// five slots are filled. The original can therefore repeat source indices; this
/// implementation preserves that behavior unless later evidence proves otherwise.
/// </summary>
public static class RecruitmentGenerator
{
    public static List<EmployeeState> BuildRoster(
        IReadOnlyList<RecruitmentSourceRecord> source,
        int minimumSourceIndex,
        int sourceUpperBound,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(random);
        if (minimumSourceIndex < 0 || sourceUpperBound < minimumSourceIndex)
            throw new ArgumentOutOfRangeException(nameof(minimumSourceIndex));

        var byIndex = source.ToDictionary(x => x.SourceIndex);
        var eligible = source.Where(x => x.SourceIndex >= minimumSourceIndex
                                      && x.SourceIndex < sourceUpperBound
                                      && x.GateValue == 0).ToArray();
        if (eligible.Length == 0)
            return [];

        var result = new List<EmployeeState>(RecruitmentRules.ApplicantSlotCount);
        // The VB6 code repeats full passes until count reaches five. A guard prevents
        // pathological custom RNG implementations from hanging a modern client forever.
        int passGuard = 0;
        while (result.Count < RecruitmentRules.ApplicantSlotCount && passGuard++ < 100_000)
        {
            for (int i = minimumSourceIndex;
                 i < sourceUpperBound && result.Count < RecruitmentRules.ApplicantSlotCount;
                 i++)
            {
                if (!byIndex.TryGetValue(i, out var row) || row.GateValue != 0)
                    continue;
                if (random.Next(RecruitmentRules.SelectionAcceptanceDenominator) != 0)
                    continue;
                result.Add(Clone(row.Employee));
            }
        }
        if (result.Count != RecruitmentRules.ApplicantSlotCount)
            throw new InvalidOperationException("Applicant random selection stalled.");
        return result;
    }

    private static EmployeeState Clone(EmployeeState x) => new()
    {
        SourceIndex = x.SourceIndex, PortraitId = x.PortraitId,
        OriginalAttributes = [.. x.OriginalAttributes],
        TechnologyLevels = x.TechnologyLevels is null ? null : new(x.TechnologyLevels),
        RawParameter34 = x.RawParameter34, RawParameter3C = x.RawParameter3C,
        RawParameter40 = x.RawParameter40, SpecialFlag = x.SpecialFlag,
        Name = x.Name,
        Level = x.Level,
        Experience = x.Experience, Stress = x.Stress,
        Fun = x.Fun,
        Novelty = x.Novelty,
        Accuracy = x.Accuracy,
        Graphics = x.Graphics,
        Music = x.Music,
        DevelopmentSpeed = x.DevelopmentSpeed,
        Inspiration = x.Inspiration,
        ContractFee = x.ContractFee,
    };
}
