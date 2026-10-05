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
    private AudioStreamPlayer _fadePlayer = null!;
    private bool _crossfading;
    private ulong _fadeStarted;
    private float _incomingGain = 1;
    private float _outgoingGain;
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
        _fadePlayer = new AudioStreamPlayer { Name = "MusicFadePlayer" }; AddChild(_fadePlayer);
        _fadePlayer.Finished += CompleteCrossfade;
        _titleTheme = GD.Load<AudioStreamOggVorbis>("res://audio/community/title-theme.ogg");
        if (_titleTheme is not null) _titleTheme.Loop = true;
        // A valid local playlist overrides the optional bundled community music.
        DirAccess.MakeDirRecursiveAbsolute(DirectoryPath);
        _officeTracks.AddRange(LoadLocalPlaylist(DirectoryPath));
        bool localOffice = _officeTracks.Count > 0;
        if (!localOffice) _officeTracks.AddRange(LoadBundledPlaylist(SeasonalMusicRules.BundledPaths(MusicSeason.Summer)));
        foreach (var season in Enum.GetValues<MusicSeason>())
        {
            var tracks = LoadLocalPlaylist($"{DirectoryPath}/{SeasonalMusicRules.Folder(season)}");
            if (tracks.Count == 0 && localOffice) tracks = _officeTracks;
            else if (tracks.Count == 0)
            {
                tracks = LoadBundledPlaylist(SeasonalMusicRules.BundledPaths(season));
                if (tracks.Count == 0) tracks = _officeTracks;
            }
            _seasonTracks[season] = tracks;
        }
        // Nonseasonal mode includes all bundled recordings, with no duplicate summer tracks.
        if (!localOffice)
            foreach (var season in new[] { MusicSeason.Spring, MusicSeason.Autumn, MusicSeason.Winter })
                foreach (var track in LoadBundledPlaylist(SeasonalMusicRules.BundledPaths(season))) _officeTracks.Add(track);
        _tracks = _officeTracks;
        _player.Finished += NextTrack;
        ApplyVolume();
    }

    private List<AudioStream> LoadBundledPlaylist(IEnumerable<string> paths)
    {
        var tracks = new List<AudioStream>();
        foreach (string path in paths)
        {
            if (!ResourceLoader.Exists($"res://{path}")) continue;
            var track = GD.Load<AudioStreamOggVorbis>($"res://{path}");
            if (track is null) continue;
            if (track.GetLength() <= 0) { track.Dispose(); continue; }
            track.Loop = false; tracks.Add(track); _ownedTracks.Add(track);
        }
        return tracks;
    }

    public override void _Process(double delta)
    {
        if (!_crossfading) return;
        // Audio transitions use real time, independent of accelerated game-clock fixtures.
        double elapsed = (Time.GetTicksUsec() - _fadeStarted) / 1_000_000d;
        if (elapsed >= SeasonalMusicRules.CrossfadeSeconds) { CompleteCrossfade(); return; }
        (_incomingGain, _outgoingGain) = SeasonalMusicRules.CrossfadeGains(elapsed);
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
        _fadePlayer.Stop(); _fadePlayer.Stream = null;
        foreach (var track in _ownedTracks) track.Dispose();
        _ownedTracks.Clear(); _seasonTracks.Clear(); _officeTracks.Clear(); _tracks.Clear();
        _titleTheme?.Dispose(); _titleTheme = null;
    }

    public void SetGameplayActive(bool active)
    {
        if (!active || _titleActive) CompleteCrossfade();
        _gameplayActive = active;
        _titleActive = false;
        UpdatePlayback();
    }

    public void SetTitleActive()
    {
        CompleteCrossfade();
        _gameplayActive = false;
        _titleActive = true;
        UpdatePlayback();
    }

    public void SetEnabled(bool enabled)
    {
        if (!enabled) CompleteCrossfade();
        Enabled = enabled; UpdatePlayback(); SaveSettings();
    }

    public void SetMonth(int month)
    {
        var season = SeasonalMusicRules.ForMonth(month);
        bool changed = CurrentSeason.HasValue && season.HasValue && season != CurrentSeason;
        CurrentSeason = season;
        SelectPlaylist(changed);
    }

    public void SetSeasonalEnabled(bool enabled)
    {
        SeasonalEnabled = enabled; SelectPlaylist(); SaveSettings();
    }

    private void SelectPlaylist(bool seasonalTransition = false)
    {
        var desired = SeasonalEnabled && CurrentSeason is MusicSeason season
            ? _seasonTracks[season] : _officeTracks;
        // Same-season ticks, dialogs and saves never restart the current recording.
        bool changed = !ReferenceEquals(_tracks, desired);
        if (changed) { _tracks = desired; TrackIndex = 0; }
        if (changed && seasonalTransition && Enabled && _gameplayActive && _tracks.Count > 0 && _player.Stream is not null)
            BeginCrossfade(_tracks[TrackIndex]);
        else if (changed) CompleteCrossfade();
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
        CompleteCrossfade();
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

    private void BeginCrossfade(AudioStream incoming)
    {
        if (_player.Stream == incoming) return;
        // Retargeting keeps at most two voices; the previous fading voice is released.
        CompleteCrossfade();
        var outgoing = _player.Stream;
        float position = _player.GetPlaybackPosition();
        bool audible = _player.Playing;
        _fadePlayer.Stream = outgoing;
        _player.Stop(); _player.Stream = incoming;
        _fadeStarted = Time.GetTicksUsec(); _crossfading = true;
        _incomingGain = 0; _outgoingGain = 1; ApplyVolume();
        _fadePlayer.StreamPaused = !Enabled;
        if (audible && DisplayServer.GetName() != "headless") _fadePlayer.Play(position);
    }

    private void CompleteCrossfade()
    {
        _crossfading = false; _incomingGain = 1; _outgoingGain = 0;
        _fadePlayer.Stop(); _fadePlayer.Stream = null; ApplyVolume();
    }

    private void ApplyVolume()
    {
        float volume = VolumePercent / 100;
        _player.VolumeDb = volume * _incomingGain <= 0 ? -80 : Mathf.LinearToDb(volume * _incomingGain);
        _fadePlayer.VolumeDb = volume * _outgoingGain <= 0 ? -80 : Mathf.LinearToDb(volume * _outgoingGain);
    }

    private void SaveSettings()
    {
        var settings = new ConfigFile(); settings.SetValue("music", "enabled", Enabled);
        settings.SetValue("music", "volume", VolumePercent);
        settings.SetValue("music", "seasonal", SeasonalEnabled);
        var result = settings.Save(SettingsPath);
        if (result != Error.Ok) GD.PushWarning($"Audio settings could not be saved: {result}");
    }
}
