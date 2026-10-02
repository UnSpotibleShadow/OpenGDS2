namespace OpenGDS2.Domain;

/// <summary>Assigned-development-computer rectangles, Module2 11 +2362..27AE.</summary>
public static class OfficeDeskPresentationRules
{
    public static OfficeSpriteDefinition ForAssignment(OfficeSpriteDefinition original, int action, int computer)
    {
        if (action is <6 or >17 || computer is <0 or >3) throw new ArgumentOutOfRangeException(nameof(action));
        if (original.Id!=93+2*(action-6)) throw new ArgumentException("Wrong imported desk tile.",nameof(original));
        var position=action switch
        {
            6=>(184,107),7=>(216,123),8=>(153,114),9=>(199,142),10=>(131,129),11=>(163,145),
            12=>(280,155),13=>(312,171),14=>(249,162),15=>(295,190),16=>(227,177),_=>(259,193)
        };
        // No depth-field write accompanies +2730/+274A; retain the allocator key.
        return original with
        {
            X=position.Item1,Y=position.Item2,
            SourceX=(action is 8 or 9 or 14 or 15 ? 571 : 608)+74*computer,
            SourceY=action is 6 or 7 or 8 or 12 or 13 or 14 ? 318 : 228,
            DepthOffset=original.Y+original.DepthOffset-position.Item2
        };
    }
}
