extends "res://tests/issue118_facilities_smoke.gd"

func facility_value(kind: int) -> Dictionary:
    var v=complete_value();v.State.DevelopmentProjects=[];v.State.Calendar.OriginalSlots0444=[kind,-1,-1];v.State.SecretarySourceIndex=1
    return v

func chain_cases() -> void:
    var v=facility_value(1)
    await pause_load(v);await facility_input()
    var owner=main.find_child("FacilityHotSpringWindow",true,false);owner.position=Vector2(740,310)
    await press_node("UseHotSpring0")
    check(main.find_child("ConversationMessage",true,false).text.contains("8,000,000") or TranslationServer.get_locale().begins_with("ja"),"Hot spring cost introduction")
    await capture_desktop("hot-spring-intro");await award_page();await press_node("ConfirmFacilityService")
    check(is_instance_valid(owner) and owner.position==Vector2(740,310) and main.find_child("PlanningDiscussionScene",true,false)==null,"Full bath owner retained through results")
    await press_node("ToolbarSAVE_GAME");var receipt=saved().PendingFacilityReceipt
    check(receipt.Reactions.size()==v.State.Employees.size() and receipt.PageIndex==0,"Small staff each receive a saved hot-spring reaction")
    await capture_desktop("hot-spring-result");await award_page();await press_node("ToolbarSAVE_GAME")
    var paid=saved();check(paid.PendingFacilityReceipt.PageIndex==1,"Reaction cursor advances independently of payment")
    await capture_desktop("hot-spring-employee");var text=main.find_child("ConversationMessage",true,false).text
    await press_node("ToolbarLOAD_GAME")
    check(main.find_child("ConversationMessage",true,false).text==text and main.find_child("FacilityHotSpringWindow",true,false)!=null,"Reload restores same reaction and illustrated facility")
    for page in range(receipt.Reactions.size()):await award_page()
    await press_node("ToolbarSAVE_GAME");check(saved().PendingFacilityReceipt==null and saved().Money==paid.Money,"All staff pages finish without another charge")
    await dispose()
    # Declining a service closes its owner; declining demolition preserves it.
    await pause_load(facility_value(7));await facility_input();await press_node("ExpandProduction0");await award_page();await press_node("CancelFacilityService")
    check(main.find_child("FacilityProductionLabWindow",true,false)==null,"Declined production service closes the facility")
    await dispose()
    await pause_load(facility_value(3));await facility_input()
    owner=main.find_child("FacilityTechnologySchoolWindow",true,false);owner.position=Vector2(720,300)
    await press_node("TechnologyRoster0");var roster=main.find_child("FacilityStudyWindow",true,false)
    check(is_instance_valid(owner),"Study roster retains the illustrated school")
    await press_node("CancelFacilityStudy")
    check(main.find_child("FacilityTechnologySchoolWindow",true,false)==owner and owner.position==Vector2(720,300),"Study cancel restores the same parent and position")
    await dispose()
    # Insufficient funds is a conversation over both school and selector.
    v=facility_value(3);v.State.Money=39999999
    await pause_load(v);await facility_input();await press_node("TechnologyRoster0")
    roster=main.find_child("FacilityStudyWindow",true,false);roster.position=Vector2(830,380)
    var tree=main.find_child("FacilityStudyTable",true,false);tree.get_root().get_first_child().select(0);tree.item_selected.emit();await process_frame
    await press_node("StudyCapacity0")
    check(is_instance_valid(roster) and main.find_child("ConversationWindow",true,false)!=null and main.find_child("FacilityTechnologySchoolWindow",true,false)!=null,"Study funds refusal retains both illustrated parent and roster")
    await capture_desktop("technology-refusal");await award_page()
    check(main.find_child("FacilityStudyWindow",true,false)==roster and roster.position==Vector2(830,380),"Study refusal returns to same selector")
    check(not main.find_child("StudyCapacity0",true,false).disabled and roster.process_mode!=Node.PROCESS_MODE_DISABLED,"Study selector input is restored after refusal")
    await dispose()
    v=facility_value(4)
    await pause_load(v);await facility_input();owner=main.find_child("FacilityCasinoWindow",true,false)
    await press_node("EnterCasino0");var selector=main.find_child("CasinoSelector",true,false);selector.position=Vector2(850,350)
    await press_node("CasinoPoker")
    check(is_instance_valid(owner) and is_instance_valid(selector) and main.find_child("ConversationWindow",true,false)!=null,"Coinless casino choice gives a conversation over retained windows")
    await capture_desktop("casino-no-coins");await award_page()
    check(main.find_child("CasinoSelector",true,false)==selector and selector.position==Vector2(850,350),"No-coins message restores the same selector")
    check(not main.find_child("CasinoPoker",true,false).disabled and selector.process_mode!=Node.PROCESS_MODE_DISABLED,"Casino selector input is restored")
    await press_node("CasinoCancel");check(main.find_child("FacilityCasinoWindow",true,false)==owner,"Casino selector cancel restores its illustrated facility")
    await dispose()
    v=facility_value(6);v.State.MysteryLab={"Progress":1,"Outcome":0}
    await pause_load(v);await facility_input();await press_node("StartMysteryExperiment0")
    check(main.find_child("ConversationMessage",true,false).text==native_facility_text("FACILITY_MYSTERY_BUSY"),"Already-running experiment has its own secretary refusal")
    await capture_desktop("mystery-busy");await award_page();await press_node("ToolbarSAVE_GAME")
    check(saved().Money==v.State.Money and saved().MysteryLab.Progress==1,"Mystery refusal does not restart or charge")
    await dispose()

func chain_hardware_value(ready: bool=false) -> Dictionary:
    var v=complete_value();v.State.CompanyActivityTicks=0
    v.State.DevelopmentProjects=[{"RoomIndex":0,"HardwareSourceIndex":0,"AudienceSourceIndex":0,"OriginalRoomState":2,"OriginalMode":1,"Title":"Authored hardware development","OriginalStage":0,"RequiredPoints":2500,"Progress":2500 if ready else 1234,
        "Quality":{"Fun":800 if ready else 140,"Novelty":800 if ready else 260,"Graphics":800 if ready else 390,"Music":800 if ready else 520},
        "PolishingTargets":{"Fun":800,"Novelty":800,"Graphics":800,"Music":800},"Focus":{"Fun":25,"Novelty":25,"Graphics":25,"Music":25},
        "HardwareDesign":{"Design":1,"Medium":1,"TechnologyFlags":{"1":1,"3":1,"6":1,"7":1 if ready else 0}}}]
    v.State.Calendar.Hardware=[]
    for id in range(11):v.State.Calendar.Hardware.append({"SourceIndex":id,"Availability":1 if id==0 else 0,"LaunchWeeks":0,"RetirementWeeks":0,"Market":40,"GrowthWeeks":0,"GrowthBoost":0})
    return v

func hardware_chain_cases() -> void:
    var v=chain_hardware_value();v.State.Calendar.OriginalSlots0444=[8,-1,-1]
    await pause_load(v);await facility_input();var owner=main.find_child("FacilityHardwareLabWindow",true,false)
    await press_node("StartHardwarePlan0")
    check(main.find_child("ConversationMessage",true,false).text==native_facility_text("FACILITY_HARDWARE_BUSY") and is_instance_valid(owner),"Hardware already-developing refusal retains laboratory")
    await capture_desktop("hardware-busy");await award_page();check(main.find_child("FacilityHardwareLabWindow",true,false)==owner,"Hardware refusal returns to same facility")
    await dispose()
    v=facility_value(8);v.State.Money=1
    await pause_load(v);await facility_input();await press_node("StartHardwarePlan0");var plan=main.find_child("HardwarePlanWindow",true,false);plan.position=Vector2(750,330)
    await press_node("StartHardwareDevelopment")
    check(main.find_child("ConversationMessage",true,false).text==native_facility_text("FACILITY_NO_FUNDS") and is_instance_valid(plan),"Hardware funds refusal is a conversation over its design form")
    await capture_desktop("hardware-no-funds");await award_page();check(main.find_child("HardwarePlanWindow",true,false)==plan and plan.position==Vector2(750,330),"Hardware funds refusal restores same design form")
    await dispose()

func construction_cases() -> void:
    var v=facility_value(0);v.State.Money=1;v.State.Calendar.OriginalSlots0444=[-1,-1,-1]
    await pause_load(v);await facility_input();var picker=main.find_child("FacilityPickerWindow",true,false)
    await press_node("OpenSchool0")
    check(is_instance_valid(picker) and main.find_child("ConversationMessage",true,false).text==native_facility_text("FACILITY_NO_FUNDS"),"Construction funds refusal retains its picker")
    await capture_desktop("construction-no-funds");await award_page()
    check(main.find_child("FacilityPickerWindow",true,false)==picker,"Construction refusal restores same picker")
    await dispose()
    v=facility_value(0)
    await pause_load(v)
    var click=InputEventMouseButton.new();click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true;click.position=Vector2(160,230)
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(click);await process_frame
    await press_node("OpenSchool1")
    var expected=native_facility_text("FACILITY_ALREADY_OWNED").replace("{facility}",tr("FACILITY_NAME_School"))
    check(main.find_child("ConversationMessage",true,false).text==expected,"Duplicate construction names the installed facility")
    await award_page();await press_node("ToolbarSAVE_GAME")
    var actual=saved()
    check(int(actual.Money)==int(v.State.Money) and actual.Calendar.OriginalSlots0444.map(func(n):return int(n))==[0,-1,-1],"Duplicate construction leaves funds and plots unchanged")
    await dispose()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    setup_issue118()
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.set_value("display","number_grouping",1);settings.save("user://interface.cfg")
        await chain_cases();await hardware_chain_cases();await construction_cases();await service_cases();await all_facility_closures();await promotion_cases()
    print("Facility and six-media promotion failures: ",failures);quit(0 if failures==0 else 1)
