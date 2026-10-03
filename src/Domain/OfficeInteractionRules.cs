namespace OpenGDS2.Domain;

/// <summary>fMainVisual 009C66BC +0758..08CB: select the first workspace hit.</summary>
public static class OfficeInteractionRules
{
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
