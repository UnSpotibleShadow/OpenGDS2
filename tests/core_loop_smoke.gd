extends SceneTree
# Author-created synthetic data only. Always use disposable XDG_DATA_HOME.
var main: Node
var failures := 0
var seen_dialogue: Dictionary = {}
var moved_meeting_picture_position = Vector2(20,180)
var heard_meeting_phases: Dictionary = {}

func check_meeting_music(phase: String) -> void:
    var music = main.find_child("MeetingMusicPlayer",true,false)
    check(music != null and music.stream != null and music.stream.resource_path.ends_with("/meetings/"+phase+".ogg"),"Meeting dialogue uses its phase track: "+phase)
    if music != null and music.stream != null:
        check(music.stream.loop,"Meeting track loops through a long dialogue sequence")
    var office_music = main.find_child("MusicPlayer",true,false)
    check(office_music.stream_paused or not office_music.playing,"Office soundtrack stays silent during meeting dialogue")
    heard_meeting_phases[phase] = true

func check(condition: bool, message: String) -> void:
    if not condition:
        failures += 1
        push_error("FAIL CORE UI: " + message)

func press(key: String) -> void:
    var found := false
    for button in main.find_children("*", "Button", true, false):
        if button.text == tr(key) and button.is_visible_in_tree():
            check(not button.disabled, "Enabled: " + key)
            if not button.disabled:
                button.pressed.emit()
            found = true
            break
    check(found, "Button exists: " + key)
    await process_frame

func press_node(name: String) -> void:
    var button = main.find_child(name, true, false)
    check(button != null and not button.disabled, name)
    if button != null and not button.disabled:
        button.pressed.emit()
    await process_frame

func write_json(path: String, value: Variant) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var file := FileAccess.open(path, FileAccess.WRITE)
    file.store_string(JSON.stringify(normalize_numbers(value)))

func normalize_numbers(value: Variant) -> Variant:
    # Godot parses JSON numbers as doubles; System.Text.Json integer fields
    # require integer tokens when this fixture is written back.
    if value is Dictionary:
        for key in value:
            value[key] = normalize_numbers(value[key])
    elif value is Array:
        for index in range(value.size()):
            value[index] = normalize_numbers(value[index])
    elif typeof(value) == TYPE_FLOAT and value == floor(value):
        return int(value)
    return value

func await_node(name: String) -> bool:
    for frame in range(15000):
        if main.find_child(name, true, false) != null:
            return true
        if main.find_child("ConversationWindow",true,false) != null:
            await finish_conversation()
        await process_frame
    check(false, "Timed out waiting for " + name)
    return false

func advance_conversation_page() -> void:
    var message = main.find_child("ConversationMessage",true,false)
    check(message != null,"Conversation page exists")
    if message == null:
        return
    var previous_position = main.find_child("ConversationWindow",true,false).position
    var meeting_music = main.find_child("MeetingMusicPlayer",true,false)
    var previous_music = meeting_music.stream
    if previous_music != null:
        check_meeting_music(previous_music.resource_path.get_file().get_basename())
    else:
        check(not main.find_child("MusicPlayer",true,false).stream_paused,"Ordinary dialogue keeps the office soundtrack")
    seen_dialogue[message.text] = int(seen_dialogue.get(message.text,0)) + 1
    var accept := InputEventKey.new()
    accept.keycode = KEY_ENTER
    accept.pressed = true
    message.gui_input.emit(accept)
    await process_frame

    if main.find_child("ConversationWindow",true,false) != null:
        check(meeting_music.stream == previous_music,"Consecutive dialogue pages retain their meeting music stream")
    else:
        check(meeting_music.stream == null,"Meeting music stops on dialogue completion")
    var next_window = main.find_child("ConversationWindow",true,false)
    if next_window != null:
        check(next_window.position == previous_position,"Advancing a dialogue preserves its window position")
    var meeting_picture = main.find_child("PlanningDiscussionScene",true,false)
    if meeting_picture != null:
        check(meeting_picture.position == moved_meeting_picture_position,"Meeting illustration retains its chosen position")
    await process_frame
    if is_instance_valid(message) and message.is_inside_tree():
        await create_timer(0.03).timeout # Complete the original 20ms reveal tick.
        message.gui_input.emit(accept)
    await process_frame

func finish_conversation() -> void:
    var pages := 0
    while main.find_child("ConversationWindow",true,false) != null and pages < 70:
        await advance_conversation_page()
        pages += 1
    check(main.find_child("ConversationWindow",true,false) == null,"Conversation sequence finishes within its verified page bound")

func _initialize() -> void:
    call_deferred("run")

func create_music_fixture() -> void:
    # Community-authored silent WAVs exercise local loading in exported builds.
    # No supplied recordings or original-game audio are required by public CI.
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://music"))
    for index in range(8):
        var track := AudioStreamWAV.new()
        track.format = AudioStreamWAV.FORMAT_16_BITS
        track.mix_rate = 8000
        var samples := PackedByteArray()
        samples.resize(16000 * (index + 1))
        track.data = samples
        check(track.save_to_wav("user://music/office-%02d.wav" % (index+1)) == OK,"Write local music fixture")
    # Autumn deliberately has no local seasonal tracks: exercise generic fallback.
    for season in ["spring", "summer", "winter"]:
        DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://music/" + season))
        for index in range(2):
            var track := AudioStreamWAV.new()
            track.format = AudioStreamWAV.FORMAT_16_BITS
            track.mix_rate = 8000
            var samples := PackedByteArray()
            samples.resize(16000 * (11 + index + ["spring", "summer", "winter"].find(season) * 2))
            track.data = samples
            check(track.save_to_wav("user://music/%s/track-%d.wav" % [season,index]) == OK,"Write seasonal fixture")

func run() -> void:
    var expected_debug := OS.get_environment("OPENGDS2_EXPECT_DEBUG")
    if expected_debug in ["0", "1"]:
        check(OS.is_debug_build() == (expected_debug == "1"), "Runtime matches requested export configuration")
    # Set after display initialization: the dummy window otherwise starts at 64².
    root.size = Vector2i(1280,720)
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("core_loop_smoke requires disposable XDG_DATA_HOME")
        quit(1)
        return
    var raw_type := {"1378":5,"1308":10,"13B0":1,"12EC":3}
    var catalog := {
        "SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6",
        "Hardware":[{"SourceIndex":0,"Name":"Synthetic Hardware","RawMembers":{"0F00":8,"0FE0":20,"0FFC":10,"0EC8":0,"0FA8":4,"1050":40,"0F54":15,"10A4":0,"10C0":0},
            "ProductionBands":[{"SizeThreshold":16,"UnitCost":300},{"SizeThreshold":32,"UnitCost":700},{"SizeThreshold":48,"UnitCost":1200},{"SizeThreshold":64,"UnitCost":1800}]}],
        "Audiences":[{"SourceIndex":0,"Name":"Synthetic Audience","RawMembers":{"14A8":1,"1454":10,"148C":20}}],
        "DevelopmentComputers":[{"SourceIndex":0,"Name":"Synthetic Computer","BitWidth":8}],
        "GameTypes":[{"SourceIndex":0,"Name":"Synthetic Type A","ExampleTitles":["Synthetic Example A","Synthetic Example B","Synthetic Example C"],"RawMembers":raw_type,"RawTable13CC":[]},
            {"SourceIndex":1,"Name":"Synthetic Type B","RawMembers":raw_type,"RawTable13CC":[]}],
        "Technologies":[{"SourceIndex":0,"Identifier":0,"Name":"Synthetic Technology","PurchaseFee":50,"InitialOwnershipLevel":0,"RequiredTrainingWork":[40,60,80],"ExperienceCosts":[10,20,30],"RawTable158C":[1,2,3],"RawTable1568":[1,2,3]}],
        "RivalCompanies":["Synthetic rival company A","Synthetic rival company B"],
        "Secretaries":[],
        "Manga":[{"SourceIndex":0,"Title":"Synthetic Comic","Author":"Synthetic Author","Publisher":"Synthetic Publisher","PopularityDescription":"Synthetic Popularity","Popularity":10,"RightsFee":100}],
        "Genres":[{"SourceIndex":0,"Name":"Synthetic Genre","RawMembers":{"1224":7,"1208":10,"1240":1,"125C":0}}]
    }
    for index in range(5):
        catalog.Secretaries.append({"SourceIndex":index,"Name":"Authored secretary %d" % index,"RankCode":min(index,3),
            "ContractFee":100+index,"AdvertisedSalary":10+index,"PortraitId":[1,2,4,5,6][index],"Description":"Authored secretary biography %d" % index})
    # Genre helper validates its fifteen-column table even for broad audience.
    for type in catalog.GameTypes:
        for column in range(15):
            type.RawTable13CC.append(1)
    var roster := {}
    for cell in range(65):
        roster[str(cell)] = 0
    var worker := {"RawParameter34":1,"TechnologyLevels":{"0":0},"Name":"Synthetic Worker","SourceIndex":0,"PortraitId":1,"Level":1,"Experience":0,"Stress":0,
        "Fun":80,"Novelty":80,"Graphics":80,"Music":80,"Accuracy":80,"DevelopmentSpeed":80,"Inspiration":80,
        "Office":{"Action":6,"SeatedAction":6,"WorkEngaged":true,"MovementState":0,"Facing":1,"Frame":13}}
    var state := {"TechnologyOwnershipLevels":{"0":0},"Money":10000000,"Year":1,"Month":4,"Week":1,"DevelopmentComputerIndex":0,"HardwareLicenses":[0],"AvailableHardware":[0],
        "SecretarySourceIndex":1,"Company":{"Name":"Synthetic UI Company","PresidentName":"Tester","Fame":12,"Rank":0,"ProductionCapacity":50,"OriginalMember90":23,"ActivityCost":5000000,"WeeklyNet":0,"MonthlyNet":0,"YearlyNet":0},
        "Employees":[worker],"OriginalAccuracyCells":{"99":0},"OriginalTypeRosterMap":roster,"SalesFee1":1500,"SalesFee2":1500,"CompanyActivityTicks":0,"CompanyActivityPerformed":false}
    write_json("user://original/catalog.json",catalog)
    write_json("user://saves/slot1.json",{"Format":"OpenGDS2","SchemaVersion":1,"State":state})
    # Authored flat 24-bit BMP exercises actual office hit handling in CI.
    # Keep all source pixels synthetic; no original graphics are distributed.
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("user://original/Graphic/Embedded"))
    var office_width := 466
    var office_height := 290
    var row_stride := int(ceil(office_width*3.0/4.0))*4
    var bmp := PackedByteArray()
    bmp.resize(54+row_stride*office_height)
    bmp[0]=66
    bmp[1]=77
    bmp.encode_u32(2,bmp.size())
    bmp.encode_u32(10,54)
    bmp.encode_u32(14,40)
    bmp.encode_u32(18,office_width)
    bmp.encode_u32(22,office_height)
    bmp.encode_u16(26,1)
    bmp.encode_u16(28,24)
    bmp.encode_u32(34,row_stride*office_height)
    for y in range(office_height):
        for x in range(office_width):
            var pixel_offset := 54+y*row_stride+x*3
            bmp[pixel_offset]=178
            bmp[pixel_offset+1]=204
            bmp[pixel_offset+2]=178
    var office_file := FileAccess.open("user://original/Graphic/Embedded/OfficeBase.bmp",FileAccess.WRITE)
    office_file.store_buffer(bmp)
    office_file.close()
    var meeting_bmp = bmp.duplicate()
    meeting_bmp.resize(54+256*3*160)
    meeting_bmp.encode_u32(2,meeting_bmp.size())
    meeting_bmp.encode_u32(18,256)
    meeting_bmp.encode_u32(22,160)
    meeting_bmp.encode_u32(34,256*3*160)
    var meeting_file = FileAccess.open("user://original/Graphic/Embedded/PlanningMeeting.bmp",FileAccess.WRITE)
    meeting_file.store_buffer(meeting_bmp)
    meeting_file.close()
    create_music_fixture()
    var fresh_audio := ConfigFile.new()
    fresh_audio.set_value("music", "enabled", true)
    fresh_audio.set_value("music", "volume", 30)
    fresh_audio.set_value("music", "seasonal", true)
    check(fresh_audio.save("user://audio.cfg") == OK,"Reset audio fixture for repeated release/debug runs")
    main = load("res://scenes/Main.tscn").instantiate()
    root.add_child(main)
    await process_frame
    var title_player = main.find_child("MusicPlayer",true,false)
    check(title_player.stream != null and title_player.stream.resource_path.ends_with("title-theme.ogg") and title_player.stream.loop,"Dedicated title theme selected even with a local office playlist")
    var original_viewport_size = root.size
    root.size = Vector2i(1920,1080)
    await process_frame
    await process_frame
    check(main.get_viewport_rect().size == Vector2(1920,1080),"Larger window adds native desktop workspace rather than stretching 1280 pixels")
    root.size = original_viewport_size
    await process_frame
    await press("LOAD_GAME")
    await press_node("TogglePause")
    # Manual secretary advice: synthetic company cells, no original data.
    var secretary_office = main.find_child("OriginalOfficeBackground",true,false)
    check(secretary_office != null,"Office surface exists for secretary click")
    var secretary_scale = min(secretary_office.size.x/466.0,secretary_office.size.y/290.0)
    var secretary_offset = (secretary_office.size-Vector2(466,290)*secretary_scale)/2.0
    var secretary_click := InputEventMouseButton.new()
    secretary_click.button_index = MOUSE_BUTTON_LEFT
    secretary_click.pressed = true
    secretary_click.position = secretary_offset+Vector2(109,92)*secretary_scale
    secretary_office.gui_input.emit(secretary_click)
    await process_frame
    check(main.find_child("SecretaryAdviceWindow",true,false) != null,"Unknown legacy calendar/fees produce an explicit advice notice")
    await press("BACK_TO_OFFICE")
    await press_node("ToolbarSAVE_GAME")
    var before_advice = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var advice_fixture = before_advice.duplicate(true)
    advice_fixture.State.Month = 2
    advice_fixture.State.Employees[0].ContractFee = 100
    advice_fixture.State.Calendar = {"HardwareStage":0,"ShipmentsSinceHardwareTrigger":0,
        "Hardware":[{"SourceIndex":0,"Availability":1,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0}],
        "Loans":[{"OriginalType":0},{"OriginalType":0},{"OriginalType":0}],
        "Tenants":[{"OriginalState":0,"Weeks":0},{"OriginalState":0,"Weeks":0}],"OriginalSlots0444":[-1,-1,-1]}
    write_json("user://saves/slot1.json",advice_fixture)
    await press("LOAD_GAME")
    secretary_office = main.find_child("OriginalOfficeBackground",true,false)
    secretary_office.gui_input.emit(secretary_click)
    await process_frame
    check(main.find_child("ConversationWindow",true,false) != null,"Secretary native hit opens compact conversation")
    check(main.find_child("ConversationMessage",true,false).text == tr("SECRETARY_ADVICE_DevelopGame"),"Idle rooms prompt game development first")
    await press_node("ToolbarSAVE_GAME")
    var queued_advice = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(queued_advice.State.PendingSecretaryAdvice.MessageIndex == 0 and queued_advice.State.PendingSecretaryAdvice.Messages.size() >= 3,"Manual advice outcome is saved before reading")
    check(main.find_child("ToolbarHIRE_EMPLOYEES",true,false).disabled,"Advice blocks other company transactions")
    await advance_conversation_page()
    check(main.find_child("ConversationMessage",true,false).text == tr("SECRETARY_ADVICE_ContractsNextMonth"),"February contract reminder follows room advice")
    await press_node("ToolbarSAVE_GAME")
    var midway_advice = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(midway_advice.State.PendingSecretaryAdvice.MessageIndex == 1,"Read page advances exactly one saved advice cursor")
    await press("LOAD_GAME")
    check(main.find_child("ConversationMessage",true,false).text == tr("SECRETARY_ADVICE_ContractsNextMonth"),"Loading resumes the unread advice page even while paused")
    await finish_conversation()
    await press_node("ToolbarSAVE_GAME")
    var completed_advice = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(completed_advice.State.PendingSecretaryAdvice == null and completed_advice.State.Money == queued_advice.State.Money,"Advice completion clears the queue without charging fees")
    check(completed_advice.State.CompanyActivityTicks == queued_advice.State.CompanyActivityTicks and completed_advice.State.SalesTicks == queued_advice.State.SalesTicks,"Office and sales clocks stay frozen during advice")
    check(seen_dialogue.has(tr("SECRETARY_ADVICE_FundsLow")),"Funds assessment ends the advice sequence")
    write_json("user://saves/slot1.json",before_advice)
    await press("LOAD_GAME")
    # Replacement is a paid, persisted two-step conversation, not a free alias change.
    await press("SETTINGS")
    await press("HIRE_SECRETARY")
    var secretary_candidates = main.find_child("SecretaryCandidates",true,false)
    check(secretary_candidates.get_root().get_child_count() == 4,"Secretary recruitment excludes the current secretary")
    var secretary_choice = secretary_candidates.get_root().get_child(1)
    check(int(secretary_choice.get_metadata(0)) == 2,"Secretary rows preserve source order")
    secretary_choice.select(0)
    secretary_candidates.item_selected.emit()
    await process_frame
    check(main.find_child("SecretaryDescription",true,false).text == tr("SECRETARY_DESCRIPTION_2"),"Selected secretary gets an English biography")
    await press_node("HireSelectedSecretary")
    check(main.find_child("ConversationMessage",true,false).text == tr("SECRETARY_FAREWELL"),"Old secretary speaks before identity changes")
    await press_node("ToolbarSAVE_GAME")
    var secretary_farewell = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(secretary_farewell.State.SecretarySourceIndex == 1 and secretary_farewell.State.PendingSecretaryReplacement.Phase == 0,"Farewell retains the previous secretary")
    check(secretary_farewell.State.Money == before_advice.State.Money-102,"Secretary replacement charges exactly the imported contract fee")
    await press("LOAD_GAME")
    check(main.find_child("ConversationMessage",true,false).text == tr("SECRETARY_FAREWELL"),"Loading resumes the paid farewell")
    await advance_conversation_page()
    check(main.find_child("ConversationSpeaker",true,false).text == tr("SECRETARY_NAMED").replace("{name}",tr("SECRETARY_NAME_2")),"Replacement changes the speaker before the greeting")
    await press_node("ToolbarSAVE_GAME")
    var secretary_greeting = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(secretary_greeting.State.SecretarySourceIndex == 2 and secretary_greeting.State.PendingSecretaryReplacement.Phase == 1,"New secretary identity and greeting phase are saved together")
    check(secretary_greeting.State.Money == secretary_farewell.State.Money and secretary_greeting.State.CompanyActivityTicks == secretary_farewell.State.CompanyActivityTicks,"Loading and reading do not charge or tick again")
    await press("LOAD_GAME")
    await finish_conversation()
    await press("SETTINGS")
    await press("SECRETARY_POLICIES")
    check(main.find_child("ConfirmSecretaryPolicies",true,false).disabled,"Unknown legacy policies require explicit choices")
    for choice in ["SecretaryProduction2","SecretaryBreak2","SecretaryReturn0"]:
        main.find_child(choice,true,false).button_pressed = true
    await press_node("ConfirmSecretaryPolicies")
    await press_node("ToolbarSAVE_GAME")
    var saved_policies = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(saved_policies.State.SecretaryPolicies.Production == 2 and saved_policies.State.SecretaryPolicies.Break == 2 and saved_policies.State.SecretaryPolicies.Return == 0,"Secretary policy choices persist as original enum values")
    await press("LOAD_GAME")
    await press("SETTINGS")
    await press("SECRETARY_POLICIES")
    check(main.find_child("SecretaryProduction2",true,false).button_pressed and main.find_child("SecretaryReturn0",true,false).button_pressed,"Loading preserves manual production and stress-40 return")
    main.find_child("SecretaryBreak0",true,false).button_pressed = true
    check(main.find_child("SecretaryReturn0",true,false).disabled,"Voluntary rest disables return controls")
    await press("BACK_TO_OFFICE")
    await press_node("ToolbarSAVE_GAME")
    var cancelled_policy = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(cancelled_policy.State.SecretaryPolicies.Break == 2,"Closing policy settings cancels the proposal")
    write_json("user://saves/slot1.json",before_advice)
    await press("LOAD_GAME")
    var seasonal_player = main.find_child("MusicPlayer",true,false)
    check(abs(seasonal_player.stream.get_length()-11.0) < 0.01,"April save immediately selects spring music")
    var soundtrack = main.find_child("BackgroundMusic",true,false)
    soundtrack.call("SetMonth", 5)
    var spring_stream = seasonal_player.stream
    soundtrack.call("SetMonth", 5)
    check(seasonal_player.stream == spring_stream,"Same-season date refresh preserves the recording")
    soundtrack.call("SetMonth", 6)
    check(abs(seasonal_player.stream.get_length()-13.0) < 0.01,"June switches to summer")
    var fading_player = main.find_child("MusicFadePlayer",true,false)
    check(fading_player.stream == spring_stream and seasonal_player.volume_db <= -70,"Season transition starts with outgoing spring and silent incoming summer")
    var fade_deadline = Time.get_ticks_msec()+1000
    while Time.get_ticks_msec() < fade_deadline:
        await process_frame
    check(fading_player.stream != null and seasonal_player.volume_db > -70 and fading_player.volume_db < -10,"Both voices change gain during crossfade")
    soundtrack.call("SetVolume",42.0)
    var mixed_power = pow(db_to_linear(seasonal_player.volume_db),2)+pow(db_to_linear(fading_player.volume_db),2)
    check(abs(mixed_power-0.42*0.42) < 0.00001,"Volume changes apply to both crossfade gains")
    soundtrack.call("SetVolume",30.0)
    var summer_stream = seasonal_player.stream
    soundtrack.call("SetMonth", 8)
    check(seasonal_player.stream == summer_stream and fading_player.stream == spring_stream,"Same-season refresh preserves the active crossfade")
    fade_deadline = Time.get_ticks_msec()+1200
    while Time.get_ticks_msec() < fade_deadline:
        await process_frame
    check(fading_player.stream == null and abs(seasonal_player.volume_db-linear_to_db(0.3)) < 0.01,"Crossfade releases the outgoing stream and restores full configured gain")
    seasonal_player.finished.emit()
    check(abs(seasonal_player.stream.get_length()-14.0) < 0.01,"Seasonal playlist advances")
    seasonal_player.finished.emit()
    check(abs(seasonal_player.stream.get_length()-13.0) < 0.01,"Seasonal playlist wraps")
    soundtrack.call("SetMonth", 9)
    check(abs(seasonal_player.stream.get_length()-1.0) < 0.01,"Absent autumn playlist falls back to local office music")
    check(fading_player.stream != null,"Fallback season changes also crossfade")
    soundtrack.call("SetEnabled", false)
    check(fading_player.stream == null,"Muting cancels and releases outgoing audio")
    soundtrack.call("SetMonth", 12)
    var muted_audio := ConfigFile.new()
    check(muted_audio.load("user://audio.cfg") == OK and not muted_audio.get_value("music","enabled"),"Season transition preserves saved mute setting")
    check(abs(seasonal_player.stream.get_length()-15.0) < 0.01 and (seasonal_player.stream_paused or not seasonal_player.playing),"Muted season transition selects winter without starting playback")
    var winter_stream = seasonal_player.stream
    soundtrack.call("SetMonth", 1)
    check(seasonal_player.stream == winter_stream,"December to January preserves winter recording")
    soundtrack.call("SetMonth", 3)
    check(abs(seasonal_player.stream.get_length()-11.0) < 0.01,"March returns to spring")
    check(fading_player.stream == null,"Muted season change never starts a crossfade")
    soundtrack.call("SetTitleActive")
    soundtrack.call("SetMonth", 6)
    check(seasonal_player.stream.resource_path.ends_with("title-theme.ogg"),"Season changes never replace the title theme")
    soundtrack.call("SetMonth", 0)
    soundtrack.call("SetGameplayActive", true)
    check(abs(seasonal_player.stream.get_length()-1.0) < 0.01,"Unknown month safely selects ordinary office music")
    soundtrack.call("SetSeasonalEnabled", false)
    soundtrack.call("SetEnabled", true)
    soundtrack.call("SetSeasonalEnabled", true)
    soundtrack.call("SetMonth", 3)
    soundtrack.call("SetMonth", 6)
    soundtrack.call("SetMonth", 12)
    check(abs(seasonal_player.stream.get_length()-15.0) < 0.01 and abs(fading_player.stream.get_length()-13.0) < 0.01,"Rapid season changes retain only the newest incoming and outgoing voices")
    var rapid_stream = seasonal_player.stream
    fading_player.finished.emit()
    check(seasonal_player.stream == rapid_stream and fading_player.stream == null,"Outgoing completion never advances the incoming playlist")
    soundtrack.call("SetMonth", 3)
    soundtrack.call("SetTitleActive")
    check(fading_player.stream == null,"Returning to title releases a pending crossfade")
    soundtrack.call("SetGameplayActive", true)
    soundtrack.call("SetSeasonalEnabled", false)
    await press_node("TogglePause")
    var toolbar = main.find_child("DesktopToolbar",true,false)
    for control in toolbar.get_children():
        if control is Button:
            check(control.icon != null,"Toolbar action has an image: " + control.text)
            check(control.tooltip_text == control.text,"Toolbar action has a localized tooltip: " + control.text)
            if control.icon != null:
                check(control.icon.get_size() == Vector2(23,22),"Toolbar image retains native dimensions")
    var save_action = main.find_child("ToolbarSAVE_GAME",true,false)
    save_action.mouse_entered.emit()
    check(main.find_child("OfficeStatus",true,false).text == tr("SAVE_GAME"),"Toolbar hover explains its action in the status bar")
    save_action.mouse_exited.emit()
    for name in ["OfficeWindow", "CompanyWindow", "EmployeeWindow", "ProductWindow"]:
        var window = main.find_child(name,true,false)
        check(window != null and window.visible,"Simultaneous desktop window: " + name)
        check(window.get_global_rect().end.x <= main.get_viewport_rect().end.x,"Desktop window fits viewport: " + name)
    var office = main.find_child("OfficeWindow",true,false)
    var staff_table = main.find_child("DesktopEmployeeTable",true,false)
    check(staff_table.get_root().get_child_count() == 1,"Desktop table shows loaded employee")
    staff_table.get_root().get_first_child().select(0)
    staff_table.item_activated.emit()
    await process_frame
    var selected_employee = main.find_child("SelectedEmployee",true,false)
    check(selected_employee != null and selected_employee.get_meta("employee_source") == 0,"Activating an employee row opens that employee's controls")
    check(main.find_child("DismissEmployee0",true,false) != null,"Selected employee supports existing management actions")
    var details = main.find_child("EmployeeDetailsWindow",true,false)
    print("Employee pane size: ",details.size)
    check(details != null and details.size.x <= 240 and details.size.y <= 410,"Employee details use a compact pane beside the office")
    check(main.find_child("ManagementWindow",true,false) == null,"Selecting an employee avoids the large management dialog")
    check(main.find_child("EmployeeAbilityChart",true,false) != null and main.find_child("EmployeeBarSpeed",true,false).max_value == 99,"Compact pane displays the original ability chart and bar scale")
    await press_node("EmployeeTechnologies")
    check(main.find_child("EmployeeTechnologiesPage",true,false).visible and not main.find_child("EmployeeAbilitiesPage",true,false).visible,"Compact technology tab switches pages")
    await press_node("EmployeeAbilities")
    await press("CANCEL")
    check(main.find_child("EmployeeDetailsWindow",true,false) == null and main.find_child("CompanyWindow",true,false).visible,"Closing employee details restores company information")
    var employee_window = main.find_child("EmployeeWindow",true,false)
    var original_position = employee_window.position
    employee_window.find_child("MinimizeWindow",true,false).pressed.emit()
    await process_frame
    check(not staff_table.is_visible_in_tree(),"Employee window minimizes")
    employee_window.find_child("MinimizeWindow",true,false).pressed.emit()
    await process_frame
    check(staff_table.is_visible_in_tree(),"Employee window restores")
    employee_window.find_child("MaximizeWindow",true,false).pressed.emit()
    await process_frame
    check(employee_window.position == Vector2.ZERO,"Employee window maximizes within desktop")
    employee_window.find_child("MaximizeWindow",true,false).pressed.emit()
    await process_frame
    check(employee_window.position == original_position,"Employee window restores original position")
    employee_window.find_child("CloseWindow",true,false).pressed.emit()
    check(not employee_window.visible,"Employee child window closes")
    main.find_child("MENU_WINDOWS",true,false).get_popup().id_pressed.emit(2)
    await process_frame
    check(employee_window.visible,"Window menu reopens employee list")
    root.size = Vector2i(1920,1080)
    await process_frame
    employee_window.position = Vector2(1500,700)
    root.size = original_viewport_size
    await process_frame
    await process_frame
    check(employee_window.position.x + employee_window.size.x <= main.find_child("DesktopWorkspace",true,false).size.x,"Window stays reachable after reducing desktop width")
    await press("START_GAME_DEVELOPMENT")
    var planning_hardware = main.find_child("PlanningHardware",true,false)
    var planning_audience = main.find_child("PlanningAudience",true,false)
    check(planning_hardware.get_root().get_child(0).get_text(0).begins_with(tr("CATALOG_HARDWARE_0")),"English platform label uses the imported source index")
    check(planning_audience.get_root().get_child(0).get_text(0) == tr("CATALOG_AUDIENCE_0"),"English audience label is translated")
    # Changing language changes labels, not the imported data or identifiers.
    await press("SETTINGS")
    await press("日本語")
    await press("BACK_TO_OFFICE")
    await press("START_GAME_DEVELOPMENT")
    check(main.find_child("PlanningHardware",true,false).get_root().get_child(0).get_text(0).begins_with("Synthetic Hardware"),"Japanese mode uses the untouched local platform name")
    check(main.find_child("PlanningAudience",true,false).get_root().get_child(0).get_text(0) == "Synthetic Audience","Japanese mode uses the untouched local audience name")
    await press("SETTINGS")
    await press("English")
    await press("BACK_TO_OFFICE")
    await press("START_GAME_DEVELOPMENT")
    var unchanged_catalog = JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    check(unchanged_catalog.Hardware[0].Name == "Synthetic Hardware" and unchanged_catalog.Audiences[0].Name == "Synthetic Audience","Localization never rewrites imported names")
    office = main.find_child("OfficeWindow",true,false)
    check(main.find_child("OfficeWindow",true,false) == office and office.visible,"Planning retains the office window")
    check(main.find_child("PlanningTitle",true,false) == null and main.find_child("PlanningFocus",true,false) == null,"Quote contains no acceptance/proposal controls")
    await press("SAVE_GAME")
    var quoted_state = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(quoted_state.State.Money == state.Money and quoted_state.State.DevelopmentProjects.is_empty(),"Opening quote does not spend or create a project")
    await press_node("PlanningMeeting")
    check(main.find_child("ConversationWindow",true,false) != null and main.find_child("AcceptPlan",true,false) == null,"Planning discussion precedes proposal acceptance")
    check_meeting_music("planning")
    var moved_meeting_position = Vector2(110,80)
    main.find_child("ConversationWindow",true,false).position = moved_meeting_position
    var meeting_picture = main.find_child("PlanningDiscussionScene",true,false)
    check(meeting_picture != null,"Synthetic illustration opens with planning dialogue")
    meeting_picture.position = moved_meeting_picture_position
    main.find_child("ConversationWindow",true,false).find_child("CloseWindow",true,false).pressed.emit()
    await process_frame
    check(main.find_child("MeetingMusicPlayer",true,false).stream == null,"Cancelling planning dialogue stops its meeting soundtrack")
    await press("START_GAME_DEVELOPMENT")
    await press_node("PlanningMeeting")
    check_meeting_music("planning")
    await finish_conversation()
    check(main.find_child("PlanningMeetingWindow",true,false) != null and main.find_child("PlanningQuoteWindow",true,false) == null,"Quote advances to a separate proposal step")
    check(main.find_child("PlanningMeetingWindow",true,false).position == moved_meeting_position,"Planning proposal retains the discussion's position")
    check(main.find_child("MeetingMusicPlayer",true,false).stream == null,"Planning proposal is outside meeting dialogue music")
    await press("SAVE_GAME")
    var meeting_state = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(meeting_state.State.Money == state.Money and meeting_state.State.DevelopmentProjects.is_empty(),"Entering meeting does not debit funds")
    check(not main.find_child("AcceptPlan",true,false).disabled,"Planning can begin")
    main.find_child("PlanningTitle",true,false).text = "Synthetic UI Game"
    var title_input = main.find_child("PlanningTitle",true,false)
    check(title_input.get_theme_color("caret_color") == Color.BLACK,"Text fields have a contrasting black caret")
    check(title_input.get_theme_color("font_selected_color") == Color.WHITE,"Selected input text remains legible")
    await press_node("AcceptPlan")
    check(main.find_child("DevelopmentProgress",true,false) != null,"Live development screen")
    Engine.time_scale = 100
    if not await await_node("SelectType0"):
        quit(1)
        return
    check(main.find_child("TypeMeetingWindow",true,false).position == moved_meeting_position,"Later type meeting retains the chosen position")
    check(main.find_child("MeetingMusicPlayer",true,false).stream == null,"Alpha choice controls are outside meeting dialogue music")
    await press("SAVE_GAME")
    var checkpoint = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    for period in ["WeeklyNet","MonthlyNet","YearlyNet"]:
        check(checkpoint.State.Company[period] == checkpoint.State.Money-10000000,"Planning expense feeds financial period: " + period)
    var financial_row = main.find_child("CompanyMetricWEEKLY_NET",true,false)
    check(financial_row != null and financial_row.text != "?","Company window displays recorded weekly finances")

    check(checkpoint.State.DevelopmentProjects[0].PendingDecision == 0,"Type checkpoint persisted")
    check(checkpoint.State.DevelopmentProjects[0].DecisionIntroductionAcknowledged == 0,"Reading the type introduction persists its acknowledgment")
    check(seen_dialogue.get(tr("CHECKPOINT_TYPE_INTRO"),0) == 1,"Type introduction precedes selection exactly once")
    var offered_types = checkpoint.State.DevelopmentProjects[0].TypeCandidates
    check(main.find_child("CheckpointColumnHeadings",true,false) != null,"Checkpoint choice columns have headings")
    check(tr("CHECKPOINT_FEE") == "Fee" and tr("CHECKPOINT_EXAMPLES") == "Example titles","English checkpoint column headings are translated")
    check(main.find_child("SelectType0",true,false).has_focus(),"Type choices start with keyboard focus")
    for index in range(offered_types.size()):
        check(main.find_child("SelectType%d" % index,true,false).text.begins_with(tr("CATALOG_TYPE_%d" % offered_types[index])),"Each randomized type label matches its saved source ID")
        var expected_examples = "\n".join(catalog.GameTypes[offered_types[index]].get("ExampleTitles",[]))
        if expected_examples.is_empty():
            expected_examples = "—"
        check(main.find_child("TypeExamples%d" % index,true,false).text == expected_examples,"Example column preserves the selected source's imported titles and order")
        check(main.find_child("SelectType%dFee" % index,true,false).text.begins_with("¥"),"Fees have their own aligned column")
        var row_fee = main.find_child("SelectType%dFee" % index,true,false)
        var header_fee = main.find_child("CheckpointColumnHeadings",true,false).get_child(1)
        check(is_equal_approx(row_fee.position.x,header_fee.position.x),"Header and fee cells stay aligned inside the scrolling list")
    # Reload checkpoint through the actual office controls before selecting.
    await press("LOAD_GAME")
    await await_node("SelectType0")
    check(main.find_child("ConversationWindow",true,false) == null and seen_dialogue.get(tr("CHECKPOINT_TYPE_INTRO"),0) == 1,"Loading a read checkpoint does not repeat its introduction")
    for unavailable_money in [0,null]:
        var unavailable = checkpoint.duplicate(true)
        unavailable.State.Money = unavailable_money
        write_json("user://saves/slot1.json",unavailable)
        await press("LOAD_GAME")
        await await_node("SelectType0")
        await press_node("SelectType0")
        check(main.find_child("SelectType0",true,false).button_pressed,"Unaffordable types remain available for selected preview")
        check(main.find_child("ConfirmTypeChoice",true,false).disabled,"Unknown or insufficient money cannot confirm a type")
        await press("SAVE_GAME")
        var uncharged = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        check(uncharged.State.Money == unavailable_money and uncharged.State.DevelopmentProjects[0].GameTypeSourceIndex == null,"Preview leaves funds and project unchanged when payment is unavailable")
        check(uncharged.State.DevelopmentProjects[0].TypeCandidates == offered_types,"Redrawing example titles never rerolls candidate order")
    write_json("user://saves/slot1.json",checkpoint)
    await press("LOAD_GAME")
    await await_node("SelectType0")
    await press_node("SelectType0")
    check(main.find_child("SelectType0",true,false).button_pressed,"Type preview retains its radio selection")
    check(main.find_child("TypeMeetingWindow",true,false) != null and main.find_child("TypeMeetingWindow",true,false).size.x <= 450,"Type choices use a compact meeting panel")
    await press("SAVE_GAME")
    var type_preview = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(type_preview.State.Money == checkpoint.State.Money and type_preview.State.DevelopmentProjects[0].GameTypeSourceIndex == null,"Previewing type never charges or confirms it")
    await press_node("ConfirmTypeChoice")
    check(main.find_child("ConversationWindow",true,false) != null,"Type confirmation precedes return to development")
    check_meeting_music("alpha")
    await press("SAVE_GAME")
    var type_confirmed = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(type_confirmed.State.DevelopmentProjects[0].CheckpointMeeting.IsConfirmation and type_confirmed.State.DevelopmentProjects[0].CheckpointMeeting.MessageIndex == 0,"Confirmed type dialogue is saved before it is read")
    await press("LOAD_GAME")
    await advance_conversation_page()
    var publicity_parts = tr("CHECKPOINT_MEETING_Publicity").split("{recognition}")
    var displayed_recognition = main.find_child("ConversationMessage",true,false).text.trim_prefix(publicity_parts[0]).trim_suffix(publicity_parts[1]).to_float()
    check(is_equal_approx(displayed_recognition,type_confirmed.State.DevelopmentProjects[0].Recognition),"Publicity line reports the resulting recognition")
    await press("SAVE_GAME")
    var type_publicity = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(type_publicity.State.Money == type_confirmed.State.Money and type_publicity.State.DevelopmentProjects[0].CheckpointMeeting.MessageIndex == 1,"Reading type confirmation does not repeat its fee")
    await press("LOAD_GAME")
    await finish_conversation()
    if not await await_node("SelectGenre0"):
        quit(1)
        return
    check(main.find_child("SelectGenre0",true,false).text.begins_with(tr("CATALOG_GENRE_0")),"Genre selection uses English catalog labels")
    check(main.find_child("SelectGenre0",true,false).has_focus(),"Genre choices start with keyboard focus")
    check(main.find_child("TypeExamples0",true,false) == null,"Genre panel contains no unrelated type-example column")
    await press_node("SelectGenre0")
    check(main.find_child("SelectGenre0",true,false).button_pressed,"Genre preview retains its radio selection")
    await press("SAVE_GAME")
    var genre_preview = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(genre_preview.State.DevelopmentProjects[0].GenreSourceIndex == null,"Previewing genre never confirms it")
    await press_node("ConfirmGenreChoice")
    check_meeting_music("beta")
    await press("SAVE_GAME")
    var genre_confirmed = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    var genre_reloaded = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(genre_reloaded.State.Money == genre_confirmed.State.Money and genre_reloaded.State.DevelopmentProjects[0].Quality == genre_confirmed.State.DevelopmentProjects[0].Quality,"Loading genre confirmation repeats neither fee nor affinity bonus")
    await finish_conversation()
    await press("SAVE_GAME")
    var genre_read = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(genre_read.State.DevelopmentProjects[0].CheckpointMeeting == null,"Genre confirmation finishes and releases the modal guard")
    var ready := false
    for frame in range(15000):
        if main.find_child("ConversationWindow",true,false) != null:
            await finish_conversation()
        for button in main.find_children("*", "Button", true, false):
            if button.text == tr("RELEASE_GAME"):
                ready = true
                break
        if ready:
            break
        await process_frame
    check(ready,"Development reaches completion")
    check(seen_dialogue.has(tr("CHECKPOINT_GENRE_INTRO")) and seen_dialogue.has(tr("CHECKPOINT_COMPLETE_INTRO").replace("{room}","1")),"Genre and completion announcements precede their decision panels")
    if not ready:
        quit(1)
        return
    await press("CONTINUE_DEBUGGING")
    var debugged := false
    for frame in range(15000):
        var report = main.find_child("DevelopmentReport",true,false)
        if report != null and report.text.contains(tr("BUGS") + ": 0"):
            debugged = true
            break
        await process_frame
    check(debugged,"Stage-three work removes bugs")
    if not debugged:
        quit(1)
        return
    await press("RELEASE_GAME")
    check(main.find_child("ReleasePrice",true,false).value == 5800,"Original default price")
    check(main.find_child("ReleaseCopies",true,false).value == 5000,"Original default production")
    await press_node("ConfirmRelease")
    check(main.find_child("ConversationWindow",true,false) != null,"Shipment commentary precedes reviews")
    await press("SAVE_GAME")
    var shipment_start = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(shipment_start.State.ReleasedGames[0].ShipmentMessageIndex == 0,"First shipment page is saved unread")
    await create_timer(0.2).timeout
    await press("SAVE_GAME")
    var shipment_paused = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(shipment_paused.State.Money == shipment_start.State.Money and shipment_paused.State.SalesTicks == shipment_start.State.SalesTicks and shipment_paused.State.CompanyActivityTicks == shipment_start.State.CompanyActivityTicks,"Shipment conversations freeze accounting and sales")
    await advance_conversation_page()
    await press("SAVE_GAME")
    var shipment_midway = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(shipment_midway.State.ReleasedGames[0].ShipmentMessageIndex == 1,"Only a completed page advances the saved shipment cursor")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    var shipment_reloaded = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(shipment_reloaded.State.Money == shipment_start.State.Money and shipment_reloaded.State.ReleasedGames[0].ReviewScores == shipment_start.State.ReleasedGames[0].ReviewScores and shipment_reloaded.State.ReleasedGames[0].MarketFactor == shipment_start.State.ReleasedGames[0].MarketFactor and shipment_reloaded.State.ReleasedGames[0].ShipmentMessageIndex == 1,"Loading shipment dialogue neither charges nor rerolls its market/reviews")
    await finish_conversation()
    check(main.find_child("ReviewScore3",true,false) != null,"Release opens four magazine reviews")
    check(seen_dialogue.has(tr("SHIPMENT_Reviews")),"Magazine introduction precedes review scores")
    await press("SAVE_GAME")
    var review_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(review_save.State.ReleasedGames[0].ReviewPending,"Pending reviews saved")
    check(review_save.State.ProductionPerformedThisWeek,"First shipment sets the production flag")
    # Isolate the budget boundary from variable development duration.
    review_save.State.CompanyActivityTicks = 0
    # Separate consumer investigations from this baseline sales/chart check.
    review_save.State.ReleasedGames[0].Recall = {"OriginalState":1,"WeeksRemaining":0,"PendingEvent":null,"OutcomeRoll":null,"RefundAmount":null}
    write_json("user://saves/slot1.json",review_save)
    Engine.time_scale = 1
    await press("LOAD_GAME")
    check(main.find_child("ReviewScore3",true,false) != null,"Load restores review ceremony")
    await press_node("AcknowledgeReviews")
    check(main.find_child("SalesProduct0",true,false) != null,"Review acknowledgement opens sales screen")
    check(main.find_child("ProduceProduct0",true,false).disabled,"Reprint waits for the next budget week")
    Engine.time_scale = 100
    for frame in range(100):
        await process_frame
    Engine.time_scale = 1
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var released = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(released.State.DevelopmentProjects.is_empty(),"Release frees room")
    check(released.State.ReleasedGames.size() == 1,"One product released")
    var product = released.State.ReleasedGames[0]
    check(product.UnitsSold > 0 and product.Stock < 5000,"Sales consume stock")
    check(product.NetSalesReceipts == product.UnitsSold * 2800,"Revenue uses original per-copy fees")
    await press("LOAD_GAME")
    await press("SALES")
    await press_node("SalesHistoryProduct0")
    check(main.find_child("WeeklySalesChart",true,false) != null,"Original thirteen-cell sales chart")
    check(main.find_child("CurrentWeeklySales",true,false) != null,"Current-week sales label")
    await press("BACK_TO_SALES")
    await press_node("ProduceProduct0")
    check(main.find_child("ProductionCopies",true,false) != null,"Reprint controls")
    main.find_child("ProductionCopies",true,false).value = 1000
    await press_node("ConfirmProduction")
    check(main.find_child("ProduceProduct0",true,false).disabled,"Reprint consumes the shared weekly allowance")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var reprinted = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(reprinted.State.ReleasedGames[0].ProductionSpending == product.ProductionSpending + 500000,"Reprint costs original band plus packaging")
    check(reprinted.State.ProductionPerformedThisWeek,"Production allowance persists in saves")
    check(reprinted.State.ReleasedGames[0].WeeklySalesHistory.size() == 13,"Sales history persists all thirteen cells")
    await press("START_GAME_DEVELOPMENT")
    await press_node("PlanningMeeting")
    check(main.find_child("ConversationWindow",true,false) != null and main.find_child("AcceptPlan",true,false) == null,"Planning discussion precedes proposal acceptance")
    await finish_conversation()
    main.find_child("PlanningTitle",true,false).text = "Synthetic Second Game"
    await press_node("AcceptPlan")
    check(main.find_child("DevelopmentProgress",true,false) != null,"Room accepts another project")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    # Synthetic boundary fixture drives the real modal/event UI without adding
    # game cheats or waiting for hundreds of thousands of physical copies.
    var milestone_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var milestone_product = milestone_save.State.ReleasedGames[0]
    milestone_product.UnitsSold = 1000000
    milestone_product.Stock = 1000
    milestone_product.Demand = 100
    milestone_product.Quality = {"Fun":100,"Novelty":100,"Graphics":100,"Music":100}
    milestone_product.Bugs = 0
    milestone_product.MarketFactor = 1
    milestone_product.SalesMilestoneStage = 0
    milestone_save.State.SalesTicks = 19
    write_json("user://saves/slot1.json",milestone_save)
    await press("LOAD_GAME")
    if not await await_node("AcknowledgeSalesMilestone"):
        quit(1)
        return
    await press("SAVE_GAME")
    var pending_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(normalize_numbers(pending_save.State.ReleasedGames[0].PendingSalesMilestones) == [1,2,3,4],"All crossed sales milestones queued")
    await press("LOAD_GAME")
    for stage in range(4):
        check(main.find_child("AcknowledgeSalesMilestone",true,false) != null,"Sequential sales milestone ceremony")
        await press_node("AcknowledgeSalesMilestone")
    check(main.find_child("SalesProduct0",true,false) != null,"Sales resume after all milestone acknowledgements")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var completed_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(completed_save.State.ReleasedGames[0].SalesMilestoneStage == 4,"Acknowledged milestone progress persists")
    # Guaranteed investigation fixture drives the real sales and weekly clocks.
    var recall_save = completed_save
    var recall_product = recall_save.State.ReleasedGames[0]
    recall_product.UnitsSold = 120
    recall_product.Stock = 30
    recall_product.Price = 5800
    recall_product.Demand = 0
    recall_product.Bugs = 50
    recall_product.Recall = {"OriginalState":0,"WeeksRemaining":0,"PendingEvent":null,"OutcomeRoll":null,"RefundAmount":null}
    recall_save.State.Money = 100000000
    recall_save.State.CompanyActivityTicks = 0
    recall_save.State.SalesTicks = 19
    var recall_project = recall_save.State.DevelopmentProjects[0]
    recall_project.OriginalStage = 3
    recall_project.GameTypeSourceIndex = 0
    recall_project.GenreSourceIndex = 0
    recall_project.Progress = recall_project.RequiredPoints
    recall_project.PendingDecision = null
    recall_project.Bugs = 0
    write_json("user://saves/slot1.json",recall_save)
    await press("LOAD_GAME")
    if not await await_node("AcknowledgeRecall"):
        quit(1)
        return
    await press("SAVE_GAME")
    var investigation_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(investigation_save.State.ReleasedGames[0].Recall.WeeksRemaining == 5,"Consumer investigation starts a five-week countdown")
    check(investigation_save.State.ReleasedGames[0].Recall.PendingEvent == 0,"Investigation modal persists")
    var paused_budget = investigation_save.State.CompanyActivityTicks
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var paused_recall_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(paused_recall_save.State.CompanyActivityTicks == paused_budget,"Investigation modal freezes the budget clock")
    await press("LOAD_GAME")
    await press_node("AcknowledgeRecall")
    Engine.time_scale = 100
    if not await await_node("AcknowledgeRecall"):
        quit(1)
        return
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var decision_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var decision_recall = decision_save.State.ReleasedGames[0].Recall
    check(decision_recall.PendingEvent == 1 and decision_recall.WeeksRemaining == 0,"Five live budget weeks produce the recall decision")
    var expected_refund = 0 if decision_recall.OutcomeRoll == 3 else (870000 if decision_recall.OutcomeRoll == 0 else 150000)
    check(decision_recall.RefundAmount == expected_refund,"Original recall refund formula")
    var investigation_receipts = decision_save.State.ReleasedGames[0].NetSalesReceipts-recall_save.State.ReleasedGames[0].NetSalesReceipts
    check(decision_save.State.Money == 100000000+investigation_receipts-expected_refund,"Recall charges once alongside ongoing sales")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == decision_save.State.Money,"Result reload does not charge a second refund")
    await press_node("AcknowledgeRecall")
    check(main.find_child("ProduceProduct0",true,false).disabled == (decision_recall.OriginalState == 2),"Recall result controls reprint eligibility")
    await press("BACK_TO_OFFICE")
    await press("TECHNOLOGIES")
    await press_node("PurchaseTechnology0")
    check(main.find_child("PurchaseTechnology0",true,false).disabled,"Owned technology cannot be bought again")
    await press("SAVE_GAME")
    var owned_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(owned_save.State.TechnologyOwnershipLevels["0"] == 3,"Purchase unlocks all training levels")
    var training_xp = owned_save.State.Employees[0].Experience
    await press("LOAD_GAME")
    await press("STAFF")
    await press("TAKE_BREAK")
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("STAFF")
    await press("TRAINING")
    check(main.find_child("TrainTechnology0",true,false).disabled,"Training needs a study seat")
    await press_node("AssignStudySeat1")
    await press_node("TrainTechnology0")
    await press("SAVE_GAME")
    var training_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(training_save.State.Employees[0].Experience == training_xp-10,"Training spends experience upfront")
    check(training_save.State.Employees[0].Office.Training != null,"Active course is saved")
    await press("LOAD_GAME")
    Engine.time_scale = 100
    for frame in range(25):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var learned_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(learned_save.State.Employees[0].TechnologyLevels["0"] == 1,"Training completes in the live office")
    check(learned_save.State.Employees[0].Office.Training == null,"Completed course is cleared")
    await press("LOAD_GAME")
    await press("STAFF")
    await press("TRAINING")
    check(main.find_child("AssignStudySeat1",true,false).disabled,"Study seat survives reload")
    await press("BACK_TO_OFFICE")
    await press("STAFF")
    # Authored ready-project fixture isolates application from checkpoint modals.
    var use_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var use_employee = use_save.State.Employees[0]
    use_employee.Experience = 100
    use_employee.Stress = 0
    use_employee.Office.Action = 6
    use_employee.Office.SeatedAction = 6
    use_employee.Office.WorkEngaged = true
    use_employee.Office.MovementState = 0
    use_employee.Office.Facing = 4
    use_employee.Office.X = 208
    use_employee.Office.Y = 104
    use_employee.Office.Frame = 22
    use_employee.Office.OffsetX = -4
    use_employee.Office.OffsetY = 4
    use_employee.Office.WorkTicks = 0
    use_employee.Office.WorkTier = 0
    use_employee.Office.WorkIdleTicks = 0
    use_employee.Office.IdleBreak = false
    var use_project = use_save.State.DevelopmentProjects[0]
    use_project.OriginalStage = 3
    use_project.GameTypeSourceIndex = 0
    use_project.GenreSourceIndex = 0
    use_project.Progress = use_project.RequiredPoints
    use_project.PendingDecision = null
    use_project.DataSize = 101.9
    use_project.Bugs = 100
    write_json("user://saves/slot1.json",use_save)
    await press("CANCEL")
    await press("LOAD_GAME")
    await press("STAFF")
    await press("USE_TECHNOLOGY")
    await press_node("UseTechnology0")
    await press("SAVE_GAME")
    var use_pending = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(use_pending.State.Employees[0].Experience == 99,"Technology use spends its imported experience cost")
    check(use_pending.State.Employees[0].Office.TechnologyApplication != null,"Active technology is saved")
    await press("LOAD_GAME")
    Engine.time_scale = 100
    for frame in range(5):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var applied = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(applied.State.DevelopmentProjects[0].DataSize >= 79 and applied.State.DevelopmentProjects[0].DataSize <= 81,"Compression changes the live project")
    check(applied.State.DevelopmentProjects[0].TechnologyApplicationCounts["0"] == 1,"Application history persists")
    check(applied.State.Employees[0].Office.TechnologyApplication == null,"Completed technology is cleared")
    await press("LOAD_GAME")
    await press("STAFF")
    await press_node("DismissEmployee0")
    check(main.find_child("ConfirmDismissal",true,false) != null,"Dismissal confirmation opens")
    await press("CANCEL")
    check(main.find_child("DismissEmployee0",true,false) != null,"Cancelled dismissal retains employee")
    await press_node("DismissEmployee0")
    await press_node("ConfirmDismissal")
    check(main.find_child("DismissEmployee0",true,false) == null,"Dismissal removes employee card")
    await press("CANCEL")
    await press("SAVE_GAME")
    var dismissal_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(dismissal_save.State.Employees.is_empty(),"Dismissal persists without employee workspace")
    check(dismissal_save.State.DevelopmentProjects.size() == 1,"Dismissal retains active development room")
    await press("COMPANY_ACTIVITIES")
    await press_node("CompanyActivity0")
    check(main.find_child("CompanyActivity1",true,false).disabled,"Business action budget shared across actions")
    await press("SAVE_GAME")
    var activity_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(activity_save.State.Company.Fame >= 13 and activity_save.State.Company.Fame <= 15,"Public relations raises fame")
    check(activity_save.State.Money == dismissal_save.State.Money - 5000000,"Public relations charges shared cost")
    await press("LOAD_GAME")
    await press("COMPANY_ACTIVITIES")
    check(main.find_child("CompanyActivity0",true,false).disabled,"Used budget survives save/load")
    await press("BACK_TO_OFFICE")
    Engine.time_scale = 100
    for frame in range(12):
        await process_frame
    Engine.time_scale = 1
    await press("COMPANY_ACTIVITIES")
    await press_node("CompanyActivity1")
    await press("SAVE_GAME")
    var research_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(research_save.State.Company.OriginalMember90 >= 25 and research_save.State.Company.OriginalMember90 <= 27,"Genre research raises unlock threshold")
    await press("BACK_TO_OFFICE")
    Engine.time_scale = 100
    for frame in range(12):
        await process_frame
    Engine.time_scale = 1
    await press("COMPANY_ACTIVITIES")
    await press_node("CompanyActivity2")
    await press("SAVE_GAME")
    var capacity_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(capacity_save.State.Company.ProductionCapacity >= 54,"Production expansion increases capacity")
    # Authored salary fixture: original staff total and shared-index debit differ.
    var payroll_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    payroll_save.State.MinimumSourceIndex = 0
    payroll_save.State.SourceUpperBound = 3
    payroll_save.State.OriginalPayrollScratchIndex = null
    payroll_save.State.EmployeeSources = []
    payroll_save.State.Employees = []
    for index in range(3):
        var salary_employee = {"Name":"Synthetic Salary "+str(index),"SourceIndex":index,"RawParameter40":[700,100,300][index]}
        payroll_save.State.EmployeeSources.append({"SourceIndex":index,"GateValue":0 if index == 0 else 1,"Employee":salary_employee})
        if index > 0:
            payroll_save.State.Employees.append(salary_employee.duplicate(true))
    var payroll_money = payroll_save.State.Money
    write_json("user://saves/slot1.json",payroll_save)
    await press("BACK_TO_OFFICE")
    await press("LOAD_GAME")
    await press("STAFF")
    check(main.find_child("StaffSalaryTotal",true,false).text.ends_with("400"),"Staff screen sums each employed salary")
    check(main.find_child("MonthlySalary1",true,false).text.ends_with("100"),"Employee monthly salary is displayed")
    check(main.find_child("OriginalPayrollPreview",true,false) == null,"Staff screen keeps the reconstruction preview in the inspector")
    await press("CANCEL")
    await press("CALENDAR_INSPECTOR")
    check(main.find_child("OriginalPayrollPreview",true,false).text.ends_with("600"),"Calendar inspector preserves the distinct shared-index payroll preview")
    await press("CANCEL")
    await press("SAVE_GAME")
    var payroll_captured = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(payroll_captured.State.OriginalPayrollScratchIndex == 2,"Staff scratch snapshot survives save")
    check(payroll_captured.State.Money == payroll_money,"Payroll preview does not charge money")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.OriginalPayrollScratchIndex == 2,"Loading retains the shared salary index")
    await press_node("TogglePause")
    check(main.find_child("TogglePause",true,false).text == tr("RESUME"),"Paused office immediately offers Resume")
    check(main.find_child("OfficeStatus",true,false).text.contains(tr("PAUSED")),"Pause status appears immediately")
    await press("SAVE_GAME")
    var pause_saved = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(main.find_child("TogglePause",true,false).text == tr("RESUME"),"Rebuilding the office retains the correct pause label")
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var pause_after_wait = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(pause_after_wait.State.CompanyActivityTicks == pause_saved.State.CompanyActivityTicks,"Manual pause freezes the shared weekly clock")
    check(pause_after_wait.State.SalesTicks == pause_saved.State.SalesTicks,"Manual pause freezes sales")
    await press_node("TogglePause")
    check(main.find_child("TogglePause",true,false).text == tr("PAUSE"),"Resumed office immediately offers Pause")
    check(not main.find_child("OfficeStatus",true,false).text.contains(tr("PAUSED")),"Resume clears pause status immediately")
    Engine.time_scale = 100
    for frame in range(3):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var resumed_save = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(resumed_save.State.CompanyActivityTicks != pause_saved.State.CompanyActivityTicks,"Resume advances the clock again")
    var player = main.find_child("MusicPlayer",true,false)
    check(player != null and player.stream != null and abs(player.stream.get_length()-1.0) < 0.01,"Local music loads during gameplay")
    for index in range(8):
        var track = AudioStreamWAV.load_from_file("user://music/office-%02d.wav" % (index+1))
        check(track != null and abs(track.get_length()-(index+1)) < 0.01,"Local music duration %d" % (index+1))
    await press("SETTINGS")
    var enabled = main.find_child("MusicEnabled",true,false)
    var seasonal = main.find_child("SeasonalMusicEnabled",true,false)
    var volume = main.find_child("MusicVolume",true,false)
    enabled.button_pressed = false
    volume.value = 42
    await process_frame
    check(player.stream_paused or not player.playing,"Music toggle prevents playback independently")
    var config = ConfigFile.new()
    check(config.load("user://audio.cfg") == OK,"Audio preferences saved")
    check(config.get_value("music","enabled") == false and config.get_value("music","volume") == 42,"Mute and volume survive configuration save")
    check(config.get_value("music","seasonal") == false and not seasonal.button_pressed,"Seasonal setting saved independently of game saves")
    enabled.button_pressed = true
    var previous_stream = player.stream
    await press("NEXT_MUSIC_TRACK")
    check(player.stream != previous_stream and not player.stream_paused,"Next soundtrack track resumes playback")
    previous_stream = player.stream
    player.finished.emit()
    await process_frame
    check(player.stream != previous_stream,"Finished recording advances the playlist")
    # Eight advances must restore the same stream identity.
    previous_stream = player.stream
    for index in range(8):
        player.finished.emit()
        await process_frame
    check(player.stream == previous_stream,"Playlist wraps after all eight local tracks")
    await press("BACK_TO_OFFICE")
    check(main.find_child("DesktopEmployeeTable",true,false).get_root().get_child_count() == 2,"Desktop table refreshes after management")
    # Calendar fixtures exercise actual modal controls, persistence and the
    # resumed payroll branch. No original records or assets are published.
    var calendar_save = payroll_captured.duplicate(true)
    var cs = calendar_save.State
    cs.Year = 3
    cs.Month = 5
    cs.Week = 2
    cs.CompanyActivityTicks = 0
    cs.Employees = []
    cs.EmployeeSources = []
    cs.DevelopmentProjects = []
    cs.ReleasedGames = []
    cs.AvailableHardware = []
    cs.Calendar = {"HardwareStage":1,"ShipmentsSinceHardwareTrigger":0,
        "Hardware":[{"SourceIndex":0,"Availability":0,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0}],
        "Loans":[{"OriginalType":0},{"OriginalType":0},{"OriginalType":0}],
        "Tenants":[{"OriginalState":0,"Weeks":0},{"OriginalState":0,"Weeks":0}],
        "OriginalSlots0444":[-1,-1,-1],
        "Pending":{"ClosingDate":{"Year":3,"Month":5,"Week":1},"Phase":4,"Index":0,"PendingEvent":3}}
    write_json("user://saves/slot1.json",calendar_save)
    await press("LOAD_GAME")
    check(main.find_child("AcknowledgeCalendar",true,false) != null,"Hardware launch news opens after loading its transaction")
    # Main-window actions cannot replace an original modal weekly decision.
    for key in ["HIRE_EMPLOYEES","COMPANY_ACTIVITIES","STAFF","DEVELOPMENT_PLANNING"]:
        var calendar_toolbar = main.find_child("Toolbar"+key,true,false)
        check(calendar_toolbar.disabled,"Calendar modal disables toolbar action: "+key)
        calendar_toolbar.pressed.emit()
        await process_frame
        check(main.find_child("AcknowledgeCalendar",true,false) != null,"Calendar guards direct action callbacks: "+key)
    var staff_menu = main.find_child("MENU_STAFF",true,false).get_popup()
    staff_menu.about_to_popup.emit()
    check(staff_menu.is_item_disabled(0) and staff_menu.is_item_disabled(1),"Calendar modal disables employee menu actions")
    check(main.find_child("TogglePause",true,false).disabled,"Calendar modal owns the pause state")
    Engine.time_scale = 100
    for frame in range(20):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var launch_paused = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(launch_paused.State.Calendar.Pending.PendingEvent == 3 and launch_paused.State.SalesTicks == cs.SalesTicks,"Hardware dialog freezes the office and survives saving")
    check(launch_paused.State.AvailableHardware.is_empty(),"Platform stays unavailable until news acknowledgement")
    await press("LOAD_GAME")
    await press_node("AcknowledgeCalendar")
    Engine.time_scale = 100
    for frame in range(3):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var launched = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(launched.State.Calendar.Pending == null and launched.State.AvailableHardware.size() == 1 and launched.State.AvailableHardware[0] == 0,"Acknowledged launch resumes the calendar and unlocks its platform")
    # Current announcements resume the exact page. Legacy saves above keep
    # their direct acknowledgement screen rather than replaying an introduction.
    var moved_dialogue_position = Vector2(80,90)
    for notice in [
        {"kind":1,"phase":2,"index":1,"launch":24,"retirement":0,"availability":0,"key":"HardwareDevelopment","pages":1},
        {"kind":2,"phase":4,"index":0,"launch":4,"retirement":0,"availability":0,"key":"HardwareSoon","pages":1},
        {"kind":4,"phase":3,"index":1,"launch":0,"retirement":12,"availability":1,"key":"HardwareRetiring","pages":2},
        {"kind":5,"phase":3,"index":1,"launch":0,"retirement":0,"availability":2,"key":"HardwareRetired","pages":2},
        {"kind":6,"phase":8,"index":0,"launch":0,"retirement":0,"availability":1,"key":"LoanRepayment","pages":1}
    ]:
        var notice_save = launched.duplicate(true)
        var ns = notice_save.State
        ns.Year = 3; ns.Month = 5; ns.Week = 2; ns.CompanyActivityTicks = 0
        ns.Money = 50000000
        ns.AvailableHardware = [0] if notice.availability == 1 else []
        ns.HardwareLicenses = [0] if notice.availability == 1 else []
        var hardware = ns.Calendar.Hardware[0]
        hardware.Availability = notice.availability
        hardware.LaunchWeeks = notice.launch
        hardware.RetirementWeeks = notice.retirement
        ns.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":5,"Week":1},"Phase":notice.phase,"Index":notice.index,"PendingEvent":notice.kind,"DialoguePage":0}
        if notice.kind == 6:
            ns.Calendar.Loans[0] = {"OriginalType":1,"BorrowedOn":{"Year":2,"Month":5,"Week":2}}
            ns.Calendar.Pending.Amount = 13000000
        write_json("user://saves/slot1.json",notice_save)
        await press("LOAD_GAME")
        var notice_window = main.find_child("ConversationWindow",true,false)
        if notice.kind == 1:
            notice_window.position = moved_dialogue_position
        check(notice_window.position == moved_dialogue_position,"Later calendar conversations keep the chosen position")
        for page in range(notice.pages):
            var key = "CALENDAR_DIALOGUE_"+notice.key+("Explanation" if page == 1 else "")
            var expected = tr(key).replace("{maker}",tr("CALENDAR_MAKER_4")).replace("{hardware}",tr("CATALOG_HARDWARE_0")).replace("{principal}","10,000,000").replace("{repayment}","13,000,000")
            var message = main.find_child("ConversationMessage",true,false)
            check(message != null and message.text == expected,"Calendar notice text: "+key+" expected="+expected+" actual="+(message.text if message != null else "<missing>"))
            check(main.find_child("ConversationWindow",true,false).position == moved_dialogue_position,"Next dialogue page keeps the chosen position: "+key)
            check(main.find_child("ToolbarSTAFF",true,false).disabled and main.find_child("TogglePause",true,false).disabled,"Calendar conversation owns modal pause: "+key)
            await press("SAVE_GAME")
            var before_reload = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
            check(before_reload.State.Calendar.Pending.DialoguePage == page and before_reload.State.Money == ns.Money,"Calendar page persists without early debit: "+key)
            await press("LOAD_GAME")
            message = main.find_child("ConversationMessage",true,false)
            check(message != null and message.text == expected,"Reload resumes calendar page: "+key)
            check(main.find_child("ConversationWindow",true,false).position == moved_dialogue_position,"Loading a conversation keeps the chosen position: "+key)
            await advance_conversation_page()
        Engine.time_scale = 100
        for frame in range(3):
            await process_frame
        Engine.time_scale = 1
        await press("SAVE_GAME")
        var notice_done = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        var expected_money = ns.Money-13000000 if notice.kind == 6 else ns.Money
        check(notice_done.State.Calendar.Pending == null and notice_done.State.Money == expected_money,"Final calendar page commits once: "+notice.key)
        if notice.kind == 6:
            check(notice_done.State.Calendar.Loans[0].OriginalType == 0,"Repayment closes its loan after the notice")
        await press("LOAD_GAME")
        await press("SAVE_GAME")
        var notice_reloaded = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        check(main.find_child("ConversationWindow",true,false) == null and notice_reloaded.State.Money == expected_money,"Completed notice never repeats on reload: "+notice.key)
    # A real calendar tick crosses May/June and switches audio before any new modal.
    var season_save = launched.duplicate(true)
    season_save.State.Year = 3
    season_save.State.Month = 5
    season_save.State.Week = 4
    season_save.State.CompanyActivityTicks = 199
    season_save.State.Calendar.Pending = null
    soundtrack.call("SetSeasonalEnabled", true)
    write_json("user://saves/slot1.json",season_save)
    await press("LOAD_GAME")
    check(abs(player.stream.get_length()-11.0) < 0.01,"Loading a May save restores spring while seasonal music is enabled")
    Engine.time_scale = 100
    for frame in range(3):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var season_advanced = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(season_advanced.State.Month == 6 and abs(player.stream.get_length()-13.0) < 0.01,"Actual May-to-June calendar advancement switches to summer music")
    # Renewal mutates the employed roster before the same week's salary debit.
    var renewal_save = payroll_captured.duplicate(true)
    var rs = renewal_save.State
    rs.Year = 3
    rs.Month = 3
    rs.Week = 4
    rs.CompanyActivityTicks = 0
    rs.DevelopmentProjects = []
    rs.ReleasedGames = []
    rs.CompanyActivityPerformed = true
    rs.Company.WeeklyNet = 0
    rs.Company.MonthlyNet = 0
    rs.Company.YearlyNet = 0
    rs.Employees[0].ContractFee = 10
    rs.Employees[1].ContractFee = 20
    rs.Calendar = launched.State.Calendar.duplicate(true)
    rs.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":3,"Week":4},"Phase":1,"Index":0,"PendingEvent":0}
    write_json("user://saves/slot1.json",renewal_save)
    await press("LOAD_GAME")
    check(main.find_child("RenewEmployee1",true,false).button_pressed,"Renewal initially selects employed staff")
    main.find_child("RenewEmployee2",true,false).button_pressed = false
    await press_node("ConfirmContractRenewal")
    Engine.time_scale = 100
    if not await await_node("ExhibitionSelection"):
        quit(1)
        return
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var renewed = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(renewed.State.Employees.size() == 1 and renewed.State.EmployeeSources[2].GateValue == 0,"Unchecked annual contract dismisses its source before payroll")
    check(renewed.State.Money == rs.Money-10-300,"Annual fee and shared-index monthly salary are each charged once")
    check(renewed.State.Month == 4 and renewed.State.Week == 1 and renewed.State.Company.WeeklyNet == -300,"April rollover resets accounting before payroll")
    check(renewed.State.Calendar.Pending.PendingEvent == 7,"Spring exhibition pauses later calendar transactions")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    var renewed_reload = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(renewed_reload.State.Money == renewed.State.Money,"Reloading the pending exhibition never repeats annual fees or salary")
    # Participate in an exhibition using a wholly synthetic product.
    var exhibition_save = renewed_reload.duplicate(true)
    var es = exhibition_save.State
    es.Employees = []
    es.EmployeeSources = []
    es.Recruitment.CandidateDatabaseDecoded = false
    es.DevelopmentProjects = []
    es.ReleasedGames = [released.State.ReleasedGames[0].duplicate(true)]
    es.ReleasedGames[0].Recognition = 95
    es.ReleasedGames[0].Demand = 0
    es.ReleasedGames[0].Stock = 0
    es.ReleasedGames[0].ReleasedOn = {"Year":3,"Month":3,"Week":1}
    es.Calendar.Pending.Exhibition = {"EligibleProducts":[0],"SelectedProducts":[]}
    es.Calendar.ExhibitionVisitorRecord = 0
    write_json("user://saves/slot1.json",exhibition_save)
    await press("LOAD_GAME")
    check(main.find_child("ExhibitionSelection",true,false) != null,"Exhibition selection survives loading")
    main.find_child("ExhibitProduct0",true,false).button_pressed = true
    check(main.find_child("ExhibitionFee",true,false).text.ends_with("10,000,000"),"Exhibition checkbox displays the proven fee")
    await press_node("ParticipateExhibition")
    await press("SAVE_GAME")
    var exhibited = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var attendance = exhibited.State.Calendar.Pending.Exhibition.Visitors
    check(attendance >= 9900 and attendance <= 17578,"Exhibition attendance stays within original timer bounds")
    check(exhibited.State.Money == es.Money-10000000 and exhibited.State.ReleasedGames[0].Recognition == 95+floor(attendance/500),"Exhibition fee and uncapped product awareness commit together")
    await press("LOAD_GAME")
    check(main.find_child("ExhibitionResult",true,false) != null,"Exhibition result survives loading without another fee")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == exhibited.State.Money,"Loading exhibition result never repeats payment")
    await press_node("CloseExhibition")
    # Purchase and close an actual school through the company facility screen.
    var school_save = exhibited.duplicate(true)
    school_save.State.Calendar.Pending = null
    school_save.State.Calendar.OriginalSlots0444 = [-1,-1,-1]
    school_save.State.CompanyActivityTicks = 0
    school_save.State.Money = 600000000
    write_json("user://saves/slot1.json",school_save)
    await press("LOAD_GAME")
    await press_node("CompanyFacilities")
    await press_node("OpenSchool1")
    check(main.find_child("CloseSchool1",true,false) != null and main.find_child("OpenSchool0",true,false).disabled,"School occupies selected slot and prevents duplicate construction")
    await press("SAVE_GAME")
    var bought_school = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(bought_school.State.Money == 0 and int(bought_school.State.Calendar.OriginalSlots0444[1]) == 0,"School construction debits exactly six hundred million")
    check(bought_school.State.Company.MonthlyNet == school_save.State.Company.MonthlyNet,"School's direct cash debit preserves original period accounting")
    await press_node("CloseSchool1")
    await press("CANCEL")
    check(main.find_child("CloseSchool1",true,false) != null,"Cancelled closure retains school")
    await press_node("CloseSchool1")
    await press_node("ConfirmSchoolClosure")
    await press("SAVE_GAME")
    var closed_school = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(closed_school.State.Money == 0 and closed_school.State.Calendar.OriginalSlots0444.all(func(slot): return int(slot) == -1),"Closing school frees slot without refund")
    await press("BACK_TO_OFFICE")
    # Award categories with no own entries require no invented rival names.
    var awards_save = exhibition_save.duplicate(true)
    var aws = awards_save.State
    aws.Month = 12
    aws.Week = 4
    aws.ReleasedGames = []
    aws.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,
        "Awards":{"Year":3,"Entries":[],"CategoryIndex":0}}
    write_json("user://saves/slot1.json",awards_save)
    await press("LOAD_GAME")
    for category in range(5):
        check(main.find_child("RevealAward",true,false) != null,"Award category awaits announcement")
        await press_node("RevealAward")
        await press("SAVE_GAME")
        var announced = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        check(announced.State.Calendar.Pending.Awards.CategoryIndex == category and announced.State.Calendar.Pending.Awards.ResultReady,"Award result and cursor persist")
        check(announced.State.Money == aws.Money,"Competitor result never invents a company payout")
        await press("LOAD_GAME")
        check(main.find_child("AwardResult",true,false) != null,"Saved award result reopens without rerolling")
        await press_node("ContinueAwards")
    # Real rival presentation uses imported catalog strings and saved reel picks.
    var rival_awards_save = awards_save.duplicate(true)
    var competitor_rows = [{"Title":"Synthetic rival A","Owner":0,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":100},
        {"Title":"Synthetic rival B","Owner":1,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":50}]
    rival_awards_save.State.Competition = {"Products":competitor_rows,"PlatformAges":[0,0,0,0,0,0,0,0]}
    rival_awards_save.State.Calendar.Pending.Awards.RivalEntries = []
    for i in range(31):
        rival_awards_save.State.Calendar.Pending.Awards.RivalEntries.append(i%2)
    write_json("user://saves/slot1.json",rival_awards_save)
    await press("LOAD_GAME")
    await press_node("RevealAward")
    check(main.find_child("ContinueAwards",true,false).disabled and not main.find_child("AwardResult",true,false).visible,"Rival reel precedes winner presentation")
    await create_timer(1.3).timeout
    check(not main.find_child("ContinueAwards",true,false).disabled and main.find_child("AwardResult",true,false).visible,"Rival reel reveals winner and continuation")
    check(main.find_child("AwardResult",true,false).text.contains("Synthetic rival") and main.find_child("AwardResult",true,false).text.contains("Synthetic rival company"),"Award presents actual rival game and company")
    await press("SAVE_GAME")
    var rival_award = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(rival_award.State.Calendar.Pending.Awards.AnimationProducts.size() == 13 and rival_award.State.Money == rival_awards_save.State.Money,"Rival award persists all thirteen draws without payment")
    await press("LOAD_GAME")
    check(not main.find_child("ContinueAwards",true,false).disabled and main.find_child("AwardResult",true,false).text.contains("Synthetic rival"),"Saved rival result opens immediately without replaying reel")
    await press_node("ContinueAwards")
    # Force only a proven sales override for a deterministic grand prize.
    var grand_save = awards_save.duplicate(true)
    grand_save.State.ReleasedGames = [exhibition_save.State.ReleasedGames[0].duplicate(true)]
    grand_save.State.ReleasedGames[0].UnitsSold = 1000000
    grand_save.State.ReleasedGames[0].Quality = {"Fun":0,"Novelty":0,"Graphics":0,"Music":0}
    grand_save.State.Calendar.Pending.Awards.Entries = [0]
    grand_save.State.Calendar.Pending.Awards.CategoryIndex = 4
    write_json("user://saves/slot1.json",grand_save)
    await press("LOAD_GAME")
    await press_node("RevealAward")
    await press("SAVE_GAME")
    var grand = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(grand.State.Money == grand_save.State.Money+500000000 and grand.State.ReleasedGames[0].OriginalAwardCode == 5,"Grand prize credits the proven reward and product marker")
    await press("LOAD_GAME")
    await press("SAVE_GAME")
    check(JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json")).State.Money == grand.State.Money,"Grand prize reload does not duplicate its payout")
    var rankings = JSON.parse_string(FileAccess.get_file_as_string("user://rankings.json"))
    check(rankings != null and int(rankings.Records[4].AchievedOn.Year) == 3 and int(rankings.Records[4].AchievedOn.Month) == 12 and int(rankings.Records[4].AchievedOn.Week) == 4,"Grand prize persists independent global ranking date")
    await press_node("ContinueAwards")
    for frame in range(8):
        await process_frame
    var help_menu = main.find_child("MENU_HELP",true,false).get_popup()
    help_menu.id_pressed.emit(1)
    await process_frame
    check(main.find_child("RankingRecord4",true,false).text.contains("Synthetic UI Company"),"Global rankings display winning company")
    await press("SALES_CHART")
    check(main.find_child("CompetitionUnknown",true,false) != null,"Legacy rival data stays explicitly unknown")
    await press("BACK_TO_OFFICE")
    # Graduate forms use source employees and one hire-or-decline per candidate.
    var graduates_save = payroll_captured.duplicate(true)
    var gs = graduates_save.State
    gs.Year = 3
    gs.Month = 4
    gs.Week = 2
    gs.CompanyActivityTicks = 0
    gs.DevelopmentProjects = []
    gs.ReleasedGames = []
    gs.Employees = []
    gs.MinimumSourceIndex = 0
    gs.MaximumSourceIndex = 2
    for row in gs.EmployeeSources:
        row.GateValue = 0
        row.Employee.ContractFee = 100
    gs.Calendar = launched.State.Calendar.duplicate(true)
    gs.Calendar.OriginalSlots0444 = [0,-1,-1]
    gs.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":4,"Week":1},"Phase":9,"Index":0,"PendingEvent":9,
        "Graduates":{"Candidates":[0,2],"CandidateIndex":0}}
    write_json("user://saves/slot1.json",graduates_save)
    await press("LOAD_GAME")
    check(main.find_child("GraduatePosition",true,false) != null,"Graduate prompt opens from its saved source queue")
    await press_node("HireGraduate")
    await press("SAVE_GAME")
    var hired_graduate = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(hired_graduate.State.Money == gs.Money-100 and hired_graduate.State.Employees.size() == 1,"Graduate hiring charges the displayed source contract fee")
    check(hired_graduate.State.Calendar.Pending.Graduates.CandidateIndex == 0 and hired_graduate.State.Calendar.Pending.Graduates.GreetingVariant != null,"Hiring persists its greeting before advancing the candidate")
    await press("LOAD_GAME")
    check(main.find_child("ConversationWindow",true,false) != null,"Reload resumes the graduate greeting")
    await finish_conversation()
    await press("SAVE_GAME")
    var greeted_graduate = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(greeted_graduate.State.Calendar.Pending.Graduates.CandidateIndex == 1,"Acknowledging the greeting advances exactly one graduate")
    check(greeted_graduate.State.Money == hired_graduate.State.Money and greeted_graduate.State.Employees.size() == 1,"Greeting acknowledgement does not charge or hire again")
    await press_node("DeclineGraduate")
    await press("SAVE_GAME")
    var graduates_done = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(graduates_done.State.Employees.size() == 1 and graduates_done.State.Money == hired_graduate.State.Money,"Declining another graduate preserves the first hire")
    # Beginner ending is terminal; Normal mode can load another save afterwards.
    var ending_save = awards_save.duplicate(true)
    ending_save.State.Mode = 0
    ending_save.State.Year = 2
    ending_save.State.Calendar.Pending = {"ClosingDate":{"Year":2,"Month":12,"Week":4},"Phase":0,"Index":0,"PendingEvent":10}
    write_json("user://saves/slot1.json",ending_save)
    await press("LOAD_GAME")
    check(main.find_child("BeginnerEnding",true,false) != null,"Beginner ending opens its title-return message")
    await press("BACK_TO_TITLE")
    check(main.find_child("BeginnerEnding",true,false) == null,"Ending returns to the title screen")
    # The uncertainty guard permits only the verified staff-list scratch writer.
    var unknown_payroll = payroll_captured.duplicate(true)
    var ps = unknown_payroll.State
    ps.Year = 3
    ps.Month = 5
    ps.Week = 4
    ps.CompanyActivityTicks = 0
    ps.OriginalPayrollScratchIndex = null
    ps.DevelopmentProjects = []
    ps.ReleasedGames = []
    ps.Calendar = launched.State.Calendar.duplicate(true)
    ps.Calendar.Pending = {"ClosingDate":{"Year":3,"Month":5,"Week":4},"Phase":1,"Index":0,"Block":0}
    write_json("user://saves/slot1.json",unknown_payroll)
    await press("LOAD_GAME")
    await press_node("OpenPayrollStaff")
    check(main.find_child("StaffSalaryTotal",true,false).text.ends_with("400"),"Uncertain payroll can open the verified staff-list projection")
    check(main.find_child("DismissEmployee1",true,false) == null and main.find_child("DismissEmployee2",true,false) == null,"Payroll resolution staff list cannot dismiss employees during the suspended transaction")
    await press("CANCEL")
    await press_node("RetryCalendar")
    await press("SAVE_GAME")
    var resolved_payroll = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(resolved_payroll.State.Calendar.Pending == null and resolved_payroll.State.Money == ps.Money-600,"Verified scratch resolution resumes the saved payroll once")
    check(resolved_payroll.State.Month == 6 and resolved_payroll.State.Week == 1,"Resolving payroll completes the calendar rollover")
    # Sequel boundary fixture: a known Hall of Fame parent feeds the actual
    # room chooser, quote, meeting, work, release, review and sales screens.
    var sequel_save = released.duplicate(true)
    var sequel_state = sequel_save.State
    sequel_state.DevelopmentProjects = []
    sequel_state.Calendar = null
    sequel_state.CompanyActivityTicks = 0
    sequel_state.SalesTicks = 0
    sequel_state.Company.HallOfFameCount = 1
    var predecessor = sequel_state.ReleasedGames[0]
    predecessor.ReviewScores = [8,8,8,8]
    predecessor.ReviewPending = false
    predecessor.HallOfFame = true
    predecessor.HasReleasedSequel = false
    predecessor.Quality = {"Fun":101.9,"Novelty":99.9,"Graphics":33.4,"Music":6.6}
    predecessor.Recognition = 99.9
    predecessor.Stock = 0
    predecessor.Demand = 0
    predecessor.Recall = {"OriginalState":1,"WeeksRemaining":0,"PendingEvent":null,"OutcomeRoll":null,"RefundAmount":null}
    for sequel_worker in sequel_state.Employees:
        sequel_worker.Stress = 0
    write_json("user://saves/slot1.json",sequel_save)
    await press("LOAD_GAME")
    var sequel_office = main.find_child("OriginalOfficeBackground",true,false)
    var room_click = InputEventMouseButton.new()
    room_click.button_index = MOUSE_BUTTON_LEFT
    room_click.pressed = true
    room_click.position = Vector2(192,148)
    sequel_office.gui_input.emit(room_click)
    await process_frame
    await press_node("RoomSequel")
    var predecessor_table = main.find_child("SequelPredecessors",true,false)
    check(predecessor_table != null and predecessor_table.get_root().get_first_child().get_text(0) == predecessor.Title,"Hall of Fame predecessor appears in organized sequel table")
    await press("CANCEL")
    await press("CANCEL")
    await press("SAVE_GAME")
    var cancelled_sequel = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(cancelled_sequel.State.Money == sequel_state.Money and not cancelled_sequel.State.ReleasedGames[0].HasReleasedSequel,"Cancelling predecessor selection has no transaction")
    sequel_office.gui_input.emit(room_click)
    await process_frame
    await press_node("RoomSequel")
    await press_node("ConfirmSequelPredecessor")
    check(main.find_child("PlanningRoom",true,false).get_selected_id() == 0,"Sequel carries selected development room")
    await press_node("PlanningMeeting")
    check(main.find_child("ConversationWindow",true,false) != null and main.find_child("AcceptPlan",true,false) == null,"Planning discussion precedes proposal acceptance")
    await finish_conversation()
    check(main.find_child("PlanningTitle",true,false).text == predecessor.Title + "2","Original series-based sequel title is suggested")
    await press_node("AcceptPlan")
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var accepted_sequel = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var sequel_project = accepted_sequel.State.DevelopmentProjects[0]
    check(sequel_project.OriginalMode == 1 and sequel_project.PredecessorProductIndex == 0,"Sequel mode and lineage survive real UI acceptance")
    # The real clock can contribute between acceptance and opening Save.
    check(sequel_project.Quality.Fun >= 30 and sequel_project.Quality.Novelty >= 29 and sequel_project.Quality.Graphics >= 10 and sequel_project.Quality.Music >= 1 and sequel_project.Recognition == 29,"Sequel begins with inherited quality and recognition")
    check(accepted_sequel.State.Money == sequel_state.Money-30 and not accepted_sequel.State.ReleasedGames[0].HasReleasedSequel,"Only quote is charged before shipment")
    await press("LOAD_GAME")
    sequel_office = main.find_child("OriginalOfficeBackground",true,false)
    sequel_office.gui_input.emit(room_click)
    await process_frame
    Engine.time_scale = 100
    if not await await_node("DevelopmentReport"):
        quit(1)
        return
    var sequel_ready := false
    for frame in range(15000):
        check(main.find_child("SelectType0",true,false) == null and main.find_child("SelectGenre0",true,false) == null,"Sequel checkpoints do not offer new type or genre")
        if main.find_child("ConversationWindow",true,false) != null:
            await finish_conversation()
        for button in main.find_children("*","Button",true,false):
            if button.text == tr("RELEASE_GAME") and button.is_visible_in_tree():
                sequel_ready = true
                break
        if sequel_ready:
            break
        await process_frame
    Engine.time_scale = 1
    check(sequel_ready,"Sequel automatic checkpoints reach debugging")
    if not sequel_ready:
        quit(1)
        return
    await press("SAVE_GAME")
    var complete_sequel = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    var completed_project = complete_sequel.State.DevelopmentProjects[0]
    check(completed_project.GameTypeSourceIndex == predecessor.GameTypeSourceIndex and completed_project.GenreSourceIndex == predecessor.GenreSourceIndex,"Sequel inherits parent's type and genre")
    check(completed_project.RequiredPoints == 50 and complete_sequel.State.Money == accepted_sequel.State.Money,"Inherited checkpoints add points without selection fees")
    await press("LOAD_GAME")
    sequel_office = main.find_child("OriginalOfficeBackground",true,false)
    sequel_office.gui_input.emit(room_click)
    await process_frame
    await press("RELEASE_GAME")
    var sequel_release_title = main.find_child("ReleaseTitle",true,false)
    if sequel_release_title == null:
        check(false,"Sequel release form opens after reload")
        quit(1)
        return
    check(sequel_release_title.text == predecessor.Title + "2","Shipment suggests the original sequel name")
    main.find_child("ReleaseCopies",true,false).value = 1000
    await press_node("ConfirmRelease")
    await finish_conversation()
    check(main.find_child("ReviewScore3",true,false) != null,"Sequel uses the standard review ceremony")
    await press("SAVE_GAME")
    var shipped_sequel = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(shipped_sequel.State.DevelopmentProjects.is_empty() and shipped_sequel.State.ReleasedGames[0].HasReleasedSequel,"Sequel shipment frees room and consumes parent eligibility")
    var shipped_child = shipped_sequel.State.ReleasedGames[1]
    check(shipped_child.OriginalMode == 1 and shipped_child.PredecessorProductIndex == 0 and shipped_child.SeriesBaseTitle == predecessor.Title and shipped_child.OriginalSequelCounter == 1,"Shipment records original lineage and fresh-slot counter")
    shipped_child.Recall = {"OriginalState":1,"WeeksRemaining":0,"PendingEvent":null,"OutcomeRoll":null,"RefundAmount":null}
    write_json("user://saves/slot1.json",shipped_sequel)
    await press("LOAD_GAME")
    await press_node("AcknowledgeReviews")
    check(main.find_child("SalesProduct1",true,false) != null,"Sequel joins normal sales screen")
    Engine.time_scale = 100
    for frame in range(100):
        await process_frame
    Engine.time_scale = 1
    await press("BACK_TO_OFFICE")
    await press("SAVE_GAME")
    var sequel_sales = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(sequel_sales.State.ReleasedGames[1].UnitsSold > 0 and sequel_sales.State.ReleasedGames[1].Stock < 1000,"Sequel sales consume stock after save/reload")
    check(sequel_sales.State.ReleasedGames[1].NetSalesReceipts == sequel_sales.State.ReleasedGames[1].UnitsSold*2800,"Sequel uses original sales receipts")
    # Manga adaptation uses the real room selector, rights meeting, quote,
    # ordinary type/genre checkpoints, shipment/reviews and sales.
    var manga_fixture = released.duplicate(true)
    manga_fixture.State.DevelopmentProjects = []
    manga_fixture.State.ReleasedGames = []
    manga_fixture.State.Calendar = null
    manga_fixture.State.CompanyActivityTicks = 0
    manga_fixture.State.Company.HallOfFameCount = 0
    manga_fixture.State.Money = 10000000
    for manga_worker in manga_fixture.State.Employees:
        manga_worker.Stress = 0
        manga_worker.Office.WorkTicks = 0
    write_json("user://saves/slot1.json",manga_fixture)
    await press("LOAD_GAME")
    var manga_office = main.find_child("OriginalOfficeBackground",true,false)
    manga_office.gui_input.emit(room_click)
    await process_frame
    await press_node("RoomManga")
    var manga_table = main.find_child("MangaTitles",true,false)
    check(manga_table != null and manga_table.get_root().get_child_count() == 1,"Manga selector loads local records")
    check(manga_table.get_root().get_first_child().get_text(0) == tr("CATALOG_MANGA_TITLE_0"),"Manga title has an English display translation")
    await press_node("ConfirmManga")
    check(main.find_child("ConversationMessage",true,false).text == tr("MANGA_MEETING_0").replace("{publisher}",tr("CATALOG_MANGA_PUBLISHER_0")),"Rights meeting begins with publisher introduction")
    await finish_conversation()
    await press("SAVE_GAME")
    var rights_preview = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(rights_preview.State.Money == 10000000 and rights_preview.State.DevelopmentProjects.is_empty(),"Rights conversation has no transaction")
    await press_node("AgreeMangaRights")
    await finish_conversation()
    check(main.find_child("PlanningQuote",true,false).text.contains("130"),"Manga quote includes the rights fee")
    await press_node("PlanningMeeting")
    await finish_conversation()
    main.find_child("PlanningTitle",true,false).text = "Synthetic Manga Game"
    await press_node("AcceptPlan")
    await press("SAVE_GAME")
    var accepted_manga = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(accepted_manga.State.Money == 9999870 and accepted_manga.State.DevelopmentProjects[0].OriginalMode == 2 and accepted_manga.State.DevelopmentProjects[0].MangaSourceIndex == 0,"Manga acceptance records the source and charges the combined fee once")
    await press("LOAD_GAME")
    Engine.time_scale = 100
    if not await await_node("SelectType0"):
        quit(1)
        return
    await press_node("SelectType0")
    await press_node("ConfirmTypeChoice")
    if not await await_node("SelectGenre0"):
        quit(1)
        return
    await press_node("SelectGenre0")
    await press_node("ConfirmGenreChoice")
    var manga_ready := false
    for frame in range(15000):
        if main.find_child("ConversationWindow",true,false) != null:
            await finish_conversation()
        for release_button in main.find_children("*","Button",true,false):
            if release_button.text == tr("RELEASE_GAME"):
                manga_ready = true
                break
        if manga_ready:
            break
        await process_frame
    check(manga_ready,"Manga development reaches the original debugging/release checkpoint")
    if not manga_ready:
        quit(1)
        return
    await press("RELEASE_GAME")
    main.find_child("ReleaseCopies",true,false).value = 1000
    await press_node("ConfirmRelease")
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var manga_shipment = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(manga_shipment.State.ReleasedGames[0].OriginalMode == 2 and manga_shipment.State.ReleasedGames[0].MangaSourceIndex == 0 and manga_shipment.State.DevelopmentProjects.is_empty(),"Manga shipment preserves adaptation identity and frees the room")
    check(manga_shipment.State.ReleasedGames[0].ShipmentMessages[0] == 0,"Popular comic commentary is the first adaptation shipment page")
    await press("LOAD_GAME")
    await finish_conversation()
    await press_node("AcknowledgeReviews")
    Engine.time_scale = 100
    for frame in range(100):
        await process_frame
    Engine.time_scale = 1
    await press("SAVE_GAME")
    var manga_sales = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
    check(manga_sales.State.ReleasedGames[0].UnitsSold > 0,"Manga release enters the normal sales simulation")
    # Saved modal hire notifications require no original artwork in public CI.
    for variant in range(10):
        var greeting_fixture = {"Format":"OpenGDS2","SchemaVersion":1,"State":state.duplicate(true)}
        greeting_fixture.State.PendingHireGreeting={"EmployeeSourceIndex":0,"Variant":variant}
        write_json("user://saves/slot1.json",greeting_fixture)
        await press("LOAD_GAME")
        var greeting_window = main.find_child("ConversationWindow",true,false)
        var greeting_message = main.find_child("ConversationMessage",true,false)
        check(greeting_window != null and greeting_window.size.x <= 350 and greeting_window.size.y <= 165,"Compact saved hire conversation")
        check(greeting_window.position == moved_dialogue_position,"New hire conversations retain the position from calendar dialogue")
        if variant == 9:
            greeting_window.position = greeting_window.get_parent().size-Vector2(greeting_window.size.x,40)
            root.size = Vector2i(640,360)
            await process_frame
            await process_frame
            var bounds = greeting_window.get_parent().size
            check(greeting_window.position.x >= 0 and greeting_window.position.x <= maxf(0,bounds.x-greeting_window.size.x) and greeting_window.position.y <= maxf(0,bounds.y-24),"Remembered dialogue position clamps inside a smaller desktop")
            var clamped_position = greeting_window.position
            await press("SAVE_GAME")
            await press("LOAD_GAME")
            greeting_window = main.find_child("ConversationWindow",true,false)
            greeting_message = main.find_child("ConversationMessage",true,false)
            check(greeting_window.position == clamped_position,"Reload retains the clamped dialogue position")
            root.size = original_viewport_size
            await process_frame
        var expected_greeting = tr("HIRE_GREETING_%d" % variant).replace("{company}",state.Company.Name)
        check(greeting_message != null and greeting_message.text == expected_greeting,"English greeting branch %d" % variant)
        check(main.find_child("ConversationSpeaker",true,false).text == worker.Name,"Synthetic/custom employee names remain unchanged")
        await press("SAVE_GAME")
        var greeting_before = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        await create_timer(0.2).timeout
        await press("SAVE_GAME")
        var greeting_after = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        check(greeting_after.State.PendingHireGreeting == greeting_before.State.PendingHireGreeting and greeting_after.State.Money == greeting_before.State.Money and greeting_after.State.CompanyActivityTicks == greeting_before.State.CompanyActivityTicks,"Conversation freezes simulation and persists once")
        var accept := InputEventKey.new()
        accept.keycode=KEY_ENTER;accept.pressed=true
        greeting_message.gui_input.emit(accept)
        await process_frame
        await process_frame
        check(main.find_child("ConversationWindow",true,false) != null and greeting_message.visible_characters == expected_greeting.length(),"First Enter reveals the current page")
        await create_timer(0.03).timeout # The original 20ms reveal timer marks the page complete.
        greeting_message.gui_input.emit(accept)
        await process_frame
        await process_frame # Allow QueueFree to retire the completed modal.
        check(main.find_child("ConversationWindow",true,false) == null,"Second Enter advances the completed page")
        await press("SAVE_GAME")
        var acknowledged = JSON.parse_string(FileAccess.get_file_as_string("user://saves/slot1.json"))
        check(acknowledged.State.PendingHireGreeting == null and acknowledged.State.Employees.size() == 1 and acknowledged.State.Money == greeting_before.State.Money,"Acknowledgment does not rehire or charge")
    previous_stream = null
    player = null
    check(heard_meeting_phases.size() == 4,"Core loop exercised all four meeting soundtracks including final/debugging dialogue")
    main.queue_free()
    main = null
    await process_frame
    await process_frame
    print("Core loop UI checks complete; failures=",failures)
    quit(0 if failures == 0 else 1)
