namespace OpenGDS2.Domain;

/// <summary>Recovered employee workspace fields, independent of the rendering engine.</summary>
public sealed class OfficeEmployeeState
{
    public int SlotIndex { get; set; }
    public int X { get; set; } = 128;
    public int Y { get; set; } = 56;
    public int Frame { get; set; } = 7;
    public int MovementDirection { get; set; }
    public int WalkingTicks { get; set; }
    public int RouteNode { get; set; } = 1;
    // Original member 2594: 3=route, 10=final approach, 4=sitting down, 6=rest.
    public int MovementState { get; set; } = 3;
    public int? LoungeSeat { get; set; }
    public int Action { get; set; }
    // Member 2754; lounge sitting does not set this field.
    public int SeatedAction { get; set; }
    // Member 28F8; set after reaching a work seat, cleared by reassignment.
    public bool WorkEngaged { get; set; }
    public int Facing { get; set; }
    public int ActionTicks { get; set; }
    public int IdleAnimation { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public int DepthAdjustment { get; set; }
    public EmployeeTrainingState? Training { get; set; }
    public int TrainingIdleTicks { get; set; }
    public int WorkTicks { get; set; }
    public int WorkIdleTicks { get; set; }
    // Zero is the original unclassified/exhausted tier.
    public int WorkTier { get; set; }
    public DevelopmentWorkCategory WorkCategory { get; set; }
    public int BurstTicks { get; set; }
    public int BurstCount { get; set; }
    public bool IdleBreak { get; set; }
}
