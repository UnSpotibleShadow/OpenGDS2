using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ExhibitionScene(GameState state,ExhibitionState exhibition)
    {
        ClearContent();var canvas=NativeWindow("ExhibitionAttendanceWindow",Tr("GAME_EXHIBITION"),new(300,238));
        var booth=OriginalAssetStore.LoadTexture("Graphic/Embedded/ExhibitionBooth.png");
        NativePlace(canvas,Picture("ExhibitionBoothPicture",booth,new(256,160)),new(22,4,256,160));
        NativeText(canvas,"ExhibitionVisitorsCaption",Tr("EXHIBITION_VISITORS")+":",new(4,167,236,19),10);
        var visitors=NativeText(canvas,"ExhibitionVisitors","",new(241,167,55,19),10,HorizontalAlignment.Right);
        NativeText(canvas,"ExhibitionRecordCaption",Tr("EXHIBITION_PREVIOUS_RECORD")+":",new(4,186,236,18),10);
        NativeText(canvas,"ExhibitionPreviousRecord",exhibition.PreviousVisitorRecord?.ToString("N0")??"?",new(241,186,55,18),10,HorizontalAlignment.Right);
        var close=NativeAction(canvas,"CloseExhibition","CONTINUE",new(100,209,100,25),()=>{});
        close.Pressed+=()=>{if(!close.Disabled){ExhibitionService.Close(state);ShowOfficeReconstruction(state);}};
        void Refresh()
        {
            visitors.Text=$"{ExhibitionService.DisplayVisitors(exhibition):N0}";
            bool done=exhibition.DisplayCallbacks is null or >=90;
            visitors.TooltipText=done?string.Format(Tr("EXHIBITION_RESULT"),exhibition.Visitors,exhibition.ReportedAwarenessGain):visitors.Text;
            close.Disabled=!done;
        }
        Refresh();if(exhibition.DisplayCallbacks is <90)AdvanceExhibitionScene(state,exhibition,_dialog!,Refresh);
    }
    private async void AdvanceExhibitionScene(GameState state,ExhibitionState exhibition,DesktopWindow window,Action refresh)
    {
        while(exhibition.DisplayCallbacks is <90)
        {
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state) || !ReferenceEquals(_dialog,window) || !GodotObject.IsInstanceValid(window) ||
                !ReferenceEquals(state.Calendar?.Pending?.Exhibition,exhibition) || !window.IsInsideTree())return;
            ExhibitionService.AdvanceDisplay(exhibition);refresh();
        }
    }
    private void AwardScene(GameState state,AwardCeremonyState awards,float resultHeight)
    {
        ClearContent();var canvas=NativeWindow("AwardCeremonyWindow",Tr("ANNUAL_GAME_AWARDS"),new(530,Math.Max(250,193+resultHeight+35)));
        var stage=OriginalAssetStore.LoadTexture("Graphic/Embedded/AwardsStage.png");
        NativePlace(canvas,Picture("AwardStagePicture",stage,new(256,160)),new(4,4,256,160));
        NativeFill(canvas,"AwardMessagePanel",new(4,170,252,27+resultHeight),Colors.White,true);
        var results=new VBoxContainer {Name="AwardCategoryRows"};results.AddThemeConstantOverride("separation",2);NativePlace(canvas,results,new(268,4,258,220));
        GameAward[] order=[GameAward.GrandPrize,GameAward.Design,GameAward.Music,GameAward.Graphics,GameAward.Worst];
        Color[] colors=[new(1,1,.7f),new(1,.85f,.68f),new(.68f,1,1),new(1,.7f,1),new(1,.42f,.54f)];
        for(int i=0;i<order.Length;i++)
        {
            var category=order[i];var box=new PanelContainer {Name=$"AwardCategory{category}",CustomMinimumSize=new(258,42)};
            box.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=colors[i],ContentMarginLeft=4,ContentMarginRight=4});results.AddChild(box);
            var line=new HBoxContainer();box.AddChild(line);
            var trophyBox=new PanelContainer {Name=$"AwardTrophyFrame{category}",CustomMinimumSize=new(42,42)};
            var border=new StyleBoxFlat {BgColor=Colors.White,BorderColor=new(.5f,.5f,.5f)};border.SetBorderWidthAll(1);trophyBox.AddThemeStyleboxOverride("panel",border);line.AddChild(trophyBox);
            trophyBox.AddChild(Picture($"AwardTrophy{category}",OriginalAssetStore.LoadTexture($"Graphic/Embedded/Trophy{AwardService.OriginalCode(category)-1}.bmp"),new(40,40)));
            var text=new Label {Name=$"AwardWinner{category}",ClipText=true,SizeFlagsHorizontal=SizeFlags.ExpandFill};line.AddChild(text);
            var outcome=awards.Outcomes?.SingleOrDefault(o=>o.Category==category);
            int position=AwardService.Order.ToList().IndexOf(category);
            if(outcome is null && position==awards.CategoryIndex && awards.ResultReady==true)
                outcome=new(category,awards.WinningProduct,awards.WinningRival);
            string winner=position<awards.CategoryIndex ? "?" : "—";
            if(outcome?.ProductIndex is int product)
                winner=state.ReleasedGames.Single(p=>p.ProductIndex==product).Title+"\n"+state.Company.Name;
            else if(outcome?.RivalIndex is int rival && state.Competition is { } competition)
            {var rivalProduct=competition.Products[rival];winner=rivalProduct.Title+"\n"+(_activeCatalog?.RivalCompanies.ElementAtOrDefault(rivalProduct.Owner) ?? "?");}
            else if(outcome is not null)winner=Tr("AWARD_OTHER_COMPANY");
            text.AddThemeFontSizeOverride("font_size",10);text.Text=Tr($"AWARD_{category}")+"\n"+winner;text.TooltipText=text.Text;
        }
        _status=NativeText(canvas,"AwardStatus",stage is null?Tr("EVENT_ARTWORK_REIMPORT"):"",new(268,227,258,19),10);
    }
}
