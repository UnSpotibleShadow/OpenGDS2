using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private bool ShowPendingCompanyActivity(GameState state)
    {
        if(state.PendingCompanyActivity is not { } scene)return false;
        string key=scene.Activity switch {CompanyActivity.PublicRelations=>"PUBLIC_RELATIONS",CompanyActivity.GenreResearch=>"GENRE_RESEARCH",_=>"EXPAND_PRODUCTION"};
        if(scene.Confirmed)
        {
            string text=scene.Page==0?string.Format(Tr(scene.Activity==CompanyActivity.ProductionExpansion && scene.After-scene.Before>=17 ?
                "ACTIVITY_SUPER_RESULT":"ACTIVITY_RESULT"),Tr(key),scene.Before,scene.After,scene.Cost):
                Tr("ACTIVITY_UNLOCKED_GENRES")+"\n"+string.Join("\n",scene.UnlockedGenres.Select(id=>
                {var c=_importer.LoadCatalog()!;return CatalogName(c,"GENRE",id,c.Genres.Single(g=>g.SourceIndex==id).Name);}));
            ShowConversation(Tr(key),[new(SecretaryCaption(state.SecretarySourceIndex),text)],()=>
            {
                if(!ReferenceEquals(_session.Current,state))return;
                if(scene.Page==0 && scene.UnlockedGenres.Count>0)scene.Page=1;else state.PendingCompanyActivity=null;
                if(!ShowPendingCompanyActivity(state))ShowCompanyActivities(state);
            });return true;
        }
        ClearContent();CompactManagementWindow("ActivityConfirmationWindow",new(440,320),420);AddHeading(Tr(key),22);
        AnnualMessage("ActivityExplanation",Tr("ACTIVITY_DESCRIPTION_"+(int)scene.Activity));
        AddHeading($"{Tr("COMPANY_ACTIVITY_COST")}: ¥{state.Company.ActivityCost:N0}",18);
        AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",18);
        var status=new Label {Name="ActivityConfirmationStatus"};_content.AddChild(status);
        var confirm=new Button {Name="ConfirmCompanyActivity",Text=Tr("CONFIRM")};_content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            var result=CompanyActivityPresentation.Confirm(state,_importer.LoadCatalog(),_session.Random);
            if(result==CompanyActivityResult.Success)ShowPendingCompanyActivity(state);else status.Text=Tr($"COMPANY_ACTIVITY_RESULT_{result}");
        };
        AddAction("CANCEL",()=>{state.PendingCompanyActivity=null;ShowCompanyActivities(state);});return true;
    }
}
