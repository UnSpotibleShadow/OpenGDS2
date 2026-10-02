namespace OpenGDS2.Domain;

public enum LevelUpResult { Success, EmployeeNotFound, LevelUnknown, MaximumLevel, ExperienceUnknown, InsufficientExperience, AttributesUnknown, FeeUnknown }

/// <summary>Manual fGMInfo level-up transaction. Experience earning is a separate unresolved rule.</summary>
public static class EmployeeLevelUpService
{
    private static readonly int[] Thresholds = [0, 100, 150, 200, 250, 300, 350, 400, 500, 600, 777];
    public static int RequiredExperience(int level) => level is >= 1 and <= 10
        ? Thresholds[level] : throw new ArgumentOutOfRangeException(nameof(level));

    // Draw order follows the original modal timer: bonus, fun, graphics, novelty,
    // music, accuracy, speed, inspiration. Validate everything before mutating.
    public static LevelUpResult TryLevelUp(GameState state, int sourceIndex, IReadOnlyList<float> draws)
    {
        var employee = state.Employees.SingleOrDefault(x => x.SourceIndex == sourceIndex);
        if (employee is null) return LevelUpResult.EmployeeNotFound;
        if (employee.Level is null) return LevelUpResult.LevelUnknown;
        if (employee.Level >= 10) return LevelUpResult.MaximumLevel;
        if (employee.Level < 1) throw new InvalidDataException("Invalid employee level.");
        if (employee.Experience is null) return LevelUpResult.ExperienceUnknown;
        if (!double.IsFinite(employee.Experience.Value)) throw new InvalidDataException("Invalid employee experience.");
        if (employee.Experience < RequiredExperience(employee.Level.Value)) return LevelUpResult.InsufficientExperience;
        int?[] current = [employee.Fun, employee.Graphics, employee.Novelty, employee.Music,
            employee.Accuracy, employee.DevelopmentSpeed, employee.Inspiration];
        if (current.Any(x => x is null) || employee.RawParameter3C is null) return LevelUpResult.AttributesUnknown;
        if (employee.ContractFee is null) return LevelUpResult.FeeUnknown;
        if (employee.ContractFee < 0) throw new InvalidDataException("Invalid employee contract fee.");
        if (draws.Count != 8) throw new ArgumentException("Eight original-order random draws are required.", nameof(draws));
        int bonus = EmployeeProgressionRules.SuperBonus(draws[0]);
        var next = current.Select((value, index) => value!.Value +
            EmployeeProgressionRules.AttributeGain(value.Value, employee.RawParameter3C.Value, bonus, draws[index + 1])).ToArray();
        long fee = checked((long)Math.Floor(employee.ContractFee.Value * 1.5d));
        int level = employee.Level.Value + 1;
        var source = state.EmployeeSources.SingleOrDefault(x => x.SourceIndex == sourceIndex);
        Apply(employee, level, fee, next);
        // The original arrays are the same records used by recruitment, not a second
        // immutable database. Keep the modern source copy consistent after progression.
        if (source is not null && !ReferenceEquals(source.Employee, employee)) Apply(source.Employee, level, fee, next);
        return LevelUpResult.Success;
    }

    private static void Apply(EmployeeState employee, int level, long fee, int[] stats)
    {
        employee.Level = level; employee.Experience = 0; employee.ContractFee = fee;
        employee.Fun = stats[0]; employee.Graphics = stats[1]; employee.Novelty = stats[2];
        employee.Music = stats[3]; employee.Accuracy = stats[4]; employee.DevelopmentSpeed = stats[5]; employee.Inspiration = stats[6];
        employee.OriginalAttributes.Clear();
        employee.OriginalAttributes.AddRange([stats[6], stats[5], stats[4], stats[3], stats[1], stats[2], stats[0]]);
    }
}
