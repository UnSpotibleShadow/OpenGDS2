namespace OpenGDS2.Domain;

/// <summary>Module2 +88F3..9C15 and +C940..CE5D. No rendering-engine dependencies.</summary>
public static class OfficeVisualRules
{
    public static readonly (int X,int Y,int Width)[] BubbleRegions=[
        (2,367,41),(2,381,43),(2,395,52),(55,395,51),(516,357,52),(516,372,52),
        (516,386,52),(569,379,52),(627,357,52),(627,371,52),(627,385,52),
        (680,357,52),(680,371,52),(680,385,52),(733,385,52),(792,355,52)];
    public const int BubbleLifetime=15*OfficeStaffRules.TimingFactor;
    public static void TickBubble(OfficeEmployeeState office)
    {
        if (office.PendingSpeechBubble>0)
        {
            if (office.SpeechBubble==0) office.SpeechBubble=office.PendingSpeechBubble;
            office.PendingSpeechBubble=0; // An active bubble drops replacement requests.
        }
        if (office.SpeechBubble>0 && ++office.SpeechBubbleTicks>BubbleLifetime)
        {office.SpeechBubble=0;office.SpeechBubbleTicks=0;}
    }
    public static void StartWork(OfficeEmployeeState office,Random random)
    {
        if (office.WorkTier is <1 or >3) return;
        if (office.WorkCategory==DevelopmentWorkCategory.Bugs) {office.PendingSpeechBubble=15;return;}
        if (office.WorkTier==3 && office.WorkCategory!=DevelopmentWorkCategory.Debugging) {office.PendingSpeechBubble=10;return;}
        int roll=random.Next(office.WorkCategory==DevelopmentWorkCategory.Debugging ? 6 : 10);
        if (roll==0) office.PendingSpeechBubble=10;
        if (roll==1) office.PendingSpeechBubble=11;
    }
    public static IEnumerable<OfficeSpriteDefinition> Sprites(EmployeeState employee)
    {
        var office=employee.Office;
        if (office is null) yield break;
        if (office.SpeechBubble is >=1 and <=16)
        {
            var region=BubbleRegions[office.SpeechBubble-1];
            yield return new(100+office.SlotIndex,office.X+office.OffsetX-4,office.Y+office.OffsetY-12,
                region.X,region.Y,region.Width,14,540+office.DepthAdjustment-office.OffsetY);
        }
        if (!office.WorkEngaged || office.SeatedAction is <6 or >17 || office.WorkTier is <1 or >3 || office.WorkTicks<2) yield break;
        // WorkTicks is advanced at the end of the modern employee pass. The
        // source draws the just-completed pass before that increment.
        int tick=office.WorkTicks-1;
        if (office.WorkTier!=3 && tick>=16) yield break;
        (int X,int Y)[] seats=[(193,112),(225,128),(175,118),(205,144),(154,132),(186,148),
            (289,160),(321,176),(271,167),(301,192),(250,180),(282,196)];
        var seat=seats[office.SeatedAction-6];
        bool medium=office.WorkTier==2;
        int category=(int)office.WorkCategory;
        int x=seat.X,y=seat.Y-(medium ? 4 : 0);
        y-=2*Math.Clamp(tick-3,0,5)+Math.Clamp(tick-8,0,3);
        int sx=category==6 ? (medium ? 155 : 156) : (medium ? 155 : 156)+13*category;
        int sy=category==6 ? (medium ? 395 : 389) : (medium ? 365 : 359);
        int size=medium ? 8 : 5;
        if (office.WorkTier==3 && tick>=16)
        {
            x--;y-=4;sx=155+13*category;sy=365;size=8;
            if (tick>=20) {x-=2;y-=2;sx=153+13*category;sy=374;size=12;}
        }
        yield return new(200+office.SlotIndex*2,x,y,sx,sy,size,size,2000-y);
        if (office.WorkTier==3 && tick>=32)
        {
            int count=office.BurstDisplayCount ?? office.BurstCount;
            yield return new(201+office.SlotIndex*2,x-3,y+1,383+17*(count/10),226+12*(count%10),17,12,1999-y);
        }
    }
}
