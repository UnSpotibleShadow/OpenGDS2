using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private ConversationRevealState? _conversationReveal;
    private double _conversationElapsed;
    private Label? _conversationMessage;
    private TextureRect? _conversationPortrait;
    private int? _conversationPortraitId;
    private int _conversationPortraitFrame=-1;
    private DesktopWindow? _conversationScene;
    private sealed record ConversationPage(string Speaker,string Text,int? PortraitId=null);

    private string DialogueText(string key)
    {
        bool english=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        return english ? Tr(key) : _importer.LoadDialogue()?.Entries.GetValueOrDefault(key) ?? Tr("DIALOGUE_IMPORT_REQUIRED");
    }
    private void TickConversation(double delta)
    {
        if (_conversationReveal is null) return;
        _conversationElapsed=Math.Min(_conversationElapsed+delta,.2);
        while (_conversationElapsed>=ConversationRevealState.TickSeconds)
        {
            _conversationReveal.Tick();_conversationElapsed-=ConversationRevealState.TickSeconds;
        }
        RefreshConversation();
    }
    private void RefreshConversation()
    {
        if (_conversationReveal is null) return;
        _conversationMessage!.VisibleCharacters=_conversationReveal.VisibleCharacters;
        if (_conversationPortraitFrame==_conversationReveal.PortraitFrame) return;
        _conversationPortraitFrame=_conversationReveal.PortraitFrame;
        _conversationPortrait!.Texture=OriginalAssetStore.LoadEmployeePortrait(_conversationPortraitId,_conversationPortraitFrame);
    }
    private void CloseConversationScene()
    {
        if (_conversationScene is null) return;
        _conversationScene.GetParent()?.RemoveChild(_conversationScene);
        _conversationScene.QueueFree();_conversationScene=null;
    }
    private void ShowConversation(string heading,IReadOnlyList<ConversationPage> pages,Action completed,Action? cancelled=null,Texture2D? scene=null)
    {
        CloseConversationScene();
        if (pages.Count==0) {completed();return;}
        int pageIndex=0;
        void ShowPage()
        {
            ClearContent(keepConversationScene:true);
            CompactManagementWindow("ConversationWindow",new Vector2(330,142),314);
            _content.AddThemeConstantOverride("separation",0);
            _dialog!.SetTitle(heading);
            _dialog.FindChild("MinimizeWindow",true,false)?.QueueFree();
            var page=pages[pageIndex];
            var row=new HBoxContainer {CustomMinimumSize=new Vector2(0,80),SizeFlagsVertical=SizeFlags.ExpandFill};
            row.AddThemeConstantOverride("separation",4);_content.AddChild(row);
            var portrait=new TextureRect {Name="ConversationPortrait",
                ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize=new Vector2(64,64),MouseFilter=MouseFilterEnum.Stop,Visible=page.PortraitId is >=1 and <=10};row.AddChild(portrait);
            var text=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};text.AddThemeConstantOverride("separation",2);row.AddChild(text);
            var strip=new PanelContainer {Name="ConversationNameStrip"};
            strip.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=new Color(.75f,1,.75f),ContentMarginLeft=3});text.AddChild(strip);
            var name=new Label {Name="ConversationSpeaker",Text=page.Speaker,ClipText=true,MouseFilter=MouseFilterEnum.Stop};
            name.AddThemeFontSizeOverride("font_size",12);strip.AddChild(name);
            var body=new PanelContainer {SizeFlagsVertical=SizeFlags.ExpandFill};
            body.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=Colors.White,BorderColor=new Color(.5f,.5f,.5f),
                BorderWidthTop=1,BorderWidthLeft=1,ContentMarginLeft=4,ContentMarginRight=4,ContentMarginTop=3});text.AddChild(body);
            var message=new Label {Name="ConversationMessage",Text=page.Text,VisibleCharacters=0,
                AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill,
                SizeFlagsVertical=SizeFlags.ExpandFill,FocusMode=FocusModeEnum.All,MouseFilter=MouseFilterEnum.Stop};
            message.AddThemeFontSizeOverride("font_size",12);body.AddChild(message);
            _conversationReveal=new(page.Text);_conversationMessage=message;_conversationPortrait=portrait;
            _conversationPortraitId=page.PortraitId;_conversationPortraitFrame=-1;RefreshConversation();
            void Advance()
            {
                if (_conversationReveal is null) return;
                if (!_conversationReveal.Advance()) {RefreshConversation();return;}
                _conversationReveal=null;
                if (++pageIndex<pages.Count) ShowPage();
                else { CloseConversationScene();_toolbarHelp=string.Empty;completed(); }
            }
            void Input(InputEvent input)
            {
                if (input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true} ||
                    input is InputEventKey {Pressed:true,Echo:false} && input.IsActionPressed("ui_accept")) Advance();
            }
            portrait.GuiInput+=Input;name.GuiInput+=Input;message.GuiInput+=Input;
            // Planning is a reversible proposal. Closing cancels it; a persisted
            // hire greeting is acknowledged only after its page has been read.
            void Close()
            {
                if (cancelled is null) {Advance();return;}
                CloseConversationScene();_conversationReveal=null;cancelled();
            }
            _dialog.CloseRequested=Close;
            if (scene is not null && _conversationScene is null && _workspace is not null)
            {
                _conversationScene=new DesktopWindow {Name="PlanningDiscussionScene"};_workspace.AddChild(_conversationScene);
                _conversationScene.Initialize(heading,_dialog.Position-new Vector2(270,0),new Vector2(262,190),false);
                _conversationScene.FindChild("MinimizeWindow",true,false)?.QueueFree();
                _conversationScene.CloseRequested=Close;
                _conversationScene.Body.AddChild(new TextureRect {Name="PlanningDiscussionPicture",Texture=scene,
                    CustomMinimumSize=new Vector2(256,160),TextureFilter=TextureFilterEnum.Nearest,
                    ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepCentered});
                _dialog.MoveToFront();
            }
            message.GrabFocus();_toolbarHelp=Tr("CONVERSATION_DISMISS_HELP");RefreshDesktop();
        }
        ShowPage();
    }
    private bool ShowPendingHireGreeting(GameState state)
    {
        if (state.PendingHireGreeting is not HireGreetingState pending) return false;
        var employee=state.Employees.Single(x=>x.SourceIndex==pending.EmployeeSourceIndex);
        ShowConversation(DialogueText("NEW_EMPLOYEE"),[new(EmployeeName(employee),
            DialogueText($"HIRE_GREETING_{pending.Variant}").Replace("{company}",state.Company.Name),employee.PortraitId)],()=>
        {
            if (!ReferenceEquals(state.PendingHireGreeting,pending)) return;
            _session.AcknowledgeHireGreeting();ShowOfficeReconstruction(state);
        });
        return true;
    }
}
