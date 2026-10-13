using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    // Dialog content uses bounded desktop pixels, independently of the surrounding
    // office's responsive management panes. No outer scrolling or expanding rows.
    private Control NativeWindow(string name,string title,Vector2 client,string? positionKey=null)
    {
        if(_dialog is null)throw new InvalidOperationException("A desktop dialog is required.");
        foreach(Node child in _dialog.Body.GetChildren()){_dialog.Body.RemoveChild(child);child.QueueFree();}
        _content=new VBoxContainer {Name="NativeDialogContent",CustomMinimumSize=client,
            SizeFlagsHorizontal=Control.SizeFlags.ShrinkBegin,SizeFlagsVertical=Control.SizeFlags.ShrinkBegin};
        _content.AddThemeConstantOverride("separation",0);_dialog.Body.AddChild(_content);
        var canvas=new Control {Name="NativeDialogCanvas",CustomMinimumSize=client,
            SizeFlagsHorizontal=Control.SizeFlags.ShrinkBegin,SizeFlagsVertical=Control.SizeFlags.ShrinkBegin};_content.AddChild(canvas);
        CompactManagementWindow(name,client+new Vector2(8,32),client.X,positionKey);_dialog.SetTitle(title);
        return canvas;
    }
    private static T NativePlace<T>(Control parent,T control,Rect2 rect) where T:Control
    {parent.AddChild(control);control.Position=rect.Position;control.Size=rect.Size;return control;}
    private static Label NativeText(Control parent,string name,string text,Rect2 rect,int font=11,HorizontalAlignment align=HorizontalAlignment.Left)
    {
        var label=new Label {Name=name,Text=text,ClipText=true,HorizontalAlignment=align,VerticalAlignment=VerticalAlignment.Center,TooltipText=text};
        label.AddThemeConstantOverride("line_spacing",0);
        label.AddThemeFontSizeOverride("font_size",font);label.AddThemeColorOverride("font_color",Colors.Black);
        return NativePlace(parent,label,rect);
    }
    private Button NativeAction(Control parent,string name,string key,Rect2 rect,Action action,bool enabled=true)
    {
        var button=new Button {Name=name,Text=Tr(key),TooltipText=Tr(key),Disabled=!enabled,ClipText=true};
        button.AddThemeFontSizeOverride("font_size",11);
        button.AddThemeStyleboxOverride("normal",ClassicTheme.CompactButton());button.AddThemeStyleboxOverride("pressed",ClassicTheme.CompactButton(true));
        int? glyph=name switch {"ProjectPromotion"=>0x289EF9,"ProjectCancel"=>0x28814D,
            "ProjectRelease" or "CompactProduceMore"=>0x28A8DD,"CheckpointCancelProject"=>0x28814D,_=>null};
        if(glyph is int offset)NativeButtonArtwork.Apply(button,offset);
        NativePlace(parent,button,rect);button.Pressed+=()=>{if(!button.Disabled)action();};return button;
    }
    private static Control NativeFill(Control parent,string name,Rect2 rect,Color color,bool border=false)
    {
        var panel=new Panel {Name=name,MouseFilter=Control.MouseFilterEnum.Ignore};
        var style=new StyleBoxFlat {BgColor=color,BorderColor=new(.5f,.5f,.5f)};if(border)style.SetBorderWidthAll(1);
        panel.AddThemeStyleboxOverride("panel",style);return NativePlace(parent,panel,rect);
    }
    private void NativeCompletionSummary(Control canvas,GameState state,DevelopmentProjectState project,OriginalCatalog catalog)
    {
        string hardware=CatalogName(catalog,"HARDWARE",project.HardwareSourceIndex,catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex).Name);
        string genre=project.GenreSourceIndex is int g?CatalogName(catalog,"GENRE",g,catalog.Genres.Single(x=>x.SourceIndex==g).Name):"—";
        string type=project.GameTypeSourceIndex is int t?CatalogName(catalog,"TYPE",t,catalog.GameTypes.Single(x=>x.SourceIndex==t).Name):"—";
        NativeFill(canvas,"CompletionCatalogFields",new(4,198,170,49),new(.77f,1,.77f));
        NativeText(canvas,"ProjectCatalogSummary",$"{Tr("TARGET_HARDWARE")}: {hardware}\n{Tr("PROJECT_GENRE")}: {genre}\n{Tr("PROJECT_CONTENT")}: {type}",new(6,198,166,49),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        double[] q=[project.Quality.Fun,project.Quality.Novelty,project.Quality.Graphics,project.Quality.Music];
        string[] keys=["STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"];
        Color[] colors=[Colors.Blue,Colors.Yellow,new(.7f,0,.8f),Colors.Cyan];
        for(int i=0;i<4;i++)
        {
            var dot=NativeText(canvas,"CompleteDot"+i,"●",new(4,250+i*16,12,16));dot.AddThemeColorOverride("font_color",colors[i]);
            NativeText(canvas,"CompleteQualityCaption"+i,Tr(keys[i]),new(16,250+i*16,80,16));
            NativeText(canvas,"ProjectQuality"+i,$"{q[i]:N0}",new(98,250+i*16,70,16),11,HorizontalAlignment.Right);
        }
        NativeText(canvas,"CompletionSize",Tr("PROJECT_SIZE")+": "+$"{Math.Floor(project.DataSize):N0} KB",new(180,198,184,21));
        NativeText(canvas,"ProjectRecognition",Tr("NATIVE_RECOGNITION")+": "+$"{project.Recognition:N0}%",new(180,226,184,18));
        NativeText(canvas,"CompleteBugCaption",Tr("BUGS"),new(180,254,70,17));
        NativeFill(canvas,"CompleteBugBox",new(184,275,49,25),Colors.Red);
        NativeText(canvas,"ProjectBugCount",project.Bugs.ToString(),new(184,275,49,25),15,HorizontalAlignment.Center);
    }

}
