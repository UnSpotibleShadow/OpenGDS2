namespace OpenGDS2.Domain;
/// <summary>Recovered BitBlt coordinates and SRCAND/SRCINVERT operations (M79).</summary>
public static class HorseRasterRules
{
    public const int Width=421,Height=173,CloseupHeight=217,SheetWidth=233,SheetHeight=316;
    public static byte[] Compose(byte[] background,byte[] colors,byte[] masks,HorseRaceState race,
        byte[]? closeupBackground=null,byte[]? finishBackground=null)
    {
        if(background.Length!=Width*Height*3||colors.Length!=SheetWidth*SheetHeight*3||masks.Length!=colors.Length||race.Positions.Count!=8||race.RacePhase is <0 or >2||race.Tick<0||!double.IsFinite(race.Camera)||
            closeupBackground is not null&&closeupBackground.Length!=Width*CloseupHeight*3||
            finishBackground is not null&&finishBackground.Length!=Width*Height*3)throw new InvalidDataException("Invalid race raster data.");
        // Native drawing precedes the tick increment and phase transition.
        int phase=race.RacePhase==1&&race.Tick==45?0:race.RacePhase==2&&race.Tick==200?1:race.RacePhase;
        byte[] scenery=phase switch{1=>closeupBackground??background,2=>finishBackground??background,_=>background};
        byte[] result=scenery.AsSpan(0,Width*Height*3).ToArray();int frame=Math.Max(race.Tick-1,0)%2;
        if(phase==1)
        {
            // The visible copy precedes the background's self-scroll as well.
            int steps=Math.Clamp(race.Tick-46,0,154);
            ScrollBand(scenery,result,0,77,2,steps);
            ScrollBand(scenery,result,82,91,6,steps);
            // The original also scrolls y=173..216 by 21 pixels; that band is
            // below the 173-pixel viewport and must not be squeezed into it.
        }
        else if(phase==2)
        {
            int steps=Math.Clamp(race.Tick-201,0,171);
            ScrollBand(scenery,result,0,Height,10,steps);
            double post=-race.Camera-800;
            if(post>-12&&post<Width)
                MaskedTile(result,colors,masks,(int)Math.Round(post,MidpointRounding.ToEven),10,0,169,12,147);
        }
        // Tick 200 stores transformed positions AFTER presenting the last
        // close-up. Reverse that known transform for this one retained frame.
        var positions=race.RacePhase==2&&phase==1
            ?race.Positions.Select(x=>race.FrontDistance-x*1.6).ToList():race.Positions;
        double maximum=positions.Max();var leaders=Enumerable.Range(0,8).Where(i=>positions[i]==maximum).ToArray();int leader=leaders.Length==1?leaders[0]:0;
        for(int i=0;i<8;i++)
        {
            double x=phase switch{0=>positions[i],1=>positions[leader]+15-positions[i],_=>415+positions[i]};
            int dx=checked((int)Math.Round(x,MidpointRounding.ToEven)),dy=phase==1?130:38+12*i;
            int w=phase==1?61:29,h=phase==1?35:21;
            int sx=phase switch{0=>29*frame,1=>116+61*frame,_=>58+29*frame},sy=(phase==1?35:21)*i;
            MaskedTile(result,colors,masks,dx,dy,sx,sy,w,h);
        }
        return result;
    }
    private static void ScrollBand(byte[] source,byte[] target,int top,int height,int shift,int steps)
    {
        if(steps==0)return;
        // Native self-BitBlt shifts right, then copies its ALREADY shifted tail
        // into the exposed strip. Its effective period is width-shift, not width.
        int period=Width-shift;
        for(int x=0;x<Width;x++)
        {
            int sx=((x-steps*shift)%period+period)%period;
            for(int y=top;y<top+height;y++)
                source.AsSpan((y*Width+sx)*3,3).CopyTo(target.AsSpan((y*Width+x)*3,3));
        }
    }
    private static void MaskedTile(byte[] target,byte[] colors,byte[] masks,int dx,int dy,int sx,int sy,int w,int h)
    {
        for(int y=0;y<h;y++)for(int column=0;column<w;column++)
            {
                int tx=dx+column,ty=dy+y,sourceX=sx+column,sourceY=sy+y;
                if(tx<0||tx>=Width||ty<0||ty>=Height||sourceX<0||sourceX>=SheetWidth||sourceY>=SheetHeight)continue;
                int at=(ty*Width+tx)*3,source=(sourceY*SheetWidth+sourceX)*3;
                for(int channel=0;channel<3;channel++)target[at+channel]=(byte)((target[at+channel]&masks[source+channel])^colors[source+channel]);
            }
    }
}
