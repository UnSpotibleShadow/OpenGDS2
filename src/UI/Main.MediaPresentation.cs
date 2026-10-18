using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowMediaInformation(GameState state,int source,double size,Action back,bool keepMeetingMusic=false)
    {
        ClearContent(keepMeetingMusic:keepMeetingMusic);var canvas=NativeWindow("MediaInformationWindow",Tr("MEDIA_INFORMATION"),new(286,142));
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog()!);var hardware=catalog.Hardware.Single(h=>h.SourceIndex==source);
        NativePlace(canvas,Picture("MediaHardwarePicture",HardwarePicture(state,source),new(64,64)),new(4,6,64,64));
        var table=CreateTable("MediaCapacityTable",["MEDIA_CAPACITY","SHIPMENT_MEDIA_COST"],[98,78]);
        NativePlace(canvas,table,new(78,4,204,88));table.ColumnTitlesVisible=false;table.SetColumnExpand(0,true);table.SetColumnExpand(1,false);table.ScrollHorizontalEnabled=false;
        var root=table.GetRoot();bool selected=false;
        foreach(var band in hardware.ProductionBands)
        {
            var row=table.CreateItem(root);row.SetText(0,$"≤ {DisplayNumbers.Format(band.SizeThreshold)} KB");row.SetText(1,$"¥{DisplayNumbers.Format(band.UnitCost)}");row.SetTextAlignment(1,HorizontalAlignment.Right);
            if(!selected&&size<=band.SizeThreshold){row.Select(0);selected=true;}
        }
        NativeText(canvas,"MediaCurrentSize",$"{Tr("PROJECT_SIZE")}: {DisplayNumbers.Format(Math.Floor(size))} KB",new(78,94,204,18),10);
        NativeAction(canvas,"CloseMediaInformation","CLOSE",new(96,113,94,25),back);
    }
}
