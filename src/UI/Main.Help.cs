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
        ClearContent();CompactManagementWindow("HelpTopicWindow",new(700,580),680);AddHeading(Tr("GAME_HELP"),24);
        var selector=new OptionButton {Name="HelpTopicSelector"};for(int i=0;i<5;i++)selector.AddItem(Tr($"HELP_TOPIC_{i}"),i);
        selector.Select(topic);_content.AddChild(selector);selector.ItemSelected+=i=>ShowHelp(state,(int)i);
        if(topic==3)_content.AddChild(Picture("HelpTitlePicture",OriginalAssetStore.LoadTexture("Graphic/Embedded/HelpTitle.bmp"),new(250,171)));
        string text=Tr($"HELP_BODY_{topic}");
        if(!TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var native=JsonSerializer.Deserialize<OriginalHelpCatalog>(System.IO.File.ReadAllText(OriginalAssetStore.PathFor("help.json")));
                if(native?.SourceHash=="4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6")
                    text=string.Join("\n\n",native.Paragraphs.Where(p=>p.Topic==topic).Select(p=>p.Text));
            }
            catch(System.IO.IOException){}catch(JsonException){}
        }
        var body=new RichTextLabel {Name="HelpTopicBody",Text=text,BbcodeEnabled=false,FitContent=false,ScrollActive=true,CustomMinimumSize=new(640,330)};_content.AddChild(body);
        AddAction(state is null?"BACK_TO_TITLE":"BACK_TO_OFFICE",()=>{if(state is null)ShowTitleScreen();else ShowOfficeReconstruction(state);});
    }
}
