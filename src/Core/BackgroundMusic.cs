using Godot;
using System;
using System.Collections.Generic;

namespace OpenGDS2.Core;

/// <summary>Optional community soundtrack; independent of the original simulation clock.</summary>
public partial class BackgroundMusic : Node
{
    public bool Enabled { get; private set; } = true;
    public float VolumePercent { get; private set; } = 30;
    public int TrackIndex { get; private set; }
    public int TrackCount => _tracks.Count;
    private readonly List<AudioStream> _tracks = new();
    private AudioStreamPlayer _player = null!;
    private bool _gameplayActive;
    private const string SettingsPath = "user://audio.cfg";
    public const string DirectoryPath = "user://music";

    public override void _Ready()
    {
        var settings = new ConfigFile();
        if (settings.Load(SettingsPath) == Error.Ok)
        {
            Enabled = settings.GetValue("music", "enabled", true).AsBool();
            float value = (float)settings.GetValue("music", "volume", 30f).AsDouble();
            VolumePercent = float.IsFinite(value) ? Math.Clamp(value, 0, 100) : 30;
        }
        _player = new AudioStreamPlayer { Name = "MusicPlayer" }; AddChild(_player);
        // A valid local playlist overrides the optional bundled community music.
        DirAccess.MakeDirRecursiveAbsolute(DirectoryPath);
        var filenames = DirAccess.GetFilesAt(DirectoryPath);
        Array.Sort(filenames, StringComparer.Ordinal);
        foreach (string filename in filenames)
        {
            string path = $"{DirectoryPath}/{filename}";
            AudioStream? track = System.IO.Path.GetExtension(filename).ToLowerInvariant() switch
            {
                ".ogg" => AudioStreamOggVorbis.LoadFromFile(path),
                ".wav" => AudioStreamWav.LoadFromFile(path),
                ".mp3" => AudioStreamMP3.LoadFromFile(path),
                _ => null
            };
            if (track is null) continue;
            if (track.GetLength() <= 0) { track.Dispose(); continue; }
            if (track is AudioStreamOggVorbis ogg) ogg.Loop = false;
            if (track is AudioStreamWav wav) wav.LoopMode = AudioStreamWav.LoopModeEnum.Disabled;
            if (track is AudioStreamMP3 mp3) mp3.Loop = false;
            _tracks.Add(track);
        }
        if (_tracks.Count == 0)
        {
            for (int index = 1; index <= 8; index++)
            {
                var track = GD.Load<AudioStreamOggVorbis>($"res://audio/community/office-{index:00}.ogg");
                if (track is null) continue;
                if (track.GetLength() <= 0) { track.Dispose(); continue; }
                track.Loop = false;
                _tracks.Add(track);
            }
        }
        _player.Finished += NextTrack;
        ApplyVolume();
    }

    public override void _ExitTree()
    {
        _player.Stop(); _player.Stream = null;
        foreach (var track in _tracks) track.Dispose();
        _tracks.Clear();
    }

    public void SetGameplayActive(bool active)
    {
        _gameplayActive = active;
        UpdatePlayback();
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled; UpdatePlayback(); SaveSettings();
    }

    public void SetVolume(float percent)
    {
        if (!float.IsFinite(percent)) throw new ArgumentOutOfRangeException(nameof(percent));
        VolumePercent = Math.Clamp(percent, 0, 100); ApplyVolume(); SaveSettings();
    }

    public void NextTrack()
    {
        if (_tracks.Count == 0) return;
        TrackIndex = (TrackIndex + 1) % _tracks.Count;
        _player.Stop(); _player.Stream = _tracks[TrackIndex]; UpdatePlayback();
    }

    private void UpdatePlayback()
    {
        if (_tracks.Count == 0) return;
        _player.Stream ??= _tracks[TrackIndex];
        _player.StreamPaused = !Enabled || !_gameplayActive;
        // Headless validation has no audible output. Avoid starting asynchronous
        // dummy-driver playback during accelerated simulation fixtures.
        if (Enabled && _gameplayActive && !_player.Playing && DisplayServer.GetName() != "headless") _player.Play();
    }

    private void ApplyVolume() => _player.VolumeDb = VolumePercent <= 0 ? -80 : Mathf.LinearToDb(VolumePercent / 100);

    private void SaveSettings()
    {
        var settings = new ConfigFile(); settings.SetValue("music", "enabled", Enabled);
        settings.SetValue("music", "volume", VolumePercent);
        var result = settings.Save(SettingsPath);
        if (result != Error.Ok) GD.PushWarning($"Audio settings could not be saved: {result}");
    }
}
