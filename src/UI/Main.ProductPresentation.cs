using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowCompactProduct(GameState state,ReleasedGameState product)
    {
        if(ShowPendingProductEvent(state))return;
        ClearContent();CompactManagementWindow("CompactProductWindow",new(350,450),330);_dialog?.SetTitle(product.Title);
        AddHeading(product.Title,18);
        var date=product.ReleasedOn;
        _content.AddChild(new Label {Name="ProductReleaseDate",Text=Tr("PRODUCT_RELEASE_DATE")+": "+
            (date is { } d ? $"{d.Year}/{d.Month}/{d.Week}" : "?")});
        SalesHistoryChart? chart=null;
        if(product.WeeklySalesHistory is not null)
        {chart=new SalesHistoryChart {Name="CompactWeeklySalesChart",Compact=true,CustomMinimumSize=new(320,150),ClipContents=true};_content.AddChild(chart);}
        else AddHeading(Tr("SALES_HISTORY_UNKNOWN"),14);
        var sales=new Label {Name="CompactProductSales"};_content.AddChild(sales);
        var contestRow=new HBoxContainer();_content.AddChild(contestRow);
        var trophy=Picture("ProductContestBadge",null,new(40,40));contestRow.AddChild(trophy);
        var contest=new Label {Name="ProductContestResult",AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill};contestRow.AddChild(contest);
        var hallRow=new HBoxContainer();_content.AddChild(hallRow);
        var hallBadge=Picture("ProductHallOfFameBadge",null,new(40,40));hallRow.AddChild(hallBadge);
        var hall=new Label {Name="ProductHallOfFame",AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill};hallRow.AddChild(hall);
        var ship=new Button {Name="CompactProduceMore",Text=Tr("PRODUCT_SHIP"),TooltipText=Tr("PRODUCE_MORE")};_content.AddChild(ship);
        ship.Pressed+=()=>ShowProduction(state,product);
        var availability=new Label {Name="CompactProductionAvailability",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(availability);
        AddAction("BACK_TO_SALES",()=>ShowSales(state));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            if(chart is not null)chart.SetHistory(product.WeeklySalesHistory!);
            sales.Text=$"{Tr("UNITS_SOLD")}: {product.UnitsSold:N0}\n{Tr("STOCK")}: {product.Stock:N0}";
            string award=product.OriginalAwardCode is null ? "?" : "—";
            foreach(var category in AwardService.Order)
                if(product.OriginalAwardCode==AwardService.OriginalCode(category))award=Tr($"AWARD_{category}");
            contest.Text=$"{Tr("PRODUCT_CONTEST_RESULT")}\n{award}";
            hall.Text=$"{Tr("MAGAZINE_REVIEWS")}\n{(product.HallOfFame ? Tr("REVIEW_HALL_OF_FAME") : "—")}";
            trophy.Texture=product.OriginalAwardCode is >=1 and <=5 ?
                Core.OriginalAssetStore.LoadTexture($"Graphic/Embedded/Trophy{product.OriginalAwardCode-1}.bmp") : Core.OriginalAssetStore.LoadTexture("Graphic/Embedded/EmptyBadge.png");
            hallBadge.Texture=Core.OriginalAssetStore.LoadTexture(product.HallOfFame ? "Graphic/Embedded/Trophy2.bmp" : "Graphic/Embedded/EmptyBadge.png");
            var result=ReleaseService.CheckProduction(state,product);
            ship.Disabled=result!=ReleaseResult.Success;
            availability.Text=Tr(result==ReleaseResult.Success ? "PRODUCTION_READY" : $"RELEASE_RESULT_{result}");
        };
        _refreshCoreStatus();
    }
}
