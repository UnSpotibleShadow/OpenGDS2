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

    // Existing authorized recordings, allocated in source order; not newly composed music.
    public static int FirstBundledTrack(MusicSeason season) => season switch
    {
        MusicSeason.Spring => 1,
        MusicSeason.Summer => 3,
        MusicSeason.Autumn => 5,
        MusicSeason.Winter => 7,
        _ => throw new System.ArgumentOutOfRangeException(nameof(season))
    };
}
