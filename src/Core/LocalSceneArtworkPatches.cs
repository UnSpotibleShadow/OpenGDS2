using Godot;
using System.Security.Cryptography;
using OpenGDS2.Domain;
namespace OpenGDS2.Core;

public static class LocalSceneArtworkPatches
{
    private static readonly Dictionary<string,(string Hash,bool Hall)> Sources=new()
    {
        ["Graphic/Embedded/HallCelebration1.bmp"]=("c0ec11169f963601cfcd731bce62d1dd76b00e11fd1b51b61b079d611ffc23b8",true),
        ["Graphic/Embedded/HallCelebration2.bmp"]=("58d4c987b7c89d67ca41c8ea0afed776e6ec878f1938713a74ff4a9752c0fa4b",true),
        ["Graphic/Embedded/ExhibitionBooth.png"]=("39b4f664437429be73254e80936a74ad4b05184ecc29ad2f83aa476d247bacb4",false)
    };
    public static string? Ensure(string relative,string source)
    {
        if(!Sources.TryGetValue(relative,out var spec)||!File.Exists(source))return null;
        using var image=new Image();if(image.Load(source)!=Error.Ok||image.GetSize()!=new Vector2I(256,160))return null;
        image.Convert(Image.Format.Rgb8);var pixels=image.GetData();
        if(Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant()!=spec.Hash)return null;
        string path=ProjectSettings.GlobalizePath("user://localized-artwork/en/event-captions-m101-v1/"+spec.Hash+"/"+System.IO.Path.GetFileNameWithoutExtension(relative)+".png");
        if(File.Exists(path))return path;
        using var patch=Image.CreateFromData(256,160,false,Image.Format.Rgb8,LocalSceneCaptionRules.Patch(pixels,spec.Hall));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        if(patch.SavePng(path)!=Error.Ok)throw new IOException("Could not write the local English scene caption.");
        return path;
    }
}
