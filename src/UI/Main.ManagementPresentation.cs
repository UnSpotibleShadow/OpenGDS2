using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ManagementSaveActions(Control canvas,GameState state,int y,int width)
    {
        NativeAction(canvas,"ManagementSave","SAVE_GAME",new(4,y,width/2-3,24),()=>{try{Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}catch(Exception ex){_status.Text=Tr("SAVE_FAILED")+": "+ex.Message;}});
        NativeAction(canvas,"ManagementLoad","LOAD_GAME",new(width/2+5,y,width/2-5,24),LoadSession,System.IO.File.Exists(SavePath));
    }
    private Tree ManagementTable(Control canvas,string name,string[] headings,int[] widths,Rect2 rect)
    {
        widths=(int[])widths.Clone();widths[0]=Math.Max(40,widths[0]-20);
        var table=CreateTable(name,headings,widths);table.ScrollHorizontalEnabled=false;
        table.SelectMode=Tree.SelectModeEnum.Row;table.AddThemeFontSizeOverride("title_button_font_size",10);
        NativePlace(canvas,table,rect);return table;
    }
    private void ShowLicenceSelection(GameState state,int? selectedSource=null,string message="")
    {
        ClearContent();var canvas=NativeWindow("HardwareLicenceWindow",Tr("HARDWARE_LICENSES"),new(390,274));
        var catalog=OwnHardwareService.Catalog(state,_activeCatalog!);
        var hardware=catalog.Hardware.Where(h=>state.AvailableHardware?.Contains(h.SourceIndex)==true).ToArray();
        var table=ManagementTable(canvas,"HardwareLicenceTable",["TARGET_HARDWARE","LICENSE_COLUMN","HARDWARE_SHIPPED","HARDWARE_MAKER"],[119,55,92,100],new(4,4,382,126));
        var picture=NativePlace(canvas,Picture("LicenceHardwarePicture",null,new(64,64)),new(4,137,64,64));
        var bits=NativeText(canvas,"LicenceBits","",new(4,202,64,20),10,HorizontalAlignment.Center);
        NativeFill(canvas,"LicenceDescriptionBox",new(76,159,310,57),Colors.White,true);
        var name=NativeText(canvas,"LicenceSelectedName","",new(76,137,310,20));
        var description=new RichTextLabel {Name="LicenceDescription",ScrollActive=true};description.AddThemeFontSizeOverride("normal_font_size",10);NativePlace(canvas,description,new(79,162,304,51));
        var fee=NativeText(canvas,"LicenceSelectedFee","",new(4,220,382,20));
        _status=NativeText(canvas,"LicenceStatus",message,new(4,247,110,23),10);_status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        HardwareDefinition? selected=null;
        var buy=NativeAction(canvas,"BuySelectedLicence","LICENSE_BUY",new(120,244,145,25),()=>{},false);
        NativeAction(canvas,"CancelLicence","CANCEL",new(272,244,114,25),()=>ShowOfficeReconstruction(state));
        void Select()
        {
            if(table.GetSelected() is not { } row)return;
            selected=hardware.Single(h=>h.SourceIndex==row.GetMetadata(0).AsInt32());
            picture.Texture=HardwarePicture(state,selected.SourceIndex);bits.Text=selected.BitWidth+" "+Tr("BIT_UNIT");
            name.Text=CatalogName(catalog,"HARDWARE",selected.SourceIndex,selected.Name);
            description.Text=TranslationServer.GetLocale().StartsWith("en")?string.Format(DisplayNumbers.Provider,Tr("LICENCE_DESCRIPTION"),name.Text,selected.BitWidth):selected.Description;
            fee.Text=$"{Tr("CONTRACT_FEE")}: ¥{DisplayNumbers.Format(selected.LicenseFee)}";
            buy.Disabled=state.HardwareLicenses is null||state.HardwareLicenses.Contains(selected.SourceIndex)||state.Money is null||state.Money<selected.LicenseFee;
            buy.TooltipText=Tr(state.HardwareLicenses is null?"LICENSE_RESULT_LicensesUnknown":state.HardwareLicenses.Contains(selected.SourceIndex)?"LICENSE_RESULT_AlreadyOwned":state.Money is null?"LICENSE_RESULT_MoneyUnknown":state.Money<selected.LicenseFee?"LICENSE_RESULT_InsufficientFunds":"LICENSE_BUY");
        }
        table.ItemSelected+=Select;
        foreach(var h in hardware)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,h.SourceIndex);row.SetText(0,CatalogName(catalog,"HARDWARE",h.SourceIndex,h.Name));
            row.SetText(1,state.HardwareLicenses is null?"?":state.HardwareLicenses.Contains(h.SourceIndex)?Tr("LICENSE_OWNED_SHORT"):"—");
            var market=state.Calendar?.Hardware.SingleOrDefault(x=>x.SourceIndex==h.SourceIndex);
            row.SetText(2,market is null?"?":(Math.Floor(market.Market)*10_000).ToDisplayNumber());row.SetTextAlignment(2,HorizontalAlignment.Right);
            row.SetText(3,h.RawMembers.TryGetValue("0FA8",out long maker)&&maker is >=0 and <=9?DialogueText("CALENDAR_MAKER_"+maker):"?");
            if(h.SourceIndex==(selectedSource??hardware.FirstOrDefault()?.SourceIndex))row.Select(0);
        }
        Select();
        buy.Pressed+=()=>{if(buy.Disabled||selected is null)return;var result=HardwareLicenseService.TryPurchase(state,catalog,selected.SourceIndex);ShowLicenceSelection(state,selected.SourceIndex,Tr("LICENSE_RESULT_"+result));};
    }
    private void ShowAdditionalProduction(GameState state,ReleasedGameState product,OriginalCatalog catalog,long? draftCopies)
    {
        ClearContent();var canvas=NativeWindow("AdditionalProductionWindow",Tr("PRODUCE_MORE"),new(406,302));
        NativeFill(canvas,"ProductionHeaderStrip",new(4,3,398,19),Colors.White,true);
        NativeText(canvas,"ProductionHeading",Tr("SHIPMENT_PROFIT_TABLE"),new(6,3,394,19),10,HorizontalAlignment.Center);
        NativeFill(canvas,"ReproductionCostTable",new(4,30,398,174),Colors.White,true);
        long unit=ReleaseService.UnitProductionCost(catalog.Hardware.Single(h=>h.SourceIndex==product.HardwareSourceIndex),product.DataSize);
        string[] keys=["SHIPMENT_MEDIA_COST","SHIPMENT_PACKAGE_COST","SHIPMENT_UNIT_COST","PRODUCTION_COST","RETAIL_PRICE","SHIPMENT_RETAILER_SHARE","SHIPMENT_TRANSPORT_SHARE","NET_PER_COPY","SHIPMENT_ALL_SOLD"];
        string[] names=["ProductionMediaCost","ProductionPackageCost","ProductionUnitCost","ProductionTotalCost","ProductionRetailPrice","ProductionRetailer","ProductionTransport","ProductionNetCopy","ProductionAllSold"];
        var values=new Label[9];
        for(int i=0;i<9;i++){int y=34+i*18+(i>=4?6:0);
            NativeFill(canvas,"ProductionRow"+i,new(8,y,390,18),i<4?new(1,1,.72f):new(.65f,1,1),true);
            NativeFill(canvas,"ProductionDivider"+i,new(249,y,1,18),Colors.Black);
            NativeText(canvas,"ProductionCaption"+i,Tr(keys[i]),new(10,y,238,18),10);values[i]=NativeText(canvas,names[i],"",new(250,y,145,18),11,HorizontalAlignment.Right);}
        NativeText(canvas,"ProductionTitleCaption",Tr("GAME_TITLE"),new(76,210,60,22),10);
        NativeText(canvas,"ProductionProductTitle",product.Title,new(138,210,160,22),10);
        NativeText(canvas,"ProductionQuantityCaption",Tr("SHIPMENT_COPIES"),new(76,238,60,24),10);
        var copies=ProductionQuantity(state,"ProductionCopies");_content.RemoveChild(copies);if(draftCopies.HasValue)copies.Value=draftCopies.Value;
        var field=new HBoxContainer {Name="ProductionQuantityField"};NativePlace(canvas,field,new(138,238,160,26));field.AddThemeConstantOverride("separation",0);copies.CustomMinimumSize=new(112,24);field.AddChild(copies);field.AddChild(HeldShipmentButtons(copies,true));
        NativeText(canvas,"ProductionReadonlyPrice",$"{Tr("RETAIL_PRICE")}: ¥{DisplayNumbers.Format(product.Price)}",new(76,268,222,22));
        void Refresh()
        {
            long? net=state.SalesFee1 is long fee1&&state.SalesFee2 is long fee2?product.Price-fee1-fee2:null;
            long?[] amounts=[unit-200,200,unit,checked(unit*(long)copies.Value),product.Price,-state.SalesFee1,-state.SalesFee2,net,net is long known?checked(known*(long)copies.Value):null];
            for(int i=0;i<9;i++)values[i].Text=amounts[i] is long amount?$"¥{DisplayNumbers.Format(amount)}":"?";
        }
        copies.ValueChanged+=_=>Refresh();Refresh();
        _status=NativeText(canvas,"ProductionStatus","",new(4,288,398,14),10);
        var media=NativePlace(canvas,new Button {Name="ProductionMediaInformation",TooltipText=Tr("MEDIA_INFORMATION")},new(4,216,64,64));
        media.AddChild(Picture("ProductionHardwarePicture",HardwarePicture(state,product.HardwareSourceIndex),new(64,64)));
        media.Pressed+=()=>{long selectedCopies=(long)copies.Value;ShowMediaInformation(state,product.HardwareSourceIndex,product.DataSize,()=>ShowProduction(state,product,selectedCopies));};
        NativeAction(canvas,"ConfirmProduction","CONFIRM",new(304,236,98,25),()=>{var result=ReleaseService.TryProduce(state,product,catalog,(long)copies.Value);if(result==ReleaseResult.Success)ShowSales(state);else _status.Text=Tr("RELEASE_RESULT_"+result);});
        NativeAction(canvas,"CancelProduction","CANCEL",new(304,267,98,25),()=>ShowSales(state));
    }
    private void ShowStaffSelection(GameState state,EmployeeState? selected=null)
    {
        ClearContent();var canvas=NativeWindow("StaffRosterWindow",Tr("STAFF"),new(442,362));
        bool complete=OriginalPayrollRules.CaptureStaffList(state);long? salary=null;
        try{if(complete)salary=OriginalPayrollRules.StaffSalaryTotal(state.EmployeeSources);}catch(OverflowException){}
        NativeText(canvas,"StaffSalaryTotal",$"{Tr("MONTHLY_SALARY_TOTAL")}: {salary?.ToDisplayNumber()??Tr("NOT_YET_DECODED")}",new(4,2,434,21));
        NativeFill(canvas,"StaffListBox",new(4,26,434,108),Colors.White,true);
        var scroll=NativePlace(canvas,new ScrollContainer {Name="StaffSelectionList",HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled},new(6,28,430,104));
        var list=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};list.AddThemeConstantOverride("separation",0);scroll.AddChild(list);
        selected=selected is not null&&state.Employees.Contains(selected)?selected:state.Employees.FirstOrDefault();
        foreach(var employee in state.Employees)
        {
            var row=new HBoxContainer();list.AddChild(row);
            var select=new Button {Name="SelectStaff"+employee.SourceIndex,Text=EmployeeName(employee),ToggleMode=true,ButtonPressed=employee==selected,SizeFlagsHorizontal=SizeFlags.ExpandFill,ClipText=true};select.AddThemeFontSizeOverride("font_size",11);row.AddChild(select);
            var amount=new Label {Name="MonthlySalary"+employee.SourceIndex,Text=$"{Tr("MONTHLY_SALARY")}: {employee.MonthlySalary?.ToDisplayNumber()??Tr("NOT_YET_DECODED")}",HorizontalAlignment=HorizontalAlignment.Right,CustomMinimumSize=new(176,0)};amount.AddThemeFontSizeOverride("font_size",10);row.AddChild(amount);
            select.Pressed+=()=>ShowStaffSelection(state,employee);
        }
        NativeAction(canvas,"CancelStaff","CANCEL",new(332,333,106,25),()=>ShowOfficeReconstruction(state));
        if(selected is null)return;var person=selected;
        NativePlace(canvas,Picture("StaffPreviewPortrait",OriginalAssetStore.LoadEmployeePortrait(person.PortraitId),new(64,64)),new(4,145,64,64));
        NativeText(canvas,"StaffSelectedName",EmployeeName(person),new(4,210,160,20));
        NativeText(canvas,"StaffSelectedLevel",$"{Tr("LEVEL")}: {person.Level?.ToString()??"?"} / 10",new(4,232,160,19));
        NativeText(canvas,"StaffSelectedExperience",$"{Tr("EXPERIENCE")}: {person.Experience?.ToString("0.0")??"?"}",new(4,254,160,19));
        NativeText(canvas,"StaffSelectedStress",$"{Tr("STRESS")}: {person.Stress?.ToString("0.0")??"?"}",new(4,276,160,19));
        NativePlace(canvas,new EmployeeAbilityChart {Name="StaffAbilityChart",Employee=person,Compact=true,MouseFilter=MouseFilterEnum.Ignore},new(168,144,164,124));
        if(CalendarService.HasPending(state))return;
        NativeAction(canvas,"StaffTraining","TRAINING",new(338,145,100,25),()=>ShowTraining(state,person),person.TechnologyLevels is not null&&_activeCatalog?.Technologies.Count>0);
        NativeAction(canvas,"StaffTechnologyUse","USE_TECHNOLOGY",new(338,177,100,25),()=>ShowTechnologyUse(state,person),person.TechnologyLevels?.Any(x=>x.Value>0)==true);
        NativeAction(canvas,"StaffDesk","ASSIGN_DESK",new(338,209,100,25),()=>ShowDeskAssignment(state,person),person.Office is not null);
        NativeAction(canvas,"StaffBreak","TAKE_BREAK",new(338,241,100,25),()=>{var result=OfficeAssignmentService.TryTakeBreak(state,person,_session.Random);if(result==OfficeAssignmentResult.Success)ShowOfficeReconstruction(state);else ShowStaffSelection(state,person);},person.Office?.Action is >=1 and <=17);
        NativeAction(canvas,"StaffLevelUp","LEVEL_UP",new(4,302,137,25),()=>{if(person.SourceIndex is not int source)return;var result=LevelUpWithPresentation(state,source,Enumerable.Range(0,8).Select(_=>_session.Random.NextSingle()).ToArray());if(result==LevelUpResult.Success)ShowPendingLevelUp(state);else ShowStaffSelection(state,person);},person.SourceIndex is not null&&person.Level is >=1 and <10&&person.Experience>=EmployeeLevelUpService.RequiredExperience(person.Level.Value));
        NativeAction(canvas,"DismissEmployee"+person.SourceIndex,"DISMISS_EMPLOYEE",new(148,302,166,25),()=>ShowDismissEmployee(state,person),person.SourceIndex is not null);
    }
    private void ShowCompanyInformation(GameState state)
    {
        ClearContent();var canvas=NativeWindow("CompanyInformationWindow",Tr("COMPANY_WINDOW"),new(340,321));
        NativeFill(canvas,"CompanyInformationFields",new(4,4,332,203),Colors.White,true);
        string secretary=state.SecretarySourceIndex is null?"?":SecretaryName(state.SecretarySourceIndex);
        var computer=_activeCatalog?.DevelopmentComputers.SingleOrDefault(c=>c.SourceIndex==state.DevelopmentComputerIndex);
        (string Key,string Value)[] fields=[("COMPANY_NAME",state.Company.Name),("SECRETARY",secretary),("SHIPMENT_RETAILER_SHARE",state.SalesFee1?.ToDisplayNumber()??"?"),("SHIPMENT_TRANSPORT_SHARE",state.SalesFee2?.ToDisplayNumber()??"?"),("COMPUTER_PROCESSING",computer is null?"?":computer.BitWidth+" bit")];
        for(int i=0;i<fields.Length;i++){NativeText(canvas,"CompanyInfoCaption"+i,Tr(fields[i].Key),new(10,8+i*23,185,21),10);NativeText(canvas,"CompanyInfoValue"+i,fields[i].Value,new(197,8+i*23,131,21),11,HorizontalAlignment.Right);}
        for(int i=0;i<3;i++)
        {
            int? type=state.Calendar?.OriginalSlots0444.ElementAtOrDefault(i);
            string tenant=state.Calendar?.OriginalSlots0444.Count>i?type==-1?Tr("FACILITY_EMPTY"):type is >=0 and <=8?FacilityName(type.Value):"?":"?";
            NativeText(canvas,"CompanyTenantCaption"+i,string.Format(DisplayNumbers.Provider,Tr("FACILITY_SLOT"),i+1),new(10,125+i*23,104,21),10);
            NativeText(canvas,"CompanyTenant"+i,tenant,new(118,125+i*23,210,21),10,HorizontalAlignment.Right);
        }
        var record=state.ReleasedGames.OrderByDescending(p=>p.UnitsSold).FirstOrDefault();
        NativeFill(canvas,"CompanySalesRecordBox",new(4,214,332,65),new(.7f,.97f,1),true);
        NativeText(canvas,"CompanySalesRecordHeading",Tr("BEST_SELLING_GAME"),new(9,216,322,20));
        NativeText(canvas,"CompanySalesRecordTitle",record?.Title??"—",new(9,238,214,20));
        NativeText(canvas,"CompanySalesRecordUnits",record?.UnitsSold.ToDisplayNumber()??"—",new(223,238,108,20),11,HorizontalAlignment.Right);
        NativeAction(canvas,"CloseCompanyInformation","CANCEL",new(226,291,110,25),()=>ShowOfficeReconstruction(state));
    }

}
