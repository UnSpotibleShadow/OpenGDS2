using Godot;
using System.Security.Cryptography;
using OpenGDS2.Domain;
namespace OpenGDS2.Core;

public static class LocalSceneArtworkPatches
{
    private enum CaptionKind { Hall,Exhibition,Awards,Reviewer,Hardware,Sales,Help,Activity,Television,Secretary,Title,Registration }
    private static readonly Dictionary<string,(string Hash,CaptionKind Caption)> Sources=new()
    {
        ["Graphic/Embedded/CompanySetup.bmp"]=("1db83a89b10937eecb87433be0901367ecd94c923ded83d66c59c63b14d02b7a",CaptionKind.Registration),
        ["Graphic/Recovered/resource-047345.bmp"]=("1db83a89b10937eecb87433be0901367ecd94c923ded83d66c59c63b14d02b7a",CaptionKind.Registration),
        ["Graphic/Title.bmp"]=("4e95e7151a12f32ae00bc6c9afd5e7cefd1b014b15d0d0c708d60d6ae9c74fa2",CaptionKind.Title),
        ["Graphic/Human/Human2.bmp"]=("ee492f2655c6cab24d87231edc3b159bcada5c6d41827831c1048a82647ddfad",CaptionKind.Secretary),
        ["Graphic/Human/Human2.1.bmp"]=("8e7b555e9a582e5400e0a9a6c5a39dd8a7670c3fd2860ec24ef5a73159364750",CaptionKind.Secretary),
        ["Graphic/Human/Human2.2.bmp"]=("c3a2ca98531faddf34f0431f6ee574c880cab1b63e754fcefab62665da8d59ba",CaptionKind.Secretary),
        ["Graphic/Embedded/Promotion2.bmp"]=("7be71e872f1572b6ca34f51b713b14725987f6680c4eaef163dbf727a3c9eedf",CaptionKind.Television),
        ["Graphic/Recovered/resource-0BEA30.bmp"]=("7be71e872f1572b6ca34f51b713b14725987f6680c4eaef163dbf727a3c9eedf",CaptionKind.Television),
        ["Graphic/Embedded/SalesAchievement1.bmp"]=("cd90c91984b285314eaa90f2308a7c777c38f8339ce83662deb0f1b1380d503a",CaptionKind.Sales),
        ["Graphic/Embedded/SalesAchievement2.bmp"]=("5408aaf42e6cb6cf62d9f27eb5f11bb9802a76d90ec982e9b59e808ee6a9fd3f",CaptionKind.Sales),
        ["Graphic/Embedded/SalesAchievement3.bmp"]=("75200e805a6dd895690613114db1236685da0567f66bf6004091814d43d0ef0e",CaptionKind.Sales),
        ["Graphic/Embedded/SalesAchievement4.bmp"]=("4d83b70f255f4865121dea66b7d87962a73d51fd751975cf1c804a48712072ad",CaptionKind.Sales),
        ["Graphic/Embedded/CompanyActivity.bmp"]=("b0785e95880be0733e0b353c48a8c6f6bedb75fc3c0a41d4940f9d6bf78ed783",CaptionKind.Activity),
        ["Graphic/Embedded/HelpTitle.bmp"]=("7e757001444ac208ac36c36ea787a078f79e125fe05b83b714069f452bfb0a97",CaptionKind.Help),
        ["Graphic/Embedded/HardwareLaunchScene.bmp"]=("c8ac9409990d3b897fc7ef49204b99df3e47d0a3d9c20d5dae8cc478497c57c0",CaptionKind.Hardware),
        ["Graphic/Embedded/HallCelebration1.bmp"]=("c0ec11169f963601cfcd731bce62d1dd76b00e11fd1b51b61b079d611ffc23b8",CaptionKind.Hall),
        ["Graphic/Embedded/HallCelebration2.bmp"]=("58d4c987b7c89d67ca41c8ea0afed776e6ec878f1938713a74ff4a9752c0fa4b",CaptionKind.Hall),
        ["Graphic/Embedded/ExhibitionBooth.png"]=("39b4f664437429be73254e80936a74ad4b05184ecc29ad2f83aa476d247bacb4",CaptionKind.Exhibition),
        ["Graphic/Embedded/AwardsStage.png"]=("d81ae783132d1866c722c3aa4e4ada3de3bd1cf43cfa6bd15108fbc75ea06463",CaptionKind.Awards),
        ["Graphic/Embedded/Reviewer0.bmp"]=("3d7f97ee48d4cb9197c30ed65ad38d48fe4753531e186134056653ea4e5545c5",CaptionKind.Reviewer),
        ["Graphic/Embedded/Reviewer1.bmp"]=("3f8952f137f0d241cfe327991b4db341c1da4e2aec125ee5cf4c0fdfa98bbd2f",CaptionKind.Reviewer),
        ["Graphic/Embedded/Reviewer2.bmp"]=("31ff15d36a651aab32eecc72fbcf43d6ab5a880f123008bedd85d181f976501b",CaptionKind.Reviewer),
        ["Graphic/Embedded/Reviewer3.bmp"]=("ffc5e4af2eefc314f2005bad5822b4fd4eaf5a9dd078894303ed4cd4d65cc363",CaptionKind.Reviewer)
    };
    public static string? Ensure(string relative,string source)
    {
        if(!Sources.TryGetValue(relative,out var spec)||!File.Exists(source))return null;
        using var image=new Image();if(image.Load(source)!=Error.Ok||image.GetSize()!=(spec.Caption==CaptionKind.Title?new Vector2I(558,385):spec.Caption is CaptionKind.Reviewer or CaptionKind.Secretary?new Vector2I(64,64):spec.Caption==CaptionKind.Help?new Vector2I(250,171):spec.Caption==CaptionKind.Television?new Vector2I(256,152):new Vector2I(256,160)))return null;
        image.Convert(Image.Format.Rgb8);var pixels=image.GetData();
        if(Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant()!=spec.Hash)return null;
        string revision=spec.Caption==CaptionKind.Registration?"registration-labels-v1":spec.Caption==CaptionKind.Title?"title-logo-v2":spec.Caption==CaptionKind.Television?"television-screen-v2":"event-captions-m101-v1";
        string path=ProjectSettings.GlobalizePath("user://localized-artwork/en/"+revision+"/"+spec.Hash+"/"+System.IO.Path.GetFileNameWithoutExtension(relative)+".png");
        if(File.Exists(path))return path;
        using var patch=Image.CreateFromData(image.GetWidth(),image.GetHeight(),false,Image.Format.Rgb8,spec.Caption==CaptionKind.Registration ? LocalSceneCaptionRules.PatchRegistration(pixels) : spec.Caption==CaptionKind.Title ? LocalTitleArtworkRules.Patch(pixels) : spec.Caption==CaptionKind.Secretary ? LocalSceneCaptionRules.PatchSecretary(pixels) : spec.Caption==CaptionKind.Television ? LocalSceneCaptionRules.PatchTelevision(pixels) : spec.Caption==CaptionKind.Activity ? LocalSceneCaptionRules.PatchActivity(pixels) : spec.Caption==CaptionKind.Sales ? LocalSceneCaptionRules.PatchSales(pixels,relative[^5]-'0') : spec.Caption==CaptionKind.Help ? LocalSceneCaptionRules.PatchHelp(pixels) : spec.Caption==CaptionKind.Hardware ? LocalSceneCaptionRules.PatchHardware(pixels) : spec.Caption==CaptionKind.Reviewer ? LocalSceneCaptionRules.PatchReviewer(pixels,relative[^5]-'0') : spec.Caption==CaptionKind.Awards ? LocalSceneCaptionRules.PatchAwards(pixels) : LocalSceneCaptionRules.Patch(pixels,spec.Caption==CaptionKind.Hall));
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        if(patch.SavePng(path)!=Error.Ok)throw new IOException("Could not write the local English scene caption.");
        return path;
    }
}
