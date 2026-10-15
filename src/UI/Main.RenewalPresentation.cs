using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowAnnualRenewal(GameState state)
    {
        ClearContent();var canvas=NativeWindow("AnnualRenewalWindow",Tr("ANNUAL_CONTRACT_RENEWAL"),new(500,366));
        NativeFill(canvas,"RenewalInstructionPanel",new(4,3,492,22),Colors.White,true);
        NativeText(canvas,"RenewalInstruction",Tr("RENEWAL_INSTRUCTION"),new(7,3,486,22),10,HorizontalAlignment.Center);
        var scroll=new ScrollContainer {Name="RenewalRoster",HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled};NativePlace(canvas,scroll,new(4,31,492,160));
        var grid=new GridContainer {Columns=9,SizeFlagsHorizontal=SizeFlags.ExpandFill};grid.AddThemeConstantOverride("h_separation",0);grid.AddThemeConstantOverride("v_separation",0);scroll.AddChild(grid);
        int[] widths=[30,122,24,28,28,28,28,28,108];int column=0;
        foreach(string key in new[]{"RENEWAL_CONTINUE","EMPLOYEE_NAME","LEVEL","SHORT_FUN","SHORT_GRAPHICS","SHORT_MUSIC","SHORT_NOVELTY","SHORT_ACCURACY","CONTRACT_FEE"})
        {
            var label=new Label {Text=Tr(key),ClipText=true,CustomMinimumSize=new(widths[column++],20)};label.AddThemeFontSizeOverride("font_size",10);grid.AddChild(label);
        }
        var details=NativePlace(canvas,new Control {Name="RenewalEmployeeDetails"},new(4,203,492,131));
        var selectedTotal=NativeText(canvas,"RenewalSelectedTotal","",new(4,343,350,19),10);
        var funds=NativeText(canvas,"RenewalFunds",$"{Tr("FUNDS")}: ¥{DisplayNumbers.Format(state.Money)}",new(4,324,350,19),10);
        _status=NativeText(canvas,"RenewalStatus","",new(278,304,218,19),10);
        var selection=new Dictionary<int,CheckBox>();
        void ShowPerson(EmployeeState employee)
        {
            foreach(Node child in details.GetChildren()){details.RemoveChild(child);child.QueueFree();}
            NativePlace(details,Picture("RenewalSelectedPortrait",OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId),new(64,64)),new(0,2,64,64));
            NativeText(details,"RenewalSelectedName",EmployeeName(employee),new(0,69,116,18),10);
            NativeText(details,"RenewalSelectedLevel",$"{Tr("LEVEL")} {employee.Level?.ToString() ?? "?"}/10",new(0,87,116,18),10);
            NativePlace(details,new EmployeeAbilityChart {Name="RenewalAbilityChart",Employee=employee,Compact=true},new(114,0,150,124));
            int y=0;
            foreach(var item in new[]{("STAT_DEVELOPMENT_SPEED",employee.DevelopmentSpeed),("STAT_INSPIRATION",employee.Inspiration)})
            {
                NativeText(details,"Renewal"+item.Item1,Tr(item.Item1),new(274,y,143,16),10);
                var bar=new ProgressBar {MaxValue=99,Value=item.Item2 ?? 0,ShowPercentage=false};
                bar.AddThemeStyleboxOverride("background",new StyleBoxFlat {BgColor=Colors.Blue});bar.AddThemeStyleboxOverride("fill",new StyleBoxFlat {BgColor=new(0,.7f,0)});
                NativePlace(details,bar,new(419,y+4,48,8));NativeText(details,"RenewalValue"+item.Item1,item.Item2?.ToString()??"?",new(470,y,22,16),10,HorizontalAlignment.Right);y+=19;
            }
            var descriptions=new RichTextLabel {Name="RenewalTechnologyList",ScrollActive=true};descriptions.AddThemeFontSizeOverride("normal_font_size",10);NativePlace(details,descriptions,new(274,42,218,58));
            if(employee.TechnologyLevels is { } levels && _activeCatalog is { } catalog)
                descriptions.Text=string.Join("\n",levels.Where(x=>x.Value>0).OrderBy(x=>x.Key).Select(x=>
                    (catalog.Technologies.SingleOrDefault(t=>t.SourceIndex==x.Key) is { } t ? CatalogName(catalog,"TECHNOLOGY",t.SourceIndex,t.Name) : "?")+" · "+x.Value));
        }
        void RefreshTotal()
        {
            var checkedPeople=state.Employees.Where(e=>selection[e.SourceIndex!.Value].ButtonPressed).ToList();
            selectedTotal.Text=Tr("RENEWAL_SELECTED_TOTAL")+": "+(checkedPeople.Any(e=>e.ContractFee is null) ? "?" : $"¥{DisplayNumbers.Format(checkedPeople.Sum(e=>e.ContractFee!.Value))}");
        }
        foreach(var employee in state.Employees.OrderBy(x=>x.SourceIndex))
        {
            int id=employee.SourceIndex ?? throw new InvalidOperationException("Unknown employee identity.");
            var check=new CheckBox {Name=$"RenewEmployee{id}",ButtonPressed=true,CustomMinimumSize=new(30,20)};selection.Add(id,check);grid.AddChild(check);
            check.Toggled+=_=>RefreshTotal();
            var name=new Button {Name=$"SelectRenewalEmployee{id}",Text=EmployeeName(employee),ClipText=true,CustomMinimumSize=new(122,20)};name.AddThemeFontSizeOverride("font_size",10);name.AddThemeStyleboxOverride("normal",new StyleBoxFlat {BgColor=Colors.White});name.Pressed+=()=>ShowPerson(employee);grid.AddChild(name);
            foreach(int? value in new[]{employee.Level,employee.Fun,employee.Graphics,employee.Music,employee.Novelty,employee.Accuracy})
                {var valueLabel=new Label {Text=value?.ToString() ?? "?",HorizontalAlignment=HorizontalAlignment.Right};valueLabel.AddThemeFontSizeOverride("font_size",10);grid.AddChild(valueLabel);}
            grid.AddChild(new Label {Text=employee.ContractFee is { } fee ? $"¥{DisplayNumbers.Format(fee)}" : "?",HorizontalAlignment=HorizontalAlignment.Right});
        }
        if(state.Employees.OrderBy(x=>x.SourceIndex).FirstOrDefault() is { } first)ShowPerson(first);
        RefreshTotal();
        var confirm=NativeAction(canvas,"ConfirmContractRenewal","CONFIRM_CONTRACT_RENEWAL",new(358,336,138,26),()=>{});
        confirm.Pressed+=()=>
        {
            var result=CalendarService.RenewContracts(state,selection.Where(x=>x.Value.ButtonPressed).Select(x=>x.Key).ToHashSet());
            if(result==ContractRenewalResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr($"CONTRACT_RENEWAL_{result}");
        };
    }
}
