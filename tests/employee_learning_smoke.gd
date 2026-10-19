extends "res://tests/window_artwork_smoke.gd"

func learning_value() -> Dictionary:
    var v=complete_value();v.State.SecretarySourceIndex=1;v.State.DevelopmentProjects=[]
    v.State.TechnologyOwnershipLevels={"0":3,"1":0,"2":3}
    for e in v.State.Employees:e.TechnologyLevels={"0":0,"1":0,"2":3};e.RawParameter34=4
    return v

func open_employee(v: Dictionary) -> void:
    await load_fixture(v);await press_node("TogglePause");await select_employee(0)
    if not main.find_child("EmployeeTechnologies",true,false).button_pressed:await press_node("EmployeeTechnologies")

func acknowledge() -> void:
    for page in range(8):
        if main.find_child("ConversationWindow",true,false)==null:break
        await award_page()

func seated_learning(v: Dictionary) -> void:
    await open_employee(v);await office_input(MOUSE_BUTTON_RIGHT,Vector2(166,68));await press_node("EmployeeLearnTechnology")
    await window("TrainingWindow",281,270)

func capture_employee() -> void:
    var output=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
    if output.is_empty() or DisplayServer.get_name()=="headless":return
    await process_frame;await RenderingServer.frame_post_draw
    var image=root.get_texture().get_image();var panel=main.find_child("EmployeeDetailsWindow",true,false)
    DirAccess.make_dir_recursive_absolute(output)
    check(image.get_region(Rect2i(panel.get_global_rect())).save_png(output.path_join(TranslationServer.get_locale()+"-EmployeeTechnologies.png"))==OK,"Rendered native Learn and Use controls")

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"));catalog.Technologies=[]
    for source in range(3):
        catalog.Technologies.append({"SourceIndex":source,"Identifier":source,"Name":"Authored technique %d"%source,"Description":"Authored description","RequiredTrainingWork":[1,2,3],"ExperienceCosts":[10,20,30],"RawTable1568":[1,2,3],"RawTable158C":[1,2,3]})
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        var v=learning_value();await load_fixture(v);await press_node("TogglePause");await select_employee(0)
        check(not main.find_child("EmployeeLearnTechnology",true,false).is_visible_in_tree(),"Learn belongs to the technology view, not the abilities page")
        await press_node("EmployeeTechnologies")
        var learn=main.find_child("EmployeeLearnTechnology",true,false);var use=main.find_child("EmployeeUseTechnology",true,false)
        check(learn.visible and use.visible and learn.get_parent()==use.get_parent(),"Original Learn and Use controls share the technology action row")
        check(main.find_child("EmployeeDetailsWindow",true,false).get_global_rect().encloses(learn.get_global_rect()),"Learn is accessible inside the employee window")
        await capture_employee()
        var gate_owner_id=main.find_child("EmployeeDetailsWindow",true,false).get_instance_id()
        await press_node("EmployeeLearnTechnology")
        check(main.find_child("TrainingWindow",true,false)==null and main.find_child("ConversationMessage",true,false).text==tr("TRAINING_RESULT_NotAtStudySeat"),"Original Learn handler asks for a workstation before opening courses")
        check(main.find_child("EmployeeDetailsWindow",true,false).get_instance_id()==gate_owner_id,"Workstation refusal preserves the employee window")
        await acknowledge();await office_input(MOUSE_BUTTON_RIGHT,Vector2(166,68))
        main.find_child("EmployeeDetailsWindow",true,false).position=Vector2(650,90)
        var employee_window_id=main.find_child("EmployeeDetailsWindow",true,false).get_instance_id()
        await press_node("EmployeeLearnTechnology");await window("TrainingWindow",281,270)
        check(main.find_child("EmployeeDetailsWindow",true,false).get_instance_id()==employee_window_id and main.find_child("EmployeeDetailsWindow",true,false).position==Vector2(650,90),"Native learning child preserves its employee owner and position")
        var courses=main.find_child("TrainingCourses",true,false)
        check(courses.get_root().get_child_count()==1 and courses.get_root().get_first_child().get_metadata(0)==0,"Learning list excludes unpurchased and fully mastered technologies")
        if locale=="en":check(main.find_child("TrainingDescription",true,false).text.contains("Lv.1"),"English description exposes the next level when the compact name column clips")
        check(main.find_child("StudySeats",true,false)==null and main.find_child("ManagementSave",true,false)==null and main.find_child("ManagementLoad",true,false)==null,"Original course dialog has no added assignment or persistence buttons")
        await press_node("CancelTraining");await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Experience==100 and saved().Employees[0].Office.Training==null,"Cancelling course selection preserves experience and assignment")
        check(main.find_child("EmployeeTechnologiesPage",true,false).is_visible_in_tree(),"Cancel returns to the employee technology view")
        await dispose()
        for reason in ["xp","capacity","zero","none"]:
            v=learning_value()
            if reason=="xp":v.State.Employees[0].Experience=5
            if reason=="capacity":v.State.Employees[0].RawParameter34=1
            if reason in ["zero","none"]:
                for e in v.State.Employees:e.TechnologyLevels={"0":0,"1":0,"2":0}
                v.State.TechnologyOwnershipLevels={"0":3,"1":0,"2":0}
            if reason=="zero":v.State.Employees[0].RawParameter34=0
            if reason=="none":v.State.TechnologyOwnershipLevels={"0":0,"1":0,"2":0}
            await seated_learning(v)
            if reason=="none":
                check(main.find_child("TrainingCourses",true,false).get_root().get_child_count()==0 and main.find_child("TrainSelectedTechnology",true,false).disabled,"No purchased technologies produces the original empty learning dialog")
                check(main.find_child("TrainingDescription",true,false).text==tr("NO_PURCHASED_TECHNOLOGIES"),"No-purchase explanation is localized")
            else:
                check(not main.find_child("TrainTechnology0",true,false).disabled,"Original confirmation can report its experience/capacity refusal")
                await press_node("TrainTechnology0")
                var key="LEARNING_INSUFFICIENT_EXPERIENCE" if reason=="xp" else "LEARNING_CAPACITY_REACHED" if reason=="capacity" else "LEARNING_CANNOT_LEARN"
                var refusal=main.find_child("ConversationMessage",true,false)
                check(refusal!=null and refusal.text==tr(key).replace("{0}",v.State.Employees[0].Name).replace("{1}",str(v.State.Employees[0].RawParameter34)),"Secretary reports the original learning refusal: "+reason)
                if refusal==null:await dispose();continue
                await acknowledge();check(main.find_child("TrainingWindow",true,false)!=null,"Learning refusal returns to course selection")
            await press_node("CancelTraining");await press_node("ToolbarSAVE_GAME")
            check(saved().Employees[0].Experience==v.State.Employees[0].Experience and saved().Employees[0].Office.Training==null,"Rejected or empty learning never charges experience: "+reason)
            await dispose()
        v=learning_value();v.State.TechnologyOwnershipLevels={"0":3,"1":0,"2":0}
        for e in v.State.Employees:e.TechnologyLevels={"0":0,"1":0,"2":0}
        await seated_learning(v);await press_node("TrainTechnology0");await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Experience==90 and saved().Employees[0].Office.Training.TechnologySourceIndex==0,"Native Learn confirmation starts one course and charges once")
        check(main.find_child("EmployeeLearningStatus",true,false).visible,"Employee technology view reports active learning")
        check(main.find_child("EmployeeUseTechnology",true,false).disabled,"Use remains unavailable before the first learned technology")
        main.call("SetSimulationPaused",false);Engine.time_scale=100
        for frame in range(625):
            await process_frame
            var learning=main.find_child("EmployeeLearningStatus",true,false)
            if learning==null or not learning.visible:break
        Engine.time_scale=1;main.call("SetSimulationPaused",true)
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].TechnologyLevels["0"]==1 and saved().Employees[0].Office.Training==null,"Original study ticks complete the course")
        for frame in range(8):await process_frame
        check(not main.find_child("EmployeeLearningStatus",true,false).visible and not main.find_child("EmployeeUseTechnology",true,false).disabled,"Employee view refreshes completion and enables Use without reopening")
        check(main.find_child("EmployeeTechnologyTable",true,false).get_root().get_first_child().get_text(1)=="1","Completed skill appears immediately in the employee technology list")
        await press_node("EmployeeLearnTechnology")
        var upgrade=main.find_child("TrainingCourses",true,false).get_root().get_first_child()
        check(upgrade.get_text(0).ends_with("Lv.2") and upgrade.get_text(2)=="20","Next learning level uses its own original experience cost")
        await press_node("CancelTraining");await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Experience==90 and saved().Employees[0].Office.Training==null,"Cancelling an upgrade does not repeat the first course charge")
        await dispose()
    print("Employee learning smoke failures: ",failures);quit(0 if failures==0 else 1)
