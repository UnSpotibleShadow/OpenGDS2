using System;
namespace OpenGDS2.Domain;

public readonly record struct GameDate(int Year, int Month, int Week);

public static class CalendarRules
{
    public static GameDate OpeningDate => new(1, 4, 1);
    // Original 009DB4F4 +01C7 checks year=2, month=12, transient week=5.
    // Modern pending transactions retain the valid week which is closing.
    public static GameDate BeginnerEndingClosingDate => new(2, 12, 4);
    public static bool IsBeginnerEnding(GameMode mode, GameDate closingDate)
        => mode == GameMode.Beginner && closingDate == BeginnerEndingClosingDate;
    // Module1 +01AE increments week; +067D tests >=5; +0715 tests month=13.
    // This is only the calendar transformation, not the complete weekly simulation.
    public static GameDate AdvanceWeek(GameDate date)
    {
        Validate(date);
        int week = date.Week + 1, month = date.Month, year = date.Year;
        if (week == 5) { week = 1; month++; }
        if (month == 13) { month = 1; year = checked(year + 1); }
        return new(year, month, week);
    }
    public static void Validate(GameDate date)
    {
        if (date.Year < 1 || date.Month is < 1 or > 12 || date.Week is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(date));
    }
}
