using Godot;

namespace OpenGDS2.UI;

public partial class Main
{
    private void QueueCompactNotice(string name)
    {
        if(name is not ("EmployeeDismissalWindow" or "LegacyRosterNoticeWindow" or
            "FinancialFailureNoticeWindow" or "CalendarInspectorWindow" or "CalendarEventWindow" or "AdditionalProductionWindow"))return;
        if(_dialog is not { } window || _workspace is null)return;
        var content=_content;
        float width=Math.Min(420,Math.Max(260,_workspace.Size.X-16));
        Callable.From(()=>
        {
            if(!GodotObject.IsInstanceValid(window)||window.IsQueuedForDeletion()||_dialog!=window||_content!=content)return;
            // Illustrated/native canvases and list forms keep their own layouts.
            if(content.GetChildren().Any(child=>child is not Label and not Button))return;
            content.CustomMinimumSize=new(width,0);content.AddThemeConstantOverride("separation",6);
            var actions=new HBoxContainer {Name="NoticeActions"};actions.AddThemeConstantOverride("separation",6);
            foreach(var button in content.GetChildren().OfType<Button>().ToArray())
            {
                content.RemoveChild(button);actions.AddChild(button);
                button.SizeFlagsHorizontal=SizeFlags.ExpandFill;button.CustomMinimumSize=new(0,26);
                button.AddThemeFontSizeOverride("font_size",11);
            }
            foreach(var label in content.GetChildren().OfType<Label>())label.AutowrapMode=TextServer.AutowrapMode.WordSmart;
            content.AddChild(actions);
            bool scheduled=false;
            void QueueFit()
            {
                if(scheduled)return;scheduled=true;
                Callable.From(()=>
                {
                    scheduled=false;
                    if(!GodotObject.IsInstanceValid(window)||window.IsQueuedForDeletion()||_dialog!=window||_content!=content)return;
                    float height=Math.Clamp(content.GetCombinedMinimumSize().Y+36,100,Math.Max(100,_workspace!.Size.Y-8));
                    var size=new Vector2(width+8,height);
                    if(!window.Size.IsEqualApprox(size))window.Size=size;
                    window.SetPositionInsideWorkspace(window.Position);
                }).CallDeferred();
            }
            content.MinimumSizeChanged+=QueueFit;
            QueueFit();
        }).CallDeferred();
    }
}
