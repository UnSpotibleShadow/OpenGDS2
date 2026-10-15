using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private DesktopWindow? _awardBoard;
    private AwardCeremonyState? _awardBoardState;
    private void CloseAwardBoard()
    {
        if(_awardBoard is { } board&&GodotObject.IsInstanceValid(board))
        {_dialogPositions["AwardCeremonyWindow"]=board.Position;board.GetParent()?.RemoveChild(board);board.QueueFree();}
        _awardBoard=null;_awardBoardState=null;
    }
    private void EnsureAwardBoard(GameState state,AwardCeremonyState awards)
    {
        if(_awardBoard is null||!GodotObject.IsInstanceValid(_awardBoard)||!ReferenceEquals(_awardBoardState,awards))
        {
            CloseAwardBoard();AwardScene(state,awards,42);_awardBoard=_dialog;_awardBoardState=awards;
            var canvas=(Control)_content.GetChild(0);NativeText(canvas,"AwardEntries",string.Format(DisplayNumbers.Provider,Tr("AWARD_ENTRIES"),awards.Entries.Count),new(8,174,246,36),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
            NativeText(canvas,"AwardResult",Tr("AWARD_"+AwardService.Order[awards.CategoryIndex]),new(8,211,246,23),10);
            _awardBoard!.CloseRequested=()=>{};_dialog=null;_dialogPositionKey=null;
        }
        foreach(var category in AwardService.Order)
        {
            var outcome=awards.Outcomes?.SingleOrDefault(o=>o.Category==category);
            if(category==AwardService.Order[awards.CategoryIndex]&&awards.PresentationPhase<3)outcome=null;
            var text=_awardBoard.FindChild("AwardWinner"+category,true,false) as Label;
            if(text is null)continue;string winner="—";
            if(outcome?.ProductIndex is int own)winner=state.ReleasedGames.Single(p=>p.ProductIndex==own).Title+"\n"+state.Company.Name;
            else if(outcome?.RivalIndex is int id&&state.Competition is { } competition)
            {var product=competition.Products[id];winner=product.Title+"\n"+(_activeCatalog?.RivalCompanies.ElementAtOrDefault(product.Owner)??"?");}
            else if(outcome is not null)winner=Tr("AWARD_OTHER_COMPANY");
            text.Text=Tr("AWARD_"+category)+"\n"+winner;text.TooltipText=text.Text;
        }
        if(_awardBoard.FindChild("AwardResult",true,false) is Label result)result.Text=Tr("AWARD_"+AwardService.Order[awards.CategoryIndex]);
    }
    private string AwardText(string key)
    {
        if(TranslationServer.GetLocale().StartsWith("en"))return Tr(key);
        return _importer.LoadDialogue()?.Entries.GetValueOrDefault(key)??Tr(key);
    }
    private void ShowAwards(GameState state,AwardCeremonyState awards)
    {
        PersistPendingRankings(state);
        // Legacy saves prepared an outcome before this presentation existed.
        if(awards.ResultReady==true&&awards.PresentationPhase==0)awards.PresentationPhase=3;
        if(awards.PresentationPhase==0&&awards.Entries.Count==0)awards.PresentationPhase=1;
        EnsureAwardBoard(state,awards);int category=awards.CategoryIndex;string key;bool secretary=false;
        switch(awards.PresentationPhase)
        {
            case 0:key="AWARD_INTRO_COUNT";secretary=true;break;
            case 1:
                if(category==0&&awards.PresentationPage==0)key="AWARD_WELCOME";
                else
                {
                    int page=awards.PresentationPage-(category==0?1:0);
                    key=page==0?"AWARD_INTRO_"+category:page==1?"AWARD_PRIZE_"+category:"AWARD_GRAND_SUSPENSE";
                }
                break;
            case 2:ShowAwardAnnouncement(state,awards);return;
            case 3:key="AWARD_WINNER_"+category;break;
            case 4:key=awards.PresentationPage==3?"AWARD_RANKING":$"AWARD_REACTION_{category}_{awards.PresentationPage}";secretary=true;break;
            case 5:
                if(category==4){awards.PresentationPhase=6;awards.PresentationPage=0;ShowAwards(state,awards);}
                else{AwardService.Continue(state);ShowAwards(state,awards);}return;
            case 6:key="AWARD_CLOSING_"+awards.PresentationPage;break;
            default:throw new InvalidOperationException("Invalid award presentation.");
        }
        string text=AwardText(key).Replace("{year}",awards.Year.ToString()).Replace("{count}",awards.Entries.Count.ToString());
        if(awards.PresentationPhase==3)
        {
            string title=Tr("UNKNOWN"),company=Tr("AWARD_OTHER_COMPANY");
            if(awards.WinningProduct is int own){title=state.ReleasedGames.Single(p=>p.ProductIndex==own).Title;company=state.Company.Name;}
            else if(awards.WinningRival is int id&&state.Competition is { } competition){var product=competition.Products[id];title=product.Title;company=_activeCatalog?.RivalCompanies.ElementAtOrDefault(product.Owner)??Tr("UNKNOWN");}
            text=text.Replace("{title}",title).Replace("{company}",company);
        }
        int phase=awards.PresentationPhase,pageIndex=awards.PresentationPage;
        ShowConversation(Tr("ANNUAL_GAME_AWARDS"),[new(secretary?DialogueText("CORE_SECRETARY"):AwardText("AWARD_HOST"),text,secretary&&phase==0?null:2)],()=>
        {
            if(!ReferenceEquals(state.Calendar?.Pending?.Awards,awards)||awards.PresentationPhase!=phase||awards.PresentationPage!=pageIndex)return;
            switch(phase)
            {
                case 0:awards.PresentationPhase=1;awards.PresentationPage=0;break;
                case 1:
                    awards.PresentationPage++;int count=category==0||category==4?3:2;
                    if(awards.PresentationPage==count){awards.PresentationPhase=2;awards.PresentationPage=0;}break;
                case 3:awards.PresentationPhase=awards.WinningProduct is null?5:4;awards.PresentationPage=0;break;
                case 4:if(++awards.PresentationPage==(awards.FirstGrandPrizePresentation?4:3)){awards.PresentationPhase=5;awards.PresentationPage=0;}break;
                case 6:
                    if(++awards.PresentationPage==3){AwardService.Continue(state);CloseAwardBoard();ShowOfficeReconstruction(state);return;}break;
            }
            ShowAwards(state,awards);
        });
        _dialog!.SetMeta("award_phase",phase);_dialog.SetMeta("award_category",category);
    }
    private void ShowAwardAnnouncement(GameState state,AwardCeremonyState awards)
    {
        if(awards.ResultReady is null)
        {
            bool first=awards.CategoryIndex==4&&state.Calendar!.FirstGrandPrizeDate is null;
            if(!AwardService.Reveal(state,_session.Random))throw new InvalidOperationException("Award outcome cannot be prepared.");
            awards.FirstGrandPrizePresentation=first&&awards.WinningProduct is not null;
        }
        ClearContent();var canvas=NativeWindow("AwardAnnouncementWindow",Tr("ANNUAL_GAME_AWARDS"),new(250,58));
        var text=NativeText(canvas,"AwardAnnouncementText","",new(4,4,242,50),14,HorizontalAlignment.Center);text.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        _dialog!.CloseRequested=()=>{};AdvanceAwardAnnouncement(state,awards,_dialog,text);
    }
    private async void AdvanceAwardAnnouncement(GameState state,AwardCeremonyState awards,DesktopWindow window,Label text)
    {
        int count=awards.AnimationProducts?.Count??1;
        while(awards.ReelIndex<count)
        {
            if(awards.AnimationProducts is { } reel&&state.Competition is { } competition)text.Text=competition.Products[reel[awards.ReelIndex]].Title;
            else text.Text=Tr("AWARD_"+AwardService.Order[awards.CategoryIndex]);
            await ToSignal(GetTree().CreateTimer(.08),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state)||!ReferenceEquals(_dialog,window)||!GodotObject.IsInstanceValid(window)||!ReferenceEquals(state.Calendar?.Pending?.Awards,awards))return;
            awards.ReelIndex++;
        }
        awards.PresentationPhase=3;awards.PresentationPage=0;ShowAwards(state,awards);
    }
}
