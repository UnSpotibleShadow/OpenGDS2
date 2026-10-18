using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private const string KairosoftWebsite="https://kairosoft.net/";

    private void ShowAbout(GameState state)
    {
        var canvas=ManagementCanvas("AboutWindow",Tr("ABOUT_GAME"),new(300,358));
        var artwork=OriginalSceneArtwork.About();
        NativePlace(canvas,Picture("AboutArtwork",artwork,new(250,171)),new(25,8,250,171));
        if(artwork is null)
            NativeText(canvas,"AboutArtworkMissing",Tr("EVENT_ARTWORK_REIMPORT"),new(25,8,250,171),11,HorizontalAlignment.Center).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeText(canvas,"AboutPortName",ProjectSettings.GetSetting("application/config/name").AsString(),new(8,182,284,20),12,HorizontalAlignment.Center);
        NativeText(canvas,"AboutVersion",string.Format(Tr("ABOUT_PORT_VERSION"),BuildInfo.Version),new(8,204,284,18),11,HorizontalAlignment.Center);
        NativeText(canvas,"AboutOriginalCredit",Tr("ABOUT_ORIGINAL_CREDIT"),new(8,224,284,18),11,HorizontalAlignment.Center);
        var website=new LinkButton {Name="AboutWebsite",Text=KairosoftWebsite,TooltipText=KairosoftWebsite};
        website.AddThemeFontSizeOverride("font_size",11);
        website.AddThemeColorOverride("font_color",new Color(0,0,.8f));
        website.AddThemeColorOverride("font_hover_color",new Color(0,0,1));
        NativePlace(canvas,website,new(8,244,284,18));
        void VisitWebsite()
        {
            if(OS.ShellOpen(KairosoftWebsite)!=Error.Ok)
                website.TooltipText=Tr("ABOUT_WEBSITE_FAILED")+" "+KairosoftWebsite;
        }
        website.Pressed+=VisitWebsite;
        NativeAction(canvas,"VisitKairosoftWebsite","ABOUT_VISIT_WEBSITE",new(8,268,284,25),VisitWebsite);
        NativeText(canvas,"AboutPortCredit",Tr("ABOUT_PORT_CREDIT"),new(8,298,284,20),11,HorizontalAlignment.Center);
        NativeAction(canvas,"VisitPortRepository","ABOUT_REPOSITORY",new(8,324,284,25),()=>OS.ShellOpen("https://github.com/UnSpotibleShadow/OpenGDS2"));
        if(_dialog is not null)_dialog.CloseRequested=()=>ShowOfficeReconstruction(state);
    }
}
