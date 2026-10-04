using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool ShowPendingHireGreeting(GameState state)
    {
        if (state.PendingHireGreeting is not HireGreetingState pending) return false;
        var employee=state.Employees.Single(x=>x.SourceIndex==pending.EmployeeSourceIndex);
        ClearContent();
        CompactManagementWindow("ConversationWindow",new Vector2(330,142),314);
        _content.AddThemeConstantOverride("separation",0);
        var original=_importer.LoadDialogue();
        bool english=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        string LocalText(string key) => english ? Tr(key) :
            original?.Entries.GetValueOrDefault(key) ?? Tr("DIALOGUE_IMPORT_REQUIRED");
        _dialog!.SetTitle(LocalText("NEW_EMPLOYEE"));
        _dialog.FindChild("MinimizeWindow",true,false)?.QueueFree();
        var row=new HBoxContainer {CustomMinimumSize=new Vector2(0,80),SizeFlagsVertical=SizeFlags.ExpandFill};
        row.AddThemeConstantOverride("separation",4);_content.AddChild(row);
        var portrait=new TextureRect {Name="ConversationPortrait",Texture=OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId),
            ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize=new Vector2(64,64),MouseFilter=MouseFilterEnum.Stop};row.AddChild(portrait);
        var text=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};text.AddThemeConstantOverride("separation",2);row.AddChild(text);
        var strip=new PanelContainer {Name="ConversationNameStrip"};
        strip.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=new Color(.75f,1,.75f),ContentMarginLeft=3});text.AddChild(strip);
        var name=new Label {Name="ConversationSpeaker",Text=EmployeeName(employee),ClipText=true,MouseFilter=MouseFilterEnum.Stop};
        name.AddThemeFontSizeOverride("font_size",12);strip.AddChild(name);
        var body=new PanelContainer {SizeFlagsVertical=SizeFlags.ExpandFill};
        body.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=Colors.White,BorderColor=new Color(.5f,.5f,.5f),
            BorderWidthTop=1,BorderWidthLeft=1,ContentMarginLeft=4,ContentMarginRight=4,ContentMarginTop=3});text.AddChild(body);
        var message=new Label {Name="ConversationMessage",Text=LocalText($"HIRE_GREETING_{pending.Variant}").Replace("{company}",state.Company.Name),
            AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill,
            SizeFlagsVertical=SizeFlags.ExpandFill,FocusMode=FocusModeEnum.All,MouseFilter=MouseFilterEnum.Stop};
        message.AddThemeFontSizeOverride("font_size",12);body.AddChild(message);
        void Dismiss()
        {
            if (!ReferenceEquals(state.PendingHireGreeting,pending)) return;
            _session.AcknowledgeHireGreeting();_toolbarHelp=string.Empty;ShowOfficeReconstruction(state);
        }
        void Input(InputEvent input)
        {
            if (input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true} ||
                input is InputEventKey {Pressed:true,Echo:false} && input.IsActionPressed("ui_accept")) Dismiss();
        }
        // The original portrait, name and message clicks share the advance handler.
        portrait.GuiInput+=Input;name.GuiInput+=Input;message.GuiInput+=Input;
        _dialog.CloseRequested=Dismiss;
        message.GrabFocus();
        _toolbarHelp=Tr("CONVERSATION_DISMISS_HELP");RefreshDesktop();
        return true;
    }
}
