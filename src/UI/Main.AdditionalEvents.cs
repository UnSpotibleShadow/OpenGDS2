using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowReviewSummary(GameState state,ReleasedGameState product)
    {
        int page=product.ReviewSummaryPage!.Value;int sum=product.ReviewScores!.Sum();
        string text=page==1?Tr("REVIEW_SEQUEL_MESSAGE"):string.Format(Tr(sum>=32?"REVIEW_HALL_MESSAGE":"REVIEW_SUMMARY_MESSAGE"),product.Title,sum);
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
                string.Format(Tr("SALES_ACHIEVEMENT_MESSAGE"),product.Title,SalesMilestoneService.Threshold(stage)))],()=>
            {
                if(!ReferenceEquals(_session.Current,state))return;
                SalesMilestoneService.Acknowledge(state,product);PersistPendingRankings(state);
                if(!ShowPendingProductEvent(state))ShowSales(state);
            });return;
        }
        ClearContent();CompactManagementWindow("SalesAchievementWindow",new(410,330),390);
        AddHeading(Tr("SALES_MILESTONE"),22);AddHeading(product.Title,16);
        var art=OriginalAssetStore.LoadTexture($"Graphic/Embedded/SalesAchievement{stage}.bmp");
        _content.AddChild(Picture("SalesAchievementPicture",art,new(256,160)));
        if(art is null)AddHeading(Tr("EVENT_ARTWORK_REIMPORT"),14);
        AddHeading($"{Tr("UNITS_SOLD")}: {SalesMilestoneService.Threshold(stage):N0}",18);
        var close=new Button {Name="AcknowledgeSalesMilestone",Text=Tr("CONTINUE")};_content.AddChild(close);
        close.Pressed+=()=>{product.SalesMilestonePage=1;ShowSalesAchievement(state,product);};
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
        ClearContent();CompactManagementWindow("HardwareAnnouncementWindow",new(400,330),380);
        AddHeading(Tr("CALENDAR_EVENT_HardwareLaunch"),22);
        _content.AddChild(Picture("HardwareAnnouncementPicture",HardwarePicture(state,source),new(64,64)));
        AddHeading(CatalogName(catalog,"HARDWARE",source,hardware.Name),20);AddHeading((hardware.RawMembers.TryGetValue("0F00",out long bits)?bits.ToString():"?")+" bit",16);
        AddHeading($"{Tr("HARDWARE_LICENSES")}: "+(hardware.RawMembers.TryGetValue("0FC4",out long fee)?$"¥{fee:N0}":Tr("UNKNOWN")),16);
        AnnualMessage("HardwareAnnouncementDescription",hardware.Description);
        var confirm=new Button {Name="AcknowledgeCalendar",Text=Tr("CONTINUE")};_content.AddChild(confirm);
        confirm.Pressed+=()=>{pending.HardwareLaunchPage=1;ShowHardwareLaunchCard(state,pending);};
    }
}
