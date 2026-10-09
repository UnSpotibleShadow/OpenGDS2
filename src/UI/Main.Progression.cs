using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Persistence;

namespace OpenGDS2.UI;

public partial class Main
{
    private string RankingsPath=>ProjectSettings.GlobalizePath("user://rankings.json");
    private void PersistPendingRankings(GameState state)
    {
        if (state.PendingRankings.Count==0) return;
        try
        {
            var book=RankingStore.Load(RankingsPath);bool changed=false;
            foreach (var achievement in state.PendingRankings)
                changed|=RankingService.Record(book,achievement,DateTime.Today.ToString("yyyy/MM/dd",System.Globalization.CultureInfo.InvariantCulture));
            if (changed) RankingStore.Save(RankingsPath,book);
            state.PendingRankings.Clear();
        }
        catch (Exception ex) {_status.Text=$"{Tr("RANKINGS_SAVE_FAILED")}: {ex.Message}";}
    }
    private void ShowFacilities(GameState state,int? selectedSlot=null)
    {
        RefreshDesktop();
        if (_officeRenderer is not null && _officeTexture is not null) _officeRenderer.UpdateTexture(_officeTexture,state);
        ClearContent();AddHeading(Tr("COMPANY_FACILITIES"),26);
        if (selectedSlot is not null) CompactManagementWindow("TenantFacilityWindow",new Vector2(410,440),390);
        AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",18);
        if (state.Calendar is null) AnnualMessage("FacilitiesUnknown",Tr("CALENDAR_LEGACY_SAVE"));
        else for (int slot=0;slot<3;slot++)
        {
            if (selectedSlot is int chosen && slot!=chosen) continue;
            int index=slot,value=state.Calendar.OriginalSlots0444[slot];
            AddHeading(string.Format(Tr("FACILITY_SLOT"),slot+1)+": "+FacilityName(value),18);
            if (value==-1)
            {
                foreach (FacilityKind kind in Enum.GetValues<FacilityKind>())
                {
                    var chosenKind=kind;bool available=FacilityService.ServiceAvailable(kind);
                    var buy=new Button {Name=kind==FacilityKind.School ? $"OpenSchool{slot}" : $"OpenFacility{slot}_{(int)kind}",Text=$"{FacilityName((int)kind)} — ¥{FacilityService.PurchasePrice(kind):N0}"+
                        (available ? "" : " · "+Tr("FACILITY_COMING_SOON")),
                        Disabled=!available || state.Calendar.OriginalSlots0444.Contains((int)kind) || FacilityService.HasPendingDecision(state)};
                    _content.AddChild(buy);buy.Pressed+=()=>
                    {var result=FacilityService.Open(state,index,chosenKind);ShowFacilities(state,selectedSlot);_status.Text=Tr($"FACILITY_RESULT_{result}");};
                }
            }
            else if (value is >=0 and <=8)
            {
                var kind=(FacilityKind)value;
                AnnualMessage($"FacilityDescription{slot}",Tr($"FACILITY_DESCRIPTION_{kind}"));
                switch (kind)
                {
                    case FacilityKind.HotSpring:
                        FacilityAction($"UseHotSpring{slot}","FACILITY_HOT_SPRING_ACTION",EmployeeCareService.HotSpringCost,
                            ()=>ConfirmFacilityService(state,index,kind,EmployeeCareService.HotSpringCost,()=>FacilityService.HotSpring(state,index)));break;
                    case FacilityKind.Recycling:
                        AnnualButton($"RecycleStock{slot}","FACILITY_RECYCLE_ACTION",()=>
                            ConfirmFacilityService(state,index,kind,checked(FacilityService.StockForRecycling(state)*FacilityService.RecyclingPrice),()=>FacilityService.Recycle(state,index)));break;
                    case FacilityKind.TechnologySchool:
                        AnnualButton($"TechnologyRoster{slot}","FACILITY_STUDY_ACTION",()=>ShowFacilityTechnologyRoster(state,index));break;
                    case FacilityKind.ProductionLab:
                        FacilityAction($"ExpandProduction{slot}","FACILITY_PRODUCTION_ACTION",FacilityService.ProductionExpansionCost,
                            ()=>ConfirmFacilityService(state,index,kind,FacilityService.ProductionExpansionCost,()=>_session.ExpandFacilityProduction(index)));break;
                }
                AnnualButton(kind==FacilityKind.School ? $"CloseSchool{slot}" : $"CloseFacility{slot}","FACILITY_CLOSE",()=>ShowFacilityClosure(state,index,kind));
            }
        }
        AddSaveActions(state);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private void FacilityAction(string name,string key,long amount,Action action)
    {
        var button=new Button {Name=name,Text=$"{Tr(key)} — ¥{amount:N0}"};_content.AddChild(button);button.Pressed+=action;
    }
    private string FacilityName(int value)=>value==-1 ? Tr("FACILITY_EMPTY") : value is >=0 and <=8 ? Tr($"FACILITY_NAME_{(FacilityKind)value}") : string.Format(Tr("FACILITY_OTHER"),value);
    private void ShowFacilityClosure(GameState state,int slot,FacilityKind kind)
    {
        ClearContent();AddHeading(Tr("FACILITY_CLOSE"),26);
        AnnualMessage("FacilityCloseConfirmation",string.Format(Tr("FACILITY_CLOSE_CONFIRMATION"),FacilityName((int)kind)));
        AnnualButton(kind==FacilityKind.School ? "ConfirmSchoolClosure" : "ConfirmFacilityClosure","CONFIRM",()=>
        {var result=FacilityService.Close(state,slot,kind);ShowFacilities(state,slot);_status.Text=Tr($"FACILITY_RESULT_{result}");});
        AddAction("CANCEL",()=>ShowFacilities(state,slot));
    }
    private void ConfirmFacilityService(GameState state,int slot,FacilityKind kind,long amount,Func<FacilityResult> action)
    {
        ClearContent();CompactManagementWindow("FacilityConfirmationWindow",new Vector2(410,280),390);
        AddHeading(FacilityName((int)kind),22);
        AnnualMessage("FacilityConfirmation",string.Format(Tr($"FACILITY_CONFIRM_{kind}"),amount));
        AnnualButton("ConfirmFacilityService","CONFIRM",()=>
        {
            if (!ReferenceEquals(_session.Current,state)) return;
            var result=action();
            if (result==FacilityResult.Success) ShowOfficeReconstruction(state);
            else {ShowFacilities(state,slot);_status.Text=Tr($"FACILITY_RESULT_{result}");}
        });
        AddAction("CANCEL",()=>ShowFacilities(state,slot));
    }
    private void ShowFacilityTechnologyRoster(GameState state,int slot)
    {
        var roster=FacilityService.OpenTechnologyRoster(state,slot);
        ClearContent();CompactManagementWindow("FacilityStudyWindow",new Vector2(410,360),390);
        AddHeading(Tr("FACILITY_STUDY_ACTION"),22);
        if (roster is null) AnnualMessage("FacilityRosterUnknown",Tr("FACILITY_RESULT_StateUnknown"));
        else if (roster.Count==0) AnnualMessage("FacilityRosterEmpty",Tr("FACILITY_RESULT_NoEmployees"));
        else foreach (var employee in roster)
        {
            var student=employee;
            var button=new Button {Name=$"StudyCapacity{employee.SourceIndex}",Text=$"{EmployeeName(employee)} · {employee.MaximumTechnologies} → {employee.MaximumTechnologies+1} · ¥{FacilityService.TechnologyStudyCost:N0}"};
            _content.AddChild(button);button.Pressed+=()=>ConfirmFacilityService(state,slot,FacilityKind.TechnologySchool,
                FacilityService.TechnologyStudyCost,()=>FacilityService.Study(state,slot,student));
        }
        AddSaveActions(state);AddAction("CANCEL",()=>ShowFacilities(state,slot));
    }
    private bool ShowPendingFacilityReceipt(GameState state)
    {
        if (state.PendingFacilityReceipt is not { } receipt) return false;
        ClearContent();CompactManagementWindow("FacilityResultWindow",new Vector2(410,280),390);
        AddHeading(FacilityName((int)receipt.Kind),22);
        AnnualMessage("FacilityServiceResult",string.Format(Tr($"FACILITY_RECEIPT_{receipt.Kind}"),receipt.Amount,receipt.Gain,receipt.EmployeeName));
        AnnualButton("AcknowledgeFacilityReceipt","CONTINUE",()=>
        {
            if (!ReferenceEquals(_session.Current,state) || !ReferenceEquals(state.PendingFacilityReceipt,receipt)) return;
            state.PendingFacilityReceipt=null;ShowOfficeReconstruction(state);
        });
        AddSaveActions(state);return true;
    }
    private void ShowTenantPlotChoice(GameState state,IReadOnlyList<int> slots)
    {
        ClearContent();CompactManagementWindow("TenantPlotChoiceWindow",new Vector2(350,230),330);
        AddHeading(Tr("FACILITY_CHOOSE_PLOT"),22);
        foreach (int slot in slots)
        {
            int chosen=slot;var button=new Button {Name=$"ChooseTenantPlot{slot}",Text=string.Format(Tr("FACILITY_SLOT"),slot+1)};
            _content.AddChild(button);button.Pressed+=()=>ShowFacilities(state,chosen);
        }
        AddAction("CANCEL",()=>ShowOfficeReconstruction(state));
    }
    private void ShowRankings(GameState? state=null)
    {
        if (state is not null) PersistPendingRankings(state);
        ClearContent();AddHeading(Tr("GLOBAL_RANKINGS"),26);AnnualMessage("RankingScope",Tr("RANKINGS_SCOPE"));
        try
        {
            var book=RankingStore.Load(RankingsPath);
            for (int i=0;i<6;i++)
            {
                var record=book.Records[i];AddHeading(Tr($"RANKING_{(RankingKind)i}"),20);
                AnnualMessage($"RankingRecord{i}",record.AchievedOn is GameDate date ?
                    $"{record.Company} — {string.Format(Tr("RANKING_DATE"),date.Year,date.Month,date.Week)}\n{record.RecordedOn}" : Tr("RANKING_NOT_REACHED"));
            }
        }
        catch (Exception ex) {AnnualMessage("RankingError",$"{Tr("RANKINGS_LOAD_FAILED")}: {ex.Message}");}
        AddAction("IMPORT_ORIGINAL_RANKINGS",()=>
        {
            var dialog=new FileDialog {FileMode=FileDialog.FileModeEnum.OpenFile,Access=FileDialog.AccessEnum.Filesystem,Filters=["*.GDS, *.gds ; GameRank.GDS"],UseNativeDialog=false};
            AddChild(dialog);dialog.FileSelected+=path=>
            {
                try
                {
                    var imported=RankingStore.ImportOriginal(System.IO.File.ReadAllBytes(path));var book=RankingStore.Load(RankingsPath);
                    for (int i=0;i<6;i++) if (imported.Records[i].AchievedOn is GameDate date &&
                        (book.Records[i].AchievedOn is not GameDate previous || RankingService.Earlier(date,previous))) book.Records[i]=imported.Records[i];
                    RankingStore.Save(RankingsPath,book);ShowRankings(state);_status.Text=Tr("RANKINGS_IMPORTED");
                }
                catch (Exception ex) {_status.Text=$"{Tr("RANKINGS_IMPORT_FAILED")}: {ex.Message}";}
                dialog.QueueFree();
            };
            dialog.Canceled+=()=>dialog.QueueFree();dialog.PopupCenteredRatio(.8f);
        });
        if (state is not null) AddAction("SALES_CHART",()=>ShowSalesChart(state));
        AddAction(state is null ? "BACK_TO_TITLE" : "BACK_TO_OFFICE",()=> {if (state is null) ShowTitleScreen();else ShowOfficeReconstruction(state);});
    }
    private void ShowSalesChart(GameState state)
    {
        ClearContent();AddHeading(Tr("SALES_CHART"),26);
        if (state.Competition is null) AnnualMessage("CompetitionUnknown",Tr("COMPETITION_UNKNOWN"));
        else
        {
            var table=CreateTable("SalesRankingTable",["COMPANY_RANK","GAME_TITLE","UNITS_SOLD","COMPANY_NAME"],[40,220,100,150]);_content.AddChild(table);
            var root=table.CreateItem();int rank=1;
            foreach (int id in CompetitionService.Chart(state))
            {
                var p=state.Competition.Products[id];var row=table.CreateItem(root);row.SetText(0,(rank++).ToString());row.SetText(1,p.Title);
                row.SetText(2,p.UnitsSold.ToString("N0"));row.SetTextAlignment(0,HorizontalAlignment.Right);row.SetTextAlignment(2,HorizontalAlignment.Right);
                row.SetText(3,p.Owner==-1 ? state.Company.Name : _activeCatalog?.RivalCompanies.ElementAtOrDefault(p.Owner) ?? Tr("NOT_YET_DECODED"));
            }
        }
        AddAction("GLOBAL_RANKINGS",()=>ShowRankings(state));AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
}
