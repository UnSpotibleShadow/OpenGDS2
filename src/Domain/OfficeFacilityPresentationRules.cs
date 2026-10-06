namespace OpenGDS2.Domain;

/// <summary>Module2.12 +0594..07A6, imported facility tiles 146–148.</summary>
public static class OfficeFacilityPresentationRules
{
    private static readonly int[] SourceColumns=[1,163,325,487,649,811,973,1297,1135];
    public static OfficeSpriteDefinition Project(OfficeSpriteDefinition original,GameState state)
    {
        if (original.Id is <146 or >148) return original;
        int slot=original.Id-146;
        if (state.Calendar?.OriginalSlots0444 is not {Count:3} facilities || facilities[slot] is <0 or >8)
            return original with {X=-1000};
        var position=slot switch {0=>(-16,130),1=>(64,170),_=>(320,170)};
        return original with {X=position.Item1,Y=position.Item2,SourceX=SourceColumns[facilities[slot]],SourceY=409,
            DepthOffset=original.Y+original.DepthOffset-position.Item2};
    }
}
