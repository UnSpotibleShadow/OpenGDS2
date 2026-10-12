using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ExhibitionScene(GameState state,ExhibitionState exhibition)
    {
        ClearContent();CompactManagementWindow("ExhibitionAttendanceWindow",new(420,400),400);_dialog?.SetTitle(Tr("GAME_EXHIBITION"));
        var booth=OriginalAssetStore.LoadTexture("Graphic/Embedded/ExhibitionBooth.png");
        _content.AddChild(Picture("ExhibitionBoothPicture",booth,new(256,160)));
        if(booth is null)_content.AddChild(new Label {Text=Tr("EVENT_ARTWORK_REIMPORT"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        var visitors=new Label {Name="ExhibitionVisitors"};_content.AddChild(visitors);
        _content.AddChild(new Label {Name="ExhibitionPreviousRecord",Text=Tr("EXHIBITION_PREVIOUS_RECORD")+": "+
            (exhibition.PreviousVisitorRecord?.ToString("N0") ?? "?")});
        var result=new Label {Name="ExhibitionResult",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(result);
        _status=new Label();_content.AddChild(_status);
        var close=new Button {Name="CloseExhibition",Text=Tr("CONTINUE")};_content.AddChild(close);
        close.Pressed+=()=>{if(!close.Disabled){ExhibitionService.Close(state);ShowOfficeReconstruction(state);}};
        void Refresh()
        {
            visitors.Text=$"{Tr("EXHIBITION_VISITORS")}: {ExhibitionService.DisplayVisitors(exhibition):N0}";
            bool done=exhibition.DisplayCallbacks is null or >=90;
            result.Text=done ? string.Format(Tr("EXHIBITION_RESULT"),exhibition.Visitors,exhibition.ReportedAwarenessGain) : "";
            close.Disabled=!done;
        }
        Refresh();
        if(exhibition.DisplayCallbacks is <90)AdvanceExhibitionScene(state,exhibition,_dialog!,Refresh);
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
    private void AwardScene(GameState state,AwardCeremonyState awards)
    {
        ClearContent();CompactManagementWindow("AwardCeremonyWindow",new(700,500),680);_dialog?.SetTitle(Tr("ANNUAL_GAME_AWARDS"));
        var row=new HBoxContainer();_content.AddChild(row);
        var stage=OriginalAssetStore.LoadTexture("Graphic/Embedded/AwardsStage.png");
        row.AddChild(Picture("AwardStagePicture",stage,new(256,160)));
        var results=new VBoxContainer {Name="AwardCategoryRows",SizeFlagsHorizontal=SizeFlags.ExpandFill};row.AddChild(results);
        GameAward[] order=[GameAward.GrandPrize,GameAward.Design,GameAward.Music,GameAward.Graphics,GameAward.Worst];
        Color[] colors=[new(1,1,.7f),new(1,.85f,.68f),new(.68f,1,1),new(1,.7f,1),new(1,.42f,.54f)];
        for(int i=0;i<order.Length;i++)
        {
            var category=order[i];var box=new PanelContainer {Name=$"AwardCategory{category}"};
            box.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=colors[i],ContentMarginLeft=4,ContentMarginRight=4});results.AddChild(box);
            var line=new HBoxContainer();box.AddChild(line);
            line.AddChild(Picture($"AwardTrophy{category}",OriginalAssetStore.LoadTexture($"Graphic/Embedded/Trophy{AwardService.OriginalCode(category)-1}.bmp"),new(40,40)));
            var text=new Label {Name=$"AwardWinner{category}",AutowrapMode=TextServer.AutowrapMode.WordSmart,SizeFlagsHorizontal=SizeFlags.ExpandFill};line.AddChild(text);
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
            text.Text=Tr($"AWARD_{category}")+"\n"+winner;
        }
        _status=new Label();_content.AddChild(_status);
        if(stage is null)_content.AddChild(new Label {Text=Tr("EVENT_ARTWORK_REIMPORT"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
    }
}
