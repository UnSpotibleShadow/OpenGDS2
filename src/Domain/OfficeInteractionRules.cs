namespace OpenGDS2.Domain;

/// <summary>fMainVisual 009C66BC +0758..08CB: select the first workspace hit.</summary>
public static class OfficeInteractionRules
{
    private static readonly (int Action,int MinX,int MaxX,int MinS,int MaxS,int MinD,int MaxD)[] Desks=[
        (6,178,223,212,255,20,47),(7,210,255,244,287,20,47),
        (8,146,191,212,239,36,77),(9,193,238,260,287,36,77),
        (10,130,175,212,255,68,95),(11,162,207,244,287,68,95),
        (12,274,319,308,351,20,47),(13,306,351,340,383,20,47),
        (14,242,287,308,335,36,77),(15,289,334,356,383,36,77),
        (16,226,271,308,351,68,95),(17,258,303,340,383,68,95)];
    public static int? DeskAt(double x,double y)
    {
        if(!double.IsFinite(x) || !double.IsFinite(y)) return null;
        float sum=(float)(y+x/2),difference=(float)(y-x/2);
        int? result=null;
        foreach(var d in Desks)
            if(x>=d.MinX && x<=d.MaxX && sum>d.MinS && sum<d.MaxS &&
                difference>d.MinD && difference<d.MaxD) result=d.Action;
        return result;
    }
    /// <summary>009C66BC +071F..0741: inclusive secretary advice hit.</summary>
    public static bool SecretaryAt(double x,double y)=>double.IsFinite(x) && double.IsFinite(y) &&
        x>=104 && x<=114 && y>=80 && y<=104;
    /// <summary>009C66BC +0223..0268 and +0470..04B5, native office pixels.</summary>
    public static int? DevelopmentRoomAt(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) return null;
        // Original narrows each transformed coordinate to a Single before the
        // strict comparisons. These are desk interiors, not sprite rectangles.
        float sum=(float)(y+x/2),difference=(float)(y-x/2);
        if (difference<=34 || difference>=69) return null;
        if (sum>228 && sum<260) return 0;
        if (sum>324 && sum<356) return 1;
        return null;
    }

    /// <summary>009C66BC +00D1..021E: three tenant diamonds, slots 0–2.</summary>
    public static int? TenantSlotAt(double x, double y)
    {
        var hits=TenantSlotsAt(x,y);
        return hits.Count==1 ? hits[0] : null;
    }

    /// <summary>The original independently forwards all matching plot hits.</summary>
    public static IReadOnlyList<int> TenantSlotsAt(double x,double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) return [];
        var hits=new List<int>(2);
        float sum=(float)(y+x/2),difference=(float)(y-x/2);
        if (difference>124 && difference<208)
        {
            if (sum>190 && sum<274) hits.Add(0);
            if (sum>270 && sum<354) hits.Add(1);
        }
        if (sum>398 && sum<482 && difference>-2 && difference<80) hits.Add(2);
        return hits;
    }

    public static EmployeeState? EmployeeAt(GameState state, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (!double.IsFinite(x) || !double.IsFinite(y)) return null;
        // Original scans workspace slots upwards and exits on the first hit.
        // Its inclusive hit box is inset two pixels from the 16 x 32 sprite.
        return state.Employees.Where(employee => employee.Office is not null)
            .OrderBy(employee => employee.Office!.SlotIndex)
            .FirstOrDefault(employee =>
            {
                var office = employee.Office!;
                long left = (long)office.X + office.OffsetX;
                long top = (long)office.Y + office.OffsetY;
                return x >= left + 2 && x <= left + 14 &&
                    y >= top + 2 && y <= top + 30;
            });
    }
}
