using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool _employeeTechnologyPage;
    private DesktopWindow? _employeeDetails;
    private EmployeeState? _selectedEmployee;
    private EmployeeSelectionArrow? _selectionArrow;
    private EmployeeAbilityChart? _employeeChart;
    private readonly Dictionary<string,Label> _employeeDetailsLabels=new();
    private readonly Dictionary<string,ProgressBar> _employeeDetailsBars=new();
    private Button? _employeeLevelUp;
    private Control? _employeeAbilities;
    private ScrollContainer? _employeeTechnologies;
    private void CloseEmployeeDetails()
    {
        if (_employeeDetails is not null && GodotObject.IsInstanceValid(_employeeDetails))
        {_dialogPositions["EmployeeDetailsWindow"]=_employeeDetails.Position;_workspace?.RemoveChild(_employeeDetails);_employeeDetails.QueueFree();}
        if (_selectionArrow is not null && GodotObject.IsInstanceValid(_selectionArrow))
        {_selectionArrow.GetParent()?.RemoveChild(_selectionArrow);_selectionArrow.QueueFree();}
        _employeeDetails=null;_selectedEmployee=null;_selectionArrow=null;_employeeChart=null;_employeeLevelUp=null;
        _employeeAbilities=null;_employeeTechnologies=null;_employeeDetailsLabels.Clear();_employeeDetailsBars.Clear();
        if (_windows.TryGetValue("CompanyWindow",out var company) && GodotObject.IsInstanceValid(company)) company.Show();
    }
    private void ShowEmployeeDetails(GameState state,EmployeeState employee)
    {
        if (CalendarService.HasPending(state)) {ShowPendingCalendarEvent(state);return;}
        if (!state.Employees.Contains(employee)) return;
        ShowOfficeReconstruction(state);CloseEmployeeDetails();
        _selectedEmployee=employee;
        _employeeDetails=new DesktopWindow {Name="EmployeeDetailsWindow"};_workspace!.AddChild(_employeeDetails);
        var position=_windows["CompanyWindow"].Position;
        _employeeDetails.Initialize(EmployeeName(employee),position,new Vector2(224,376),false);
        if(_dialogPositions.TryGetValue("EmployeeDetailsWindow",out var savedPosition))_employeeDetails.SetPositionInsideWorkspace(savedPosition);
        _employeeDetails.CloseRequested=CloseEmployeeDetails;
        _windows["CompanyWindow"].Hide();
        _employeeDetails.Theme=ClassicTheme.Create();_employeeDetails.Theme.DefaultFontSize=11;
        foreach (string type in new[] {"Label","Button","MenuButton","PopupMenu"})
            _employeeDetails.Theme.SetFontSize("font_size",type,11);
        _employeeDetails.Theme.SetConstant("separation","VBoxContainer",2);
        _employeeDetails.Theme.SetConstant("separation","HBoxContainer",2);
        // Keep the information window compact even with unusually tall fallback
        // fonts or long translations. Actions stay visible below the scroll area.
        var scroll=new ScrollContainer {Name="EmployeeDetailsScroll",SizeFlagsVertical=SizeFlags.ExpandFill,
            SizeFlagsHorizontal=SizeFlags.ExpandFill,HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled};_employeeDetails.Body.AddChild(scroll);
        var selected=new VBoxContainer {Name="SelectedEmployee",SizeFlagsHorizontal=SizeFlags.ExpandFill};
        selected.AddThemeConstantOverride("separation",2);scroll.AddChild(selected);selected.SetMeta("employee_source",employee.SourceIndex ?? -1);
        var header=new HBoxContainer();header.AddThemeConstantOverride("separation",5);selected.AddChild(header);
        var picture=new VBoxContainer();picture.AddThemeConstantOverride("separation",0);header.AddChild(picture);
        var frame=new PanelContainer();frame.AddThemeStyleboxOverride("panel",ClassicTheme.WindowFrame());picture.AddChild(frame);
        var portrait=OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId);
        frame.AddChild(new TextureRect {Name="EmployeePortrait",Texture=portrait,TextureFilter=TextureFilterEnum.Nearest,
            ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,CustomMinimumSize=new Vector2(64,64)});
        AddEmployeeDetailLabel(picture,"Level","",true);
        var financials=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};financials.AddThemeConstantOverride("separation",0);header.AddChild(financials);
        AddEmployeeDetailLabel(financials,"Name",EmployeeName(employee),true);
        financials.AddChild(new Label {Text=Tr("MONTHLY_SALARY")});AddEmployeeDetailLabel(financials,"Salary","",false,$"MonthlySalary{employee.SourceIndex}");
        financials.AddChild(new Label {Text=Tr("CONTRACT_FEE")});AddEmployeeDetailLabel(financials,"Fee","",false);
        var tabs=new HBoxContainer();tabs.AddThemeConstantOverride("separation",2);selected.AddChild(tabs);
        var tabGroup=new ButtonGroup();
        var abilityTab=EmployeeDetailButton(tabs,"EmployeeAbilities","ABILITIES",()=> { _employeeTechnologyPage=false;_employeeAbilities!.Show();_employeeTechnologies!.Hide(); });
        var technologyTab=EmployeeDetailButton(tabs,"EmployeeTechnologies","TECHNOLOGIES",()=> { _employeeTechnologyPage=true;_employeeAbilities!.Hide();_employeeTechnologies!.Show(); });
        abilityTab.ToggleMode=technologyTab.ToggleMode=true;abilityTab.ButtonGroup=technologyTab.ButtonGroup=tabGroup;
        abilityTab.ButtonPressed=!_employeeTechnologyPage;technologyTab.ButtonPressed=_employeeTechnologyPage;
        abilityTab.Pressed+=()=>abilityTab.ButtonPressed=true;technologyTab.Pressed+=()=>technologyTab.ButtonPressed=true;
        _employeeAbilities=new VBoxContainer {Name="EmployeeAbilitiesPage"};selected.AddChild(_employeeAbilities);
        _employeeChart=new EmployeeAbilityChart {Name="EmployeeAbilityChart",Employee=employee,Compact=true,CustomMinimumSize=new Vector2(210,124),ClipContents=true,
            MouseFilter=MouseFilterEnum.Ignore};_employeeAbilities.AddChild(_employeeChart);
        EmployeeDetailBar((VBoxContainer)_employeeAbilities,"Speed","STAT_DEVELOPMENT_SPEED",99);
        EmployeeDetailBar((VBoxContainer)_employeeAbilities,"Inspiration","STAT_INSPIRATION",99);
        EmployeeDetailBar((VBoxContainer)_employeeAbilities,"Stress","STRESS",99);
        _employeeTechnologies=new ScrollContainer {Name="EmployeeTechnologiesPage",CustomMinimumSize=new Vector2(210,210),Visible=false};selected.AddChild(_employeeTechnologies);
        var technologyContents=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};_employeeTechnologies.AddChild(technologyContents);
        technologyContents.AddChild(new Label {Name="EmployeeTechnologyCapacity",Text=$"{Tr("MAXIMUM_TECHNOLOGIES")}: {employee.MaximumTechnologies?.ToString()??"?"}"});
        var technologies=CreateTable("EmployeeTechnologyTable",["TECHNOLOGY_NAME","EMPLOYEE_LEVEL"],[154,35]);technologies.CustomMinimumSize=new(210,184);technologies.ScrollHorizontalEnabled=false;technologies.AddThemeFontSizeOverride("title_button_font_size",10);technologyContents.AddChild(technologies);
        if(employee.TechnologyLevels is null){var row=technologies.CreateItem(technologies.GetRoot());row.SetText(0,Tr("NOT_YET_DECODED"));}
        else
        {
            var learned=employee.TechnologyLevels.Where(p=>p.Value>0).OrderBy(p=>p.Key).ToList();
            if(learned.Count==0){var row=technologies.CreateItem(technologies.GetRoot());row.SetText(0,Tr("EMPLOYEE_NO_TECHNOLOGIES"));}
            foreach(var skill in learned){var original=_activeCatalog?.Technologies.SingleOrDefault(t=>t.SourceIndex==skill.Key);var row=technologies.CreateItem(technologies.GetRoot());row.SetText(0,original is null?"?":TechnologyName(original));row.SetText(1,skill.Value.ToString());row.SetTextAlignment(1,HorizontalAlignment.Right);}
        }
        _employeeAbilities.Visible=!_employeeTechnologyPage;_employeeTechnologies.Visible=_employeeTechnologyPage;
        var footer=new HBoxContainer();footer.AddThemeConstantOverride("separation",2);_employeeDetails.Body.AddChild(footer);
        AddEmployeeDetailLabel(footer,"Experience","",false);
        _employeeLevelUp=EmployeeDetailButton(footer,"EmployeeLevelUp","LEVEL_UP",()=>
        {
            if (employee.SourceIndex is not int source) return;
            var result=LevelUpWithPresentation(state,source,Enumerable.Range(0,8).Select(_=>_session.Random.NextSingle()).ToArray());
            RefreshEmployeeDetails();if(result==LevelUpResult.Success)ShowPendingLevelUp(state);if (result!=LevelUpResult.Success && _desktopStatus is not null) _desktopStatus.Text=Tr($"LEVEL_UP_RESULT_{result}");
        });
        var dismiss=EmployeeDetailButton(footer,$"DismissEmployee{employee.SourceIndex}","EMPLOYEE_DISMISS_SHORT",()=>ShowDismissEmployee(state,employee));
        dismiss.TooltipText=Tr("DISMISS_EMPLOYEE");dismiss.Disabled=employee.SourceIndex is null;
        var actions=new HBoxContainer();_employeeDetails.Body.AddChild(actions);
        var more=new MenuButton {Name="EmployeeActions",Text=Tr("EMPLOYEE_ACTIONS"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(more);
        (string Key,Action Action,bool Enabled)[] choices=[
            ("ASSIGN_DESK",()=>ShowDeskAssignment(state,employee),employee.Office is not null),
            ("TRAINING",()=>ShowTraining(state,employee),employee.TechnologyLevels is not null && _activeCatalog?.Technologies.Count>0),
            ("USE_TECHNOLOGY",()=>ShowTechnologyUse(state,employee),employee.TechnologyLevels?.Any(p=>p.Value>0)==true),
            ("TAKE_BREAK",()=> {var result=OfficeAssignmentService.TryTakeBreak(state,employee,_session.Random);RefreshEmployeeDetails();
                if (result!=OfficeAssignmentResult.Success && _desktopStatus is not null) _desktopStatus.Text=Tr($"ASSIGNMENT_RESULT_{result}");},employee.Office?.Action is >=1 and <=17),
            ("STAFF",()=>ShowStaff(state),true)];
        for (int i=0;i<choices.Length;i++) {more.GetPopup().AddItem(Tr(choices[i].Key),i);more.GetPopup().SetItemDisabled(i,!choices[i].Enabled);}
        more.GetPopup().IdPressed+=id=> {if (_dialog is null && !CalendarService.HasPending(state) && choices[(int)id].Enabled) choices[(int)id].Action();};
        EmployeeDetailButton(actions,"CloseEmployeeDetails","CANCEL",CloseEmployeeDetails);
        var officeImage=_windows["OfficeWindow"].FindChild("OriginalOfficeBackground",true,false) as TextureRect;
        if (officeImage is not null)
        {
            _selectionArrow=new EmployeeSelectionArrow {Name="EmployeeSelectionArrow",MouseFilter=MouseFilterEnum.Ignore};officeImage.AddChild(_selectionArrow);
        }
        RefreshEmployeeDetails();RefreshEmployeeSelectionArrow();
    }
    private void AddEmployeeDetailLabel(Container parent,string key,string text,bool center=false,string? name=null)
    {
        var label=new Label {Name=name ?? "EmployeeDetail"+key,Text=text,ClipText=true,SizeFlagsHorizontal=SizeFlags.ExpandFill,
            HorizontalAlignment=center ? HorizontalAlignment.Center : HorizontalAlignment.Right};parent.AddChild(label);_employeeDetailsLabels[key]=label;
    }
    private Button EmployeeDetailButton(Container parent,string name,string key,Action action)
    {
        var button=new Button {Name=name,Text=Tr(key),TooltipText=Tr(key),SizeFlagsHorizontal=SizeFlags.ExpandFill};
        button.AddThemeStyleboxOverride("normal",ClassicTheme.CompactButton());button.AddThemeStyleboxOverride("pressed",ClassicTheme.CompactButton(true));
        foreach(string mode in new[]{"hover","disabled","focus"})button.AddThemeStyleboxOverride(mode,ClassicTheme.CompactButton());
        if(name=="EmployeeLevelUp")NativeButtonArtwork.Apply(button,0x29DC34);
        else if(name.StartsWith("DismissEmployee",StringComparison.Ordinal))NativeButtonArtwork.Apply(button,0x29C8D8);
        else if(name=="EmployeeAbilities")NativeButtonArtwork.ApplyTab(button,0x29FE77,0x29F691);
        else if(name=="EmployeeTechnologies")NativeButtonArtwork.ApplyTab(button,0x2A162B,0x2A0E45);
        parent.AddChild(button);button.Pressed+=()=> {if (_dialog is null && _desktopState is GameState state && !CalendarService.HasPending(state)) action();};return button;
    }
    private void EmployeeDetailBar(VBoxContainer parent,string key,string caption,double maximum)
    {
        var row=new HBoxContainer();row.AddThemeConstantOverride("separation",3);parent.AddChild(row);
        var label=new Label {Text=Tr(caption),CustomMinimumSize=new Vector2(88,0)};row.AddChild(label);
        var bar=new ProgressBar {Name="EmployeeBar"+key,MaxValue=maximum,ShowPercentage=false,CustomMinimumSize=new Vector2(76,8),
            SizeFlagsHorizontal=SizeFlags.ExpandFill,SizeFlagsVertical=SizeFlags.ShrinkCenter};
        bar.AddThemeStyleboxOverride("background",new StyleBoxFlat {BgColor=new Color(0,0,.7f),BorderColor=Colors.Black,
            BorderWidthTop=1,BorderWidthBottom=1,BorderWidthLeft=1,BorderWidthRight=1});
        bar.AddThemeStyleboxOverride("fill",new StyleBoxFlat {BgColor=new Color(0,1,0),BorderColor=Colors.Black,
            BorderWidthTop=1,BorderWidthBottom=1,BorderWidthLeft=1,BorderWidthRight=1});row.AddChild(bar);_employeeDetailsBars[key]=bar;
        var number=new Label {Name="EmployeeValue"+key,CustomMinimumSize=new Vector2(23,0),HorizontalAlignment=HorizontalAlignment.Right};row.AddChild(number);_employeeDetailsLabels[key]=number;
    }
    private void RefreshEmployeeDetails()
    {
        var e=_selectedEmployee;if (e is null || _desktopState is null) return;
        if (!_desktopState.Employees.Contains(e)) {CloseEmployeeDetails();return;}
        _employeeDetailsLabels["Name"].Text=EmployeeName(e);_employeeDetailsLabels["Level"].Text=$"{Tr("LEVEL")} {e.Level?.ToString() ?? "?"}/10";
        _employeeDetailsLabels["Salary"].Text=e.MonthlySalary?.ToDisplayNumber()?? "?";_employeeDetailsLabels["Fee"].Text=e.ContractFee?.ToDisplayNumber()?? "?";
        _employeeDetailsLabels["Experience"].Text=$"{(e.Experience is double exp ? Math.Floor(exp).ToString() : "?")}/{(e.Level is >=1 and <=10 ? EmployeeLevelUpService.RequiredExperience(e.Level.Value).ToString() : "?")}";
        foreach (var (key,value) in new (string,double?)[] {("Speed",e.DevelopmentSpeed),("Inspiration",e.Inspiration),("Stress",e.Stress)})
        {_employeeDetailsLabels[key].Text=value is double v ? Math.Floor(v).ToString() : "?";_employeeDetailsBars[key].Visible=value is not null;_employeeDetailsBars[key].Value=value ?? 0;}
        if (_employeeLevelUp is not null) _employeeLevelUp.Disabled=e.SourceIndex is null || e.Level is not (>=1 and <10) || e.Experience is not double experience || experience<EmployeeLevelUpService.RequiredExperience(e.Level.Value);
        _employeeChart?.QueueRedraw();
    }
    private void RefreshEmployeeSelectionArrow()
    {
        if (_selectionArrow is null || _selectedEmployee?.Office is not OfficeEmployeeState office || _selectionArrow.GetParent() is not TextureRect image || image.Texture is null) return;
        Vector2 native=image.Texture.GetSize();float scale=Math.Min(image.Size.X/native.X,image.Size.Y/native.Y);
        Vector2 offset=(image.Size-native*scale)/2;
        _selectionArrow.Position=offset+new Vector2(office.X+office.OffsetX+4,office.Y+office.OffsetY-12)*scale;
        _selectionArrow.Size=new Vector2(8,10)*scale;_selectionArrow.QueueRedraw();
    }
}
