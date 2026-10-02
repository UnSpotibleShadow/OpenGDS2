using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowDeskAssignment(GameState state, EmployeeState employee)
    {
        ClearContent(); AddHeading(Tr("ASSIGN_DESK"),28);
        AddEmployeeCard(employee);
        AddHeading(Tr("DESK_ASSIGNMENT_NOTE"),16);
        var grid=new GridContainer {Name="DeskAssignments",Columns=2};
        grid.AddThemeConstantOverride("h_separation",16);
        grid.AddThemeConstantOverride("v_separation",8);
        _content.AddChild(grid);
        for (int seat=0; seat<12; seat++)
        {
            int action=seat+6;
            var occupant=state.Employees.FirstOrDefault(x=>x.Office?.Action==action);
            var button=new Button
            {
                Name=$"AssignDesk{action}",
                Text=$"{Tr("DEVELOPMENT_ROOM")} {seat/6+1} / {Tr("DESK")} {seat%6+1}"+
                    (occupant is null ? "" : $" — {occupant.Name}"),
                CustomMinimumSize=new Vector2(360,42),
                Disabled=occupant is not null
            };
            button.Pressed+=()=>
            {
                var result=OfficeAssignmentService.TryAssignDesk(state,employee,action);
                if (result==OfficeAssignmentResult.Success) ShowOfficeReconstruction(state);
                else { ShowDeskAssignment(state,employee); AddHeading(Tr($"ASSIGNMENT_RESULT_{result}"),16); }
            };
            grid.AddChild(button);
        }
        AddAction("CANCEL",()=>ShowStaff(state));
    }
}
