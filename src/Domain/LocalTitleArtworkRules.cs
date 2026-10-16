namespace OpenGDS2.Domain;

/// <summary>English title lettering, generated only from the verified local title image.</summary>
public static class LocalTitleArtworkRules
{
    public const int Width=558,Height=385,CaptionHeight=165;
    public static byte[] Patch(byte[] source)
    {
        if(source.Length!=Width*Height*3)throw new InvalidDataException("Unexpected title artwork dimensions.");
        var output=(byte[])source.Clone();
        // The leftmost pixel is clear sky throughout this band. Reconstruct the
        // original vertical sky gradient. Keep the tiny tower cap that begins
        // inside the final four rows of the logo band.
        for(int y=0;y<CaptionHeight;y++)for(int x=0;x<Width;x++)
        {
            if(y>=161&&x>=269&&x<=290)continue;
            Array.Copy(source,y*Width*3,output,(y*Width+x)*3,3);
        }
        Letter("OPEN GAME",26);
        Letter("DEV STORY II",96);
        return output;

        void Letter(string text,int y)
        {
            const int scale=6;
            int x=(Width-(text.Length*6-1)*scale)/2;
            // Warm layered outlines echo the source logo's cream/red/gold palette.
            Draw(x+4,y+5,(115,50,0),8);
            Draw(x,y,(255,255,255),7);
            Draw(x,y,(91,18,0),4);
            Draw(x,y,(255,245,195),0);
            var ink=(byte[])output.Clone();
            LocalBitmapLetteringRules.DrawLettering(ink,Width,Height,text,x,y,scale,scale,(255,193,44));
            for(int row=y+24;row<y+42;row++)for(int col=x;col<x+(text.Length*6-1)*scale;col++)
            {
                int i=(row*Width+col)*3;
                if(ink[i]==255&&ink[i+1]==193&&ink[i+2]==44)
                    Array.Copy(ink,i,output,i,3);
            }
            void Draw(int px,int py,(byte R,byte G,byte B) color,int outline)
                =>LocalBitmapLetteringRules.DrawLettering(output,Width,Height,text,px,py,scale,scale,color,outline);
        }
    }
}
