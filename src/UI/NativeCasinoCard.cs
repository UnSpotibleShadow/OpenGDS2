using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public sealed class CasinoCardArtwork
{
    public Texture2D? Frame=OriginalAssetStore.LoadTexture("Graphic/Embedded/CardFrame.bmp");
    public Texture2D? Back=OriginalAssetStore.LoadTexture("Graphic/Embedded/CardBack.bmp");
    public Texture2D?[] Ranks=Enumerable.Range(0,26).Select(i=>OriginalAssetStore.LoadTexture($"Graphic/Embedded/CardRank{i}.bmp")).ToArray();
    public Texture2D?[] Suits=Enumerable.Range(0,4).Select(i=>OriginalAssetStore.LoadTexture($"Graphic/Embedded/CardSuit{i}.bmp")).ToArray();
    public Texture2D?[] Faces=Enumerable.Range(0,3).Select(i=>OriginalAssetStore.LoadTexture($"Graphic/Embedded/CardFace{i}.bmp")).ToArray();
}
/// <summary>Native card components at their recovered offsets; draws no cards or outcomes.</summary>
public partial class NativeCasinoCard : Button
{
    public CasinoCardArtwork Artwork {get;set;}=null!;
    private CasinoCard _card;private bool _known,_exists,_held;
    public void Present(CasinoCard card,bool known,bool exists=true,bool held=false)
    {_card=card;_known=known;_exists=exists;_held=held;Text="";TooltipText=known?$"{new[]{"A","2","3","4","5","6","7","8","9","10","J","Q","K"}[card.Rank]} {new[]{"♥","♠","♦","♣"}[card.Suit]}":"?";SetMeta("revealed",known);SetMeta("held",held);QueueRedraw();}
    public override void _Draw()
    {
        Texture2D? basis=_known||!_exists?Artwork.Frame:Artwork.Back;
        if(basis is null){DrawStyleBox(ClassicTheme.WindowFrame(),new(0,0,71,96));if(_known)DrawString(GetThemeDefaultFont(),new(8,26),TooltipText,HorizontalAlignment.Left,65,13,_card.Suit is 0 or 2?Colors.Red:Colors.Black);}
        else DrawTextureRect(basis,new(0,0,71,96),false);
        if(_known)
        {
            int rank=_card.Rank+(_card.Suit is 0 or 2?13:0),suit=new[]{1,0,3,2}[_card.Suit];
            if(Artwork.Ranks[rank] is { } r)DrawTextureRect(r,new(4,2,20,20),false);
            if(Artwork.Suits[suit] is { } s){DrawTextureRect(s,new(4,22,20,20),false);DrawTextureRect(s,new(16,40,52,52),false);}
            if(_card.Rank>=10&&Artwork.Faces[_card.Rank-10] is { } face)DrawTextureRect(face,new(24,8,44,33),false);
        }
        if(_held)DrawString(GetThemeDefaultFont(),new(6,109),Tr("POKER_HELD"),HorizontalAlignment.Center,59,10,Colors.Black);
    }
}
