using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowAnnualRenewal(GameState state)
    {
        ClearContent();CompactManagementWindow("AnnualRenewalWindow",new(730,500),710);
        _dialog?.SetTitle(Tr("ANNUAL_CONTRACT_RENEWAL"));
        AddHeading(Tr("RENEWAL_INSTRUCTION"),16);
        var scroll=new ScrollContainer {Name="RenewalRoster",CustomMinimumSize=new(0,150),HorizontalScrollMode=ScrollContainer.ScrollMode.Auto};_content.AddChild(scroll);
        var grid=new GridContainer {Columns=9,SizeFlagsHorizontal=SizeFlags.ExpandFill};scroll.AddChild(grid);
        foreach(string key in new[]{"RENEWAL_CONTINUE","EMPLOYEE_NAME","LEVEL","STAT_FUN","STAT_GRAPHICS","STAT_MUSIC","STAT_NOVELTY","STAT_ACCURACY","CONTRACT_FEE"})
            grid.AddChild(new Label {Text=Tr(key)});
        var details=new HBoxContainer {Name="RenewalEmployeeDetails"};_content.AddChild(details);
        var selectedTotal=new Label {Name="RenewalSelectedTotal"};_content.AddChild(selectedTotal);
        var funds=new Label {Name="RenewalFunds",Text=$"{Tr("FUNDS")}: ¥{state.Money:N0}"};_content.AddChild(funds);
        _status=new Label {Name="RenewalStatus"};_content.AddChild(_status);
        var selection=new Dictionary<int,CheckBox>();
        void ShowPerson(EmployeeState employee)
        {
            foreach(Node child in details.GetChildren()){details.RemoveChild(child);child.QueueFree();}
            var identity=new VBoxContainer();details.AddChild(identity);
            identity.AddChild(Picture("RenewalSelectedPortrait",OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId),new(64,64)));
            identity.AddChild(new Label {Name="RenewalSelectedName",Text=EmployeeName(employee)});
            identity.AddChild(new Label {Text=$"{Tr("LEVEL")} {employee.Level?.ToString() ?? "?"}/10"});
            details.AddChild(new EmployeeAbilityChart {Name="RenewalAbilityChart",Employee=employee,CustomMinimumSize=new(210,150)});
            var skills=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};details.AddChild(skills);
            foreach(var item in new[]{("STAT_DEVELOPMENT_SPEED",employee.DevelopmentSpeed),("STAT_INSPIRATION",employee.Inspiration)})
            {
                skills.AddChild(new Label {Text=$"{Tr(item.Item1)}: {item.Item2?.ToString() ?? "?"}"});
                skills.AddChild(new ProgressBar {MaxValue=99,Value=item.Item2 ?? 0,ShowPercentage=false,CustomMinimumSize=new(100,12)});
            }
            if(employee.TechnologyLevels is { } levels && _activeCatalog is { } catalog)
            {
                var descriptions=new RichTextLabel {Name="RenewalTechnologyList",CustomMinimumSize=new(150,64),ScrollActive=true,FitContent=false};skills.AddChild(descriptions);
                descriptions.Text=string.Join("\n",levels.Where(x=>x.Value>0).OrderBy(x=>x.Key).Select(x=>
                    (catalog.Technologies.SingleOrDefault(t=>t.SourceIndex==x.Key) is { } t ? CatalogName(catalog,"TECHNOLOGY",t.SourceIndex,t.Name) : "?")+" · "+x.Value));
            }
        }
        void RefreshTotal()
        {
            var checkedPeople=state.Employees.Where(e=>selection[e.SourceIndex!.Value].ButtonPressed).ToList();
            selectedTotal.Text=Tr("RENEWAL_SELECTED_TOTAL")+": "+(checkedPeople.Any(e=>e.ContractFee is null) ? "?" : $"¥{checkedPeople.Sum(e=>e.ContractFee!.Value):N0}");
        }
        foreach(var employee in state.Employees.OrderBy(x=>x.SourceIndex))
        {
            int id=employee.SourceIndex ?? throw new InvalidOperationException("Unknown employee identity.");
            var check=new CheckBox {Name=$"RenewEmployee{id}",ButtonPressed=true};selection.Add(id,check);grid.AddChild(check);
            check.Toggled+=_=>RefreshTotal();
            var name=new Button {Name=$"SelectRenewalEmployee{id}",Text=EmployeeName(employee)};name.Pressed+=()=>ShowPerson(employee);grid.AddChild(name);
            foreach(int? value in new[]{employee.Level,employee.Fun,employee.Graphics,employee.Music,employee.Novelty,employee.Accuracy})
                grid.AddChild(new Label {Text=value?.ToString() ?? "?",HorizontalAlignment=HorizontalAlignment.Right});
            grid.AddChild(new Label {Text=employee.ContractFee is { } fee ? $"¥{fee:N0}" : "?",HorizontalAlignment=HorizontalAlignment.Right});
        }
        if(state.Employees.OrderBy(x=>x.SourceIndex).FirstOrDefault() is { } first)ShowPerson(first);
        RefreshTotal();
        var confirm=new Button {Name="ConfirmContractRenewal",Text=Tr("CONFIRM_CONTRACT_RENEWAL")};_content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            var result=CalendarService.RenewContracts(state,selection.Where(x=>x.Value.ButtonPressed).Select(x=>x.Key).ToHashSet());
            if(result==ContractRenewalResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr($"CONTRACT_RENEWAL_{result}");
        };
    }
}
