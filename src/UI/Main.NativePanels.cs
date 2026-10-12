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
    private Action ProjectPanel(Container parent,GameState state,DevelopmentProjectState project,OriginalCatalog catalog)
    {
        var panel=new VBoxContainer {Name="ProjectSummaryPanel",CustomMinimumSize=new(245,0)};parent.AddChild(panel);
        var top=new HBoxContainer();panel.AddChild(top);
        var device=new VBoxContainer();top.AddChild(device);
        device.AddChild(Picture("ProjectHardwarePicture",HardwarePicture(state,project.HardwareSourceIndex),new(64,64)));
        device.AddChild(new Label {Text=CatalogName(catalog,"HARDWARE",project.HardwareSourceIndex,catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex).Name),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        var summary=new Label {Name="ProjectCatalogSummary",AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill};top.AddChild(summary);
        var quality=new GridContainer {Columns=2};panel.AddChild(quality);
        string[] keys=["STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"];
        Color[] colors=[new(.12f,.12f,.85f),new(.7f,.6f,0),new(.65f,.08f,.85f),new(0,.65f,.7f)];
        var values=new Label[4];
        var weights=new Label[4];
        for(int i=0;i<4;i++)
        {
            var row=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};quality.AddChild(row);
            var label=new Label {Text="● "+Tr(keys[i])};label.AddThemeColorOverride("font_color",colors[i]);row.AddChild(label);
            var numbers=new HBoxContainer();row.AddChild(numbers);
            values[i]=new Label {Name=$"ProjectQuality{i}",SizeFlagsHorizontal=SizeFlags.ExpandFill};numbers.AddChild(values[i]);
            weights[i]=new Label {Name=$"ProjectFocus{i}"};weights[i].AddThemeColorOverride("font_color",Colors.Red);numbers.AddChild(weights[i]);
        }
        var bugsRow=new HBoxContainer();panel.AddChild(bugsRow);
        bugsRow.AddChild(new Label {Text=Tr("BUGS")});
        var bugBox=new PanelContainer();bugBox.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=Colors.Red});bugsRow.AddChild(bugBox);
        var bugs=new Label {Name="ProjectBugCount",HorizontalAlignment=HorizontalAlignment.Center,CustomMinimumSize=new(42,24)};bugs.AddThemeColorOverride("font_color",Colors.Black);bugBox.AddChild(bugs);
        var recognition=new Label {Name="ProjectRecognition"};bugsRow.AddChild(recognition);
        var progress=new Label {Name="ProjectPointSummary"};panel.AddChild(progress);
        var bar=new ProgressBar {Name="ProjectSummaryProgress",MinValue=0,MaxValue=100,ShowPercentage=true};panel.AddChild(bar);
        void Refresh()
        {
            string audience=CatalogName(catalog,"AUDIENCE",project.AudienceSourceIndex,catalog.Audiences.Single(x=>x.SourceIndex==project.AudienceSourceIndex).Name);
            string genre=project.GenreSourceIndex is int g ? CatalogName(catalog,"GENRE",g,catalog.Genres.Single(x=>x.SourceIndex==g).Name) : "—";
            string type=project.GameTypeSourceIndex is int t ? CatalogName(catalog,"TYPE",t,catalog.GameTypes.Single(x=>x.SourceIndex==t).Name) : "—";
            summary.Text=$"{Tr("PROJECT_AUDIENCE")}: {audience}\n{Tr("PROJECT_GENRE")}: {genre}\n{Tr("PROJECT_CONTENT")}: {type}\n{Tr("PROJECT_SIZE")}: {Math.Floor(project.DataSize):N0} KB";
            double[] q=[project.Quality.Fun,project.Quality.Novelty,project.Quality.Graphics,project.Quality.Music];
            int?[] f=[project.Focus?.Fun,project.Focus?.Novelty,project.Focus?.Graphics,project.Focus?.Music];
            for(int i=0;i<4;i++){values[i].Text=$"{q[i]:N0}";weights[i].Text=$"{f[i]?.ToString() ?? "?"}%";}
            bugs.Text=project.Bugs.ToString();recognition.Text=$"{Tr("GAME_RECOGNITION")}: {project.Recognition:N0}%";
            progress.Text=$"{Tr("DEVELOPMENT_POINTS")}: {Math.Floor(project.Progress):N0}/{project.RequiredPoints:N0}";
            bar.Value=OfficeRoomPresentationRules.Percentage(project);
        }
        Refresh();return Refresh;
    }
    private void CheckpointPanel(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,string name,string heading)
    {
        CompactManagementWindow(name,new(770,410),750);_dialog?.SetTitle(heading);
        var row=new HBoxContainer();row.AddThemeConstantOverride("separation",12);_content.AddChild(row);
        ProjectPanel(row,state,project,catalog);
        var choices=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};row.AddChild(choices);_content=choices;
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
