using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowNativeFacility(GameState state,int slot)
    {
        int type=state.Calendar!.OriginalSlots0444[slot];if(type==-1){ShowNativeFacilityPicker(state,slot);return;}
        if(type is <0 or >8)return;var kind=(FacilityKind)type;ClearContent();
        bool law=kind==FacilityKind.Lawyer;bool fee=kind is FacilityKind.HotSpring or FacilityKind.TechnologySchool or FacilityKind.ProductionLab or FacilityKind.MysteryLab;
        var canvas=NativeWindow("Facility"+kind+"Window",FacilityName(type),new(270,law?318:fee?279:259));
        var art=FacilityScene(kind);
        NativePlace(canvas,Picture("FacilityIllustration",art,new(256,160)),new(7,3,256,160));
        if(art is null)NativeText(canvas,"FacilityArtworkMissing",Tr("EVENT_ARTWORK_REIMPORT"),new(11,50,248,80),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeFill(canvas,"FacilityDescriptionPanel",new(5,170,260,49),Colors.White,true);
        var description=NativeText(canvas,"FacilityDescription"+slot,Tr("FACILITY_DESCRIPTION_"+kind),new(8,172,254,45),10);_status=description;description.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        int y=224;
        if(law)
        {
            NativeText(canvas,"NegotiationServiceFee",string.Format(DisplayNumbers.Provider,Tr("NEGOTIATION_SERVICE_FEE"),FeeNegotiationService.RequiredFunds),new(5,y,260,20),10);y+=24;
            foreach(var target in Enum.GetValues<FeeNegotiationTarget>())
            {var choice=target;NativeAction(canvas,$"Negotiate{target}{slot}","NEGOTIATION_TARGET_"+target,new(target==FeeNegotiationTarget.Transport?64:166,y,99,25),()=>ShowFeeNegotiationConfirmation(state,slot,choice));}
            y+=32;
        }
        else if(fee)
        {
            long cost=kind switch {FacilityKind.HotSpring=>EmployeeCareService.HotSpringCost,FacilityKind.TechnologySchool=>FacilityService.TechnologyStudyCost,FacilityKind.ProductionLab=>FacilityService.ProductionExpansionCost,_=>MysteryLabService.ExperimentCost};
            NativeText(canvas,"FacilityServiceFee",kind==FacilityKind.MysteryLab&&state.MysteryLab.Progress>0?string.Format(DisplayNumbers.Provider,Tr("MYSTERY_LAB_PROGRESS"),Math.Min(100,state.MysteryLab.Progress)):Tr("FACILITY_SERVICE_FEE").ToString().Replace("{fee}",cost.ToDisplayNumber()),new(5,y,260,19),10);y+=20;
        }
        NativeAction(canvas,"CloseFacilityView","BACK_TO_OFFICE",new(5,y,82,26),()=>ShowOfficeReconstruction(state));
        NativeAction(canvas,kind==FacilityKind.School?$"CloseSchool{slot}":$"CloseFacility{slot}","FACILITY_CLOSE",new(181,y,84,26),()=>ShowFacilityClosure(state,slot,kind));
        if(law||kind==FacilityKind.School)return;
        string key=kind switch {FacilityKind.HotSpring=>"FACILITY_HOT_SPRING_ACTION",FacilityKind.Recycling=>"FACILITY_RECYCLE_ACTION",FacilityKind.TechnologySchool=>"FACILITY_STUDY_ACTION",FacilityKind.Casino=>"CASINO_ENTER",FacilityKind.ProductionLab=>"FACILITY_PRODUCTION_ACTION",FacilityKind.HardwareLab=>"OWN_HARDWARE_START",_=>"MYSTERY_LAB_START"};
        string id=kind switch {FacilityKind.HotSpring=>$"UseHotSpring{slot}",FacilityKind.Recycling=>$"RecycleStock{slot}",FacilityKind.TechnologySchool=>$"TechnologyRoster{slot}",FacilityKind.Casino=>$"EnterCasino{slot}",FacilityKind.ProductionLab=>$"ExpandProduction{slot}",FacilityKind.HardwareLab=>$"StartHardwarePlan{slot}",_=>$"StartMysteryExperiment{slot}"};
        var service=NativeAction(canvas,id,key,new(91,y,86,26),()=>
        {
            switch(kind)
            {
                case FacilityKind.HotSpring:ConfirmFacilityService(state,slot,kind,EmployeeCareService.HotSpringCost,()=>FacilityService.HotSpring(state,slot,_session.Random));break;
                case FacilityKind.Recycling:ConfirmFacilityService(state,slot,kind,checked(FacilityService.StockForRecycling(state)*FacilityService.RecyclingPrice),()=>FacilityService.Recycle(state,slot));break;
                case FacilityKind.TechnologySchool:ShowFacilityTechnologyRoster(state,slot);break;
                case FacilityKind.Casino:ShowCasino(state);break;
                case FacilityKind.ProductionLab:ConfirmFacilityService(state,slot,kind,FacilityService.ProductionExpansionCost,()=>_session.ExpandFacilityProduction(slot));break;
                case FacilityKind.HardwareLab:
                    if(state.OwnHardware.Launched is not null)FacilityMessage(state,kind,FacilityText("FACILITY_HARDWARE_LIMIT"),()=>RestoreFacilityOwner(state));
                    else if(state.DevelopmentProjects.Any(p=>p.OriginalRoomState==2))FacilityMessage(state,kind,FacilityText("FACILITY_HARDWARE_BUSY"),()=>RestoreFacilityOwner(state));
                    else ShowHardwarePlan(state,slot);break;
                case FacilityKind.MysteryLab:
                    if(state.MysteryLab.Progress>0){FacilityMessage(state,kind,FacilityText("FACILITY_MYSTERY_BUSY"),()=>FinishFacility(state));break;}
                    ConfirmFacilityService(state,slot,kind,MysteryLabService.ExperimentCost,()=>_session.StartMysteryExperiment(slot));break;
            }
        });

    }
    private void ShowNativeFacilityPicker(GameState state,int slot)
    {
        ClearContent();var canvas=NativeWindow("FacilityPickerWindow",Tr("COMPANY_FACILITIES"),new(300,281));
        var table=CreateTable("FacilityPickerTable",["EMPLOYEE_NAME","CONTRACT_FEE"],[169,108]);table.ScrollHorizontalEnabled=false;StyleSelection(table,new(.72f,1,.72f));NativePlace(canvas,table,new(4,4,292,166));
        NativeFill(canvas,"FacilityPickerDescriptionPanel",new(4,176,292,61),Colors.White,true);var text=NativeText(canvas,"FacilityPickerDescription","",new(7,178,286,57),10);text.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        _status=NativeText(canvas,"FacilityPickerStatus","",new(4,237,292,16),9);FacilityKind? selected=null;
        var buy=NativeAction(canvas,$"OpenSchool{slot}","FACILITY_BUY",new(80,255,94,24),()=>{});NativeAction(canvas,"CancelFacilityPicker","CANCEL",new(180,255,116,24),()=>ShowOfficeReconstruction(state));
        void Select(){if(table.GetSelected() is not { } row)return;selected=(FacilityKind)row.GetMetadata(0).AsInt32();text.Text=Tr("FACILITY_DESCRIPTION_"+selected);buy.Name=selected==FacilityKind.School?$"OpenSchool{slot}":$"OpenFacility{slot}_{(int)selected}";buy.Disabled=FacilityService.HasPendingDecision(state);}
        table.ItemSelected+=Select;
        foreach(var kind in Enum.GetValues<FacilityKind>()){var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,(int)kind);row.SetText(0,FacilityName((int)kind));row.SetText(1,$"¥{DisplayNumbers.Format(FacilityService.PurchasePrice(kind))}");row.SetTextAlignment(1,HorizontalAlignment.Right);if(kind==FacilityKind.School)row.Select(0);}
        Select();buy.Pressed+=()=>{if(buy.Disabled||selected is not { } kind)return;var result=FacilityService.Open(state,slot,kind);
            if(result==FacilityResult.Success){ShowFacilities(state,slot);return;}
            RetainFacilityChild();
            string body=result==FacilityResult.InsufficientFunds?FacilityText("FACILITY_NO_FUNDS"):
                result==FacilityResult.AlreadyOwned?FacilityText("FACILITY_ALREADY_OWNED").Replace("{facility}",FacilityName((int)kind)):Tr("FACILITY_RESULT_"+result);
            ShowConversation(Tr("COMPANY_FACILITIES"),[new(SecretaryCaption(state.SecretarySourceIndex),body,SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],()=>RestoreFacilityChild(state));};
    }
    private void ShowNativeStudyRoster(GameState state,int slot,bool receiptView=false)
    {
        var roster=receiptView?state.Employees:FacilityService.OpenTechnologyRoster(state,slot);
        if(roster is null){EnsureFacilityOwner(state,FacilityKind.TechnologySchool,slot);FacilityFailure(state,FacilityKind.TechnologySchool,FacilityResult.StateUnknown,()=>RestoreFacilityOwner(state));return;}
        if(roster.Count==0){FacilityFailure(state,FacilityKind.TechnologySchool,FacilityResult.NoEmployees,()=>RestoreFacilityOwner(state));return;}
        RetainFacilityOwner();ClearContent();var canvas=NativeWindow("FacilityStudyWindow",Tr("FACILITY_STUDY_ACTION"),new(318,226));
        var table=CreateTable("FacilityStudyTable",["EMPLOYEE_NAME","TECHNOLOGY_CAPACITY"],[212,82]);table.ScrollHorizontalEnabled=false;StyleSelection(table,new(.72f,1,.72f));NativePlace(canvas,table,new(4,4,310,155));int? selected=null;
        NativeText(canvas,"FacilityStudyFee",Tr("FACILITY_SERVICE_FEE").ToString().Replace("{fee}",FacilityService.TechnologyStudyCost.ToDisplayNumber()),new(4,164,310,21),10);
        var study=NativeAction(canvas,"ConfirmFacilityStudy","FACILITY_STUDY_ACTION",new(110,194,100,26),()=>{},false);NativeAction(canvas,"CancelFacilityStudy","CANCEL",new(216,194,98,26),()=>RestoreFacilityOwner(state));
        _dialog!.CloseRequested=()=>RestoreFacilityOwner(state);
        table.ItemSelected+=()=>{if(table.GetSelected() is { } row){selected=row.GetMetadata(0).AsInt32();study.Disabled=false;study.Name="StudyCapacity"+selected;}};
        foreach(var person in roster??[]){var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,person.SourceIndex!.Value);row.SetText(0,EmployeeName(person));row.SetText(1,person.MaximumTechnologies+" → "+(person.MaximumTechnologies+1));}
        study.Pressed+=()=>{if(selected is not int source)return;var person=state.Employees.Single(e=>e.SourceIndex==source);var result=FacilityService.Study(state,slot,person);
            RetainFacilityChild();
            if(result==FacilityResult.Success)ShowPendingFacilityReceipt(state);
            else FacilityFailure(state,FacilityKind.TechnologySchool,result,()=>RestoreFacilityChild(state));};
    }
}
