namespace OpenGDS2.Domain;

/// <summary>Authored lettering over bounded caption areas in locally owned scenes.</summary>
public static class LocalSceneCaptionRules
{
    public static readonly (int X,int Y,int Width,int Height) HallRegion=(20,68,222,66);
    public static readonly (int X,int Y,int Width,int Height) ExhibitionRegion=(60,0,137,22);
    public static readonly (int X,int Y,int Width,int Height) AwardsRegion=(29,4,182,18);
    public static readonly (int X,int Y,int Width,int Height) HardwareRegion=(49,0,164,23);
    public static byte[] PatchHardware(byte[] source)
    {
        const int width=256,height=160;
        if(source.Length!=width*height*3)throw new InvalidDataException("Unexpected hardware scene dimensions.");
        var output=(byte[])source.Clone();var r=HardwareRegion;
        // Authored title plate; preserve the stage, audience and devices below it.
        for(int y=r.Y;y<r.Y+r.Height;y++)for(int x=r.X;x<r.X+r.Width;x++)
        {int i=(y*width+x)*3;output[i]=107;output[i+1]=99;output[i+2]=91;}
        LocalBitmapLetteringRules.DrawLettering(output,width,height,"NEW HARDWARE",59,4,2,2,(0,0,0),1);
        LocalBitmapLetteringRules.DrawLettering(output,width,height,"NEW HARDWARE",59,4,2,2,(255,107,95));
        return output;
    }
    public static readonly (int X,int Y,int Width,int Height)[] SalesRegions=[(80,40,91,45),(38,18,181,25),(64,25,109,23),(64,7,127,23)];
    public static readonly (int X,int Y,int Width,int Height) HelpRegion=(6,30,238,119);
    public static byte[] PatchSales(byte[] source,int stage)
    {
        if(source.Length!=256*160*3||stage is <1 or >4)throw new InvalidDataException("Unexpected sales picture.");
        var output=(byte[])source.Clone();var area=SalesRegions[stage-1];
        for(int y=area.Y;y<area.Y+area.Height;y++)for(int x=area.X;x<area.X+area.Width;x++){int i=(y*256+x)*3;output[i]=output[i+1]=output[i+2]=255;}
        if(stage==1)
        {
            LocalBitmapLetteringRules.DrawLettering(output,256,160,"100K",90,44,3,3,(0,0,0),1);LocalBitmapLetteringRules.DrawLettering(output,256,160,"100K",90,44,3,3,(255,0,160));
            LocalBitmapLetteringRules.DrawLettering(output,256,160,"SOLD!",96,68,2,2,(0,0,0));
        }
        else
        {
            string caption=new[]{"","250K SOLD!","500K SOLD!","1M SOLD!"}[stage-1];int scale=stage==3?1:2;
            int x=area.X+(area.Width-(caption.Length*6-1)*scale)/2,y=area.Y+5;
            LocalBitmapLetteringRules.DrawLettering(output,256,160,caption,x,y,scale,2,(0,0,0),1);
            LocalBitmapLetteringRules.DrawLettering(output,256,160,caption,x,y,scale,2,stage==2?((byte)255,(byte)107,(byte)95):((byte)255,(byte)255,(byte)0));
        }
        return output;
    }
    public static byte[] PatchHelp(byte[] source)
    {
        const int width=250,height=171;
        if(source.Length!=width*height*3)throw new InvalidDataException("Unexpected Help title.");
        var output=(byte[])source.Clone();var area=HelpRegion;
        for(int y=area.Y;y<area.Y+area.Height;y++)for(int x=area.X;x<area.X+area.Width;x++)for(int channel=0;channel<3;channel++)
        {int left=source[y*width*3+channel],right=source[(y*width+width-1)*3+channel];output[(y*width+x)*3+channel]=(byte)Math.Round(left+(right-left)*x/(width-1.0));}
        foreach(var (text,y) in new[]{("GAME DEV",52),("STORY II",94)})
        {LocalBitmapLetteringRules.DrawLettering(output,width,height,text,56,y+2,3,3,(0,0,0),1);LocalBitmapLetteringRules.DrawLettering(output,width,height,text,54,y,3,3,(220,220,220));}
        return output;
    }
    public static byte[] PatchReviewer(byte[] source,int index)
    {
        if(source.Length!=64*64*3 || index is <0 or >3)throw new InvalidDataException("Unexpected reviewer picture.");
        var output=(byte[])source.Clone();
        for(int y=48;y<64;y++)for(int x=0;x<64;x++)
        {int i=(y*64+x)*3;output[i]=255;output[i+1]=255;output[i+2]=255;}
        // Authored role labels identify the four bitmap-only reviewers without
        // guessing personal-name transliterations from stylized source lettering.
        LocalBitmapLetteringRules.DrawLettering(output,64,64,"REVIEWER "+(index+1),3,53,1,1,(0,0,0),1);
        LocalBitmapLetteringRules.DrawLettering(output,64,64,"REVIEWER "+(index+1),3,53,1,1,(224,24,24));
        return output;
    }
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
