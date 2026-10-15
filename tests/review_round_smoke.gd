extends "res://tests/native_layout_smoke.gd"

func complete_value() -> Dictionary:
    var v=value()
    v.State.TechnologyOwnershipLevels={"0":0,"1":0,"2":0,"3":0,"4":0,"5":0,"6":0,"7":0}
    v.State.Money=2000000000
    for employee in v.State.Employees:
        employee.Level=1;employee.Experience=100;employee.ContractFee=10;employee.RawParameter3C=8
        employee.Fun=50;employee.Graphics=50;employee.Novelty=50;employee.Music=50;employee.Accuracy=50;employee.DevelopmentSpeed=50;employee.Inspiration=50
    v.State.EmployeeSources[0].Employee=v.State.Employees[0]
    v.State.EmployeeSources[1].Employee=v.State.Employees[1]
    return v

func award_page() -> void:
    var message=main.find_child("ConversationMessage",true,false)
    if message==null:return
    var accept=InputEventKey.new();accept.keycode=KEY_ENTER;accept.pressed=true
    message.gui_input.emit(accept)
    await process_frame
    if is_instance_valid(message) and message.is_inside_tree():
        await create_timer(.03).timeout
        message.gui_input.emit(accept)
        await process_frame

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.DevelopmentComputers=[]
    catalog.Technologies=[]
    for i in range(4):catalog.DevelopmentComputers.append({"SourceIndex":i,"Name":"Authored computer %d" % i,"BitWidth":8*int(pow(2,i)),"PurchasePrice":100*(i+1),"Description":"Authored computer details"})
    for i in range(8):catalog.Technologies.append({"SourceIndex":i,"Identifier":i,"Name":"Authored technology %d" % i,"Description":"Authored skill details","PurchaseFee":10,"InitialOwnershipLevel":1,"RequiredTrainingWork":[40,60,80],"ExperienceCosts":[10,20,30],"RawTable1568":[1,2,3],"RawTable158C":[1,2,3]})
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Review locale")
        await load_fixture(complete_value())
        await press_node("ToolbarDEVELOPMENT_COMPUTERS")
        await window("ComputerPurchaseWindow",325,280)
        var table=main.find_child("ComputerPurchaseTable",true,false)
        var selected=table.get_root().get_first_child().get_next()
        selected.select(0)
        await process_frame
        check(not main.find_child("ConfirmComputerPurchase",true,false).disabled,"Computer purchase follows selected row")
        await press_node("ConfirmComputerPurchase")
        await press_node("ToolbarSAVE_GAME")
        check(saved().DevelopmentComputerIndex==1 and saved().Money==1999999800,"Selected computer charges once")
        await press_node("CancelComputerPurchase")
        await press_node("ToolbarTECHNOLOGIES")
        await window("TechnologyPurchaseWindow",285,285)
        check(main.find_child("TechnologyPurchaseTable",true,false).select_mode==Tree.SELECT_ROW,"Technology highlights complete row")
        await press_node("PurchaseTechnology0")
        await press_node("ToolbarSAVE_GAME")
        check(saved().TechnologyOwnershipLevels["0"]==3 and saved().Money==1999999790,"Technology purchase remains connected")
        await press_node("CancelTechnologyPurchase")
        await dispose()
        var sales=complete_value();var game=product();game.ReviewPending=false;game.ReviewDisplayCallbacks=90;game.WeeklySalesHistory=[6000,3000,4500,7500,0,0,0,0,0,0,0,0,0];sales.State.ReleasedGames=[game]
        await load_fixture(sales)
        await press_node("ToolbarSALES")
        check(main.find_child("CompactProductWindow",true,false)==null,"Software toolbar opens the Products list")
        table=main.find_child("DesktopProductTable",true,false)
        check(table.select_mode==Tree.SELECT_ROW,"Product selection spans all columns")
        table.get_root().get_first_child().select(0);table.item_activated.emit();await process_frame
        await window("CompactProductWindow",195,325)
        check(main.find_child("ProductSelector",true,false)==null,"Native compact report has no dropdown")
        if not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty():check(main.find_child("OfficeWindow",true,false).find_child("WindowTitleIcon",true,false).texture!=null,"Office icon comes from native form")
        await dispose()
        for kind in range(9):
            var facility=complete_value();facility.State.Calendar.OriginalSlots0444=[kind,-1,-1]
            await load_fixture(facility)
            await press_node("CompanyFacilities")
            # Clicking the corresponding office plot uses the selected-slot path.
            var office=main.find_child("OriginalOfficeBackground",true,false)
            main.find_child("FacilitiesOverviewWindow",true,false).find_child("CloseWindow",true,false).pressed.emit()
            await process_frame
            var native_pos=Vector2(90,215) # first occupied facility slot hit region
            var click=InputEventMouseButton.new();click.button_index=MOUSE_BUTTON_LEFT;click.pressed=true;click.position=native_pos
            office.gui_input.emit(click);await process_frame
            var name=["School","HotSpring","Recycling","TechnologySchool","Casino","Lawyer","MysteryLab","ProductionLab","HardwareLab"][kind]
            await window("Facility"+name+"Window",300,365)
            if main.find_child("FacilityIllustration",true,false)!=null and not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty():check(main.find_child("FacilityIllustration",true,false).texture!=null,"Distinct native facility illustration")
            await dispose()
        await load_fixture(complete_value())
        table=main.find_child("DesktopEmployeeTable",true,false)
        table.get_root().get_first_child().select(0);table.item_activated.emit();await process_frame
        await press_node("EmployeeLevelUp")
        await window("LevelUpResultWindow",165,300)
        await press_node("ToolbarSAVE_GAME")
        var upgraded=saved()
        check(upgraded.PendingLevelUpResult.Gains.size()==7 and upgraded.Employees[0].Level==2,"Level Up displays actual committed stat gains")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("LevelUpResultWindow",true,false)!=null,"Reload restores unread gain popup")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].OriginalAttributes==upgraded.Employees[0].OriginalAttributes,"Reload does not apply the upgrade again")
        main.find_child("LevelUpResultWindow",true,false).find_child("CloseWindow",true,false).pressed.emit();await process_frame
        var caption=main.find_child("OfficeWindow",true,false).find_child("WindowCaption",true,false)
        await press_node("ToolbarSAVE_GAME");var before=saved().CompanyActivityTicks
        var down=InputEventMouseButton.new();down.button_index=MOUSE_BUTTON_LEFT;down.pressed=true
        caption.gui_input.emit(down)
        await create_timer(.2).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks==before,"Dragging freezes gameplay clocks")
        var up=InputEventMouseButton.new();up.button_index=MOUSE_BUTTON_LEFT;up.pressed=false
        Input.parse_input_event(up);await create_timer(.2).timeout
        await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks>before,"Mouse release outside caption resumes gameplay")
        caption.gui_input.emit(down)
        main.find_child("OfficeWindow",true,false).notification(Node.NOTIFICATION_APPLICATION_FOCUS_OUT)
        await create_timer(.2).timeout;await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks>before,"Lost capture does not leave time suspended")
        await press_node("TogglePause")
        caption.gui_input.emit(down);Input.parse_input_event(up);await process_frame
        await press_node("ToolbarSAVE_GAME");before=saved().CompanyActivityTicks
        await create_timer(.2).timeout;await press_node("ToolbarSAVE_GAME")
        check(saved().CompanyActivityTicks==before,"Drag release preserves manual pause")
        await dispose()
        var awards=complete_value();awards.State.Month=12;awards.State.Week=4;awards.State.ReleasedGames=[game]
        awards.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,"Awards":{"Year":3,"Entries":[0],"CategoryIndex":0}}
        await load_fixture(awards)
        var board=main.find_child("AwardCeremonyWindow",true,false);board.position=Vector2(590,260)
        await window("AwardCeremonyWindow",550,320)
        check(board.find_children("*","Button",true,false).size()==3,"Award board has only window chrome buttons")
        for frame in range(600):
            var talk=main.find_child("ConversationWindow",true,false)
            if talk!=null and talk.get_meta("award_phase",-1)==3:break
            if talk!=null:await award_page()
            else:await process_frame
        check(is_instance_valid(board) and board.position==Vector2(590,260),"Same board stays positioned through announcements")
        await press_node("ToolbarSAVE_GAME");var announced=saved()
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("ConversationWindow",true,false).get_meta("award_phase")==3,"Reload resumes result dialogue")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==announced.Money and saved().SimulationRandomState==announced.SimulationRandomState,"Reload repeats neither award draw nor payment")
        for frame in range(1600):
            if main.find_child("AwardCeremonyWindow",true,false)==null:break
            if main.find_child("ConversationWindow",true,false)!=null:await award_page()
            else:await process_frame
        check(main.find_child("AwardCeremonyWindow",true,false)==null,"Final conversation closes the awards board")
        await dispose()
    print("Third review smoke failures: ",failures)
    quit(0 if failures==0 else 1)
