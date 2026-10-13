namespace OpenGDS2.Domain;

/// <summary>Authored lettering over bounded caption areas in locally owned scenes.</summary>
public static class LocalSceneCaptionRules
{
    public static readonly (int X,int Y,int Width,int Height) HallRegion=(20,68,222,66);
    public static readonly (int X,int Y,int Width,int Height) ExhibitionRegion=(60,0,137,22);
    public static readonly (int X,int Y,int Width,int Height) AwardsRegion=(29,4,182,18);
    public static byte[] PatchAwards(byte[] source)
    {
        const int width=256,height=160;
        if(source.Length!=width*height*3)throw new InvalidDataException("Unexpected scene caption dimensions.");
        var output=(byte[])source.Clone();var r=AwardsRegion;
        for(int y=r.Y;y<r.Y+r.Height;y++)for(int x=r.X;x<r.X+r.Width;x++)
        {int i=(y*width+x)*3;output[i]=224;output[i+1]=224;output[i+2]=224;}
        LocalBitmapLetteringRules.DrawLettering(output,width,height,"GAME CONTEST",49,6,2,2,(0,0,0),1);
        LocalBitmapLetteringRules.DrawLettering(output,width,height,"GAME CONTEST",49,6,2,2,(120,160,255));
        return output;
    }
    public static byte[] Patch(byte[] source,bool hall)
    {
        const int width=256,height=160;
        if(source.Length!=width*height*3)throw new InvalidDataException("Unexpected scene caption dimensions.");
        var output=(byte[])source.Clone();var r=hall?HallRegion:ExhibitionRegion;
        for(int y=r.Y;y<r.Y+r.Height;y++)for(int x=r.X;x<r.X+r.Width;x++)
        {
            (byte R,byte G,byte B) color=hall?HallBackground(x,y):((byte)158,(byte)158,(byte)170);
            int i=(y*width+x)*3;output[i]=color.R;output[i+1]=color.G;output[i+2]=color.B;
        }
        if(hall)
        {
            LocalBitmapLetteringRules.DrawLettering(output,width,height,"HALL OF FAME",24,80,3,6,(95,95,95),1);
            LocalBitmapLetteringRules.DrawLettering(output,width,height,"HALL OF FAME",22,78,3,6,(0,0,0),1);
            LocalBitmapLetteringRules.DrawLettering(output,width,height,"HALL OF FAME",22,78,3,6,(0,255,0));
        }
        else
        {
            LocalBitmapLetteringRules.DrawLettering(output,width,height,"GAME DEX",81,4,2,2,(0,0,0),1);
            LocalBitmapLetteringRules.DrawLettering(output,width,height,"GAME DEX",81,4,2,2,(224,24,24));
        }
        return output;

        (byte R,byte G,byte B) HallBackground(int x,int y)
        {
            // Restore the burst along its original ray and the white oval behind
            // the caption. Scenery outside the lettering rectangle is untouched.
            double dx=x-128,dy=y-126;
            if(dx*dx/(123*123.0)+dy*dy/(15*15.0)<=1)return (255,255,255);
            for(double scale=1.1;scale<18;scale+=.1)
            {
                int sx=(int)Math.Round(128+dx*scale),sy=(int)Math.Round(126+dy*scale);
                if(sx<0||sy<0||sx>=width||sy>=height)break;
                if(sx>=r.X&&sx<r.X+r.Width&&sy>=r.Y&&sy<r.Y+r.Height)continue;
                int i=(sy*width+sx)*3;
                if(source[i]>180&&source[i+2]<90)return (source[i],source[i+1],source[i+2]);
            }
            return (255,255,0);
        }
    }
}
