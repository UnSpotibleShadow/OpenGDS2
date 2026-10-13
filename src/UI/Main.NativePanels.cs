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
        NativeText(panel,"ProjectBugsCaption",Tr("BUGS"),new(78,152,40,19));
        NativeFill(panel,"ProjectBugBox",new(118,152,36,20),Colors.Red);
        var bugs=NativeText(panel,"ProjectBugCount","",new(118,152,36,20),14,HorizontalAlignment.Center);
        var recognition=NativeText(panel,"ProjectRecognition","",new(165,152,121,20),10);
        var progress=NativeText(panel,"ProjectPointSummary","",new(78,173,208,16),10);
        var bar=new ProgressBar {Name="ProjectSummaryProgress",MinValue=0,MaxValue=100,ShowPercentage=false};
        bar.AddThemeStyleboxOverride("background",new StyleBoxFlat {BgColor=Colors.White});bar.AddThemeStyleboxOverride("fill",new StyleBoxFlat {BgColor=Colors.Green});
        NativePlace(panel,bar,new(78,190,166,8));
        var percent=NativeText(panel,"ProjectProgressPercent","",new(246,184,42,18),10,HorizontalAlignment.Right);
        void Refresh()
        {
            string audience=CatalogName(catalog,"AUDIENCE",project.AudienceSourceIndex,catalog.Audiences.Single(x=>x.SourceIndex==project.AudienceSourceIndex).Name);
            string genre=project.GenreSourceIndex is int g ? CatalogName(catalog,"GENRE",g,catalog.Genres.Single(x=>x.SourceIndex==g).Name) : "—";
            string type=project.GameTypeSourceIndex is int t ? CatalogName(catalog,"TYPE",t,catalog.GameTypes.Single(x=>x.SourceIndex==t).Name) : "—";
            summary.Text=$"{Tr("PROJECT_AUDIENCE")}: {audience}\n{Tr("PROJECT_GENRE")}: {genre}\n{Tr("PROJECT_CONTENT")}: {type}\n{Tr("PROJECT_SIZE")}: {Math.Floor(project.DataSize):N0} KB";summary.TooltipText=summary.Text;
            double[] q=[project.Quality.Fun,project.Quality.Novelty,project.Quality.Graphics,project.Quality.Music];
            int?[] f=[project.Focus?.Fun,project.Focus?.Novelty,project.Focus?.Graphics,project.Focus?.Music];
            for(int i=0;i<4;i++){values[i].Text=$"{q[i]:N0}";weights[i].Text=$"{f[i]?.ToString() ?? "?"}%";}
            bugs.Text=project.Bugs.ToString();recognition.Text=$"{Tr("NATIVE_RECOGNITION")}: {project.Recognition:N0}%";
            progress.Text=$"{Tr("DEVELOPMENT_POINTS")}: {Math.Floor(project.Progress):N0}/{project.RequiredPoints:N0}";
            int value=OfficeRoomPresentationRules.Percentage(project);bar.Value=value;percent.Text=value+"%";
        }
        Refresh();return Refresh;
    }
    private void CheckpointPanel(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,string name,string heading)
    {
        var canvas=NativeWindow(name,heading,new(562,212));ProjectPanel(canvas,state,project,catalog);
        NativeAction(canvas,"CheckpointCancelProject","CANCEL",new(4,122,70,28),()=>ShowProjectCancellation(state,project));
        var choices=new VBoxContainer {Name="CheckpointChoicePane",CustomMinimumSize=new(266,204)};
        choices.AddThemeConstantOverride("separation",2);NativePlace(canvas,choices,new(294,4,266,204));_content=choices;
        _status=new Label {Name="CheckpointStatus",ClipText=true};_status.AddThemeFontSizeOverride("font_size",10);
        choices.AddChild(_status);
    }
    private static Label MoneyCell(GridContainer grid,string name,string caption)
    {
        grid.AddChild(new Label {Text=caption,SizeFlagsHorizontal=SizeFlags.ExpandFill});
        var label=new Label {Name=name,HorizontalAlignment=HorizontalAlignment.Right};grid.AddChild(label);return label;
    }
    private void ShipmentBreakdown(GameState state,long unit,SpinBox price,SpinBox copies)
    {
        var costBox=new PanelContainer {Name="ShipmentCostTable"};costBox.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=new(1,1,.65f),ContentMarginLeft=6,ContentMarginRight=6});_content.AddChild(costBox);
        var costs=new GridContainer {Columns=2};costBox.AddChild(costs);
        var media=MoneyCell(costs,"ShipmentMediaCost",Tr("SHIPMENT_MEDIA_COST"));
        var package=MoneyCell(costs,"ShipmentPackageCost",Tr("SHIPMENT_PACKAGE_COST"));
        var each=MoneyCell(costs,"ShipmentUnitCost",Tr("SHIPMENT_UNIT_COST"));
        var total=MoneyCell(costs,"ShipmentTotalCost",Tr("PRODUCTION_COST"));
        var netBox=new PanelContainer {Name="ShipmentDistributionTable"};netBox.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=new(.65f,1,1),ContentMarginLeft=6,ContentMarginRight=6});_content.AddChild(netBox);
        var net=new GridContainer {Columns=2};netBox.AddChild(net);
        var retail=MoneyCell(net,"ShipmentRetail",Tr("RETAIL_PRICE"));
        var retailer=MoneyCell(net,"ShipmentRetailer",Tr("SHIPMENT_RETAILER_SHARE"));
        var transport=MoneyCell(net,"ShipmentTransport",Tr("SHIPMENT_TRANSPORT_SHARE"));
        var receipt=MoneyCell(net,"ShipmentNetCopy",Tr("NET_PER_COPY"));
        var all=MoneyCell(net,"ShipmentAllSold",Tr("SHIPMENT_ALL_SOLD"));
        void Refresh()
        {
            long count=(long)copies.Value,p=(long)price.Value,per=p-state.SalesFee1!.Value-state.SalesFee2!.Value;
            media.Text=$"¥{unit-200:N0}";package.Text="¥200";each.Text=$"¥{unit:N0}";total.Text=$"¥{checked(unit*count):N0}";
            retail.Text=$"¥{p:N0}";retailer.Text=$"−¥{state.SalesFee1:N0}";transport.Text=$"−¥{state.SalesFee2:N0}";
            receipt.Text=$"¥{per:N0}";all.Text=$"¥{(System.Numerics.BigInteger)per*count:N0}";
        }
        price.ValueChanged+=_=>Refresh();copies.ValueChanged+=_=>Refresh();Refresh();
    }
}
