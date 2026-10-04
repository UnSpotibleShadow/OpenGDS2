using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowDeskAssignment(GameState state, EmployeeState employee)
    {
        ClearContent(); AddHeading(Tr("ASSIGN_DESK"),18);
        CompactManagementWindow("DeskAssignmentWindow",new Vector2(410,340),390);
        _content.AddChild(new Label {Text=employee.Name,ClipText=true,HorizontalAlignment=HorizontalAlignment.Center});
        _content.AddChild(new Label {Text=Tr("DESK_ASSIGNMENT_NOTE"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        var grid=new GridContainer {Name="DeskAssignments",Columns=2};
        grid.AddThemeConstantOverride("h_separation",8);
        grid.AddThemeConstantOverride("v_separation",3);
        _content.AddChild(grid);
        for (int room=0;room<2;room++) grid.AddChild(new Label
            {Text=$"{Tr("DEVELOPMENT_ROOM")} {room+1}",HorizontalAlignment=HorizontalAlignment.Center});
        for (int seat=0; seat<12; seat++)
        {
            // Each column is one room, with desks 1–6 down the column.
            int room=seat%2,desk=seat/2,action=6+room*6+desk;
            var occupant=state.Employees.FirstOrDefault(x=>x.Office?.Action==action);
            var button=new Button
            {
                Name=$"AssignDesk{action}",
                Text=$"{Tr("DESK")} {desk+1}"+(occupant is null ? "" : $" — {occupant.Name}"),
                TooltipText=$"{Tr("DEVELOPMENT_ROOM")} {room+1} / {Tr("DESK")} {desk+1}"+
                    (occupant is null ? "" : $" — {occupant.Name}"),
                TextOverrunBehavior=TextServer.OverrunBehavior.TrimEllipsis,
                CustomMinimumSize=new Vector2(185,26),SizeFlagsHorizontal=SizeFlags.ExpandFill,
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
