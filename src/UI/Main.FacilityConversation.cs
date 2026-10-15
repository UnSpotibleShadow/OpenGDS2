using Godot;
namespace OpenGDS2.UI;
public partial class Main
{
    private DesktopWindow? _facilityOwner;
    private string? _facilityOwnerPositionKey;
    private void RetainFacilityOwner()
    {
        if(_facilityOwner is not null||_dialog is null)return;
        _facilityOwner=_dialog;_facilityOwnerPositionKey=_dialogPositionKey??_dialog.Name.ToString();
        foreach(var button in _facilityOwner.FindChildren("*","Button",true,false).OfType<Button>())button.Disabled=true;
        _facilityOwner.CloseRequested=()=>{};_dialog=null;_dialogPositionKey=null;
    }
    private void CloseFacilityOwner()
    {
        if(_facilityOwner is { } window&&GodotObject.IsInstanceValid(window))
        {
            if(_facilityOwnerPositionKey is { } key)_dialogPositions[key]=window.Position;
            window.GetParent()?.RemoveChild(window);window.QueueFree();
        }
        _facilityOwner=null;_facilityOwnerPositionKey=null;
    }
}
