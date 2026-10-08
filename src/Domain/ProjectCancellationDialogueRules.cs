namespace OpenGDS2.Domain;

public sealed class CancellationStaffReaction
{
    public string Name {get;set;}=string.Empty;
    public int? SourceIndex {get;set;}
    public int? PortraitId {get;set;}
    public int Variant {get;set;}
}

/// <summary>Modern persisted cursor over the verified fGMDevW cancellation message sequence.</summary>
public sealed class ProjectCancellationDialogueState
{
    public int RoomIndex {get;set;}
    public List<CancellationStaffReaction> Staff {get;set;}=[];
    public int MessageIndex {get;set;}
}

public static class ProjectCancellationDialogueRules
{
    public static readonly IReadOnlyList<string> ImportKeys =
        Enumerable.Range(0,7).Select(ReactionKey).Concat(new[]{"CANCELLATION_EMPLOYEE_NAMED",
            "CANCELLATION_STRESS_SUMMARY","CANCELLATION_DISCARD_SUMMARY"}).ToArray();
    public static string ReactionKey(int variant)=>variant is >=0 and <=6
        ? $"CANCELLATION_REACTION_{variant}" : throw new ArgumentOutOfRangeException(nameof(variant));

    internal static ProjectCancellationDialogueState Prepare(int room,IEnumerable<EmployeeState> staff,Random random)
    {
        var pending=new ProjectCancellationDialogueState {RoomIndex=room};
        foreach (var employee in staff)
        {
            // Original Rnd returns Single, followed by Int(Rnd * 7).
            float roll=random.NextSingle();
            if (!float.IsFinite(roll) || roll<0 || roll>=1) throw new ArgumentOutOfRangeException(nameof(random));
            pending.Staff.Add(new() {Name=employee.Name,SourceIndex=employee.SourceIndex,
                PortraitId=employee.PortraitId is >=1 and <=10 ? employee.PortraitId : null,
                Variant=(int)Math.Floor(roll*7)});
        }
        Validate(pending);return pending;
    }

    public static bool Acknowledge(GameState state)
    {
        if (state.PendingProjectCancellation is not { } pending) return false;
        Validate(pending);
        if (++pending.MessageIndex==pending.Staff.Count+2) state.PendingProjectCancellation=null;
        return true;
    }

    public static void Validate(ProjectCancellationDialogueState pending)
    {
        if (pending.RoomIndex is <0 or >1 || pending.Staff is null || pending.Staff.Count>60 ||
            pending.MessageIndex<0 || pending.MessageIndex>=pending.Staff.Count+2 ||
            pending.Staff.Any(x=>x is null || string.IsNullOrWhiteSpace(x.Name) || x.SourceIndex<0 ||
                x.PortraitId is not (null or >=1 and <=10) || x.Variant is <0 or >6) ||
            pending.Staff.Where(x=>x.SourceIndex.HasValue).Select(x=>x.SourceIndex).Distinct().Count()!=pending.Staff.Count(x=>x.SourceIndex.HasValue))
            throw new InvalidDataException("Invalid pending project cancellation dialogue.");
    }
}
