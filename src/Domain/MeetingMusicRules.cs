namespace OpenGDS2.Domain;

public enum MeetingMusicPhase { Planning, Alpha, Beta, Final }

public static class MeetingMusicRules
{
    public static string ResourcePath(MeetingMusicPhase phase) => phase switch
    {
        MeetingMusicPhase.Planning => "res://audio/community/meetings/planning.ogg",
        MeetingMusicPhase.Alpha => "res://audio/community/meetings/alpha.ogg",
        MeetingMusicPhase.Beta => "res://audio/community/meetings/beta.ogg",
        MeetingMusicPhase.Final => "res://audio/community/meetings/final.ogg",
        _ => throw new ArgumentOutOfRangeException(nameof(phase))
    };

    public static MeetingMusicPhase ForMilestone(DevelopmentMilestone milestone) => milestone switch
    {
        DevelopmentMilestone.SelectType => MeetingMusicPhase.Alpha,
        DevelopmentMilestone.SelectGenre => MeetingMusicPhase.Beta,
        DevelopmentMilestone.StartDebugging => MeetingMusicPhase.Final,
        _ => throw new ArgumentOutOfRangeException(nameof(milestone))
    };
}
