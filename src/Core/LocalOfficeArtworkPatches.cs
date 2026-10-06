using Godot;
using System.Security.Cryptography;
using System.Text;
using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

/// <summary>Generates local English PNG patches only from an exact imported distribution.</summary>
public static class LocalOfficeArtworkPatches
{
    public const string Root="user://localized-artwork/en";
    public const string ColorHash="2817ea925508bb57aa33a2e711432bd640073508dfd5d65224dadb48690f3928";
    public const string MaskHash="c7d9f1619ba7f9f6ba3c276d25ab81e7955188253134d077ae493cd40a01fbd4";
    public const string BackgroundHash="ca909aa021f8c95ff8b90e35f81f9f59c222d1d98bae198dd1b795ca6e3f507e";
    private static readonly int[] CaptionIds=[1,2,3,4,5,6,7,8,9,10,11,12,15,16];
    public static string? EnsureEnglishPatch()
    {
        if (!TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)) return null;
        string source=ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot+"/Graphic/Embedded/OfficeSprites.bmp");
        string mask=ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot+"/Graphic/Embedded/OfficeMasks.bmp");
        string background=ProjectSettings.GlobalizePath(OriginalGameImporter.CacheRoot+"/Graphic/Embedded/OfficeBase.bmp");
        if (!File.Exists(source) || !File.Exists(mask) || !File.Exists(background)) return null;
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))).ToLowerInvariant()!=ColorHash ||
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(mask))).ToLowerInvariant()!=MaskHash ||
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(background))).ToLowerInvariant()!=BackgroundHash) return null;
        var captions=CaptionIds.ToDictionary(id=>id,id=>TranslationServer.Translate($"BITMAP_BUBBLE_{id}").ToString());
        string facilityCaption=TranslationServer.Translate("BITMAP_FACILITY_AVAILABLE").ToString();
        string revision=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("m60-lettering-v2\n"+facilityCaption+"\n"+string.Join("\n",captions.Select(x=>$"{x.Key}:{x.Value}"))))).ToLowerInvariant();
        string root=ProjectSettings.GlobalizePath(Root+"/"+revision);
        string output=System.IO.Path.Combine(root,"Graphic","Embedded","OfficeSprites.png");
        string backgroundOutput=System.IO.Path.Combine(root,"Graphic","Embedded","OfficeBase.png");
        if (File.Exists(output) && File.Exists(backgroundOutput)) return root;
        using var colors=new Image();using var masks=new Image();
        if (colors.Load(source)!=Error.Ok || masks.Load(mask)!=Error.Ok) return null;
        colors.Convert(Image.Format.Rgb8);masks.Convert(Image.Format.Rgb8);
        var patched=LocalBitmapLetteringRules.PatchOfficeCaptions(colors.GetData(),masks.GetData(),colors.GetWidth(),colors.GetHeight(),captions);
        using var image=Image.CreateFromData(colors.GetWidth(),colors.GetHeight(),false,Image.Format.Rgb8,patched);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output)!);
        if (image.SavePng(output)!=Error.Ok) throw new IOException("Could not write local English bitmap patch.");
        using var baseImage=new Image();
        if (baseImage.Load(background)!=Error.Ok) return null;
        baseImage.Convert(Image.Format.Rgb8);
        var translatedBase=LocalBitmapLetteringRules.PatchFacilitySigns(baseImage.GetData(),baseImage.GetWidth(),baseImage.GetHeight(),facilityCaption);
        using var basePatch=Image.CreateFromData(baseImage.GetWidth(),baseImage.GetHeight(),false,Image.Format.Rgb8,translatedBase);
        if (basePatch.SavePng(backgroundOutput)!=Error.Ok) throw new IOException("Could not write local facility sign patch.");
        return root;
    }
}
