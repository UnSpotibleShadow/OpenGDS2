using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ReviewRows(GameState state,ReleasedGameState product)
    {
        CompactManagementWindow("MagazineReviewsWindow",new(560,490),540);
        var window=_dialog;
        var scores=product.ReviewScores!;
        var scoreLabels=new Label[4];var scoreImages=new TextureRect[4];
        var reactions=new Label[4];var reactionImages=new TextureRect[4];var comments=new RichTextLabel[4];
        bool english=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        var catalog=_importer.LoadCatalog();
        bool artworkMissing=false;
        for(int i=0;i<4;i++)
        {
            var row=new HBoxContainer {Name=$"ReviewerRow{i}"};row.AddThemeConstantOverride("separation",4);_content.AddChild(row);
            var portrait=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Reviewer{i}.bmp");artworkMissing|=portrait is null;
            var face=Picture($"ReviewerPortrait{i}",portrait,new(64,64));face.TooltipText=$"{Tr("REVIEWER")} {i+1}";row.AddChild(face);
            var reactionCell=new VBoxContainer {CustomMinimumSize=new(18,64)};row.AddChild(reactionCell);
            reactionImages[i]=Picture($"ReviewReactionImage{i}",OriginalAssetStore.LoadTexture($"Graphic/Embedded/ReviewReaction{i}.bmp"),new(18,64));
            reactionImages[i].Visible=!english;reactionCell.AddChild(reactionImages[i]);
            reactions[i]=new Label {Name=$"ReviewReaction{i}",Text="…",HorizontalAlignment=HorizontalAlignment.Center,Visible=english};
            reactions[i].AddThemeFontSizeOverride("font_size",10);reactionCell.AddChild(reactions[i]);
            var number=new VBoxContainer {CustomMinimumSize=new(64,64)};row.AddChild(number);
            scoreImages[i]=Picture($"ReviewScoreImage{i}",OriginalAssetStore.LoadTexture($"Graphic/Embedded/ReviewScore{scores[i]}.png"),new(64,64));
            artworkMissing|=scoreImages[i].Texture is null;number.AddChild(scoreImages[i]);
            scoreLabels[i]=new Label {Name=$"ReviewScore{i}",HorizontalAlignment=HorizontalAlignment.Center,Text="?"};
            scoreLabels[i].AddThemeFontSizeOverride("font_size",48);scoreLabels[i].AddThemeColorOverride("font_color",new(.67f,.79f,.94f));number.AddChild(scoreLabels[i]);
            comments[i]=new RichTextLabel {Name=$"ReviewComment{i}",BbcodeEnabled=false,FitContent=false,
                CustomMinimumSize=new(0,64),SizeFlagsHorizontal=SizeFlags.ExpandFill,ScrollActive=true};row.AddChild(comments[i]);
        }
        var total=new Label {Name="ReviewTotal"};_content.AddChild(total);
        var assessment=new Label {Name="ReviewAssessment",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(assessment);
        var sequel=new Label {Name="ReviewSequelEligibility",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(sequel);
        if(artworkMissing)_content.AddChild(new Label {Text=Tr("REVIEW_ARTWORK_REIMPORT"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try{Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch(Exception ex){_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
        var acknowledge=new Button {Name="AcknowledgeReviews",Text=Tr("CONTINUE")};_content.AddChild(acknowledge);
        acknowledge.Pressed+=()=>
        {
            if(acknowledge.Disabled)return;
            ReviewService.Acknowledge(state,product);
            if(!ShowPendingProductEvent(state))ShowSales(state);
        };
        string Comment(int row)
        {
            if(product.ReviewCommentVariants is not { } variants)return Tr("REVIEW_COMMENT_UNAVAILABLE");
            string key=$"REVIEW_COMMENT_{ReviewPresentationRules.Band(scores[row])}_{variants[row]}";
            string text=DialogueText(key);if(text==key)return Tr("REVIEW_COMMENT_UNAVAILABLE");
            string genre=catalog?.Genres.SingleOrDefault(x=>x.SourceIndex==product.GenreSourceIndex) is { } g ? CatalogName(catalog!,"GENRE",g.SourceIndex,g.Name) : "?";
            string type=catalog?.GameTypes.SingleOrDefault(x=>x.SourceIndex==product.GameTypeSourceIndex) is { } t ? CatalogName(catalog!,"TYPE",t.SourceIndex,t.Name) : "?";
            return text.Replace("{genre}",genre).Replace("{type}",type).Replace("{company}",state.Company.Name);
        }
        void Refresh()
        {
            for(int i=0;i<4;i++)
            {
                bool final=ReviewPresentationRules.Revealed(product,i);
                scoreImages[i].Visible=final && scoreImages[i].Texture is not null;
                scoreLabels[i].Visible=!scoreImages[i].Visible && ReviewPresentationRules.Previewed(product,i);scoreLabels[i].Text=final ? scores[i].ToString() : "?";
                comments[i].Text=final ? Comment(i) : "";
                // Score 9 has no final reaction write. Retain the initial row image/text.
                if(final && ReviewPresentationRules.Reaction(scores[i]) is int source)
                {
                    if(english)reactions[i].Text=string.Join("\n",Tr($"REVIEW_REACTION_{source}").Select(c=>c.ToString()));
                    else reactionImages[i].Texture=OriginalAssetStore.LoadTexture($"Graphic/Embedded/ReviewReaction{source}.bmp");
                }
            }
            bool done=ReviewPresentationRules.Finished(product);int sum=scores.Sum();
            total.Text=done ? $"{Tr("REVIEW_TOTAL")}: {sum} / 40" : "";
            assessment.Text=done ? Tr(sum>=32 ? "REVIEW_HALL_OF_FAME" : sum>=24 ? "REVIEW_GOOD" : sum>=12 ? "REVIEW_LOW" : "REVIEW_POOR") : "";
            sequel.Text=!done ? "" : sum>=32 && !product.HasReleasedSequel ? Tr("SEQUEL_UNLOCKED") : sum<32 && product.OriginalMode==1 ? Tr("SEQUEL_SERIES_ENDED") : "";
            acknowledge.Disabled=!done;
        }
        Refresh();
        if(!ReviewPresentationRules.Finished(product))AdvanceReviewRows(state,product,window!,Refresh);
    }
    private async void AdvanceReviewRows(GameState state,ReleasedGameState product,DesktopWindow window,Action refresh)
    {
        while(!ReviewPresentationRules.Finished(product))
        {
            // Modern display pacing; original callback boundaries are retained,
            // but native Windows timer delivery/cosmetic RNG is not benchmarked.
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state) || !ReferenceEquals(_dialog,window) ||
                !GodotObject.IsInstanceValid(window) || !window.IsInsideTree())return;
            ReviewPresentationRules.Advance(product);refresh();
        }
    }
}
