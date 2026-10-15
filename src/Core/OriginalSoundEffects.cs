using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.Core;

/// <summary>Locally imported native asynchronous effects on one replaceable voice.</summary>
public partial class OriginalSoundEffects : Node
{
    public bool Enabled {get;private set;}=true;
    public float VolumePercent {get;private set;}=70;
    public int PlayCount {get;private set;}
    public int LastCue {get;private set;}
    private AudioStreamPlayer _player=null!;
    private readonly Dictionary<OriginalSoundCue,AudioStreamWav> _clips=new();
    private const string Settings="user://audio.cfg";
    public override void _Ready()
    {
        var config=new ConfigFile();if(config.Load(Settings)==Error.Ok)
        {
            Enabled=config.GetValue("effects","enabled",true).AsBool();float volume=(float)config.GetValue("effects","volume",70f).AsDouble();
            VolumePercent=float.IsFinite(volume)?Math.Clamp(volume,0,100):70;
        }
        _player=new AudioStreamPlayer {Name="OriginalEffectsPlayer"};AddChild(_player);ApplyVolume();
    }
    public void Play(OriginalSoundCue cue)
    {
        if(!Enabled || !Enum.IsDefined(cue))return;
        if(!_clips.TryGetValue(cue,out var clip))
        {
            string filename=cue switch {OriginalSoundCue.Gold=>"Gold.wav",OriginalSoundCue.Up=>"Up.wav",OriginalSoundCue.Explosion=>"Bakuhatu.wav",_=>"Fire.wav"};
            string directory=ProjectSettings.GlobalizePath("user://original/Sound");
            try
            {
                if(!System.IO.Directory.Exists(directory))return;
                string? path=System.IO.Directory.EnumerateFiles(directory).FirstOrDefault(p=>System.IO.Path.GetFileName(p).Equals(filename,StringComparison.OrdinalIgnoreCase));
                if(path is null)return;
                clip=AudioStreamWav.LoadFromFile(path);if(clip is null)return;
                if(clip.GetLength()<=0){clip.Dispose();return;}clip.LoopMode=AudioStreamWav.LoopModeEnum.Disabled;_clips.Add(cue,clip);
            }
            catch(System.IO.IOException){return;}catch(UnauthorizedAccessException){return;}
        }
        _player.Stop();_player.Stream=clip;LastCue=(int)cue;PlayCount++;_player.Play();
    }
    public void Stop()=>_player?.Stop();
    public void SetEnabled(bool enabled){Enabled=enabled;if(!enabled)Stop();Save();}
    public void SetVolume(float volume){VolumePercent=float.IsFinite(volume)?Math.Clamp(volume,0,100):70;ApplyVolume();Save();}
    private void ApplyVolume(){if(_player is not null)_player.VolumeDb=VolumePercent<=0?-80:Mathf.LinearToDb(VolumePercent/100);}
    private void Save(){var config=new ConfigFile();config.Load(Settings);config.SetValue("effects","enabled",Enabled);config.SetValue("effects","volume",VolumePercent);config.Save(Settings);}
    public override void _ExitTree(){Stop();if(_player is not null)_player.Stream=null;foreach(var clip in _clips.Values)clip.Dispose();_clips.Clear();}
}
