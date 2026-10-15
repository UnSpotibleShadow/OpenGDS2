using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowMediaInformation(GameState state,int source,double size,Action back)
    {
        ClearContent();var canvas=NativeWindow("MediaInformationWindow",Tr("MEDIA_INFORMATION"),new(286,250));
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog()!);var hardware=catalog.Hardware.Single(h=>h.SourceIndex==source);
        NativePlace(canvas,Picture("MediaHardwarePicture",HardwarePicture(state,source),new(64,64)),new(4,6,64,64));
        NativeText(canvas,"MediaHardwareName",CatalogName(catalog,"HARDWARE",source,hardware.Name),new(78,7,204,23),12);
        NativeText(canvas,"MediaCurrentSize",$"{Tr("PROJECT_SIZE")}: {Math.Floor(size):N0} KB",new(78,36,204,23));
        var table=CreateTable("MediaCapacityTable",["MEDIA_CAPACITY","SHIPMENT_MEDIA_COST"],[116,128]);
        NativePlace(canvas,table,new(4,80,278,116));table.SetColumnExpand(0,true);table.SetColumnExpand(1,false);table.ScrollHorizontalEnabled=false;table.AddThemeFontSizeOverride("title_button_font_size",10);var root=table.GetRoot();
        foreach(var band in hardware.ProductionBands)
        {var row=table.CreateItem(root);row.SetText(0,$"≤ {band.SizeThreshold:N0} KB");row.SetText(1,$"¥{band.UnitCost:N0}");row.SetTextAlignment(1,HorizontalAlignment.Right);}
        long cost=ReleaseService.UnitProductionCost(hardware,size);
        NativeText(canvas,"MediaPackageCost",$"{Tr("SHIPMENT_PACKAGE_COST")}: ¥200",new(4,201,278,18));
        NativeText(canvas,"MediaUnitCost",$"{Tr("SHIPMENT_UNIT_COST")}: ¥{cost:N0}",new(4,220,182,18));
        NativeAction(canvas,"CloseMediaInformation","CANCEL",new(188,221,94,25),back);
    }
}
