using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private Texture2D? HardwarePicture(GameState state,int source)
    {
        int picture=source==11 && state.OwnHardware.LaunchedDesign is { } design && state.OwnHardware.LaunchedMedium is { } medium
            ? OwnHardwareService.OwnPictureIndex(design,medium) : source;
        return OriginalAssetStore.LoadTexture($"Graphic/Embedded/Hardware{picture}.bmp");
    }
    private static TextureRect Picture(string name,Texture2D? texture,Vector2 size)=>new()
    {
        Name=name,Texture=texture,TextureFilter=TextureFilterEnum.Nearest,
        ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
        CustomMinimumSize=size,MouseFilter=MouseFilterEnum.Ignore
    };
    private Action ProjectPanel(Control parent,GameState state,DevelopmentProjectState project,OriginalCatalog catalog)
    {
        var panel=new Control {Name="ProjectSummaryPanel",CustomMinimumSize=new(288,204),Size=new(288,204)};parent.AddChild(panel);
        NativePlace(panel,Picture("ProjectHardwarePicture",HardwarePicture(state,project.HardwareSourceIndex),new(64,64)),new(4,6,64,64));
        NativeText(panel,"ProjectHardwareName",CatalogName(catalog,"HARDWARE",project.HardwareSourceIndex,catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex).Name),new(2,70,72,20),10,HorizontalAlignment.Center);
        var summary=NativeText(panel,"ProjectCatalogSummary","",new(78,4,208,66),10);summary.AddThemeConstantOverride("line_spacing",0);summary.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeFill(panel,"ProjectAudienceFields",new(78,4,208,50),new(1,.75f,1));summary.MoveToFront();
        string[] keys=["STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"];
        Color[] colors=[new(.1f,.1f,.9f),new(.8f,.7f,0),new(.7f,.05f,.85f),new(0,.7f,.7f)];
        var values=new Label[4];var weights=new Label[4];
        NativeFill(panel,"ProjectQualityFields",new(78,74,208,77),new(.77f,.77f,1));
        for(int i=0;i<4;i++)
        {
            int x=78+(i%2)*106,y=74+(i/2)*38;
            var dot=NativeText(panel,$"ProjectQualityDot{i}","●",new(x,y,12,16));dot.AddThemeColorOverride("font_color",colors[i]);
            NativeText(panel,$"ProjectQualityCaption{i}",Tr(keys[i]),new(x+13,y,90,16));
            values[i]=NativeText(panel,$"ProjectQuality{i}","",new(x+3,y+16,54,21),18);
            weights[i]=NativeText(panel,$"ProjectFocus{i}","",new(x+61,y+20,39,16),10,HorizontalAlignment.Right);weights[i].AddThemeColorOverride("font_color",Colors.Red);
        }
        NativeFill(panel,"ProjectBugBox",new(78,152,76,20),Colors.Red);
        NativeText(panel,"ProjectBugsCaption",Tr("BUGS"),new(78,152,40,19));
        var bugs=NativeText(panel,"ProjectBugCount","",new(118,152,36,20),14,HorizontalAlignment.Center);
        bugs.AddThemeColorOverride("font_color",Colors.White);
        var recognition=NativeText(panel,"ProjectRecognition","",new(165,152,121,20),10);
        var progress=NativeText(panel,"ProjectPointSummary","",new(78,173,208,16),10);
        var bar=new ProgressBar {Name="ProjectSummaryProgress",MinValue=0,MaxValue=100,ShowPercentage=false};
        NativePlace(panel,bar,new(78,190,166,8));
        var percent=NativeText(panel,"ProjectProgressPercent","",new(246,184,42,18),10,HorizontalAlignment.Right);
        void Refresh()
        {
            string audience=CatalogName(catalog,"AUDIENCE",project.AudienceSourceIndex,catalog.Audiences.Single(x=>x.SourceIndex==project.AudienceSourceIndex).Name);
            string genre=project.GenreSourceIndex is int g ? CatalogName(catalog,"GENRE",g,catalog.Genres.Single(x=>x.SourceIndex==g).Name) : "—";
            string type=project.GameTypeSourceIndex is int t ? CatalogName(catalog,"TYPE",t,catalog.GameTypes.Single(x=>x.SourceIndex==t).Name) : "—";
            summary.Text=$"{Tr("PROJECT_AUDIENCE")}: {audience}\n{Tr("PROJECT_GENRE")}: {genre}\n{Tr("PROJECT_CONTENT")}: {type}\n{Tr("PROJECT_SIZE")}: {DisplayNumbers.Format(Math.Floor(project.DataSize))} KB";summary.TooltipText=summary.Text;
            double[] q=[project.Quality.Fun,project.Quality.Novelty,project.Quality.Graphics,project.Quality.Music];
            int?[] f=[project.Focus?.Fun,project.Focus?.Novelty,project.Focus?.Graphics,project.Focus?.Music];
            for(int i=0;i<4;i++){values[i].Text=$"{DisplayNumbers.Format(q[i])}";weights[i].Text=$"{f[i]?.ToString() ?? "?"}%";}
            bugs.Text=project.Bugs.ToString();recognition.Text=$"{Tr("NATIVE_RECOGNITION")}: {DisplayNumbers.Format(project.Recognition)}%";
            progress.Text=$"{Tr("NATIVE_PROGRESS")}: {DisplayNumbers.Format(Math.Floor(project.Progress))}/{DisplayNumbers.Format(project.RequiredPoints)}";
            int value=OfficeRoomPresentationRules.Percentage(project);bar.Value=value;percent.Text=value+"%";
        }
        Refresh();return Refresh;
    }
    private void CheckpointPanel(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,string name,string heading)
    {
        var canvas=NativeWindow(name,heading,new(640,212));ProjectPanel(canvas,state,project,catalog);
        NativeAction(canvas,"CheckpointCancelProject","CANCEL",new(4,122,70,28),()=>ShowProjectCancellation(state,project));
        var choices=new VBoxContainer {Name="CheckpointChoicePane",CustomMinimumSize=new(344,204)};
        choices.AddThemeConstantOverride("separation",2);NativePlace(canvas,choices,new(294,4,344,204));_content=choices;
        _status=new Label {Name="CheckpointStatus",ClipText=true};_status.AddThemeFontSizeOverride("font_size",10);
        choices.AddChild(_status);
    }
    private static Label MoneyCell(GridContainer grid,string name,string caption)
    {
        grid.AddChild(new Label {Text=caption,SizeFlagsHorizontal=SizeFlags.ExpandFill});
        var label=new Label {Name=name,HorizontalAlignment=HorizontalAlignment.Right};grid.AddChild(label);return label;
    }
    private void NativeShipmentBreakdown(Control canvas,GameState state,long unit,SpinBox price,SpinBox copies)
    {
        NativeFill(canvas,"ShipmentTableFrame",new(4,28,416,185),Colors.White,true);
        string[] keys=["SHIPMENT_MEDIA_COST","SHIPMENT_PACKAGE_COST","SHIPMENT_UNIT_COST","PRODUCTION_COST","RETAIL_PRICE","SHIPMENT_RETAILER_SHARE","SHIPMENT_TRANSPORT_SHARE","NET_PER_COPY","SHIPMENT_ALL_SOLD"];
        string[] names=["ShipmentMediaCost","ShipmentPackageCost","ShipmentUnitCost","ShipmentTotalCost","ShipmentRetail","ShipmentRetailer","ShipmentTransport","ShipmentNetCopy","ShipmentAllSold"];
        var values=new Label[9];
        for(int i=0;i<9;i++)
        {
            int y=34+i*18+(i>=4?8:0);Color color=i<4?new(1,1,.72f):new(.65f,1,1);
            NativeFill(canvas,"ShipmentRow"+i,new(12,y,400,18),color,true);
            NativeFill(canvas,"ShipmentDivider"+i,new(254,y,1,18),Colors.Black);
            NativeFill(canvas,"ShipmentAmountDivider"+i,new(300,y,1,18),Colors.Black);
            NativeText(canvas,"ShipmentCaption"+i,Tr(keys[i]),new(15,y,237,18),10);
            values[i]=NativeText(canvas,names[i],"",new(i is 5 or 6?257:303,y,i is 5 or 6?42:106,18),10,HorizontalAlignment.Right);
        }
        void Refresh()
        {
            long count=(long)copies.Value,p=(long)price.Value,per=p-state.SalesFee1!.Value-state.SalesFee2!.Value;
            System.Numerics.BigInteger[] amounts=[unit-200,200,unit,(System.Numerics.BigInteger)unit*count,p,-state.SalesFee1.Value,-state.SalesFee2.Value,per,(System.Numerics.BigInteger)per*count];
            for(int i=0;i<9;i++){values[i].Text="¥"+DisplayNumbers.Format(amounts[i]);values[i].TooltipText=values[i].Text;}
        }
        price.ValueChanged+=_=>Refresh();copies.ValueChanged+=_=>Refresh();Refresh();
    }
}
