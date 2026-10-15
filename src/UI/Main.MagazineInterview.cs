using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private bool ShowPendingMagazineInterview(GameState state)
    {
        if(state.PendingMagazineInterview is not { } scene)return false;
        var project=state.DevelopmentProjects.Single(p=>p.RoomIndex==scene.RoomIndex);
        var catalog=_importer.LoadCatalog();var type=catalog?.GameTypes.SingleOrDefault(t=>t.SourceIndex==project.GameTypeSourceIndex);
        string subject=type is null?"?":CatalogName(catalog,"TYPE",type.SourceIndex,type.Name);
        // Member135C is the same third source string historically exposed as an
        // example title. English uses an authored subject/role description.
        string magazine=TranslationServer.GetLocale().StartsWith("en")?string.Format(Tr("INTERVIEW_MAGAZINE"),subject):type?.ExampleTitles.ElementAtOrDefault(2)??subject;
        string Text(string key)=>TranslationServer.GetLocale().StartsWith("en")?Tr(key):_importer.LoadDialogue()?.Entries.GetValueOrDefault(key)??Tr(key);
        string reporter=scene.ReporterNameIndex is int name?Text("INTERVIEW_REPORTER_"+name):"";
        string key=scene.Page==0?"INTERVIEW_INTRO":scene.Page==5?"INTERVIEW_RESULT":"INTERVIEW_PAGE_"+(scene.Page-1);
        string text=Text(key).Replace("{room}",(scene.RoomIndex+1).ToString()).Replace("{magazine}",magazine)
            .Replace("{name}",reporter).Replace("{subject}",subject).Replace("{gain}",scene.Gain is double gain?DisplayNumbers.Format(gain):"?");
        bool secretary=scene.Page is 0 or 5;int cursor=scene.Page;
        ShowConversation(Tr("MAGAZINE_INTERVIEW"),[new(secretary?SecretaryCaption(state.SecretarySourceIndex):Text("INTERVIEW_SPEAKER"),text,
            secretary?SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex):scene.PortraitId)],()=>
        {
            if(!ReferenceEquals(state.PendingMagazineInterview,scene)||scene.Page!=cursor)return;
            MagazineInterviewService.Acknowledge(state,_session.Random);
            if(!ShowPendingCompanyEvent(state))ShowOfficeReconstruction(state);
        },scene:secretary?null:OriginalSceneArtwork.BusinessMeeting());
        return true;
    }
}
