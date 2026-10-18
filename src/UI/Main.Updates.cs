using Godot;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public partial class Main
{
    private const string UpdatePreferences="user://updates.cfg";
    private HttpRequest? _releaseRequest;
    private CanvasLayer? _releaseNotice;
    private bool UpdateChecksEnabled()
    {var config=new ConfigFile();config.Load(UpdatePreferences);return config.GetValue("updates","enabled",true).AsBool();}
    private void SetUpdateChecksEnabled(bool enabled)
    {
        var config=new ConfigFile();config.Load(UpdatePreferences);config.SetValue("updates","enabled",enabled);config.Save(UpdatePreferences);
        if(!enabled){_releaseRequest?.CancelRequest();_releaseNotice?.QueueFree();_releaseNotice=null;}
    }
    private void CheckForReleaseUpdate()
    {
        // Automated/headless runs never contact GitHub. Startup is never awaited.
        if(!UpdateChecksEnabled())return;
        var config=new ConfigFile();config.Load(UpdatePreferences);
        long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),last=config.GetValue("updates","checked",0L).AsInt64();
        if(now>=last&&now-last<86400)
        {ShowReleaseNotice(ReleaseUpdateRules.Select(config.GetValue("updates","cache","[]").AsString(),BuildInfo.Version));return;}
        if(DisplayServer.GetName()=="headless")return;
        long attempted=config.GetValue("updates","attempted",0L).AsInt64();
        if(now>=attempted&&now-attempted<3600)return;
        config.SetValue("updates","attempted",now);config.Save(UpdatePreferences);
        _releaseRequest=new HttpRequest {Name="ReleaseUpdateRequest",Timeout=8,BodySizeLimit=1_048_576,MaxRedirects=2};AddChild(_releaseRequest);
        _releaseRequest.RequestCompleted+=(result,code,headers,body)=>
        {
            if(result!=(long)HttpRequest.Result.Success||code!=200||!UpdateChecksEnabled())return;
            string json=body.GetStringFromUtf8();
            // Load again: an opt-out or dismissal may have changed while waiting.
            var current=new ConfigFile();current.Load(UpdatePreferences);
            current.SetValue("updates","cache",json);current.SetValue("updates","checked",now);current.Save(UpdatePreferences);
            ShowReleaseNotice(ReleaseUpdateRules.Select(json,BuildInfo.Version));
        };
        _releaseRequest.Request("https://api.github.com/repos/UnSpotibleShadow/OpenGDS2/releases?per_page=100",["Accept: application/vnd.github+json","X-GitHub-Api-Version: 2022-11-28","User-Agent: OpenGDS2/"+BuildInfo.Version]);
    }
    private void ShowReleaseNotice(ReleaseNotice? release)
    {
        if(release is null||!UpdateChecksEnabled())return;
        var config=new ConfigFile();config.Load(UpdatePreferences);
        if(ReleaseUpdateRules.Compare(release.Version,config.GetValue("updates","notified","0.0.0").AsString()) is <=0)return;
        config.SetValue("updates","notified",release.Version);config.Save(UpdatePreferences);
        _releaseNotice?.QueueFree();_releaseNotice=new CanvasLayer {Name="ReleaseNoticeLayer",Layer=100};AddChild(_releaseNotice);
        var panel=new PanelContainer {Name="ReleaseNotice",Position=new(12,12),CustomMinimumSize=new(320,0)};_releaseNotice.AddChild(panel);
        var contents=new VBoxContainer();panel.AddChild(contents);
        contents.AddChild(new Label {Name="ReleaseNoticeText",Text=string.Format(Tr("UPDATE_AVAILABLE"),release.Version),AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new(300,0)});
        var buttons=new HBoxContainer();contents.AddChild(buttons);
        var view=new Button {Text=Tr("UPDATE_VIEW")};buttons.AddChild(view);view.Pressed+=()=>OS.ShellOpen(release.Url);
        var dismiss=new Button {Text=Tr("CLOSE")};buttons.AddChild(dismiss);dismiss.Pressed+=()=>{_releaseNotice?.QueueFree();_releaseNotice=null;};
    }
}
