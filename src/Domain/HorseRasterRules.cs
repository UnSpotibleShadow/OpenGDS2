namespace OpenGDS2.Domain;
/// <summary>Recovered BitBlt coordinates and SRCAND/SRCINVERT operations (M79).</summary>
public static class HorseRasterRules
{
    public const int Width=421,Height=173,SheetWidth=233,SheetHeight=316;
    public static byte[] Compose(byte[] background,byte[] colors,byte[] masks,HorseRaceState race)
    {
        if(background.Length!=Width*Height*3||colors.Length!=SheetWidth*SheetHeight*3||masks.Length!=colors.Length||race.Positions.Count!=8||race.RacePhase is <0 or >2)throw new InvalidDataException("Invalid race raster data.");
        byte[] result=(byte[])background.Clone();int frame=race.Tick%2;
        double maximum=race.Positions.Max();var leaders=Enumerable.Range(0,8).Where(i=>race.Positions[i]==maximum).ToArray();int leader=leaders.Length==1?leaders[0]:0;
        for(int i=0;i<8;i++)
        {
            double x=race.RacePhase switch{0=>race.Positions[i],1=>race.Positions[leader]+15-race.Positions[i],_=>415+race.Positions[i]};
            int dx=checked((int)Math.Round(x,MidpointRounding.ToEven)),dy=race.RacePhase==1?130:38+12*i;
            int w=race.RacePhase==1?61:29,h=race.RacePhase==1?35:21;
            int sx=race.RacePhase switch{0=>29*frame,1=>116+61*frame,_=>58+29*frame},sy=(race.RacePhase==1?35:21)*i;
            for(int y=0;y<h;y++)for(int column=0;column<w;column++)
            {
                int tx=dx+column,ty=dy+y,sourceX=sx+column,sourceY=sy+y;
                if(tx<0||tx>=Width||ty<0||ty>=Height||sourceX<0||sourceX>=SheetWidth||sourceY>=SheetHeight)continue;
                int target=(ty*Width+tx)*3,source=(sourceY*SheetWidth+sourceX)*3;
                for(int channel=0;channel<3;channel++)result[target+channel]=(byte)((result[target+channel]&masks[source+channel])^colors[source+channel]);
            }
        }
        return result;
    }
}
