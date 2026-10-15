using Godot;
using OpenGDS2.Core;
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
        if(selectedSlot is int nativeSlot && state.Calendar is not null){ShowNativeFacility(state,nativeSlot);return;}
        RefreshDesktop();
        if (_officeRenderer is not null && _officeTexture is not null) _officeRenderer.UpdateTexture(_officeTexture,state);
        ClearContent();IdentifyManagementWindow("FacilitiesOverviewWindow");AddHeading(Tr("COMPANY_FACILITIES"),26);
        if (selectedSlot is not null) CompactManagementWindow("TenantFacilityWindow",new Vector2(410,440),390);
        AddHeading($"{Tr("FUNDS")}: ¥{DisplayNumbers.Format(state.Money)}",18);
        if (state.Calendar is null) AnnualMessage("FacilitiesUnknown",Tr("CALENDAR_LEGACY_SAVE"));
        else for (int slot=0;slot<3;slot++)
        {
            if (selectedSlot is int chosen && slot!=chosen) continue;
            int index=slot,value=state.Calendar.OriginalSlots0444[slot];
            AddHeading(string.Format(DisplayNumbers.Provider,Tr("FACILITY_SLOT"),slot+1)+": "+FacilityName(value),18);
            if (value==-1)
            {
                foreach (FacilityKind kind in Enum.GetValues<FacilityKind>())
                {
                    var chosenKind=kind;bool available=FacilityService.ServiceAvailable(kind);
                    var buy=new Button {Name=kind==FacilityKind.School ? $"OpenSchool{slot}" : $"OpenFacility{slot}_{(int)kind}",Text=$"{FacilityName((int)kind)} — ¥{DisplayNumbers.Format(FacilityService.PurchasePrice(kind))}"+
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
                    case FacilityKind.Casino:
                        AnnualButton($"EnterCasino{slot}","CASINO_ENTER",()=>ShowCasino(state));break;
                    case FacilityKind.HotSpring:
                        FacilityAction($"UseHotSpring{slot}","FACILITY_HOT_SPRING_ACTION",EmployeeCareService.HotSpringCost,
                            ()=>ConfirmFacilityService(state,index,kind,EmployeeCareService.HotSpringCost,()=>FacilityService.HotSpring(state,index,_session.Random)));break;
                    case FacilityKind.Recycling:
                        AnnualButton($"RecycleStock{slot}","FACILITY_RECYCLE_ACTION",()=>
                            ConfirmFacilityService(state,index,kind,checked(FacilityService.StockForRecycling(state)*FacilityService.RecyclingPrice),()=>FacilityService.Recycle(state,index)));break;
                    case FacilityKind.TechnologySchool:
                        AnnualButton($"TechnologyRoster{slot}","FACILITY_STUDY_ACTION",()=>ShowFacilityTechnologyRoster(state,index));break;
                    case FacilityKind.Lawyer:
                        foreach (var target in Enum.GetValues<FeeNegotiationTarget>())
                        {
                            var chosenTarget=target;
                            AnnualMessage($"NegotiationFee{target}",string.Format(DisplayNumbers.Provider,Tr("NEGOTIATION_CURRENT_FEE"),Tr($"NEGOTIATION_TARGET_{target}"),FeeNegotiationService.CurrentFee(state,target)));
                            AnnualButton($"Negotiate{target}{slot}",$"NEGOTIATION_TARGET_{target}",()=>ShowFeeNegotiationConfirmation(state,index,chosenTarget));
                        }
                        break;
                    case FacilityKind.ProductionLab:
                        FacilityAction($"ExpandProduction{slot}","FACILITY_PRODUCTION_ACTION",FacilityService.ProductionExpansionCost,
                            ()=>ConfirmFacilityService(state,index,kind,FacilityService.ProductionExpansionCost,()=>_session.ExpandFacilityProduction(index)));break;
                    case FacilityKind.HardwareLab:
                        var hard=new Button {Name=$"StartHardwarePlan{slot}",Text=Tr("OWN_HARDWARE_START"),
                            Disabled=state.OwnHardware.Launched is not null || state.DevelopmentProjects.Any(p=>p.OriginalRoomState==2)};
                        _content.AddChild(hard);hard.Pressed+=()=>ShowHardwarePlan(state,index);break;
                    case FacilityKind.MysteryLab:
                        if (state.MysteryLab.Progress>0)
                            AnnualMessage("MysteryLabProgress",string.Format(DisplayNumbers.Provider,Tr("MYSTERY_LAB_PROGRESS"),Math.Min(100,state.MysteryLab.Progress)));
                        AnnualMessage("MysteryLabLifecycle",Tr("MYSTERY_LAB_LIFECYCLE"));
                        var experiment=new Button {Name=$"StartMysteryExperiment{slot}",
                            Text=$"{Tr("MYSTERY_LAB_START")} — ¥{DisplayNumbers.Format(MysteryLabService.ExperimentCost)}",
                            Disabled=state.MysteryLab.Progress>0 || FacilityService.HasPendingDecision(state)};
                        _content.AddChild(experiment);experiment.Pressed+=()=>ConfirmFacilityService(state,index,kind,
                            MysteryLabService.ExperimentCost,()=>_session.StartMysteryExperiment(index));break;
                }
                AnnualButton(kind==FacilityKind.School ? $"CloseSchool{slot}" : $"CloseFacility{slot}","FACILITY_CLOSE",()=>ShowFacilityClosure(state,index,kind));
            }
        }
        AddSaveActions(state);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private void FacilityAction(string name,string key,long amount,Action action)
    {
        var button=new Button {Name=name,Text=$"{Tr(key)} — ¥{DisplayNumbers.Format(amount)}"};_content.AddChild(button);button.Pressed+=action;
    }
    private string FacilityName(int value)=>value==-1 ? Tr("FACILITY_EMPTY") : value is >=0 and <=8 ? Tr($"FACILITY_NAME_{(FacilityKind)value}") : string.Format(DisplayNumbers.Provider,Tr("FACILITY_OTHER"),value);
    private Texture2D? FacilityScene(FacilityKind kind)=>OriginalSceneArtwork.Facility(kind);
    private void FacilityQuestion(GameState state,FacilityKind kind,string question,string name,Action confirm,Action cancel,string confirmName="ConfirmFacilityService",string? prompt=null,bool retainOnConfirm=false)
    {
        RetainFacilityOwner();
        void Cancel(){cancel();}
        void Confirm(){if(!retainOnConfirm)CloseFacilityOwner();confirm();}
        ShowConversation(FacilityName((int)kind),[new(SecretaryCaption(state.SecretarySourceIndex),question,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>
        {
            ClearContent();var canvas=NativeWindow(name,FacilityName((int)kind),new(330,126));
            NativeText(canvas,"FacilityConfirmation",prompt??question,new(8,5,314,76),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
            NativeAction(canvas,confirmName,"CONFIRM",new(126,92,94,26),Confirm);
            NativeAction(canvas,"CancelFacilityService","CANCEL",new(226,92,94,26),Cancel);
            _dialog!.CloseRequested=Cancel;
        },Cancel);
    }
    private void ShowFacilityClosure(GameState state,int slot,FacilityKind kind)
    {
        string question=FacilityText("FACILITY_CLOSE_INTRO").Replace("{facility}",FacilityName((int)kind));
        FacilityQuestion(state,kind,question,"FacilityClosureWindow",()=>
        {
            if(!ReferenceEquals(_session.Current,state))return;
            var result=FacilityService.Close(state,slot,kind);ShowFacilities(state,slot);
            if(result!=FacilityResult.Success)_status.Text=Tr("FACILITY_RESULT_"+result);
        },()=>RestoreFacilityOwner(state),kind==FacilityKind.School?"ConfirmSchoolClosure":"ConfirmFacilityClosure",FacilityText("FACILITY_CLOSE_QUESTION").Replace("{facility}",FacilityName((int)kind)));
    }
    private void ConfirmFacilityService(GameState state,int slot,FacilityKind kind,long amount,Func<FacilityResult> action)
    {
        string question=FacilityText("FACILITY_INTRO_"+kind).Replace("{fee}",amount.ToDisplayNumber())
            .Replace("{stock}",kind==FacilityKind.Recycling?FacilityService.StockForRecycling(state).ToDisplayNumber():"");
        FacilityQuestion(state,kind,question,"FacilityConfirmationWindow",()=>
        {
            if(!ReferenceEquals(_session.Current,state))return;
            var result=action();
            if(result==FacilityResult.Success)
            {if(state.PendingFacilityReceipt is not null)ShowPendingFacilityReceipt(state);else FinishFacility(state);}
            else FacilityFailure(state,kind,result);
        },()=>FinishFacility(state),prompt:FacilityText("FACILITY_QUESTION_"+kind),retainOnConfirm:true);
    }
    private void ShowFacilityTechnologyRoster(GameState state,int slot)=>ShowNativeStudyRoster(state,slot);
    private bool ShowPendingFacilityReceipt(GameState state)
    {
        if(state.PendingFacilityReceipt is not { } receipt)return false;
        EnsureFacilityOwner(state,receipt.Kind,receipt.SourceSlot);
        if(receipt.Kind==FacilityKind.TechnologySchool && _facilityChild is null && receipt.SourceSlot is int studySlot)
        {ShowNativeStudyRoster(state,studySlot,receiptView:true);RetainFacilityChild();}
        int cursor=receipt.PageIndex;
        string employeeName=state.Employees.FirstOrDefault(e=>e.Name==receipt.EmployeeName) is { } recipient?EmployeeName(recipient):receipt.EmployeeName??"";
        string body=FacilityText("FACILITY_DONE_"+receipt.Kind).Replace("{amount}",receipt.Amount.ToDisplayNumber())
            .Replace("{name}",employeeName).Replace("{capacity}",receipt.Gain?.ToDisplayNumber()??"?")
            .Replace("{before}",receipt.CapacityBefore?.ToDisplayNumber()??"?")
            .Replace("{after}",receipt.CapacityBefore is int before ? ((long)before+receipt.Gain!.Value).ToDisplayNumber():"?");
        // Older expansion snapshots never recorded the old capacity.
        if(receipt.Kind==FacilityKind.ProductionLab && receipt.CapacityBefore is null)
            body=string.Format(DisplayNumbers.Provider,Tr("FACILITY_RECEIPT_ProductionLab"),receipt.Amount,receipt.Gain,receipt.EmployeeName);
        ConversationPage page=new(SecretaryCaption(state.SecretarySourceIndex),body,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex));
        if(cursor>0 && receipt.Reactions is { } reactions)
        {
            var reaction=reactions[cursor-1];var employee=state.Employees.SingleOrDefault(e=>e.SourceIndex==reaction.SourceIndex);
            page=new(employee is null?reaction.Name:EmployeeName(employee),FacilityText("HOT_SPRING_REACTION_"+reaction.Variant),reaction.PortraitId);
        }
        ShowConversation(FacilityName((int)receipt.Kind),[page],()=>
        {
            if(!ReferenceEquals(_session.Current,state)||!ReferenceEquals(state.PendingFacilityReceipt,receipt)||receipt.PageIndex!=cursor)return;
            FacilityService.AcknowledgeReceipt(state);
            if(state.PendingFacilityReceipt is null)FinishFacility(state);else ShowPendingFacilityReceipt(state);
        });return true;
    }
    private bool ShowPendingMysteryLabResult(GameState state)
    {
        if (state.PendingMysteryLabResult is not { } pending) return false;
        int cursor=pending.MessageIndex;string key=MysteryLabService.MessageKey(pending);
        string body=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)
            ? Tr(key) : _importer.LoadDialogue()?.Entries.GetValueOrDefault(key) ?? Tr(key);
        ShowConversation(FacilityName((int)FacilityKind.MysteryLab),[new(SecretaryCaption(state.SecretarySourceIndex),body,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>
        {
            if (!ReferenceEquals(_session.Current,state) || !ReferenceEquals(state.PendingMysteryLabResult,pending) || pending.MessageIndex!=cursor) return;
            MysteryLabService.Acknowledge(state);ShowOfficeReconstruction(state);
        },scene:FacilityScene(FacilityKind.MysteryLab));
        return true;
    }
    private void ShowFeeNegotiationConfirmation(GameState state,int slot,FeeNegotiationTarget target)
    {
        RetainFacilityOwner();ClearContent();var canvas=NativeWindow("NegotiationConfirmationWindow",Tr("NEGOTIATION_TARGET_"+target),new(310,152));
        NativeFill(canvas,"NegotiationConfirmationBox",new(4,4,302,91),Colors.White,true);NativeText(canvas,"NegotiationConfirmation",string.Format(DisplayNumbers.Provider,Tr("NEGOTIATION_CONFIRM"),FeeNegotiationService.RequiredFunds),new(8,7,294,85),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeAction(canvas,"ConfirmFeeNegotiation","CONFIRM",new(112,120,94,26),()=>
        {if(!ReferenceEquals(_session.Current,state))return;CloseFacilityOwner();var result=_session.BeginFeeNegotiation(slot,target);if(result==FacilityResult.Success)ShowOfficeReconstruction(state);else{ShowFacilities(state,slot);_status.Text=Tr("FACILITY_RESULT_"+result);}});
        NativeAction(canvas,"CancelFeeNegotiation","CANCEL",new(212,120,94,26),()=>{CloseFacilityOwner();ShowFacilities(state,slot);});
        _dialog!.CloseRequested=()=>{CloseFacilityOwner();ShowFacilities(state,slot);};
    }
    private bool ShowPendingFeeNegotiation(GameState state)
    {
        if (state.PendingFeeNegotiation is not { } pending) return false;
        int cursor=pending.MessageIndex;
        string Text(string key)=>TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)
            ? Tr(key) : _importer.LoadDialogue()?.Entries.GetValueOrDefault(key) ?? Tr(key);
        string speaker=cursor is >=1 and <=3 ? Text(FeeNegotiationService.Key(pending.Target,"SPEAKER")) : SecretaryCaption(state.SecretarySourceIndex);
        string body=Text(FeeNegotiationService.MessageKey(pending)).Replace("{fee}",pending.After.ToString());
        ShowConversation(Tr($"NEGOTIATION_TARGET_{pending.Target}"),[new(speaker,body,cursor is >=1 and <=3 ? 4 : SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>
        {
            if (!ReferenceEquals(_session.Current,state) || !ReferenceEquals(state.PendingFeeNegotiation,pending) || pending.MessageIndex!=cursor) return;
            FeeNegotiationService.Acknowledge(state);ShowOfficeReconstruction(state);
        },scene:OriginalSceneArtwork.BusinessMeeting());
        return true;
    }
    private void ShowTenantPlotChoice(GameState state,IReadOnlyList<int> slots)
    {
        ClearContent();CompactManagementWindow("TenantPlotChoiceWindow",new Vector2(350,230),330);
        AddHeading(Tr("FACILITY_CHOOSE_PLOT"),22);
        foreach (int slot in slots)
        {
            int chosen=slot;var button=new Button {Name=$"ChooseTenantPlot{slot}",Text=string.Format(DisplayNumbers.Provider,Tr("FACILITY_SLOT"),slot+1)};
            _content.AddChild(button);button.Pressed+=()=>ShowFacilities(state,chosen);
        }
        AddAction("CANCEL",()=>ShowOfficeReconstruction(state));
    }
    private void ShowRankings(GameState? state=null)
    {
        if (state is not null) PersistPendingRankings(state);
        var canvas=ManagementCanvas("ReachTimeRecordsWindow",Tr("GLOBAL_RANKINGS"),new(562,242));NativeText(canvas,"RankingScope",Tr("RANKINGS_SCOPE"),new(4,4,554,25),10);
        _status=NativeText(canvas,"RankingStatus","",new(4,185,554,22),10);
        try
        {
            var book=RankingStore.Load(RankingsPath);
            var table=ManagementTable(canvas,"ReachTimeRecords",["RANKING_CATEGORY","COMPANY_NAME","RANKING_REACHED_ON","RANKING_RECORDED_ON"],[184,130,113,106],new(4,33,554,147));
            var root=table.GetRoot();
            for(int i=0;i<6;i++)
            {
                var record=book.Records[i];var row=table.CreateItem(root);row.SetText(0,Tr($"RANKING_{(RankingKind)i}"));row.SetText(1,record.Company);
                row.SetText(2,record.AchievedOn is { } d?$"{d.Year}/{d.Month}/{d.Week}":Tr("RANKING_NOT_REACHED"));row.SetText(3,record.RecordedOn.Length>10?record.RecordedOn[..10]:record.RecordedOn);row.SetTooltipText(3,record.RecordedOn);
            }
        }
        catch(Exception ex){_status.Text=Tr("RANKINGS_LOAD_FAILED")+": "+ex.Message;}
        NativeAction(canvas,"ImportOriginalRankings","IMPORT_ORIGINAL_RANKINGS",new(4,211,254,25),()=>
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
        if(state is not null)NativeAction(canvas,"RankingsSalesChart","SALES_CHART",new(264,211,141,25),()=>ShowSalesChart(state));
        NativeAction(canvas,"CloseRankings",state is null?"BACK_TO_TITLE":"BACK_TO_OFFICE",new(411,211,147,25),()=>{if(state is null)ShowTitleScreen();else ShowOfficeReconstruction(state);});
    }
    private void ShowSalesChart(GameState state)
    {
        ClearContent();var canvas=NativeWindow("SalesRankingWindow",Tr("SALES_CHART"),new(508,287));
        if(state.Competition is null)NativeText(canvas,"CompetitionUnknown",Tr("COMPETITION_UNKNOWN"),new(4,4,500,70),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        else
        {
            var table=ManagementTable(canvas,"SalesRankingTable",["COMPANY_RANK","GAME_TITLE","UNITS_SOLD","COMPANY_NAME"],[35,185,109,140],new(4,4,500,244));
            int? selected=null;
            table.ItemSelected+=()=>{var row=table.GetSelected();if(row is not null)selected=(int)row.GetMetadata(0);};
            var rows=new Dictionary<int,TreeItem>();int[] previous=[];
            void Refresh()
            {
                int[] ids=CompetitionService.Chart(state).ToArray();
                if(!previous.SequenceEqual(ids))
                {
                    Vector2 scroll=table.GetScroll();table.Clear();rows.Clear();var root=table.CreateItem();
                    foreach(int id in ids){var row=table.CreateItem(root);row.SetMetadata(0,id);rows[id]=row;}
                    previous=ids;
                    Callable.From(()=>
                    {
                        if(!GodotObject.IsInstanceValid(table)||!table.IsInsideTree())return;
                        foreach(var bar in table.GetChildren(true).OfType<VScrollBar>())bar.Value=scroll.Y;
                        foreach(var bar in table.GetChildren(true).OfType<HScrollBar>())bar.Value=scroll.X;
                    }).CallDeferred();
                }
                int rank=1;
                foreach(int id in ids)
                {
                    var p=state.Competition.Products[id];var row=rows[id];row.SetText(0,(rank++).ToString());row.SetText(1,RivalTitle(p));row.SetTooltipText(1,RivalTitle(p));
                    row.SetText(2,p.UnitsSold.ToDisplayNumber());row.SetTextAlignment(0,HorizontalAlignment.Right);row.SetTextAlignment(2,HorizontalAlignment.Right);
                    row.SetText(3,p.Owner==-1?state.Company.Name:RivalCompany(p.Owner));row.SetTooltipText(3,row.GetText(3));
                    if(selected==id)row.Select(0);
                }
            }
            _activeCatalog=_importer.LoadCatalog();Refresh();_clockRunning=true;_refreshCoreStatus=Refresh;
        }
        NativeAction(canvas,"SalesChartRecords","GLOBAL_RANKINGS",new(210,258,143,25),()=>ShowRankings(state));NativeAction(canvas,"CloseSalesChart","BACK_TO_OFFICE",new(359,258,145,25),()=>ShowOfficeReconstruction(state));
    }
}
