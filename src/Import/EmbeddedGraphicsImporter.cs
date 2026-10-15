using System.Buffers.Binary;
using System.IO;

namespace OpenGDS2.Import;

/// <summary>Known serialized VB6 form bitmaps in the supported executable.
/// These bytes are extracted only into the user's local cache.</summary>
public static class EmbeddedGraphicsImporter
{
    public static void Extract(byte[] executable, string destination)
    {
        Directory.CreateDirectory(destination);NativePresentationAssetImporter.Extract(executable,destination);
        SerializedImageBinding(executable,0x26D228,"Image1",1);WriteBitmap(executable,0x26D228,64,64,destination,"PoachingRival.bmp");
        SerializedImageBinding(executable,0x26E6E8,"Image1",2);WriteBitmap(executable,0x26E6E8,64,64,destination,"PoachingPresident.bmp");
        WriteBitmap(executable,0x2C65B4,71,96,destination,"CardFrame.bmp");
        WriteBitmap(executable,0x2BABBE,71,96,destination,"CardBack.bmp");
        for(int i=0;i<26;i++){int offset=0x2C5FC2-i*0x5F2;SerializedImageBinding(executable,offset,"Image3",i);WriteBitmap(executable,offset,20,20,destination,$"CardRank{i}.bmp");}
        for(int i=0;i<4;i++){int offset=0x2CA523-i*0x5F2;SerializedImageBinding(executable,offset,"Image4",i);WriteBitmap(executable,offset,20,20,destination,$"CardSuit{i}.bmp");}
        for(int i=0;i<3;i++)WriteBitmap(executable,0x2BA50C-i*0x6B2,28,21,destination,$"CardFace{i}.bmp");
        WriteBitmap(executable,0x39AFFC,421,173,destination,"HorseTrack.bmp");
        for(int i=0;i<24;i++){int offset=i<8?0x391387+i*0x562:0x3B50DC-(i-8)*0x562;WriteBitmap(executable,offset,16,16,destination,$"HorseMarker{i}.bmp");}
        int[] computers=[0x278842,0x2777B4,0x27670A,0x27566C];
        for(int i=0;i<4;i++)WriteBitmap(executable,computers[i],64,64,destination,$"Computer{i}.bmp");
        WriteBitmap(executable, 0x3B5DC6, 466, 290, destination, "OfficeBase.bmp");
        WriteBitmap(executable, 0x3D7469, 1600, 530, destination, "OfficeSprites.bmp");
        WriteBitmap(executable, 0x4A6960, 1600, 530, destination, "OfficeMasks.bmp");
        // Meeting mode 0 uses MDIForm1.ImageFormG[0], getter 032C.
        const int meetingOffset=0x18B188;
        if (meetingOffset>executable.Length-54 ||
            !executable.AsSpan(meetingOffset-28,13).SequenceEqual(System.Text.Encoding.ASCII.GetBytes("ImageFormG\0\x18\x01")) ||
            BinaryPrimitives.ReadUInt16LittleEndian(executable.AsSpan(meetingOffset-15,2))!=0)
            throw new InvalidDataException("Unsupported embedded planning illustration.");
        WriteBitmap(executable,meetingOffset,256,160,destination,"PlanningMeeting.bmp");
        // fGMKikakusyo.Picture1: proposal document, not ImageFormG[0].
        WriteBitmap(executable,0x2AE079,256,160,destination,"PlanningProposal.bmp");
        // fGMSyukkaH.Picture1 and fGMHardHatubai.Picture1 share the same scene.
        const int launchScene=0x28D66E,announcementScene=0x2A3130;
        int launchSize=BinaryPrimitives.ReadInt32LittleEndian(executable.AsSpan(launchScene+2));
        if(launchSize!=42038 || announcementScene>executable.Length-launchSize ||
            !executable.AsSpan(launchScene,launchSize).SequenceEqual(executable.AsSpan(announcementScene,launchSize)))
            throw new InvalidDataException("Unsupported hardware launch illustration.");
        WriteBitmap(executable,launchScene,256,160,destination,"HardwareLaunchScene.bmp");
        SerializedImageBinding(executable,0x2F2EA1,"Image1",2);WriteBitmap(executable,0x2F2EA1,256,160,destination,"HallCelebration2.bmp");
        SerializedImageBinding(executable,0x2FD305,"Image1",1);WriteBitmap(executable,0x2FD305,256,160,destination,"HallCelebration1.bmp");
        int[] achievements=[0x380C40,0x3767DE,0x36C37C,0x361F1A];
        for(int i=0;i<4;i++){SerializedImageBinding(executable,achievements[i],"Image1",i);WriteBitmap(executable,achievements[i],256,160,destination,$"SalesAchievement{i+1}.bmp");}
        WriteBitmap(executable,0x33F306,250,171,destination,"HelpTitle.bmp");
        SerializedImageBinding(executable,0x16585,"ImageFormG",25);
        WriteBitmap(executable,0x16585,256,160,destination,"CompanyActivity.bmp");
        WriteGif(executable,0x11243,21264,destination,"ExhibitionBooth.png");
        WriteGif(executable,0x1367D8,19520,destination,"AwardsStage.png");
        int[] trophies=[0x87EA,0x9B0F,0xC159,0xAE34,0xD47E];
        for(int index=0;index<5;index++)
        {SerializedImageBinding(executable,trophies[index],"ImageTF",index);WriteBitmap(executable,trophies[index],40,40,destination,$"Trophy{index}.bmp");}
        SerializedImageBinding(executable,0x845D,"ImageTF",5);WriteGif(executable,0x845D,862,destination,"EmptyBadge.png",40,40);
        int[] reviewers=[0x31CB24,0x31B6C2,0x31A260,0x318DFE];
        int[] scores=[0x312826,0x311F4A,0x3115CE,0x310CF2,0x3103F6,0x30FA3A,0x30F21E,0x30E862,0x30DE86,0x30D1CA];
        int[] reactions=[0x31849A,0x317B36,0x3171D2,0x31686E,0x314C48,0x315F0C,0x3155AA,0x3142E6,0x313984,0x313022];
        for(int index=0;index<4;index++)
        {SerializedImageBinding(executable,reviewers[index],"Image1",index);WriteBitmap(executable,reviewers[index],64,64,destination,$"Reviewer{index}.bmp");}
        for(int index=0;index<10;index++)
        {
            SerializedImageBinding(executable,scores[index],"Image4",index);
            int size=BinaryPrimitives.ReadInt32LittleEndian(executable.AsSpan(scores[index]+2));
            if(size<54 || size>executable.Length-scores[index])throw new InvalidDataException("Invalid review score resource.");
            var raster=LocalRasterDecoder.Rle8(executable.AsSpan(scores[index],size).ToArray(),64,64);
            File.WriteAllBytes(Path.Combine(destination,$"ReviewScore{index+1}.png"),LocalRasterDecoder.Png(raster));
            SerializedImageBinding(executable,reactions[index],"Image3",index);
            WriteBitmap(executable,reactions[index],18,64,destination,$"ReviewReaction{index}.bmp");
        }
        // Hidden BGraphic skins contain the original button glyphs and captions.
        // Recover every bounded 63x22 skin, plus shipment arrow and roster icons.
        byte[] signature=System.Text.Encoding.ASCII.GetBytes("BGraphic\0\x18\x01");
        for(int at=0;at<executable.Length-signature.Length-80;at++)
            if(executable.AsSpan(at,signature.Length).SequenceEqual(signature))
            {
                int offset=at+26;
                if(executable[offset]=='B'&&executable[offset+1]=='M'&&
                    BinaryPrimitives.ReadInt32LittleEndian(executable.AsSpan(offset+18,4)) is >=8 and <=256&&
                    BinaryPrimitives.ReadInt32LittleEndian(executable.AsSpan(offset+22,4)) is >=8 and <=256)
                    WriteBitmap(executable,offset,BinaryPrimitives.ReadInt32LittleEndian(executable.AsSpan(offset+18,4)),BinaryPrimitives.ReadInt32LittleEndian(executable.AsSpan(offset+22,4)),destination,$"Button-{offset:X6}.bmp");
            }
        foreach(int offset in new[]{0x26511E,0x265680,0x265C47,0x2661A9,0x2668F5,0x266E59,0x2673BD,0x267921})
            WriteBitmap(executable,offset,16,16,destination,$"Button-{offset:X6}.bmp");
        // MDIForm1 BGraphicH picture properties, verified in the supplied build.
        int[] toolbar = [0x1AD1, 0x216A, 0x2803, 0x2E9C, 0x3535, 0x3BCC, 0x4265, 0x48FE,
            0x4F97, 0x5630, 0x5CC9, 0x6362, 0x69FB, 0x7094, 0x772D, 0x7DC6];
        for (int index = 0; index < toolbar.Length; index++)
            WriteBitmap(executable, toolbar[index], 23, 22, destination, $"Toolbar{index}.bmp");
        // MDIForm1.ImageHard is serialized in descending array-index order.
        // The quote accesses this array by selected source index (009940B8+B0).
        for (int index = 0; index < 15; index++)
        {
            int offset = 0x1A9F3E + (14-index)*0x3069;
            if (offset > executable.Length-54 ||
                !executable.AsSpan(offset-27,12).SequenceEqual(System.Text.Encoding.ASCII.GetBytes("ImageHard\0\x18\x01")) ||
                BinaryPrimitives.ReadUInt16LittleEndian(executable.AsSpan(offset-15,2)) != index)
                throw new InvalidDataException("Unsupported embedded hardware picture array.");
            WriteBitmap(executable, offset, 64, 64, destination, $"Hardware{index}.bmp");
        }
    }

    private static void SerializedImageBinding(byte[] bytes,int offset,string control,int index)
    {
        int prefix=control.Length+18;
        if(offset<prefix || offset>bytes.Length-54 ||
            !bytes.AsSpan(offset-prefix,control.Length+3).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(control+"\0\x18\x01")) ||
            BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset-15))!=index)
            throw new InvalidDataException("Unsupported serialized review artwork.");
    }
    private static void WriteGif(byte[] bytes,int offset,int size,string destination,string name,int width=256,int height=160)
    {
        if(offset<0 || size<13 || offset>bytes.Length-size)throw new InvalidDataException("Missing scene GIF.");
        var raster=LocalRasterDecoder.Gif(bytes.AsSpan(offset,size).ToArray(),width,height);
        File.WriteAllBytes(Path.Combine(destination,name),LocalRasterDecoder.Png(raster));
    }

    private static void WriteBitmap(byte[] bytes, int offset, int width, int height, string destination, string name)
    {
        if (offset < 0 || offset > bytes.Length - 54) throw new InvalidDataException("Missing embedded bitmap.");
        var header = bytes.AsSpan(offset, 54);
        int size = BinaryPrimitives.ReadInt32LittleEndian(header[2..]);
        int pixelOffset = BinaryPrimitives.ReadInt32LittleEndian(header[10..]);
        if (header[0] != 'B' || header[1] != 'M' || size < 54 || size > bytes.Length - offset ||
            pixelOffset < 54 || pixelOffset >= size ||
            BinaryPrimitives.ReadInt32LittleEndian(header[18..]) != width ||
            BinaryPrimitives.ReadInt32LittleEndian(header[22..]) != height)
            throw new InvalidDataException("Unsupported embedded office graphics.");
        Directory.CreateDirectory(destination);
        File.WriteAllBytes(Path.Combine(destination, name), bytes.AsSpan(offset, size).ToArray());
    }
}
