using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private int _heldShipmentGeneration;
    private Control HeldShipmentButtons(SpinBox value,bool quantity)
    {
        var row=new HBoxContainer {Name=value.Name+"HeldControls"};var window=_dialog;
        foreach(int sign in new[]{-1,1})
        {
            int direction=sign;bool repeated=false;
            var button=new Button {Name=value.Name+(sign<0?"DecreaseHeld":"IncreaseHeld"),Text=sign<0?"−":"+",CustomMinimumSize=new(24,22)};row.AddChild(button);
            NativeButtonArtwork.Apply(button,sign>0?0x2661A9:0x265C47,true);button.TooltipText=sign>0?"+":"−";
            button.ButtonDown+=()=>{repeated=false;AdvanceHeldShipment(value,quantity,direction,++_heldShipmentGeneration,window!,()=>repeated=true);};
            button.ButtonUp+=()=>_heldShipmentGeneration++;
            button.Pressed+=()=>{if(!repeated)ApplyShipmentStep(value,direction*(quantity?100:10));};
        }
        return row;
    }
    private static void ApplyShipmentStep(SpinBox value,double step)
    {double next=value.Value+step;value.Value=Math.Max(value.MinValue,value.AllowGreater?next:Math.Min(value.MaxValue,next));}
    private async void AdvanceHeldShipment(SpinBox value,bool quantity,int direction,int generation,DesktopWindow window,Action repeated)
    {
        int ticks=0;
        while(generation==_heldShipmentGeneration)
        {
            // Serialized fGMSyukka.Timer1 interval is 0x50 (80 ms), not 25 ms.
            await ToSignal(GetTree().CreateTimer(.08),SceneTreeTimer.SignalName.Timeout);
            if(generation!=_heldShipmentGeneration||!ReferenceEquals(_dialog,window)||!GodotObject.IsInstanceValid(value)||!value.IsInsideTree())return;
            ApplyShipmentStep(value,direction*HeldShipmentRules.Step(++ticks,quantity));repeated();
        }
    }
}
