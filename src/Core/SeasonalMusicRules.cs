namespace OpenGDS2.Core;

// Community presentation only: no original rules, clock changes or RNG calls.
public enum MusicSeason { Spring, Summer, Autumn, Winter }

public static class SeasonalMusicRules
{
    public static MusicSeason? ForMonth(int? month) => month switch
    {
        >= 3 and <= 5 => MusicSeason.Spring,
        >= 6 and <= 8 => MusicSeason.Summer,
        >= 9 and <= 11 => MusicSeason.Autumn,
        12 or 1 or 2 => MusicSeason.Winter,
        _ => null
    };

    public static string Folder(MusicSeason season) => season.ToString().ToLowerInvariant();

    public static System.Collections.Generic.IEnumerable<string> BundledPaths(MusicSeason season)
    {
        if (!System.Enum.IsDefined(season)) throw new System.ArgumentOutOfRangeException(nameof(season));
        string prefix = season == MusicSeason.Summer ? "audio/community" : $"audio/community/{Folder(season)}";
        int count = season == MusicSeason.Summer ? 8 : 4;
        for (int index = 1; index <= count; index++) yield return $"{prefix}/office-{index:00}.ogg";
    }

    public const double CrossfadeSeconds = 2;

    // Equal-power gains avoid the dip caused by a linear blend of unrelated recordings.
    public static (float Incoming, float Outgoing) CrossfadeGains(double elapsedSeconds)
    {
        double fraction = double.IsFinite(elapsedSeconds) ? System.Math.Clamp(elapsedSeconds / CrossfadeSeconds, 0, 1) : 1;
        return ((float)System.Math.Sin(fraction * System.Math.PI / 2), (float)System.Math.Cos(fraction * System.Math.PI / 2));
    }
}
