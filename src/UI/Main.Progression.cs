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
        if (selectedSlot is not null) CompactManagementWindow("TenantFacilityWindow",new Vector2(350,330),330);
        AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",18);
        AnnualMessage("SchoolDescription",Tr("SCHOOL_DESCRIPTION"));
        if (state.Calendar is null) AnnualMessage("FacilitiesUnknown",Tr("CALENDAR_LEGACY_SAVE"));
        else for (int slot=0;slot<3;slot++)
        {
            if (selectedSlot is int chosen && slot!=chosen) continue;
            int index=slot,value=state.Calendar.OriginalSlots0444[slot];
            AddHeading(string.Format(Tr("FACILITY_SLOT"),slot+1)+": "+FacilityName(value),18);
            if (value==-1)
            {
                var buy=new Button {Name=$"OpenSchool{slot}",Text=$"{Tr("OPEN_GAME_SCHOOL")} — ¥{SchoolService.PurchasePrice:N0}",Disabled=state.Calendar.OriginalSlots0444.Contains(0)};
                _content.AddChild(buy);buy.Pressed+=()=>
                {var result=SchoolService.Open(state,index);ShowFacilities(state,selectedSlot);_status.Text=Tr($"SCHOOL_RESULT_{result}");};
            }
            else if (value==0)
                AnnualButton($"CloseSchool{slot}","CLOSE_GAME_SCHOOL",()=>ShowSchoolClosure(state,index));
        }
        AddSaveActions(state);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private string FacilityName(int value)=>value switch {-1=>Tr("FACILITY_EMPTY"),0=>Tr("GAME_SCHOOL"),_=>string.Format(Tr("FACILITY_OTHER"),value)};
    private void ShowSchoolClosure(GameState state,int slot)
    {
        ClearContent();AddHeading(Tr("CLOSE_GAME_SCHOOL"),26);AnnualMessage("SchoolCloseConfirmation",Tr("SCHOOL_CLOSE_CONFIRMATION"));
        AnnualButton("ConfirmSchoolClosure","CONFIRM",()=>
        {var result=SchoolService.Close(state,slot);ShowFacilities(state);_status.Text=Tr($"SCHOOL_RESULT_{result}");});
        AddAction("CANCEL",()=>ShowFacilities(state));
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
