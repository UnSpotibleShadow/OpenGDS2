extends "res://tests/issue118_smoke.gd"

func facility_input() -> void:
    var click=InputEventMouseButton.new();click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true;click.position=Vector2(90,215)
    main.find_child("OriginalOfficeBackground",true,false).gui_input.emit(click);await process_frame

func hover_cases() -> void:
    for enlarged in [false,true]:
        var v=project_value();v.State.OfficeGraphicsEnlarged=enlarged
        await pause_load(v)
        var office=main.find_child("OriginalOfficeBackground",true,false)
        if not enlarged:
            var viewport=main.find_child("OfficeViewport",true,false)
            check(not viewport.get_v_scroll_bar().visible and not viewport.get_h_scroll_bar().visible,"Normal office fits without unnecessary scrollbars")
        var scale: float=min(office.size.x/466.0,office.size.y/290.0)
        var offset: Vector2=(office.size-Vector2(466,290)*scale)/2
        for item in [[Vector2(220,60),"OFFICE_TRAINING_HINT"],[Vector2(362,130),"OFFICE_VENDING_HINT"]]:
            var motion=InputEventMouseMotion.new();motion.position=offset+item[0]*scale;office.gui_input.emit(motion);await process_frame
            check(office.tooltip_text==tr(item[1]),"Object-specific office hint at scale "+str(scale))
        var theme=office.get_theme_stylebox("panel","TooltipPanel")
        check(theme.bg_color.a==1 and office.get_theme_color("font_color","TooltipLabel").is_equal_approx(Color.BLACK),"Tooltip has opaque background and black text")
        await dispose()

func all_facility_closures() -> void:
    var empty=complete_value();empty.State.DevelopmentProjects=[];empty.State.Calendar.OriginalSlots0444=[-1,-1,-1]
    await pause_load(empty);await facility_input()
    var prices=[600000000,60000000,80000000,100000000,500000000,250000000,300000000,150000000,1000000000]
    var row=main.find_child("FacilityPickerTable",true,false).get_root().get_first_child()
    for expected in prices:
        check(int(row.get_text(1).replace("¥","").replace(",",""))==expected,"Displayed facility price matches source")
        row=row.get_next()
    await dispose()
    for kind in range(9):
        var v=complete_value();v.State.DevelopmentProjects=[];v.State.Calendar.OriginalSlots0444=[kind,-1,-1];v.State.SecretarySourceIndex=0
        await pause_load(v);await facility_input()
        await press_node("CloseSchool0" if kind==0 else "CloseFacility0")
        check(main.find_child("ConversationWindow",true,false)!=null and main.find_child("FacilityIllustration",true,false)!=null,"Facility closure retains full owner behind secretary: "+str(kind))
        await award_page();await window("FacilityClosureWindow",344,162)
        await press_node("CancelFacilityService");await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.OriginalSlots0444[0]==kind and saved().Money==v.State.Money,"Cancelling closure preserves facility and funds: "+str(kind))
        await dispose()

func service_cases() -> void:
    for kind in [1,2,3,6,7]:
        var v=complete_value();v.State.DevelopmentProjects=[];v.State.Calendar.OriginalSlots0444=[kind,-1,-1];v.State.SecretarySourceIndex=0
        if kind==2:
            var stock=product();stock.ReviewPending=false;stock.ReviewDisplayCallbacks=90;stock.Recall={"OriginalState":1,"WeeksRemaining":0};v.State.ReleasedGames=[stock]
        await pause_load(v);await facility_input()
        var action={1:"UseHotSpring0",2:"RecycleStock0",3:"TechnologyRoster0",6:"StartMysteryExperiment0",7:"ExpandProduction0"}[kind]
        await press_node(action)
        if kind==3:
            var roster=main.find_child("FacilityStudyTable",true,false);roster.get_root().get_first_child().select(0);roster.item_selected.emit();await process_frame
            await press_node("StudyCapacity0")
        check(main.find_child("ConversationWindow",true,false)!=null,"Facility-specific service conversation: "+str(kind))
        if kind!=3:
            await capture_desktop("facility-intro-"+str(kind))
            await award_page();await press_node("ConfirmFacilityService")
        else:check(main.find_child("FacilityConfirmationWindow",true,false)==null,"Study applies directly from its employee selector")
        await press_node("ToolbarSAVE_GAME")
        var result=saved()
        var cost={1:8000000,2:-500000,3:40000000,6:25000000,7:10000000}[kind]
        check(result.Money==v.State.Money-(0 if kind in [2,7] else cost),"Facility payment boundary before result: "+str(kind))
        if kind!=6:
            await capture_desktop("facility-result-"+str(kind))
            if kind==7:check(result.PendingFacilityReceipt.CapacityBefore==v.State.Company.ProductionCapacity,"Production keeps its old capacity for the result")
            await press_node("ToolbarLOAD_GAME")
            for page in range(12):
                await award_page();await press_node("ToolbarSAVE_GAME")
                check(saved().Money==v.State.Money-cost,"Facility acknowledgement applies each payment exactly once")
                if saved().PendingFacilityReceipt==null:break
            check(saved().PendingFacilityReceipt==null,"Facility chain completes: "+str(kind))
        else:check(result.MysteryLab.Progress>0,"Mystery experiment remains active after its confirmation")
        await dispose()
    var v=complete_value();v.State.DevelopmentProjects=[];v.State.Calendar.OriginalSlots0444=[5,-1,-1];v.State.SecretarySourceIndex=0;v.State.SalesFee1=1500;v.State.SalesFee2=1500
    await pause_load(v);await facility_input();await press_node("NegotiateRetail0");await press_node("ConfirmFeeNegotiation")
    check(main.find_child("PlanningDiscussionPicture",true,false).texture!=null,"Lawyer negotiation restores business meeting artwork")
    await capture_desktop("lawyer");await press_node("ToolbarSAVE_GAME");var negotiated=saved()
    check(negotiated.Money==v.State.Money and negotiated.SalesFee1==1300,"Original lawyer checks funds without charging; first negotiation reduces fee")
    await press_node("ToolbarLOAD_GAME")
    for i in range(5):await award_page()
    await press_node("ToolbarSAVE_GAME");check(saved().Money==negotiated.Money and saved().SalesFee1==1300,"Lawyer conversation reload never rerolls outcome")
    await dispose()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    setup_issue118()
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.set_value("display","number_grouping",1);settings.save("user://interface.cfg")
        await hover_cases();await all_facility_closures();await service_cases()
    print("Issue118 facility smoke failures: ",failures);quit(0 if failures==0 else 1)
