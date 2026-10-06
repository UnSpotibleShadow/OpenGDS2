extends SceneTree
# A small real-time playback check, separate from accelerated simulation.
var failures := 0

func check(condition: bool, message: String) -> void:
    if not condition:
        failures += 1
        push_error("FAIL MEETING MUSIC: "+message)

func _initialize() -> void:
    call_deferred("run")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("meeting_music_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://music"))
    var silence := AudioStreamWAV.new()
    silence.format = AudioStreamWAV.FORMAT_16_BITS
    silence.mix_rate = 8000
    var samples := PackedByteArray()
    samples.resize(16000*5)
    silence.data = samples
    check(silence.save_to_wav("user://music/office-01.wav") == OK,"Write local office fixture")
    var music = load("res://src/Core/BackgroundMusic.cs").new()
    root.add_child(music)
    music.call("SetEnabled",true)
    music.call("SetGameplayActive",true)
    var office = music.find_child("MusicPlayer",true,false)
    var meeting = music.find_child("MeetingMusicPlayer",true,false)
    office.play(1.0)
    await create_timer(0.12).timeout
    for phase in range(4):
        music.call("SetMeetingPhase",phase)
        check(meeting.stream != null and meeting.stream.loop,"Looping meeting stream loads")
        check(office.stream_paused or not office.playing,"Office audio stays silent while meeting plays")
        meeting.play(1.0)
        await create_timer(0.12).timeout
        var position = meeting.get_playback_position()
        var office_position = office.get_playback_position()
        music.call("SetMeetingPhase",phase)
        await create_timer(0.12).timeout
        check(meeting.playing and meeting.get_playback_position() >= position,"Same-phase dialogue does not restart playback")
        check(absf(office.get_playback_position()-office_position) < 0.03,"Meeting pauses the office recording without advancing it")
        music.call("SetVolume",42.0)
        check(absf(meeting.volume_db-linear_to_db(0.42)) < 0.01,"Meeting uses shared music volume")
        music.call("SetEnabled",false)
        check(meeting.stream_paused and (office.stream_paused or not office.playing),"Mute silences both soundtracks")
        music.call("SetEnabled",true)
        check(not meeting.stream_paused and (office.stream_paused or not office.playing),"Unmute resumes only meeting audio")
        music.call("NextTrack")
        check(meeting.playing and meeting.stream != null,"Next office track cannot replace meeting audio")
        music.call("SetMeetingPhase",-1)
        check(not meeting.playing and meeting.stream == null and not office.stream_paused,"Dialogue completion stops meeting and restores office audio")
    music.call("SetMeetingPhase",0)
    music.call("SetTitleActive")
    check(meeting.stream == null and office.stream.resource_path.ends_with("title-theme.ogg"),"Returning to title clears meeting audio")
    music.queue_free()
    await process_frame
    await process_frame
    print("Meeting music playback checks complete; failures=",failures)
    quit(0 if failures == 0 else 1)
