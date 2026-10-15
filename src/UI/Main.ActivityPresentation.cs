using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public partial class Main
{
    private void BeginCompanyActivity(GameState state,CompanyActivity activity)
    {
        if(!ReferenceEquals(_session.Current,state) || state.CompanyActivityPerformed!=false || state.Company.ActivityCost is null || state.PendingCompanyActivity is not null)return;
        state.PendingCompanyActivity=new(){Activity=activity};ShowPendingCompanyActivity(state);
    }
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
                if(!ShowPendingCompanyActivity(state))ShowOfficeReconstruction(state);
            });return true;
        }
        var canvas=ManagementCanvas("ActivityConfirmationWindow",Tr(key),new(290,285));
        var artwork=OriginalAssetStore.LoadTexture("Graphic/Embedded/CompanyActivity.bmp");
        NativePlace(canvas,Picture("CompanyActivityArtwork",artwork,new(256,160)),new(17,8,256,160));
        if(artwork is null)NativeText(canvas,"ActivityArtworkMissing",Tr("EVENT_ARTWORK_REIMPORT"),new(17,60,256,50),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeFill(canvas,"ActivityMessageBox",new(8,176,274,53),Colors.White,true);
        NativeText(canvas,"ActivityExplanation",Tr("ACTIVITY_DESCRIPTION_"+(int)scene.Activity),new(12,179,266,47),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeText(canvas,"ActivityCost",$"{Tr("COMPANY_ACTIVITY_COST")}: ¥{state.Company.ActivityCost:N0}",new(8,231,274,18),10,HorizontalAlignment.Right);
        var status=NativeText(canvas,"ActivityConfirmationStatus","",new(8,251,86,29),10);status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var confirm=NativeAction(canvas,"ConfirmCompanyActivity","CONFIRM",new(98,253,84,25),()=>{});
        confirm.Pressed+=()=>
        {
            var result=CompanyActivityPresentation.Confirm(state,_importer.LoadCatalog(),_session.Random);
            if(result==CompanyActivityResult.Success)ShowPendingCompanyActivity(state);else status.Text=Tr($"COMPANY_ACTIVITY_RESULT_{result}");
        };
        NativeAction(canvas,"CancelCompanyActivity","CANCEL",new(190,253,92,25),()=>{state.PendingCompanyActivity=null;ShowOfficeReconstruction(state);});return true;
    }
}
