namespace OpenGDS2.Domain;

/// <summary>
/// Verified hiring-to-lounge subset of Module2 11. Work assignment and calendar
/// economics are separate, still incomplete systems; this does not advance dates.
/// </summary>
public static class OfficeStaffRules
{
    public const double TickSeconds = 0.040; // Main loop +DCE7..DD0D.
    private const int TimingFactor = 2;      // Main loop +0000..0005.

    public static OfficeEmployeeState CreateHire(GameState state, Random random)
    {
        var occupiedSlots = state.Employees.Where(x => x.Office is not null).Select(x => x.Office!.SlotIndex).ToHashSet();
        int slot = 0;
        while (occupiedSlots.Contains(slot)) slot++;
        var office = new OfficeEmployeeState { SlotIndex = slot, MovementState = 0 };
        var occupiedSeats = state.Employees.Where(x => x.Office?.LoungeSeat is not null)
            .Select(x => x.Office!.LoungeSeat!.Value).ToHashSet();
        if (occupiedSeats.Count == 12) return office;
        // Module2 10 +00B0..019E: after an occupied draw, counters 0..12
        // each retry. The initial draw plus these retries gives 14 draws.
        for (int attempt = 0; attempt < 14; attempt++)
        {
            int seat = random.Next(12);
            if (occupiedSeats.Contains(seat)) continue;
            office.LoungeSeat = seat;
            office.Action = 18 + seat / 4;
            office.MovementState = 3;
            return office;
        }
        // An exhausted random search does not authorize a guessed seat.
        return office;
    }

    public static void Tick(EmployeeState employee, Random random)
    {
        var office = employee.Office;
        if (office?.LoungeSeat is not int seat || office.Action is < 18 or > 20) return;
        int destinationNode = office.Action - 6;
        if (office.MovementState == 3)
        {
            var target = office.RouteNode switch
            {
                1 => (43,41), 2 => (55,53), 3 => (57,55),
                4 => (63,61), 12 => (63,49), _ => throw new InvalidDataException("Unsupported lounge route.")
            };
            if (At(office, target))
            {
                office.RouteNode = office.RouteNode switch
                {
                    1 => 2, 2 => 3, 3 => destinationNode == 14 ? 4 : 12,
                    4 => 14, 12 => 13, _ => office.RouteNode
                };
            }
            // Main loop +20ED..2358 switches to final approach as soon as the
            // selected route node is entered, before movement in this tick.
            if (office.RouteNode == destinationNode) office.MovementState = 10;
            else { Move(office, target); return; }
        }
        if (office.MovementState == 10)
        {
            var target = SeatTarget(seat);
            office.Facing = office.Action switch { 18 => 3, 19 => 4, _ => 1 };
            if (!At(office, target)) { Move(office, target); return; }
            office.MovementState = 4;
            office.MovementDirection = 0;
        }
        if (office.MovementState == 4)
        {
            if (office.ActionTicks == 2 * TimingFactor)
            {
                (office.OffsetX, office.OffsetY, office.DepthAdjustment) = seat switch
                {
                    0 => (-6,2,2), 1 => (0,-1,0), 2 => (-2,0,0), 3 => (-4,1,0),
                    >=4 and <=7 => (2,-2,0), >=8 and <=10 => (8,4,0), _ => (0,8,0)
                };
                office.Frame = 11 + office.Facing * 3;
            }
            if (office.ActionTicks == 4 * TimingFactor)
            {
                office.Frame = 10 + office.Facing * 3;
                office.MovementState = 6;
                office.ActionTicks = 0;
            }
            office.ActionTicks++;
        }
        // This follows the original fall-through into the rest block on the
        // tick which completes sitting, including its second counter increment.
        if (office.MovementState == 6)
        {
            if (office.ActionTicks == 1) office.IdleAnimation = random.Next(4);
            int tick = office.ActionTicks;
            int baseFrame = 10 + office.Facing * 3;
            switch (office.IdleAnimation)
            {
                case 0:
                    if (tick == 22) office.Frame = 28 + office.Facing;
                    if (tick == 44) office.Frame = baseFrame;
                    if (tick == 50) office.ActionTicks = 0;
                    break;
                case 1:
                    if (tick == 22) office.Frame = 32 + office.Facing;
                    if (tick == 60) office.Frame = baseFrame;
                    if (tick == 80) office.ActionTicks = 0;
                    break;
                case 2:
                    if (tick == 20 || tick == 60) office.Frame = 36 + office.Facing;
                    if (tick == 40 || tick == 80) office.Frame = 40 + office.Facing;
                    if (tick == 100) { office.Frame = baseFrame; office.ActionTicks = 0; }
                    break;
                case 3:
                    // Original +3F6D multiplies, rather than divides, this counter.
                    if (tick * TimingFactor >= 20) office.ActionTicks = 0;
                    break;
            }
            if (employee.Stress is double stress) employee.Stress = Math.Max(0, stress - 0.3 / TimingFactor);
            office.ActionTicks++;
        }
    }

    public static (int X, int Y) SeatTarget(int seat) => seat switch
    {
        0 or 1 => (60,52), 2 => (61,51), 3 => (62,50),
        4 => (66,52), 5 => (67,53), 6 => (68,54), 7 => (69,55),
        8 => (68,56), 9 => (67,57), 10 or 11 => (66,58),
        _ => throw new ArgumentOutOfRangeException(nameof(seat))
    };
    private static bool At(OfficeEmployeeState office, (int X, int Y) target)
        => office.X / 8.0 + 20 == target.X && office.Y / 4.0 + 20 == target.Y;

    private static void Move(OfficeEmployeeState office, (int X, int Y) target)
    {
        int dx = Math.Sign(target.X - (office.X / 8.0 + 20));
        int dy = Math.Sign(target.Y - (office.Y / 4.0 + 20));
        // Recovered entry/lounging segments are diagonal. Unsupported routes
        // must be reconstructed before being passed to this subset.
        if (dx == 0 && dy == 0) { office.MovementDirection = 0; return; }
        if (dx == 0 || dy == 0) throw new InvalidDataException("Unsupported non-diagonal lounge segment.");
        int direction = dx < 0 ? (dy < 0 ? 1 : 4) : (dy < 0 ? 2 : 3);
        office.MovementDirection = direction;
        office.X += dx * 8 / TimingFactor;
        office.Y += dy * 4 / TimingFactor;
        office.WalkingTicks++;
        int phase = office.WalkingTicks / TimingFactor;
        if (office.WalkingTicks % TimingFactor == 0)
        {
            office.Frame = (direction - 1) * 3 + (phase == 2 ? 2 : phase == 4 ? 3 : 1);
            if (phase == 4) office.WalkingTicks = 0;
        }
    }

    public static OfficeSpriteDefinition Sprite(EmployeeState employee)
    {
        var office = employee.Office ?? throw new InvalidOperationException("Unknown employee workspace.");
        if (employee.PortraitId is not int portrait || portrait is < 1 or > 10 || office.Frame is < 1 or > 44)
            throw new InvalidDataException("Invalid employee sprite.");
        int frame = office.Frame;
        int sx, sy;
        if (frame <= 24)
        {
            int index = (frame - 1) % 12;
            sx = (frame <= 12 ? 0 : 48) + index % 3 * 16;
            sy = index / 3 * 32;
        }
        else if (frame <= 28) { sx = 96; sy = (frame - 25) * 32; }
        else
        {
            int group = (frame - 29) / 4;
            sx = (group >= 2 ? 64 : 0) + (frame - 29) % 4 * 16;
            sy = group % 2 == 0 ? 128 : 160;
        }
        return new(office.SlotIndex, office.X + office.OffsetX, office.Y + office.OffsetY,
            sx + (portrait - 1) * 152, sy, 16, 32, 28 + office.DepthAdjustment - office.OffsetY);
    }
}
