using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private CasinoCardArtwork _casinoArtwork=null!;
    private NativeCasinoCard PlaceCasinoCard(Control canvas,string name,int x,int y,Action action)
    {
        var card=new NativeCasinoCard {Name=name,Artwork=_casinoArtwork,TextureFilter=TextureFilterEnum.Nearest};
        foreach(string mode in new[]{"normal","pressed","hover","disabled"})card.AddThemeStyleboxOverride(mode,new StyleBoxEmpty());
        NativePlace(canvas,card,new(x,y,71,112));card.Pressed+=()=>{if(!card.Disabled)action();};return card;
    }
    private void PokerPayTable(Control canvas,bool chain=false)
    {
        NativeFill(canvas,"CasinoPayTable",new(4,28,410,94),Colors.White,true);
        if(chain)
        {
            for(int i=0;i<5;i++){int outcome=9-i;NativeText(canvas,"ChainBonusName"+outcome,Tr("POKER_OUTCOME_"+outcome),new(9,31+i*17,130,17),9);NativeText(canvas,"ChainBonusMultiplier"+outcome,(outcome switch{9 or 8=>20,7=>5,6=>2,_=>1}).ToString(),new(140,31+i*17,25,17),10,HorizontalAlignment.Right);}
            return;
        }
        for(int i=0;i<9;i++)
        {
            int outcome=9-i,x=i<5?9:213,y=31+(i<5?i:i-5)*17;
            NativeText(canvas,"PokerPayLabel"+outcome,Tr("POKER_OUTCOME_"+outcome),new(x,y,157,17),10);
            NativeText(canvas,"PokerPayAmount"+outcome,CasinoPokerService.Multipliers[outcome].ToString(),new(x+158,y,39,17),10,HorizontalAlignment.Right);
        }
    }
    private Control CasinoCanvas(string name,string title,int width,int height)
    {
        ClearContent();_casinoArtwork=new CasinoCardArtwork();var canvas=NativeWindow(name,Tr(title),new(width,height));
        NativeFill(canvas,"CasinoHeadingBox",new(4,2,width-8,22),Colors.White,true);NativeText(canvas,"CasinoHeading",Tr(title),new(6,2,width-12,22),11,HorizontalAlignment.Center);return canvas;
    }
}
