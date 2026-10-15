extends "res://tests/casino_layout_smoke.gd"

func authored_wave(path: String) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var f=FileAccess.open(path,FileAccess.WRITE);var samples=2205
    f.store_buffer("RIFF".to_ascii_buffer());f.store_32(36+samples*2);f.store_buffer("WAVEfmt ".to_ascii_buffer());f.store_32(16);f.store_16(1);f.store_16(1);f.store_32(22050);f.store_32(44100);f.store_16(2);f.store_16(16);f.store_buffer("data".to_ascii_buffer());f.store_32(samples*2)
    for i in range(samples):f.store_16(int(1000*sin(i*.1))&65535)
    f.close()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    for name in ["Gold.wav","UP.WAV","Fire.wav","Bakuhatu.wav"]:authored_wave("user://original/Sound/"+name)
    var prefs=ConfigFile.new();prefs.set_value("music","enabled",false);prefs.set_value("effects","enabled",true);prefs.set_value("effects","volume",65.0);check(prefs.save("user://audio.cfg")==OK,"Authored audio preferences")
    await load_fixture(casino_value());await press_node("TogglePause");await open_casino("CasinoPoker")
    var effects=main.find_child("OriginalSoundEffects",true,false);var player=effects.find_child("OriginalEffectsPlayer",true,false)
    check(effects.get("PlayCount")==0,"Opening a saved view does not replay audio")
    await press_node("PokerBetCollect")
    check(effects.get("LastCue")==1 and effects.get("PlayCount")==1,"Bet transfer plays local Gold independently of music")
    check(player.stream is AudioStreamWAV and player.stream.loop_mode==AudioStreamWAV.LOOP_DISABLED,"Original SND_ASYNC is a single nonlooping WAV voice")
    await press_node("PokerDealDouble")
    for frame in range(110):await process_frame
    check(effects.get("LastCue")==2 and effects.get("PlayCount")==6,"Initial five-card reveal plays five Up cues")
    effects.call("SetEnabled",false);var count=effects.get("PlayCount");effects.call("Play",4)
    check(not player.playing and effects.get("PlayCount")==count,"Muting stops effects and suppresses new requests")
    effects.call("SetVolume",23.0);effects.call("SetEnabled",true);effects.call("Play",4)
    check(effects.get("LastCue")==4 and effects.get("PlayCount")==count+1,"Native Fire loads on the same replaceable voice")
    await press_node("ToolbarSAVE_GAME");await press_node("ToolbarLOAD_GAME")
    check(not player.playing,"Loading stops outstanding transient effects")
    await dispose()
    await load_fixture(casino_value());effects=main.find_child("OriginalSoundEffects",true,false)
    check(effects.get("Enabled") and effects.get("VolumePercent")==23.0,"Effect preferences survive scene recreation")
    # Unsupported/missing filenames are never replaced with a fabricated sound.
    DirAccess.remove_absolute(ProjectSettings.globalize_path("user://original/Sound/Fire.wav"));effects.call("Play",4);check(effects.get("PlayCount")==0,"Missing local WAV is a silent no-op")
    await dispose()
    print("Original audio smoke failures: ",failures);quit(0 if failures==0 else 1)
