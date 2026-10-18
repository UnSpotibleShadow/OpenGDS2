namespace OpenGDS2.Domain;

/// <summary>Assigned-development-computer rectangles, Module2 11 +2362..27AE.</summary>
public static class OfficeDeskPresentationRules
{
    // Initializer +331D..34F9 maps actions 1–5 to IDs 45–49. Main loop
    // +2374..246C and +2717..27AE supplies their occupied PC rectangles.
    public static OfficeSpriteDefinition ForStudy(OfficeSpriteDefinition original,int action,int computer,bool training)
    {
        if(action is <1 or >5||computer is <0 or >3||original.Id!=44+action)throw new ArgumentOutOfRangeException(nameof(action));
        int x=action==5?277:147+(action-1)*32,y=action==5?86:43+(action-1)*16;
        return original with {X=x,Y=y,SourceX=(action==5?571:608)+74*computer,SourceY=228+(training?60:0),DepthOffset=original.Y+original.DepthOffset-y};
    }
    public static OfficeSpriteDefinition ForAssignment(OfficeSpriteDefinition original, int action, int computer)
    {
        if (action is <6 or >17 || computer is <0 or >3) throw new ArgumentOutOfRangeException(nameof(action));
        if (original.Id!=50+(action-6)) throw new ArgumentException("Wrong imported desk tile.",nameof(original));
        var position=action switch
        {
            6=>(184,107),7=>(216,123),8=>(153,114),9=>(199,142),10=>(131,129),11=>(163,145),
            12=>(280,155),13=>(312,171),14=>(249,162),15=>(295,190),16=>(227,177),_=>(259,193)
        };
        // Member 2218 maps to IDs 50–61. Their 36×29 allocation and depth
        // are retained; member 2234 IDs 93/95/... are hidden placeholders.
        return original with
        {
            X=position.Item1,Y=position.Item2,
            SourceX=(action is 8 or 9 or 14 or 15 ? 571 : 608)+74*computer,
            SourceY=action is 6 or 7 or 8 or 12 or 13 or 14 ? 318 : 228,
            DepthOffset=original.Y+original.DepthOffset-position.Item2
        };
    }
}
