namespace OpenGDS2.Domain;

/// <summary>fMainVisual 009C66BC +0758..08CB: select the first workspace hit.</summary>
public static class OfficeInteractionRules
{
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
        if (!double.IsFinite(x) || !double.IsFinite(y)) return null;
        float sum=(float)(y+x/2),difference=(float)(y-x/2);
        if (difference>124 && difference<208)
        {
            // The source's independent tests overlap here. Its modal sequence
            // needs runtime verification; do not invent a slot priority.
            if (sum>270 && sum<274) return null;
            if (sum>190 && sum<274) return 0;
            if (sum>270 && sum<354) return 1;
        }
        if (sum>398 && sum<482 && difference>-2 && difference<80) return 2;
        return null;
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
