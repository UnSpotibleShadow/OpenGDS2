using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowSalesHistory(GameState state,ReleasedGameState product)
    {
        ClearContent();AddHeading(Tr("SALES_HISTORY"),28);AddHeading(product.Title,22);
        if (product.WeeklySalesHistory is null)
        {
            AddHeading(Tr("SALES_HISTORY_UNKNOWN"),18);AddAction("CANCEL",()=>ShowSales(state));return;
        }
        var chart=new SalesHistoryChart {Name="WeeklySalesChart",CustomMinimumSize=new(600,190),ClipContents=true};_content.AddChild(chart);
        var current=new Label {Name="CurrentWeeklySales"};_content.AddChild(current);
        var prior=new Label {Name="PreviousWeeklySales"};_content.AddChild(prior);
        var total=new Label {Name="HistoryTotalSales"};_content.AddChild(total);
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("BACK_TO_SALES",()=>ShowSales(state));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            chart.SetHistory(product.WeeklySalesHistory);
            current.Text=$"{Tr("SALES_HISTORY_CURRENT")}: {product.WeeklySalesHistory[0]:N0}";
            prior.Text=$"{Tr("SALES_HISTORY_PREVIOUS")}: {product.WeeklySalesHistory[1]:N0}";
            total.Text=$"{Tr("UNITS_SOLD")}: {product.UnitsSold:N0}";
        };
        _refreshCoreStatus();
    }
}
