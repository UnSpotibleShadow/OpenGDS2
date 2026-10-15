using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowCompactProduct(GameState state,ReleasedGameState product)
    {
        if(ShowPendingProductEvent(state))return;
        ClearContent();var canvas=NativeWindow("CompactProductWindow",product.Title,new(170,288));
        /* Native report has no product selector. Reports open from Products. */
        var date=product.ReleasedOn;NativeFill(canvas,"ProductDateStrip",new(4,3,162,20),new(.77f,.77f,1));
        NativeText(canvas,"ProductReleaseDate",Tr("PRODUCT_RELEASE_DATE")+": "+(date is { } d?$"{d.Year}/{d.Month}/{d.Week}":"?"),new(6,3,158,20));
        SalesHistoryChart? chart=null;
        if(product.WeeklySalesHistory is not null)
        {chart=new SalesHistoryChart {Name="CompactWeeklySalesChart",Compact=true,ClipContents=true};NativePlace(canvas,chart,new(4,29,162,111));}
        else NativeText(canvas,"UnknownCompactHistory",Tr("SALES_HISTORY_UNKNOWN"),new(4,29,162,111));
        var sales=NativeText(canvas,"CompactProductSales","",new(4,143,162,36));sales.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var ship=NativeAction(canvas,"CompactProduceMore","PRODUCT_SHIP",new(90,180,76,24),()=>ShowProduction(state,product));
        var availability=NativeText(canvas,"CompactProductionAvailability","",new(4,180,80,24),9);availability.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeFill(canvas,"ContestResultStrip",new(54,210,112,34),new(.65f,1,1));
        NativeFill(canvas,"CrossReviewStrip",new(54,250,112,34),new(.65f,1,1));
        var trophy=NativePlace(canvas,Picture("ProductContestBadge",null,new(40,40)),new(10,206,40,40));
        var hallBadge=NativePlace(canvas,Picture("ProductHallOfFameBadge",null,new(40,40)),new(10,247,40,40));
        var contest=NativeText(canvas,"ProductContestResult","",new(56,210,108,34),10,HorizontalAlignment.Center);contest.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var hall=NativeText(canvas,"ProductHallOfFame","",new(56,250,108,34),10,HorizontalAlignment.Center);hall.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            if(chart is not null)chart.SetHistory(product.WeeklySalesHistory!);
            sales.Text=$"{Tr("UNITS_SOLD")}: {DisplayNumbers.Format(product.UnitsSold)}\n{Tr("STOCK")}: {DisplayNumbers.Format(product.Stock)}";
            string award=product.OriginalAwardCode is null ? "?" : "—";
            foreach(var category in AwardService.Order)
                if(product.OriginalAwardCode==AwardService.OriginalCode(category))award=Tr($"AWARD_{category}");
            contest.Text=$"{Tr("PRODUCT_CONTEST_RESULT")}\n{award}";
            hall.Text=$"{Tr("MAGAZINE_REVIEWS")}\n{(product.HallOfFame ? Tr("NATIVE_HALL_TITLE") : "—")}";
            trophy.Texture=product.OriginalAwardCode is >=1 and <=5 ?
                Core.OriginalAssetStore.LoadTexture($"Graphic/Embedded/Trophy{product.OriginalAwardCode-1}.bmp") : Core.OriginalAssetStore.LoadTexture("Graphic/Embedded/EmptyBadge.png");
            hallBadge.Texture=Core.OriginalAssetStore.LoadTexture(product.HallOfFame ? "Graphic/Embedded/Trophy2.bmp" : "Graphic/Embedded/EmptyBadge.png");
            var result=ReleaseService.CheckProduction(state,product);
            ship.Disabled=result!=ReleaseResult.Success;
            availability.Text=Tr(result==ReleaseResult.Success ? "NATIVE_SHIP_READY" : "NATIVE_SHIP_UNAVAILABLE");
            availability.TooltipText=Tr(result==ReleaseResult.Success ? "PRODUCTION_READY" : $"RELEASE_RESULT_{result}");
        };
        _refreshCoreStatus();
    }
}
