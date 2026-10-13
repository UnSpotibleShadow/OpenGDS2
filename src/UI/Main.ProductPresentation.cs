using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowCompactProduct(GameState state,ReleasedGameState product)
    {
        if(ShowPendingProductEvent(state))return;
        ClearContent();var canvas=NativeWindow("CompactProductWindow",product.Title,new(230,314));
        var products=state.ReleasedGames.OrderByDescending(p=>p.ProductIndex).ToList();
        var chooser=new OptionButton {Name="ProductSelector",ClipText=true};chooser.AddThemeFontSizeOverride("font_size",11);
        NativePlace(canvas,chooser,new(4,2,198,23));foreach(var p in products)chooser.AddItem(p.Title,p.ProductIndex);
        chooser.Select(products.IndexOf(product));chooser.ItemSelected+=i=>ShowCompactProduct(state,products[(int)i]);
        var more=new MenuButton {Name="ProductReportActions",Text="…",ClipText=true};more.AddThemeStyleboxOverride("normal",ClassicTheme.CompactButton());more.AddThemeFontSizeOverride("font_size",11);NativePlace(canvas,more,new(206,2,20,23));
        more.GetPopup().AddItem(Tr("SALES_HISTORY"),0);more.GetPopup().AddItem(Tr("MAGAZINE_REVIEWS"),1);more.GetPopup().SetItemDisabled(1,product.ReviewScores is null);more.GetPopup().AddItem(Tr("BACK_TO_OFFICE"),2);
        more.GetPopup().IdPressed+=id=>{if(id==0)ShowSalesHistory(state,product);else if(id==1)ShowReviews(state,product);else ShowOfficeReconstruction(state);};
        var date=product.ReleasedOn;NativeFill(canvas,"ProductDateStrip",new(4,29,222,20),new(.77f,.77f,1));
        NativeText(canvas,"ProductReleaseDate",Tr("PRODUCT_RELEASE_DATE")+": "+(date is { } d?$"{d.Year}/{d.Month}/{d.Week}":"?"),new(6,29,218,20));
        SalesHistoryChart? chart=null;
        if(product.WeeklySalesHistory is not null)
        {chart=new SalesHistoryChart {Name="CompactWeeklySalesChart",Compact=true,ClipContents=true};NativePlace(canvas,chart,new(4,55,222,111));}
        else NativeText(canvas,"UnknownCompactHistory",Tr("SALES_HISTORY_UNKNOWN"),new(4,55,222,111));
        var sales=NativeText(canvas,"CompactProductSales","",new(4,169,222,36));sales.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var ship=NativeAction(canvas,"CompactProduceMore","PRODUCT_SHIP",new(124,206,102,24),()=>ShowProduction(state,product));
        var availability=NativeText(canvas,"CompactProductionAvailability","",new(4,206,114,24),9);availability.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeFill(canvas,"ContestResultStrip",new(50,236,176,34),new(.65f,1,1));
        NativeFill(canvas,"CrossReviewStrip",new(50,276,176,34),new(.65f,1,1));
        var trophy=NativePlace(canvas,Picture("ProductContestBadge",null,new(40,40)),new(4,232,40,40));
        var hallBadge=NativePlace(canvas,Picture("ProductHallOfFameBadge",null,new(40,40)),new(4,273,40,40));
        var contest=NativeText(canvas,"ProductContestResult","",new(52,236,172,34),11,HorizontalAlignment.Center);contest.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var hall=NativeText(canvas,"ProductHallOfFame","",new(52,276,172,34),11,HorizontalAlignment.Center);hall.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            if(chart is not null)chart.SetHistory(product.WeeklySalesHistory!);
            sales.Text=$"{Tr("UNITS_SOLD")}: {product.UnitsSold:N0}\n{Tr("STOCK")}: {product.Stock:N0}";
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
