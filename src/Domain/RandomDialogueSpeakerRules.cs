namespace OpenGDS2.Domain;

public sealed record RandomDialogueSpeaker(int SourceIndex,int PortraitId);

/// <summary>Module1 009933DC +0033..00E5, portrait argument -1.</summary>
public static class RandomDialogueSpeakerRules
{
    public static RandomDialogueSpeaker? Pick(GameState state,Random random)
    {
        // This rebuilds payroll scratch as well as the source-ordered portrait
        // roster. Incomplete modern tables cannot stand in for native arrays.
        if (!OriginalPayrollRules.CaptureStaffList(state)) return null;
        var rows=state.EmployeeSources.Where(x=>x.GateValue==1).OrderBy(x=>x.SourceIndex).ToList();
        // The native zero-count path indexes retained map cell 0. Its contents
        // are unestablished here; preserve the known count but invent no person.
        if(rows.Count==0) return null;
        int index=(int)Math.Floor(random.NextSingle()*rows.Count);
        var row=rows[index];
        return row.Employee.PortraitId is >=1 and <=10 ? new(row.SourceIndex,row.Employee.PortraitId.Value) : null;
    }
}
