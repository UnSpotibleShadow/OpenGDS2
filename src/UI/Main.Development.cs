using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private VBoxContainer AddEmployeeCard(EmployeeState employee)
    {
        var frame=new PanelContainer();_content.AddChild(frame);
        var card=new VBoxContainer();frame.AddChild(card);
        var row=new HBoxContainer();row.AddThemeConstantOverride("separation",12);card.AddChild(row);
        var texture=OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId);
        if (texture is not null)
            row.AddChild(new TextureRect
            {
                Name="EmployeePortrait",Texture=texture,TextureFilter=TextureFilterEnum.Nearest,
                ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize=new Vector2(64,64)
            });
        var details=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};row.AddChild(details);
        details.AddChild(new Label {Text=$"{employee.Name}   Lv. {employee.Level}"});
        details.AddChild(new Label {Text=$"{Tr("CONTRACT_FEE")}: ¥{employee.ContractFee:N0}"});
        details.AddChild(new Label {Text=$"{Tr("STAT_FUN")}: {employee.Fun}   {Tr("STAT_NOVELTY")}: {employee.Novelty}   {Tr("STAT_ACCURACY")}: {employee.Accuracy}"});
        details.AddChild(new Label {Text=$"{Tr("STAT_GRAPHICS")}: {employee.Graphics}   {Tr("STAT_MUSIC")}: {employee.Music}   {Tr("STAT_DEVELOPMENT_SPEED")}: {employee.DevelopmentSpeed}   {Tr("STAT_INSPIRATION")}: {employee.Inspiration}"});
        return card;
    }
    private void ShowPlanningPreview(GameState state)
    {
        var catalog=_importer.LoadCatalog() ?? throw new InvalidOperationException("No local catalog.");
        ClearContent();AddHeading(Tr("DEVELOPMENT_PLANNING"),28);
        _content.AddChild(new Label {Text=Tr("PLANNING_PREVIEW_NOTE"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        AddHeading(Tr("TARGET_HARDWARE"),18);
        var hardware=new OptionButton {Name="PlanningHardware"};
        foreach (var item in catalog.Hardware.Where(x=>state.AvailableHardware?.Contains(x.SourceIndex)==true))
            hardware.AddItem($"{item.Name} · {item.BitWidth} bit",item.SourceIndex);
        _content.AddChild(hardware);
        AddHeading(Tr("TARGET_AUDIENCE"),18);
        var audience=new OptionButton {Name="PlanningAudience"};
        foreach (var item in catalog.Audiences) audience.AddItem(item.Name,item.SourceIndex);
        _content.AddChild(audience);
        var quote=new Label {Name="PlanningQuote",HorizontalAlignment=HorizontalAlignment.Center};_content.AddChild(quote);
        var gate=new Label {Name="PlanningGate",HorizontalAlignment=HorizontalAlignment.Center};_content.AddChild(gate);
        var grid=new GridContainer {Name="PlanningFocus",Columns=2};_content.AddChild(grid);
        var random=new Random();
        void ShowFocus()
        {
            foreach (Node child in grid.GetChildren()) {grid.RemoveChild(child);child.QueueFree();}
            var focus=DevelopmentFocusRules.Generate(random);
            foreach (var item in new[] {("STAT_FUN",focus.Fun),("STAT_NOVELTY",focus.Novelty),("STAT_GRAPHICS",focus.Graphics),("STAT_MUSIC",focus.Music)})
                AddMetric(grid,Tr(item.Item1),$"{item.Item2}%");
        }
        void RefreshQuote()
        {
            if (hardware.ItemCount==0 || audience.ItemCount==0) {quote.Text=Tr("PLANNING_NO_HARDWARE");return;}
            var result=DevelopmentProjectService.CheckNewPlan(state,catalog,0,hardware.GetSelectedId(),audience.GetSelectedId(),out var value);
            quote.Text=$"{Tr("PLANNING_COST")}: ¥{value.Cost:N0}   {Tr("DEVELOPMENT_POINTS")}: {value.Points}";
            gate.Text=Tr($"PLANNING_RESULT_{result}");
        }
        hardware.ItemSelected+=_=>RefreshQuote();audience.ItemSelected+=_=>RefreshQuote();
        RefreshQuote();ShowFocus();
        AddAction("PLANNING_NEW_PROPOSAL",ShowFocus);
        AddAction("CANCEL",()=>ShowOfficeReconstruction(state));
    }
}
