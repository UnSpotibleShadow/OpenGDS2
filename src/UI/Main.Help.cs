using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
using OpenGDS2.Import;
using System.Text.Json;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowHelp(GameState? state=null,int topic=0)
    {
        var canvas=ManagementCanvas("HelpTopicWindow",Tr("GAME_HELP"),new(408,369));
        var selector=new OptionButton {Name="HelpTopicSelector"};for(int i=0;i<5;i++)selector.AddItem(Tr($"HELP_TOPIC_{i}"),i);
        selector.Select(topic);selector.AddThemeFontSizeOverride("font_size",11);NativePlace(canvas,selector,new(4,4,400,27));selector.ItemSelected+=i=>ShowHelp(state,(int)i);
        NativeFill(canvas,"HelpBodyBox",new(4,39,400,286),Colors.White,true);
        var scroll=NativePlace(canvas,new ScrollContainer {Name="HelpScroll",HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled},new(7,42,394,280));
        var contents=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};scroll.AddChild(contents);
        if(topic==3)
        {
            contents.AddChild(Picture("HelpTitlePicture",OriginalAssetStore.LoadTexture("Graphic/Embedded/HelpTitle.bmp"),new(250,171)));
            var website=new LineEdit {Name="OriginalHelpWebsite",Text=OriginalHelpEnglish.OriginalWebsite,Editable=false,CustomMinimumSize=new(370,25),TooltipText=Tr("HELP_HISTORICAL_WEBSITE")};website.AddThemeFontSizeOverride("font_size",11);contents.AddChild(website);
        }
        string text=string.Join("\n\n",OriginalHelpEnglish.Page(topic));
        bool english=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        if(!english)
        {
            try
            {
                var native=JsonSerializer.Deserialize<OriginalHelpCatalog>(System.IO.File.ReadAllText(OriginalAssetStore.PathFor("help.json")));
                if(native?.SourceHash=="4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6")
                    text=string.Join("\n\n",native.Paragraphs.Where(p=>p.Topic==topic).Select(p=>p.Text));
            }
            catch(System.IO.IOException){}catch(JsonException){}
        }
        var body=new RichTextLabel {Name="HelpTopicBody",Text=text,BbcodeEnabled=false,FitContent=true,ScrollActive=false,CustomMinimumSize=new(370,0),SizeFlagsHorizontal=SizeFlags.ExpandFill};body.AddThemeFontSizeOverride("normal_font_size",11);contents.AddChild(body);
        if(english)
        {
            var note=new RichTextLabel {Name="HelpPortNotes",Text=Tr("HELP_PORT_NOTES"),BbcodeEnabled=false,FitContent=true,ScrollActive=false,CustomMinimumSize=new(370,0),SizeFlagsHorizontal=SizeFlags.ExpandFill};note.AddThemeFontSizeOverride("normal_font_size",11);contents.AddChild(note);
        }
        NativeAction(canvas,"CloseHelp",state is null?"BACK_TO_TITLE":"BACK_TO_OFFICE",new(246,337,158,27),()=>{if(state is null)ShowTitleScreen();else ShowOfficeReconstruction(state);});
    }
}
