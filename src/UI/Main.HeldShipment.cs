using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private int _heldShipmentGeneration;
    private void HeldShipmentButtons(SpinBox value,bool quantity)
    {
        var row=new HBoxContainer {Name=value.Name+"HeldControls"};_content.AddChild(row);var window=_dialog;
        foreach(int sign in new[]{-1,1})
        {
            int direction=sign;var button=new Button {Name=value.Name+(sign<0?"DecreaseHeld":"IncreaseHeld"),Text=sign<0?"−":"+",CustomMinimumSize=new(36,24)};row.AddChild(button);
            button.ButtonDown+=()=>AdvanceHeldShipment(value,quantity,direction,++_heldShipmentGeneration,window!);
            button.ButtonUp+=()=>_heldShipmentGeneration++;
        }
    }
    private async void AdvanceHeldShipment(SpinBox value,bool quantity,int direction,int generation,DesktopWindow window)
    {
        int ticks=0;
        while(generation==_heldShipmentGeneration)
        {
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(generation!=_heldShipmentGeneration||!ReferenceEquals(_dialog,window)||!GodotObject.IsInstanceValid(value)||!value.IsInsideTree())return;
            double next=value.Value+direction*HeldShipmentRules.Step(++ticks,quantity);
            value.Value=Math.Max(value.MinValue,value.AllowGreater?next:Math.Min(value.MaxValue,next));
        }
    }
}
