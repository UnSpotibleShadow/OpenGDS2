namespace OpenGDS2.Domain;

/// <summary>Authored 5×7 lettering, applied to locally imported opaque caption interiors.</summary>
public static class LocalBitmapLetteringRules
{
    private static readonly Dictionary<char,string> Glyphs=new()
    {
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
}
