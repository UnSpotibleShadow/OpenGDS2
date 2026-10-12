using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowCompactProduct(GameState state,ReleasedGameState product)
    {
        if(ShowPendingProductEvent(state))return;
        ClearContent();CompactManagementWindow("CompactProductWindow",new(350,410),330);_dialog?.SetTitle(product.Title);
        AddHeading(product.Title,18);
        var date=product.ReleasedOn;
        _content.AddChild(new Label {Name="ProductReleaseDate",Text=Tr("PRODUCT_RELEASE_DATE")+": "+
            (date is { } d ? $"{d.Year}/{d.Month}/{d.Week}" : "?")});
        SalesHistoryChart? chart=null;
        if(product.WeeklySalesHistory is not null)
        {chart=new SalesHistoryChart {Name="CompactWeeklySalesChart",Compact=true,CustomMinimumSize=new(320,150),ClipContents=true};_content.AddChild(chart);}
        else AddHeading(Tr("SALES_HISTORY_UNKNOWN"),14);
        var sales=new Label {Name="CompactProductSales"};_content.AddChild(sales);
        var row=new HBoxContainer();_content.AddChild(row);
        row.AddChild(Picture("ProductHardwarePicture",HardwarePicture(state,product.HardwareSourceIndex),new(64,64)));
        var results=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};row.AddChild(results);
        var contest=new Label {Name="ProductContestResult",AutowrapMode=TextServer.AutowrapMode.WordSmart};results.AddChild(contest);
        var hall=new Label {Name="ProductHallOfFame",AutowrapMode=TextServer.AutowrapMode.WordSmart};results.AddChild(hall);
        var ship=new Button {Name="CompactProduceMore",Text=Tr("PRODUCE_MORE")};_content.AddChild(ship);
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
            var result=ReleaseService.CheckProduction(state,product);
            ship.Disabled=result!=ReleaseResult.Success;
            availability.Text=Tr(result==ReleaseResult.Success ? "PRODUCTION_READY" : $"RELEASE_RESULT_{result}");
        };
        _refreshCoreStatus();
    }
}
