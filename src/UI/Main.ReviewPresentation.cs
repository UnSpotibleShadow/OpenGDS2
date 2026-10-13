using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ReviewRows(GameState state,ReleasedGameState product)
    {
        var canvas=NativeWindow("MagazineReviewsWindow",Tr("MAGAZINE_REVIEWS"),new(420,306));
        var window=_dialog;var scores=product.ReviewScores!;
        var scoreLabels=new Label[4];var scoreImages=new TextureRect[4];
        var reactions=new Label[4];var reactionImages=new TextureRect[4];var comments=new RichTextLabel[4];
        bool english=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        var catalog=_importer.LoadCatalog();bool artworkMissing=false;
        for(int i=0;i<4;i++)
        {
            var row=new Control {Name=$"ReviewerRow{i}"};NativePlace(canvas,row,new(4,4+i*69,412,65));
            var portrait=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Reviewer{i}.bmp");artworkMissing|=portrait is null;
            NativeFill(row,$"ReviewerFrame{i}",new(0,0,66,65),Colors.White,true);
            NativePlace(row,Picture($"ReviewerPortrait{i}",portrait,new(64,64)),new(1,0,64,64));
            NativeFill(row,$"ReviewReactionFrame{i}",new(72,0,19,65),Colors.White,true);
            reactionImages[i]=NativePlace(row,Picture($"ReviewReactionImage{i}",OriginalAssetStore.LoadTexture($"Graphic/Embedded/ReviewReaction{i}.bmp"),new(18,64)),new(72,0,18,64));reactionImages[i].Visible=!english;
            reactions[i]=NativeText(row,$"ReviewReaction{i}","…",new(72,0,18,64),9,HorizontalAlignment.Center);reactions[i].Visible=english;
            NativeFill(row,$"ReviewCommentPanel{i}",new(96,0,316,65),Colors.White,true);
            scoreImages[i]=NativePlace(row,Picture($"ReviewScoreImage{i}",OriginalAssetStore.LoadTexture($"Graphic/Embedded/ReviewScore{scores[i]}.png"),new(64,64)),new(97,0,64,64));artworkMissing|=scoreImages[i].Texture is null;
            scoreLabels[i]=NativeText(row,$"ReviewScore{i}","?",new(97,0,64,64),48,HorizontalAlignment.Center);scoreLabels[i].AddThemeColorOverride("font_color",new(.67f,.79f,.94f));
            comments[i]=new RichTextLabel {Name=$"ReviewComment{i}",BbcodeEnabled=false,FitContent=false,ScrollActive=true};
            comments[i].AddThemeColorOverride("default_color",Colors.Black);comments[i].AddThemeFontSizeOverride("normal_font_size",11);
            NativePlace(row,comments[i],new(166,3,241,59));
        }
        _status=NativeText(canvas,"ReviewArtworkStatus",artworkMissing?Tr("REVIEW_ARTWORK_REIMPORT"):"",new(4,282,310,21),10);
        var acknowledge=NativeAction(canvas,"AcknowledgeReviews","CONTINUE",new(320,279,96,24),()=>{});
        acknowledge.Pressed+=()=>
        {
            if(acknowledge.Disabled)return;
            if(product.ReviewPending){product.ReviewSummaryPage=0;ShowReviewSummary(state,product);}
            else ShowCompactProduct(state,product);
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
                comments[i].Text=final ? Comment(i) : "";comments[i].TooltipText=comments[i].Text;
                // Score 9 has no final reaction write. Retain the initial row image/text.
                if(final && ReviewPresentationRules.Reaction(scores[i]) is int source)
                {
                    if(english)reactions[i].Text=string.Join("\n",Tr($"REVIEW_REACTION_{source}").Select(c=>c.ToString()));
                    else reactionImages[i].Texture=OriginalAssetStore.LoadTexture($"Graphic/Embedded/ReviewReaction{source}.bmp");
                }
            }
            bool done=ReviewPresentationRules.Finished(product);int sum=scores.Sum();
            acknowledge.TooltipText=done?$"{Tr("REVIEW_TOTAL")}: {sum}/40":Tr("CONTINUE");
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
