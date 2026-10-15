namespace OpenGDS2.Domain;

/// <summary>Authored 5×7 lettering, applied to locally imported opaque caption interiors.</summary>
public static class LocalBitmapLetteringRules
{
    public static readonly (int X,int Y,int Width,int Height)[] OfficeDateLabelRegions=[(378,22,22,25),(422,22,25,25)];
    public static byte[] PatchOfficeDateLabels(byte[] source,int width,int height)
    {
        if(width!=466 || height!=290 || source.Length!=width*height*3)throw new InvalidDataException("Unexpected office background dimensions.");
        var output=(byte[])source.Clone();string[] labels=["MO","WK"];
        for(int n=0;n<OfficeDateLabelRegions.Length;n++)
        {
            var r=OfficeDateLabelRegions[n];
            for(int y=r.Y;y<r.Y+r.Height;y++)for(int x=r.X;x<r.X+r.Width;x++)for(int c=0;c<3;c++)
            {
                int left=source[(y*width+r.X-1)*3+c],right=source[(y*width+r.X+r.Width)*3+c];
                output[(y*width+x)*3+c]=(byte)Math.Round(left+(right-left)*(x-r.X+1)/(double)(r.Width+1));
            }
            DrawLettering(output,width,height,labels[n],r.X+(r.Width-11)/2,r.Y+5,1,2,(90,90,200));
        }
        return output;
    }
    private static readonly Dictionary<char,string> Glyphs=new()
    {
        ['0']="01110/10001/10011/10101/11001/10001/01110",
        ['1']="00100/01100/00100/00100/00100/00100/01110",
        ['2']="01110/10001/00001/00010/00100/01000/11111",
        ['3']="11110/00001/00001/01110/00001/00001/11110",
        ['4']="00010/00110/01010/10010/11111/00010/00010",
        ['5']="11111/10000/10000/11110/00001/00001/11110",
        ['A']="01110/10001/10001/11111/10001/10001/10001",
        ['B']="11110/10001/10001/11110/10001/10001/11110",
        ['C']="01111/10000/10000/10000/10000/10000/01111",
        ['D']="11110/10001/10001/10001/10001/10001/11110",
        ['E']="11111/10000/10000/11110/10000/10000/11111",
        ['F']="11111/10000/10000/11110/10000/10000/10000",
        ['G']="01111/10000/10000/10111/10001/10001/01111",
        ['H']="10001/10001/10001/11111/10001/10001/10001",
        ['I']="11111/00100/00100/00100/00100/00100/11111",
        ['J']="00111/00010/00010/00010/10010/10010/01100",
        ['K']="10001/10010/10100/11000/10100/10010/10001",
        ['L']="10000/10000/10000/10000/10000/10000/11111",
        ['M']="10001/11011/10101/10101/10001/10001/10001",
        ['N']="10001/11001/10101/10011/10001/10001/10001",
        ['O']="01110/10001/10001/10001/10001/10001/01110",
        ['P']="11110/10001/10001/11110/10000/10000/10000",
        ['Q']="01110/10001/10001/10001/10101/10010/01101",
        ['R']="11110/10001/10001/11110/10100/10010/10001",
        ['S']="01111/10000/10000/01110/00001/00001/11110",
        ['T']="11111/00100/00100/00100/00100/00100/00100",
        ['U']="10001/10001/10001/10001/10001/10001/01110",
        ['V']="10001/10001/10001/10001/10001/01010/00100",
        ['W']="10001/10001/10001/10101/10101/10101/01010",
        ['X']="10001/10001/01010/00100/01010/10001/10001",
        ['Y']="10001/10001/01010/00100/00100/00100/00100",
        ['Z']="11111/00001/00010/00100/01000/10000/11111",
        ['!']="00100/00100/00100/00100/00100/00000/00100",
        ['?']="01110/10001/00001/00010/00100/00000/00100",
        [' ']="00000/00000/00000/00000/00000/00000/00000"
    };
    public static byte[] PatchOfficeCaptions(byte[] colors,byte[] masks,int width,int height,IReadOnlyDictionary<int,string> captions)
    {
        if (width!=1600 || height!=530 || colors.Length!=width*height*3 || masks.Length!=colors.Length)
            throw new InvalidDataException("Unexpected local office atlas dimensions.");
        var output=(byte[])colors.Clone();
        foreach (var (id,caption) in captions)
        {
            if (id is <1 or >16) throw new InvalidDataException("Unknown speech caption.");
            var region=OfficeVisualRules.BubbleRegions[id-1];
            string text=caption.ToUpperInvariant();
            int x=region.X+3,y=region.Y+2,w=region.Width-6,h=8;
            if (text.Length==0 || text.Length*6-1>w || text.Any(c=>!Glyphs.ContainsKey(c)))
                throw new InvalidDataException("Local caption does not fit the verified lettering rectangle.");
            for (int row=0;row<h;row++) for (int column=0;column<w;column++)
            {
                int pos=((y+row)*width+x+column)*3;
                if (masks[pos]!=0 || masks[pos+1]!=0 || masks[pos+2]!=0)
                    throw new InvalidDataException("The caption interior is not opaque in the original mask.");
                // Authored fills preserve the verified red/blue bubble palette;
                // sampling an edge could accidentally copy part of an old glyph.
                byte[] background=id is 1 or 6 ? [246,82,79] : row<4 ? [213,243,255] : row<6 ? [197,233,254] : [187,211,251];
                Array.Copy(background,0,output,pos,3);
            }
            int origin=x+(w-(text.Length*6-1))/2;
            for (int letter=0;letter<text.Length;letter++)
            {
                var rows=Glyphs[text[letter]].Split('/');
                for (int row=0;row<7;row++) for (int column=0;column<5;column++)
                {
                    if (rows[row][column]!='1') continue;
                    int pos=((y+row)*width+origin+letter*6+column)*3;
                    output[pos]=output[pos+1]=output[pos+2]=0;
                }
            }
        }
        return output;
    }
    public static readonly (string Text,int X,int Y,int Width)[] RoomCaptionInteriors=[
        ("DEV",264,361,27),("PAUSED",572,359,51),("DEBUG",462,382,51),
        ("COMPLETE",951,362,51),("COMPLETE",1008,362,51),("HARD",795,371,27)];

    public static byte[] PatchRoomCaptions(byte[] colors,byte[] masks,int width,int height)
    {
        if(width!=1600 || height!=530 || colors.Length!=width*height*3 || masks.Length!=colors.Length)
            throw new InvalidDataException("Unexpected local office atlas dimensions.");
        var output=(byte[])colors.Clone();
        foreach(var r in RoomCaptionInteriors)
        {
            for(int y=0;y<8;y++) for(int x=0;x<r.Width;x++)
            {
                int pos=((r.Y+y)*width+r.X+x)*3;
                if(masks[pos]!=0 || masks[pos+1]!=0 || masks[pos+2]!=0)
                    throw new InvalidDataException("Room caption interior is not opaque.");
                byte[] fill=r.Text=="PAUSED" ? [246,82,79] : y<4 ? [213,243,255] : y<6 ? [197,233,254] : [187,211,251];
                Array.Copy(fill,0,output,pos,3);
            }
            int origin=r.X+(r.Width-(r.Text.Length*6-1))/2;
            for(int i=0;i<r.Text.Length;i++)
            {
                var glyph=Glyphs[r.Text[i]].Split('/');
                for(int y=0;y<7;y++) for(int x=0;x<5;x++) if(glyph[y][x]=='1')
                {
                    int pos=((r.Y+y)*width+origin+i*6+x)*3;
                    output[pos]=output[pos+1]=output[pos+2]=0;
                }
            }
        }
        return output;
    }

    public static readonly (int X,int Y,int Width,int Height)[] FacilitySignInteriors=[
        (35,186,60,13),(116,227,60,13),(372,227,60,13)];

    public static byte[] PatchFacilitySigns(byte[] pixels,int width,int height,string caption)
    {
        if (width!=466 || height!=290 || pixels.Length!=width*height*3)
            throw new InvalidDataException("Unexpected local office background dimensions.");
        string text=caption.ToUpperInvariant();
        if (text.Length==0 || text.Length*6-1>60 || text.Any(c=>!Glyphs.ContainsKey(c)))
            throw new InvalidDataException("Facility caption does not fit its lettering rectangle.");
        var output=(byte[])pixels.Clone();
        foreach (var r in FacilitySignInteriors)
        {
            for (int y=r.Y;y<r.Y+r.Height;y++) for (int x=r.X;x<r.X+r.Width;x++)
            {
                int pos=(y*width+x)*3;output[pos]=output[pos+1]=output[pos+2]=220;
            }
            int left=r.X+(r.Width-(text.Length*6-1))/2,top=r.Y+(r.Height-7)/2;
            for (int letter=0;letter<text.Length;letter++)
            {
                var rows=Glyphs[text[letter]].Split('/');
                for (int y=0;y<7;y++) for (int x=0;x<5;x++)
                {
                    if (rows[y][x]!='1') continue;
                    int pos=((top+y)*width+left+letter*6+x)*3;
                    output[pos]=output[pos+1]=output[pos+2]=0;
                }
            }
        }
        return output;
    }

    public static void DrawLettering(byte[] pixels,int width,int height,string text,int x,int y,int scaleX,int scaleY,(byte R,byte G,byte B) color,int outline=0)
    {
        if(pixels.Length!=width*height*3 || scaleX<1 || scaleY<1 || outline<0)throw new InvalidDataException("Invalid lettering canvas.");
        text=text.ToUpperInvariant();if(text.Any(c=>!Glyphs.ContainsKey(c)))throw new InvalidDataException("Unsupported authored glyph.");
        for(int letter=0;letter<text.Length;letter++)
        {
            var rows=Glyphs[text[letter]].Split('/');
            for(int row=0;row<7;row++)for(int column=0;column<5;column++)if(rows[row][column]=='1')
            for(int dy=-outline;dy<scaleY+outline;dy++)for(int dx=-outline;dx<scaleX+outline;dx++)
            {
                int px=x+(letter*6+column)*scaleX+dx,py=y+row*scaleY+dy;
                if(px<0||py<0||px>=width||py>=height)throw new InvalidDataException("Authored lettering exceeds its canvas.");
                int index=(py*width+px)*3;pixels[index]=color.R;pixels[index+1]=color.G;pixels[index+2]=color.B;
            }
        }
    }

}
