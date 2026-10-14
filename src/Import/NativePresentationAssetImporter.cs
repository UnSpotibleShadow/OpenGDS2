using System.Buffers.Binary;
namespace OpenGDS2.Import;
/// <summary>Locally decoded native window icons and facility illustrations.</summary>
public static class NativePresentationAssetImporter
{
    private static readonly (string Form,int Offset)[] Icons=[
        ("MainForm3",0x22EA49),
        ("MainForm16",0x22EA49),
        ("fKeibaModosi",0x22EE33),
        ("fGMKikakuKaigi",0x22F485),
        ("fGMKontesutoHp",0x22FB0D),
        ("fGMGameKaishi",0x2301E2),
        ("fGMHikinukiKnS",0x2308FD),
        ("fGMHikinukiKeS",0x231097),
        ("fGMTenantGameS",0x23183B),
        ("fGMSeisanZ",0x231FFB),
        ("fGMMedia",0x2327D5),
        ("fGMKoudou",0x232FB4),
        ("fGMTenantCasino",0x2337B8),
        ("fGMGameDexKs",0x233FC3),
        ("fGMTenantHard",0x2347C2),
        ("fGMTenantReS",0x234FD1),
        ("fGMGameNew",0x2357D5),
        ("fGMKaiwa2",0x235FDC),
        ("fGMKaiwaC",0x236810),
        ("fGMKaihatuS",0x23706A),
        ("fGMTenantCasinoSn",0x2378D2),
        ("fGMTenantNazoP",0x238137),
        ("fGMSendenBaitai",0x2389C6),
        ("fGMTenantTechS",0x23925C),
        ("fGMTenantSeisan",0x239AF0),
        ("fGMTenantTechSel",0x23A390),
        ("fGMTenantBengo",0x23AC5D),
        ("fGMTenantOnsen",0x23B541),
        ("fGMTechKau",0x23BE35),
        ("fGMGameSave",0x23C748),
        ("fGMGameLoad",0x23D0AA),
        ("fGMZishaH",0x23DA10),
        ("fGMTenaSel",0x23E3A1),
        ("fGMGameHardInfo",0x23ED44),
        ("fGMKoudo",0x23F721),
        ("fGMZishaNet",0x24017E),
        ("fGMWSTech",0x240BEB),
        ("fGMLevelUP",0x241671),
        ("fGMHisyoSet",0x24213E),
        ("fGMGameDexSn",0x242CF5),
        ("fGMHLicense",0x2438F6),
        ("fGMBank",0x2444FE),
        ("fGMHisyoK",0x245166),
        ("fGMTenantHardS",0x245DED),
        ("fGMKontesuto",0x246C39),
        ("fGMKansei",0x247A98),
        ("fGMKaiwa",0x248907),
        ("fGMManga",0x2497BD),
        ("fGMToutatuRank",0x24A6BF),
        ("fGMDevH",0x24B610),
        ("fGMKeiyakuS",0x24C626),
        ("fGMZokuhen",0x24D65B),
        ("fGMRank100",0x24E6DE),
        ("fGMKaihatu",0x24F7FA),
        ("fGMZisha",0x25127F),
        ("fGMZishaK",0x252F66),
        ("fGMHikinukiSel",0x254CC7),
        ("fGMSKoyou",0x257B9C),
        ("fGMSKoyouKz",0x25AA82),
        ("fGMSyukkaSai",0x25DAD5),
        ("fGMZishaKW",0x260D2F),
        ("fGMSyukka",0x26420C),
        ("fGMSyain",0x267F29),
        ("fGMHikinuki",0x26BE3E),
        ("fGMbeta",0x26FE66),
        ("fGMComK",0x274C52),
        ("fGMalpha",0x279A5B),
        ("fGMDevW2",0x27E8DE),
        ("fGMDevW1",0x285CB2),
        ("fGMSyukkaH",0x28D0AD),
        ("fGMInfo",0x297CB2),
        ("fGMHardHatubai",0x2A2B6F),
        ("fGMKikakusyo",0x2ADABD),
        ("fCasinoPoker",0x2B8D4B),
        ("fCasinoBJ",0x2CADA8),
        ("fCasinoCC",0x2DDA22),
        ("fGMCrossRDen",0x2F2881),
        ("fGMCrossR",0x307874),
        ("fMainAbout",0x31DFFA),
        ("fGMHelp",0x33E55C),
        ("fGMOuri",0x3618F6),
        ("fKeibaMain",0x38B163),
        ("fMainVisual",0x3B574B)
    ];
    public static void Extract(byte[] data,string destination)
    {
        foreach(var (form,offset) in Icons)
        {
            int size=BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset-4));
            File.WriteAllBytes(Path.Combine(destination,"WindowIcon-"+form+".png"),LocalRasterDecoder.Png(Icon(data.AsSpan(offset,size).ToArray())));
        }
        (string Name,int Offset,int Size)[] scenes=[("School",0x8CB31,0),("HotSpring",0x88E14,0),("Recycling",0x4597A,0),
            ("TechnologySchool",0x345EF,0),("Casino",0x40AF6,0),("Lawyer",0x42598,0),("ProductionLab",0x3AF24,0),("HardwareLab",0x376B5,0)];
        foreach(var (name,offset,_) in scenes)
        {
            int size=BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset-4));
            File.WriteAllBytes(Path.Combine(destination,"Facility-"+name+".png"),LocalRasterDecoder.Png(LocalRasterDecoder.Gif(data.AsSpan(offset,size).ToArray(),256,160)));
        }
        int mysterySize=BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(0x653AF+2));
        File.WriteAllBytes(Path.Combine(destination,"Facility-MysteryLab.bmp"),data.AsSpan(0x653AF,mysterySize).ToArray());
    }
    public static LocalRasterDecoder.Raster Icon(byte[] bytes)
    {
        int U16(int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(at,2));
        int I32(int at)=>BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(at,4));
        if(bytes.Length<22||U16(0)!=0||U16(2)!=1||U16(4)<1||U16(4)>8)throw new InvalidDataException("Unsupported icon.");
        int dib=I32(18),size=I32(14);
        if(dib<22||size<40||dib>bytes.Length-size||I32(dib)!=40)throw new InvalidDataException("Invalid icon DIB.");
        int w=I32(dib+4),h=I32(dib+8)/2,bits=U16(dib+14);
        if(w<1||w>64||h<1||h>64||I32(dib+16)!=0||bits is not (1 or 4 or 8 or 24 or 32))throw new InvalidDataException("Unsupported icon format.");
        int colors=bits<=8?(I32(dib+32)==0?1<<bits:I32(dib+32)):0;
        if(colors<0||colors>256)throw new InvalidDataException("Invalid icon palette.");
        int pixels=dib+40+colors*4,stride=((w*bits+31)/32)*4,mask=pixels+stride*h,maskStride=((w+31)/32)*4;
        if(mask+maskStride*h>dib+size)throw new InvalidDataException("Truncated icon.");
        bool alpha=bits==32&&Enumerable.Range(0,h).Any(y=>Enumerable.Range(0,w).Any(x=>bytes[pixels+y*stride+x*4+3]!=0));
        var rgba=new byte[w*h*4];
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            int row=pixels+(h-1-y)*stride,src;
            if(bits<=8)
            {
                int index=bits==8?bytes[row+x]:bits==4?(bytes[row+x/2]>>(x%2==0?4:0))&15:(bytes[row+x/8]>>(7-x%8))&1;
                if(index>=colors)throw new InvalidDataException("Icon palette index out of range.");
                src=dib+40+index*4;
            }
            else src=row+x*(bits/8);
            int dest=(y*w+x)*4;rgba[dest]=bytes[src+2];rgba[dest+1]=bytes[src+1];rgba[dest+2]=bytes[src];
            bool transparent=(bytes[mask+(h-1-y)*maskStride+x/8]&(128>>(x%8)))!=0;
            rgba[dest+3]=transparent?(byte)0:alpha?bytes[src+3]:(byte)255;
        }
        return new(w,h,rgba);
    }
}
