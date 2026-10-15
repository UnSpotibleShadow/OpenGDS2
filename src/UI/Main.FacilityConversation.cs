using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private DesktopWindow? _facilityOwner;
    private string? _facilityOwnerPositionKey;
    private Action? _facilityOwnerClose;
    private ProcessModeEnum _facilityOwnerMode;
    private readonly Dictionary<BaseButton,bool> _facilityOwnerButtons=new();
    private DesktopWindow? _facilityChild;
    private string? _facilityChildKey;
    private Action? _facilityChildClose;
    private ProcessModeEnum _facilityChildMode;
    private readonly Dictionary<BaseButton,bool> _facilityChildButtons=new();
    private void RetainFacilityChild()
    {
        if(_facilityChild is not null||_dialog is null)return;
        _facilityChild=_dialog;_facilityChildKey=_dialogPositionKey;_facilityChildClose=_dialog.CloseRequested;
        _facilityChildMode=_facilityChild.ProcessMode;_facilityChild.ProcessMode=ProcessModeEnum.Disabled;
        foreach(var b in _facilityChild.FindChildren("*","BaseButton",true,false).OfType<BaseButton>())
        {_facilityChildButtons[b]=b.Disabled;b.Disabled=true;}
        _facilityChild.CloseRequested=()=>{};_dialog=null;_dialogPositionKey=null;
    }
    private void CloseFacilityChild()
    {
        if(_facilityChild is { } child && GodotObject.IsInstanceValid(child))
        {if(_facilityChildKey is { } key)_dialogPositions[key]=child.Position;child.GetParent()?.RemoveChild(child);child.QueueFree();}
        _facilityChild=null;_facilityChildKey=null;_facilityChildClose=null;_facilityChildButtons.Clear();
    }
    private void RestoreFacilityChild(GameState state)
    {
        if(_facilityChild is not { } child || !GodotObject.IsInstanceValid(child)){RestoreFacilityOwner(state);return;}
        CloseConversationScene();CloseDesktopDialog();_dialog=child;_dialogPositionKey=_facilityChildKey;child.CloseRequested=_facilityChildClose;child.ProcessMode=_facilityChildMode;
        foreach(var pair in _facilityChildButtons)if(GodotObject.IsInstanceValid(pair.Key))pair.Key.Disabled=pair.Value;
        _facilityChild=null;_facilityChildKey=null;_facilityChildClose=null;_facilityChildButtons.Clear();child.MoveToFront();RefreshDesktop();
    }
    private void RestoreFacilityOwner(GameState state)
    {
        if(_facilityOwner is not { } owner || !GodotObject.IsInstanceValid(owner)){CloseFacilityOwner();ShowOfficeReconstruction(state);return;}
        CloseConversationScene();CloseDesktopDialog();CloseFacilityChild();
        _dialog=owner;_dialogPositionKey=_facilityOwnerPositionKey;owner.CloseRequested=_facilityOwnerClose;owner.ProcessMode=_facilityOwnerMode;
        foreach(var pair in _facilityOwnerButtons)if(GodotObject.IsInstanceValid(pair.Key))pair.Key.Disabled=pair.Value;
        _facilityOwner=null;_facilityOwnerPositionKey=null;_facilityOwnerClose=null;_facilityOwnerButtons.Clear();
        owner.MoveToFront();RefreshDesktop();
    }
    private void EnsureFacilityOwner(GameState state,FacilityKind kind,int? sourceSlot)
    {
        if(_facilityOwner is not null)return;
        if(_dialog?.Name.ToString()=="Facility"+kind+"Window"){RetainFacilityOwner();return;}
        int slot=sourceSlot??state.Calendar?.OriginalSlots0444.IndexOf((int)kind)??-1;
        if(slot is <0 or >2 || state.Calendar?.OriginalSlots0444[slot]!=(int)kind)return;
        ShowNativeFacility(state,slot);RetainFacilityOwner();
    }
    private string FacilityText(string key)=>TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)
        ?Tr(key):_importer.LoadDialogue()?.Entries.GetValueOrDefault(key)??Tr(key);
    private void FacilityMessage(GameState state,FacilityKind kind,string text,Action completed)
    {
        RetainFacilityOwner();ShowConversation(FacilityName((int)kind),[new(SecretaryCaption(state.SecretarySourceIndex),text,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],completed);
    }
    private void FinishFacility(GameState state){CloseFacilityOwner();ShowOfficeReconstruction(state);}
    private void FacilityFailure(GameState state,FacilityKind kind,FacilityResult result,Action? completed=null)
    {
        string key=result switch {FacilityResult.NoEmployees=>"FACILITY_NO_EMPLOYEES",FacilityResult.InsufficientFunds=>"FACILITY_NO_FUNDS",_=>"FACILITY_RESULT_"+result};
        FacilityMessage(state,kind,FacilityText(key),completed??(()=>FinishFacility(state)));
    }
    private void RetainFacilityOwner()
    {
        if(_facilityOwner is not null||_dialog is null)return;
        _facilityOwner=_dialog;_facilityOwnerPositionKey=_dialogPositionKey??_dialog.Name.ToString();
        _facilityOwnerClose=_facilityOwner.CloseRequested;
        _facilityOwnerMode=_facilityOwner.ProcessMode;_facilityOwner.ProcessMode=ProcessModeEnum.Disabled;
        foreach(var button in _facilityOwner.FindChildren("*","BaseButton",true,false).OfType<BaseButton>())
        {_facilityOwnerButtons[button]=button.Disabled;button.Disabled=true;}
        _facilityOwner.CloseRequested=()=>{};_dialog=null;_dialogPositionKey=null;
    }
    private void CloseFacilityOwner()
    {
        CloseFacilityChild();
        if(_facilityOwner is { } window&&GodotObject.IsInstanceValid(window))
        {
            if(_facilityOwnerPositionKey is { } key)_dialogPositions[key]=window.Position;
            window.GetParent()?.RemoveChild(window);window.QueueFree();
        }
        _facilityOwner=null;_facilityOwnerPositionKey=null;_facilityOwnerClose=null;_facilityOwnerButtons.Clear();
    }
}
