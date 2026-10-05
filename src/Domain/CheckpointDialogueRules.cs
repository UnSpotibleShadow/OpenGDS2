namespace OpenGDS2.Domain;

// Presentation acknowledgment only; it cannot advance a simulation milestone.
public static class CheckpointDialogueRules
{
    public static bool HasPending(DevelopmentProjectState project)
        =>project.PendingDecision.HasValue && project.DecisionIntroductionAcknowledged!=project.PendingDecision;
    public static bool ShouldIntroduce(GameState state,DevelopmentProjectState project)
        =>HasPending(project) && (project.PendingDecision==DevelopmentMilestone.StartDebugging || state.ReleasedGames.Count<=2);
    public static string MessageKey(DevelopmentMilestone milestone)=>milestone switch
    {
        DevelopmentMilestone.SelectType=>"CHECKPOINT_TYPE_INTRO",
        DevelopmentMilestone.SelectGenre=>"CHECKPOINT_GENRE_INTRO",
        DevelopmentMilestone.StartDebugging=>"CHECKPOINT_COMPLETE_INTRO",
        _=>throw new ArgumentOutOfRangeException(nameof(milestone))
    };
    public static bool Acknowledge(GameState state,DevelopmentProjectState project,DevelopmentMilestone milestone)
    {
        if (!state.DevelopmentProjects.Contains(project) || project.PendingDecision!=milestone || !HasPending(project)) return false;
        project.DecisionIntroductionAcknowledged=milestone;return true;
    }
}
