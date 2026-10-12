using System.Buffers.Binary;
using System.IO.Compression;

namespace OpenGDS2.Import;

/// <summary>Portable local decoding of the original's opaque GIF and RLE8 pictures.
/// No executable code runs, and no source pixels are embedded in this application.</summary>
public static class LocalRasterDecoder
{
    public sealed record Raster(int Width,int Height,byte[] Rgba);
    public static Raster Gif(byte[] bytes,int expectedWidth,int expectedHeight)
    {
        int at=0;
        byte Read(){if(at>=bytes.Length)throw new InvalidDataException("Truncated GIF.");return bytes[at++];}
        int Word()=>Read()|Read()<<8;
        byte[] Palette(int n){var p=new byte[n*3];for(int i=0;i<p.Length;i++)p[i]=Read();return p;}
        byte[] Blocks()
        {
            using var s=new MemoryStream();int n;
            while((n=Read())!=0)for(int i=0;i<n;i++)s.WriteByte(Read());
            return s.ToArray();
        }
        if(bytes.Length<13 || !(bytes.AsSpan(0,6).SequenceEqual("GIF87a"u8)||bytes.AsSpan(0,6).SequenceEqual("GIF89a"u8)))
            throw new InvalidDataException("Unsupported GIF header.");
        at=6;int width=Word(),height=Word();
        if(width!=expectedWidth || height!=expectedHeight || width<=0 || height<=0 || (long)width*height>1_048_576)
            throw new InvalidDataException("Unsupported GIF dimensions.");
        int flags=Read();Read();Read();
        byte[] global=(flags&128)!=0 ? Palette(1<<((flags&7)+1)) : [];
        int transparent=-1;Raster? frame=null;
        while(at<bytes.Length)
        {
            int marker=Read();
            if(marker==0x3b)
                return frame ?? throw new InvalidDataException("Missing GIF frame.");
            if(marker==0x21)
            {
                int label=Read();
                if(label==0xf9)
                {
                    if(Read()!=4)throw new InvalidDataException("Invalid GIF control block.");
                    int control=Read();Word();int index=Read();transparent=(control&1)!=0 ? index : -1;
                    if(Read()!=0)throw new InvalidDataException("Invalid GIF control terminator.");
                }
                else Blocks();
                continue;
            }
            if(marker!=0x2c || frame is not null)throw new InvalidDataException("Only single-frame GIF resources are supported.");
            int left=Word(),top=Word(),fw=Word(),fh=Word(),imageFlags=Read();
            if(left!=0 || top!=0 || fw!=width || fh!=height)throw new InvalidDataException("Unsupported GIF frame rectangle.");
            byte[] palette=(imageFlags&128)!=0 ? Palette(1<<((imageFlags&7)+1)) : global;
            if(palette.Length==0)throw new InvalidDataException("Missing GIF palette.");
            int minimum=Read();byte[] compressed=Blocks();
            byte[] indices=Lzw(compressed,minimum,width*height);
            if((imageFlags&64)!=0)
            {
                var rows=new byte[indices.Length];int source=0;
                foreach(var pass in new[]{(0,8),(4,8),(2,4),(1,2)})
                    for(int y=pass.Item1;y<height;y+=pass.Item2){Array.Copy(indices,source,rows,y*width,width);source+=width;}
                indices=rows;
            }
            frame=new(width,height,Rgba(indices,palette,transparent));
        }
        throw new InvalidDataException("Missing GIF trailer.");
    }
    private static byte[] Lzw(byte[] data,int minimum,int count)
    {
        if(minimum is <2 or >8)throw new InvalidDataException("Invalid GIF code size.");
        int clear=1<<minimum,end=clear+1,next=end+1,bits=minimum+1,old=-1,first=0,bit=0,written=0;
        var prefix=new int[4096];var suffix=new byte[4096];var stack=new byte[4097];var output=new byte[count];
        for(int i=0;i<clear;i++)suffix[i]=(byte)i;
        while(bit+bits<=data.Length*8)
        {
            int code=0;for(int i=0;i<bits;i++,bit++)code|=((data[bit/8]>>(bit%8))&1)<<i;
            if(code==clear){next=end+1;bits=minimum+1;old=-1;continue;}
            if(code==end)
            {if(written!=count)throw new InvalidDataException("Incomplete GIF frame.");return output;}
            if(code>next || code==next && old<0)throw new InvalidDataException("Invalid GIF LZW reference.");
            int original=code,used=0;
            if(code==next){stack[used++]=(byte)first;code=old;}
            while(code>=clear)
            {
                if(code>=next || used>=4096)throw new InvalidDataException("Invalid GIF LZW chain.");
                stack[used++]=suffix[code];code=prefix[code];
            }
            first=code;stack[used++]=(byte)first;
            while(used>0)
            {if(written>=count)throw new InvalidDataException("Oversized GIF frame.");output[written++]=stack[--used];}
            if(old>=0 && next<4096)
            {prefix[next]=old;suffix[next]=(byte)first;next++;if(next==(1<<bits) && bits<12)bits++;}
            old=original;
        }
        throw new InvalidDataException("Missing GIF LZW terminator.");
    }
    public static Raster Rle8(byte[] bytes,int expectedWidth,int expectedHeight)
    {
        if(bytes.Length<54 || bytes[0]!='B' || bytes[1]!='M')throw new InvalidDataException("Invalid bitmap.");
        int width=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(18)),height=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(22));
        int pixel=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(10));
        int colors=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(46));if(colors==0)colors=256;
        if(width!=expectedWidth || height!=expectedHeight || width<=0 || height<=0 || (long)width*height>1_048_576 ||
            BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(14))!=40 || BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(28))!=8 ||
            BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(30))!=1 || colors is <1 or >256 || pixel<54+colors*4 || pixel>=bytes.Length)
            throw new InvalidDataException("Unsupported RLE8 bitmap.");
        var palette=new byte[colors*3];
        for(int i=0;i<colors;i++){palette[i*3]=bytes[54+i*4+2];palette[i*3+1]=bytes[54+i*4+1];palette[i*3+2]=bytes[54+i*4];}
        var indices=new byte[width*height];int at=pixel,x=0,y=0;
        byte Read(){if(at>=bytes.Length)throw new InvalidDataException("Truncated RLE8 bitmap.");return bytes[at++];}
        void Put(byte value)
        {
            if(x<0 || x>=width || y<0 || y>=height || value>=colors)throw new InvalidDataException("Invalid RLE8 run.");
            indices[(height-1-y)*width+x++]=value;
        }
        while(at<bytes.Length)
        {
            int n=Read(),value=Read();
            if(n>0){for(int i=0;i<n;i++)Put((byte)value);}
            else if(value==0){x=0;y++;}
            else if(value==1)return new(width,height,Rgba(indices,palette,-1));
            else if(value==2){x+=Read();y+=Read();if(x>width || y>=height)throw new InvalidDataException("Invalid RLE8 delta.");}
            else {for(int i=0;i<value;i++)Put(Read());if((value&1)!=0)Read();}
        }
        throw new InvalidDataException("Missing RLE8 terminator.");
    }
    private static byte[] Rgba(byte[] indices,byte[] palette,int transparent)
    {
        var rgba=new byte[indices.Length*4];
        for(int i=0;i<indices.Length;i++)
        {
            int index=indices[i];if(index*3+2>=palette.Length)throw new InvalidDataException("Invalid palette index.");
            Array.Copy(palette,index*3,rgba,i*4,3);rgba[i*4+3]=index==transparent ? (byte)0 : (byte)255;
        }
        return rgba;
    }
    public static byte[] Png(Raster image)
    {
        if(image.Width<=0 || image.Height<=0 || image.Rgba.Length!=(long)image.Width*image.Height*4)
            throw new InvalidDataException("Invalid local raster.");
        using var output=new MemoryStream();output.Write(new byte[]{137,80,78,71,13,10,26,10});
        void Chunk(string type,byte[] payload)
        {
            var name=System.Text.Encoding.ASCII.GetBytes(type);var length=new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(length,payload.Length);output.Write(length);output.Write(name);output.Write(payload);
            uint crc=0xffffffff;
            foreach(byte value in name.Concat(payload))
            {crc^=value;for(int i=0;i<8;i++)crc=(crc&1)!=0 ? 0xedb88320^(crc>>1) : crc>>1;}
            BinaryPrimitives.WriteUInt32BigEndian(length,~crc);output.Write(length);
        }
        var header=new byte[13];BinaryPrimitives.WriteInt32BigEndian(header,image.Width);BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4),image.Height);
        header[8]=8;header[9]=6;Chunk("IHDR",header);
        using var compressed=new MemoryStream();
        using(var zip=new ZLibStream(compressed,CompressionLevel.Optimal,true))
            for(int y=0;y<image.Height;y++){zip.WriteByte(0);zip.Write(image.Rgba,y*image.Width*4,image.Width*4);}
        Chunk("IDAT",compressed.ToArray());Chunk("IEND",[]);return output.ToArray();
    }
}
