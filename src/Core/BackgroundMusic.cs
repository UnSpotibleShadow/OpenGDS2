using Godot;
using System;
using System.Collections.Generic;

namespace OpenGDS2.Core;

/// <summary>Optional community soundtrack; independent of the original simulation clock.</summary>
public partial class BackgroundMusic : Node
{
    public bool Enabled { get; private set; } = true;
    public float VolumePercent { get; private set; } = 30;
    public bool SeasonalEnabled { get; private set; } = true;
    public MusicSeason? CurrentSeason { get; private set; }
    public int TrackIndex { get; private set; }
    public int TrackCount => _tracks.Count;
    private List<AudioStream> _tracks = new();
    private readonly List<AudioStream> _officeTracks = new();
    private readonly Dictionary<MusicSeason, List<AudioStream>> _seasonTracks = new();
    private readonly HashSet<AudioStream> _ownedTracks = new();
    private AudioStreamPlayer _player = null!;
    private bool _gameplayActive;
    private bool _titleActive;
    private AudioStreamOggVorbis? _titleTheme;
    private const string SettingsPath = "user://audio.cfg";
    public const string DirectoryPath = "user://music";

    public override void _Ready()
    {
        var settings = new ConfigFile();
        if (settings.Load(SettingsPath) == Error.Ok)
        {
            Enabled = settings.GetValue("music", "enabled", true).AsBool();
            SeasonalEnabled = settings.GetValue("music", "seasonal", true).AsBool();
            float value = (float)settings.GetValue("music", "volume", 30f).AsDouble();
            VolumePercent = float.IsFinite(value) ? Math.Clamp(value, 0, 100) : 30;
        }
        _player = new AudioStreamPlayer { Name = "MusicPlayer" }; AddChild(_player);
        _titleTheme = GD.Load<AudioStreamOggVorbis>("res://audio/community/title-theme.ogg");
        if (_titleTheme is not null) _titleTheme.Loop = true;
        // A valid local playlist overrides the optional bundled community music.
        DirAccess.MakeDirRecursiveAbsolute(DirectoryPath);
        _officeTracks.AddRange(LoadLocalPlaylist(DirectoryPath));
        bool localOffice = _officeTracks.Count > 0;
        var bundled = new Dictionary<int, AudioStream>();
        for (int index = 1; index <= 8; index++)
        {
            var track = GD.Load<AudioStreamOggVorbis>($"res://audio/community/office-{index:00}.ogg");
            if (track is null) continue;
            if (track.GetLength() <= 0) { track.Dispose(); continue; }
            track.Loop = false;
            bundled[index] = track; _ownedTracks.Add(track);
            if (!localOffice) _officeTracks.Add(track);
        }
        foreach (var season in Enum.GetValues<MusicSeason>())
        {
            var tracks = LoadLocalPlaylist($"{DirectoryPath}/{SeasonalMusicRules.Folder(season)}");
            if (tracks.Count == 0 && localOffice) tracks = _officeTracks;
            else if (tracks.Count == 0)
            {
                int first = SeasonalMusicRules.FirstBundledTrack(season);
                for (int index = first; index < first + 2; index++)
                    if (bundled.TryGetValue(index, out var track)) tracks.Add(track);
                if (tracks.Count == 0) tracks = _officeTracks;
            }
            _seasonTracks[season] = tracks;
        }
        _tracks = _officeTracks;
        _player.Finished += NextTrack;
        ApplyVolume();
    }

    private List<AudioStream> LoadLocalPlaylist(string directory)
    {
        var tracks = new List<AudioStream>();
        if (!DirAccess.DirExistsAbsolute(directory)) return tracks;
        var filenames = DirAccess.GetFilesAt(directory);
        Array.Sort(filenames, StringComparer.Ordinal);
        foreach (string filename in filenames)
        {
            string path = $"{directory}/{filename}";
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
            tracks.Add(track); _ownedTracks.Add(track);
        }
        return tracks;
    }

    public override void _ExitTree()
    {
        _player.Stop(); _player.Stream = null;
        foreach (var track in _ownedTracks) track.Dispose();
        _ownedTracks.Clear(); _seasonTracks.Clear(); _officeTracks.Clear(); _tracks.Clear();
        _titleTheme?.Dispose(); _titleTheme = null;
    }

    public void SetGameplayActive(bool active)
    {
        _gameplayActive = active;
        _titleActive = false;
        UpdatePlayback();
    }

    public void SetTitleActive()
    {
        _gameplayActive = false;
        _titleActive = true;
        UpdatePlayback();
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled; UpdatePlayback(); SaveSettings();
    }

    public void SetMonth(int month)
    {
        CurrentSeason = SeasonalMusicRules.ForMonth(month);
        SelectPlaylist();
    }

    public void SetSeasonalEnabled(bool enabled)
    {
        SeasonalEnabled = enabled; SelectPlaylist(); SaveSettings();
    }

    private void SelectPlaylist()
    {
        var desired = SeasonalEnabled && CurrentSeason is MusicSeason season
            ? _seasonTracks[season] : _officeTracks;
        // Same-season ticks, dialogs and saves never restart the current recording.
        if (!ReferenceEquals(_tracks, desired)) { _tracks = desired; TrackIndex = 0; }
        UpdatePlayback();
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
        // Advancing the office playlist never replaces the title theme.
        if (_gameplayActive) { _player.Stop(); _player.Stream = _tracks[TrackIndex]; UpdatePlayback(); }
    }

    private void UpdatePlayback()
    {
        AudioStream? desired = _titleActive ? _titleTheme :
            _gameplayActive && _tracks.Count > 0 ? _tracks[TrackIndex] : null;
        if (_player.Stream != desired) { _player.Stop(); _player.Stream = desired; }
        _player.StreamPaused = !Enabled || desired is null;
        // Headless validation has no audible output. Avoid starting asynchronous
        // dummy-driver playback during accelerated simulation fixtures.
        if (Enabled && desired is not null && !_player.Playing && DisplayServer.GetName() != "headless") _player.Play();
    }

    private void ApplyVolume() => _player.VolumeDb = VolumePercent <= 0 ? -80 : Mathf.LinearToDb(VolumePercent / 100);

    private void SaveSettings()
    {
        var settings = new ConfigFile(); settings.SetValue("music", "enabled", Enabled);
        settings.SetValue("music", "volume", VolumePercent);
        settings.SetValue("music", "seasonal", SeasonalEnabled);
        var result = settings.Save(SettingsPath);
        if (result != Error.Ok) GD.PushWarning($"Audio settings could not be saved: {result}");
    }
}
