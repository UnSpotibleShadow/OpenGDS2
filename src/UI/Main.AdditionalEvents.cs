using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowReviewSummary(GameState state,ReleasedGameState product)
    {
        int page=product.ReviewSummaryPage!.Value;int sum=product.ReviewScores!.Sum();
        string text=page==1?Tr("REVIEW_SEQUEL_MESSAGE"):string.Format(DisplayNumbers.Provider,Tr(sum>=32?"REVIEW_HALL_MESSAGE":"REVIEW_SUMMARY_MESSAGE"),product.Title,sum);
        ShowConversation(Tr("MAGAZINE_REVIEWS"),[new(SecretaryCaption(state.SecretarySourceIndex),text)],()=>
        {
            if(!ReferenceEquals(_session.Current,state)||product.ReviewSummaryPage!=page)return;
            if(page==0){ReviewService.Acknowledge(state,product);product.ReviewSummaryPage=product.HallOfFame?1:null;}
            else product.ReviewSummaryPage=null;
            if(!ShowPendingProductEvent(state))ShowSales(state);
        });
    }
    private void ShowSalesAchievement(GameState state,ReleasedGameState product)
    {
        int stage=product.PendingSalesMilestones[0];
        if(product.SalesMilestonePage==1)
        {
            ShowConversation(Tr("SALES_MILESTONE"),[new(SecretaryCaption(state.SecretarySourceIndex),
                string.Format(DisplayNumbers.Provider,Tr("SALES_ACHIEVEMENT_MESSAGE"),product.Title,SalesMilestoneService.Threshold(stage)))],()=>
            {
                if(!ReferenceEquals(_session.Current,state))return;
                SalesMilestoneService.Acknowledge(state,product);PersistPendingRankings(state);
                if(!ShowPendingProductEvent(state))ShowSales(state);
            });return;
        }
        ClearContent();var canvas=NativeWindow("SalesAchievementWindow",Tr("SALES_MILESTONE"),new(274,225));
        NativeText(canvas,"SalesAchievementTitle",product.Title,new(4,2,266,20),10,HorizontalAlignment.Center);
        var art=OriginalAssetStore.LoadTexture($"Graphic/Embedded/SalesAchievement{stage}.bmp");NativePlace(canvas,Picture("SalesAchievementPicture",art,new(256,160)),new(9,27,256,160));
        if(art is null)NativeText(canvas,"SalesArtworkMissing",Tr("EVENT_ARTWORK_REIMPORT"),new(12,75,250,45),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeAction(canvas,"AcknowledgeSalesMilestone","CONTINUE",new(90,196,94,25),()=>{product.SalesMilestonePage=1;ShowSalesAchievement(state,product);});
    }

    private void ShowHardwareLaunchCard(GameState state,CalendarTransaction pending)
    {
        int source=state.Calendar!.Hardware[pending.Index].SourceIndex;
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog()!);var hardware=catalog.Hardware.Single(h=>h.SourceIndex==source);
        if(pending.HardwareLaunchPage==1)
        {
            ShowConversation(Tr("CALENDAR_EVENT_HardwareLaunch"),[new(SecretaryCaption(state.SecretarySourceIndex),Tr("HARDWARE_LAUNCH_LICENSE_MESSAGE"))],()=>
            {if(ReferenceEquals(state.Calendar?.Pending,pending)){CalendarService.Acknowledge(state);ShowOfficeReconstruction(state);}});return;
        }
        ShowNativeHardwareAnnouncement(state,pending,hardware,catalog);

    }
}
