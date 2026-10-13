using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowMediaInformation(GameState state,int source,double size,Action back)
    {
        ClearContent();CompactManagementWindow("MediaInformationWindow",new(430,400),410);AddHeading(Tr("MEDIA_INFORMATION"),22);
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog()!);var hardware=catalog.Hardware.Single(h=>h.SourceIndex==source);
        _content.AddChild(Picture("MediaHardwarePicture",HardwarePicture(state,source),new(64,64)));
        AddHeading(CatalogName(catalog,"HARDWARE",source,hardware.Name),16);AddHeading($"{Tr("PROJECT_SIZE")}: {Math.Floor(size):N0} KB",18);
        var table=CreateTable("MediaCapacityTable",["MEDIA_CAPACITY","SHIPMENT_MEDIA_COST"],[190,150]);_content.AddChild(table);var root=table.GetRoot();
        foreach(var band in hardware.ProductionBands)
        {var row=table.CreateItem(root);row.SetText(0,$"≤ {band.SizeThreshold:N0} KB");row.SetText(1,$"¥{band.UnitCost:N0}");}
        long cost=ReleaseService.UnitProductionCost(hardware,size);
        AddHeading($"{Tr("SHIPMENT_PACKAGE_COST")}: ¥200",16);AddHeading($"{Tr("SHIPMENT_UNIT_COST")}: ¥{cost:N0}",18);
        AddAction("CANCEL",back);
    }
    private void ReproductionBreakdown(GameState state,ReleasedGameState product,long unit,SpinBox copies)
    {
        _content.AddChild(Picture("ProductionHardwarePicture",HardwarePicture(state,product.HardwareSourceIndex),new(64,64)));
        var box=new PanelContainer {Name="ReproductionCostTable"};box.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=new(1,1,.65f),ContentMarginLeft=6,ContentMarginRight=6});_content.AddChild(box);
        var grid=new GridContainer {Columns=2};box.AddChild(grid);
        MoneyCell(grid,"ProductionMediaCost",Tr("SHIPMENT_MEDIA_COST")).Text=$"¥{unit-200:N0}";
        MoneyCell(grid,"ProductionPackageCost",Tr("SHIPMENT_PACKAGE_COST")).Text="¥200";
        MoneyCell(grid,"ProductionUnitCost",Tr("SHIPMENT_UNIT_COST")).Text=$"¥{unit:N0}";
        var total=MoneyCell(grid,"ProductionTotalCost",Tr("PRODUCTION_COST"));
        MoneyCell(grid,"ProductionFunds",Tr("FUNDS")).Text=$"¥{state.Money:N0}";
        void Refresh()=>total.Text=$"¥{checked(unit*(long)copies.Value):N0}";
        copies.ValueChanged+=_=>Refresh();Refresh();
        AddAction("MEDIA_INFORMATION",()=>
        {
            long selected=(long)copies.Value;
            ShowMediaInformation(state,product.HardwareSourceIndex,product.DataSize,()=>ShowProduction(state,product,selected));
        });
    }
}
