using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;

public partial class Main
{
    private PoachingState? _shownPoaching;
    private double _poachingElapsed;
    private Action? _refreshPoaching;
    private void ShowPoachingSelection(GameState state)
    {
        var canvas=ManagementCanvas("PoachingSelectionWindow",Tr("POACH_EMPLOYEE"),new(478,335));
        var candidates=PoachingService.Candidates(state).ToArray();
        var table=ManagementTable(canvas,"PoachingCandidates",["EMPLOYEE_NAME","EMPLOYEE_LEVEL","CONTRACT_FEE"],[220,60,190],new(4,4,470,130));
        EmployeeState? selected=null;
        var preview=NativePlace(canvas,new Control {Name="PoachingCandidatePreview"},new(4,139,470,163));
        var confirm=NativeAction(canvas,"ConfirmPoachingTarget","POACH_EMPLOYEE",new(262,306,106,25),()=>{},false);
        NativeAction(canvas,"CancelPoachingSelection","CANCEL",new(374,306,100,25),()=>ShowOfficeReconstruction(state));
        foreach(var candidate in candidates)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,candidate.SourceIndex);
            row.SetText(0,EmployeeName(candidate.Employee));row.SetText(1,candidate.Employee.Level?.ToString()??"?");
            row.SetText(2,candidate.Employee.ContractFee?.ToDisplayNumber()??"?");row.SetTextAlignment(2,HorizontalAlignment.Right);
        }
        void Select()
        {
            if(table.GetSelected() is not { } row)return;
            selected=candidates.Single(s=>s.SourceIndex==row.GetMetadata(0).AsInt32()).Employee;
            foreach(Node child in preview.GetChildren()){preview.RemoveChild(child);child.QueueFree();}
            EmployeePreview(preview,selected,"Poaching",new(0,0,470,163));confirm.Disabled=state.Money is null;
        }
        table.ItemSelected+=Select;table.GetRoot().GetFirstChild()?.Select(0);Select();
        if(candidates.Length==0)NativeText(canvas,"PoachingNoCandidates",Tr("NO_POACHING_TARGETS"),new(8,143,450,24));
        confirm.Pressed+=()=>{if(selected?.SourceIndex is int source && PoachingService.Begin(state,source))ShowPendingCompanyEvent(state);};
    }
    private bool ShowPendingPoaching(GameState state)
    {
        if(state.PendingPoaching is not { } scene){_shownPoaching=null;_refreshPoaching=null;return false;}
        var target=state.EmployeeSources.Single(s=>s.SourceIndex==scene.SourceIndex).Employee;
        string name=EmployeeName(target);
        string Text(string key)=>DialogueText(key).Replace("{name}",name).Replace("{company}",state.Company.Name).Replace("{amount}",DisplayNumbers.Format(scene.PendingDonation));
        if(scene.Phase==PoachingPhase.Confirmation)
        {
            var canvas=ManagementCanvas("PoachingConfirmationWindow",Tr("POACH_EMPLOYEE"),new(330,122));
            NativeText(canvas,"PoachingQuestion",Text("POACHING_CONFIRM"),new(8,8,314,67),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
            NativeAction(canvas,"StartPoachingNegotiation","CONFIRM",new(128,88,94,25),()=>{PoachingService.Confirm(state);ShowPendingPoaching(state);});
            NativeAction(canvas,"CancelPoachingConfirmation","CANCEL",new(228,88,94,25),()=>{PoachingService.Cancel(state);ShowOfficeReconstruction(state);});return true;
        }
        if(scene.Phase!=PoachingPhase.Negotiating)
        {
            _refreshPoaching=null;
            string key=scene.Phase switch
            {
                PoachingPhase.TargetIntroduction=>"POACHING_TARGET_INTRO",
                PoachingPhase.DonationAcknowledgement=>"POACHING_DONATION_ACK",
                PoachingPhase.RivalAlert=>"POACHING_RIVAL_ALERT",PoachingPhase.DefenceIntroduction=>"POACHING_DEFENCE_INTRO",
                PoachingPhase.DecisionIntroduction=>"POACHING_DECISION_INTRO",PoachingPhase.DecisionSuspense=>"POACHING_DECISION_SUSPENSE",
                PoachingPhase.DecisionChoice=>scene.PlayerWins==true?"POACHING_CHOICE_PLAYER":"POACHING_CHOICE_RIVAL",
                _=>$"POACHING_RESULT_{(scene.PlayerWins==true?"WON":"LOST")}_{(scene.Defensive?"DEFENSIVE":"OFFENSIVE")}"
            };
            bool employee=scene.Phase is PoachingPhase.DecisionSuspense or PoachingPhase.DecisionChoice;
            ShowConversation(Tr("POACH_EMPLOYEE"),[new(employee?name:SecretaryCaption(state.SecretarySourceIndex),Text(key),employee?target.PortraitId:null)],()=>
            {
                if(!ReferenceEquals(state.PendingPoaching,scene))return;
                PoachingService.Acknowledge(state,_session.Random);
                if(!ShowPendingPoaching(state))ShowOfficeReconstruction(state);
            });return true;
        }
        if(!ReferenceEquals(_shownPoaching,scene)){_shownPoaching=scene;_poachingElapsed=0;}
        var board=ManagementCanvas("PoachingWindow",Tr("POACH_EMPLOYEE"),new(506,354));
        if(_dialog is not null)_dialog.CloseRequested=()=>{PoachingService.Cancel(state);ShowOfficeReconstruction(state);};
        NativePlace(board,Picture("PoachingEmployeePortrait",OriginalAssetStore.LoadEmployeePortrait(target.PortraitId),new(64,64)),new(4,7,64,64));
        NativeText(board,"PoachingEmployeeName",name,new(76,7,260,20));
        NativePlace(board,new EmployeeAbilityChart {Name="PoachingAbilities",Employee=target,Compact=true},new(348,4,150,130));
        NativeFill(board,"PoachingEmployeeBubble",new(76,31,264,69),Colors.White,true);
        var employeeText=NativeText(board,"PoachingEmployeeSpeech","",new(80,35,256,61),10);employeeText.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        NativeText(board,"PoachingRivalLabel",Tr("POACHING_RIVAL"),new(8,139,238,20));
        NativeText(board,"PoachingPlayerLabel",state.Company.Name,new(256,139,242,20));
        NativePlace(board,Picture("PoachingRivalPortrait",OriginalAssetStore.LoadTexture("Graphic/Embedded/PoachingRival.bmp"),new(64,64)),new(8,176,64,64));
        NativePlace(board,Picture("PoachingPresidentPortrait",OriginalAssetStore.LoadTexture("Graphic/Embedded/PoachingPresident.bmp"),new(64,64)),new(256,176,64,64));
        NativeFill(board,"PoachingRivalBubble",new(80,176,168,64),Colors.White,true);
        NativeFill(board,"PoachingPlayerBubble",new(328,176,170,64),Colors.White,true);
        var rivalText=NativeText(board,"PoachingRivalSpeech","",new(84,180,160,56),10);rivalText.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var playerText=NativeText(board,"PoachingPlayerSpeech","",new(332,180,162,56),10);playerText.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var rivalMoney=NativeText(board,"PoachingRivalAmounts","",new(8,248,240,54),10);
        var playerMoney=NativeText(board,"PoachingPlayerAmounts","",new(256,248,242,54),10);
        var countdown=NativeText(board,"PoachingCountdown","",new(8,322,236,24),11);
        NativeAction(board,"PoachingDonate","POACHING_DONATE",new(264,320,112,25),()=>ShowPoachingAmounts(state,scene,true));
        NativeAction(board,"PoachingRaiseOffer","POACHING_RAISE_OFFER",new(382,320,116,25),()=>ShowPoachingAmounts(state,scene,false));
        _refreshPoaching=()=>
        {
            if(!GodotObject.IsInstanceValid(board)||!ReferenceEquals(state.PendingPoaching,scene))return;
            employeeText.Text=Text("POACHING_EMPLOYEE_MESSAGE_"+scene.EmployeeMessage);
            rivalText.Text=scene.RivalMessage<0?"":Text("POACHING_RIVAL_MESSAGE_"+scene.RivalMessage);
            playerText.Text=scene.PlayerMessage<0?"":Tr(scene.PlayerMessage==0?"POACHING_DONATION_SENT":"POACHING_OFFER_RAISED");
            rivalMoney.Text=$"{Tr("CONTRACT_FEE")}: ¥{DisplayNumbers.Format(scene.RivalOffer)}\n{Tr("POACHING_DONATIONS")}: ¥{DisplayNumbers.Format(scene.RivalDonations)}";
            playerMoney.Text=$"{Tr("CONTRACT_FEE")}: ¥{DisplayNumbers.Format(scene.PlayerOffer)}\n{Tr("POACHING_DONATIONS")}: ¥{DisplayNumbers.Format(scene.PlayerDonations)}";
            countdown.Text=string.Format(DisplayNumbers.Provider,Tr("POACHING_TIME_LEFT"),Math.Max(0,100-scene.TimerCallbacks/2));
        };
        _refreshPoaching();return true;
    }
    private void ShowPoachingAmounts(GameState state,PoachingState scene,bool donation)
    {
        _refreshPoaching=null;
        var canvas=ManagementCanvas(donation?"PoachingDonationWindow":"PoachingContractWindow",Tr(donation?"POACHING_DONATE":"POACHING_RAISE_OFFER"),new(180,175));
        if(_dialog is not null)_dialog.CloseRequested=()=>ShowPendingPoaching(state);
        var amounts=donation?PoachingService.DonationAmounts:PoachingService.ContractIncrements;
        for(int i=0;i<amounts.Length;i++)
        {
            long amount=amounts[i];var button=NativeAction(canvas,"PoachingAmount"+i,"",new(8,7+i*31,164,27),()=>
            {
                if(!ReferenceEquals(state.PendingPoaching,scene))return;
                var result=donation?PoachingService.QueueDonation(state,amount):PoachingService.RaiseOffer(state,amount);
                if(result==PoachingActionResult.Success)ShowPendingPoaching(state);
                else ShowConversation(Tr("POACH_EMPLOYEE"),[new(SecretaryCaption(state.SecretarySourceIndex),Tr("POACHING_ACTION_"+result))],()=>ShowPendingPoaching(state));
            });button.Text=(donation?"":"+")+$"¥{DisplayNumbers.Format(amount)}";
        }
        NativeAction(canvas,"CancelPoachingAmount","CANCEL",new(8,137,164,27),()=>ShowPendingPoaching(state));
    }
    private void TickPoachingInterface(double delta)
    {
        if(_session.Current is not {PendingPoaching:{Phase:PoachingPhase.Negotiating} scene} state || _refreshPoaching is null || _paused)return;
        if(GetTree().GetNodesInGroup("GameplayWindows").OfType<DesktopWindow>().Any(w=>w.IsDragging&&w.IsVisibleInTree())){_poachingElapsed=0;return;}
        _poachingElapsed+=delta;if(_poachingElapsed<.1)return;_poachingElapsed=0;
        PoachingService.Tick(state,_session.Random);
        if(scene.Phase!=PoachingPhase.Negotiating)ShowPendingPoaching(state);else _refreshPoaching?.Invoke();
    }
}
