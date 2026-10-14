extends "res://tests/presentation_completion_smoke.gd"

# Public fixtures are authored. Optional imported art/captures stay outside the
# checkout and are never part of the distributable game or repository.
func integer_tokens(data: Variant) -> Variant:
    if data is Dictionary:
        for key in data:
            data[key]=integer_tokens(data[key])
    elif data is Array:
        for i in range(data.size()):
            data[i]=integer_tokens(data[i])
    elif typeof(data)==TYPE_FLOAT and data==floor(data):
        return int(data)
    return data

func setup_catalog() -> void:
    panel_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.Hardware[0].RawMembers.merge({"0F00":16,"0FFC":40000000,"0FE0":300,"0EC8":0,"0FC4":20000000,"0F1C":1})
    catalog.Audiences[0].RawMembers={"14A8":0,"1454":200,"148C":0}
    catalog.DevelopmentComputers=[{"SourceIndex":0,"Name":"Synthetic workstation","BitWidth":16}]
    catalog.GameTypes=[]
    catalog.Genres=[]
    for i in range(8):
        catalog.GameTypes.append({"SourceIndex":i,"Name":"Synthetic content %d" % i,"RawMembers":{"1378":6000000,"1308":60,"13B0":0,"12EC":1},"RawTable13CC":[0,0,0,0,0,0,0,0],"ExampleTitles":["Authored example one","Authored example two"]})
        catalog.Genres.append({"SourceIndex":i,"Name":"Synthetic genre %d" % i,"RawMembers":{"1224":20000000,"1208":100,"125C":0,"1240":i}})
    write_json("user://original/catalog.json",integer_tokens(catalog))
    artwork_fixture()
    for i in range(1,3):
        blank_bmp("user://original/Graphic/Embedded/HallCelebration%d.bmp" % i,256,160,false)
    blank_bmp("user://original/Graphic/Embedded/PlanningProposal.bmp",256,160,false)
    var private_art=OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART")
    if not private_art.is_empty():
        var copied=true
        for filename in DirAccess.get_files_at(private_art.path_join("Graphic/Embedded")):
            copied=copied and DirAccess.copy_absolute(private_art.path_join("Graphic/Embedded").path_join(filename),ProjectSettings.globalize_path("user://original/Graphic/Embedded/"+filename))==OK
        check(copied,"Copy private local artwork batch")
        if FileAccess.file_exists(private_art.path_join("dialogue.json")):
            check(DirAccess.copy_absolute(private_art.path_join("dialogue.json"),ProjectSettings.globalize_path("user://original/dialogue.json"))==OK,"Private native dialogue cache")

func value() -> Dictionary:
    var state=neutral()
    state.State.Money=200000000
    state.State.DevelopmentComputerIndex=0
    state.State.Company.OriginalMember90=100
    state.State.OriginalTypeRosterMap={}
    for i in range(65):
        state.State.OriginalTypeRosterMap[str(i)]=0
    return state

func window(name: String,width: float,height: float) -> void:
    for i in range(4):
        await process_frame
    var dialog=main.find_child(name,true,false)
    check(dialog!=null,"Native window exists: "+name)
    if dialog==null:
        return
    check(dialog.size.x<=width and dialog.size.y<=height,"Compact bounds: "+name)
    var canvas=dialog.find_child("NativeDialogCanvas",true,false)
    check(canvas!=null,"Fixed client area: "+name)
    check(not canvas.get_parent() is ScrollContainer,"No outer dialog scrolling: "+name)
    for control in canvas.find_children("*","Control",true,false):
        if not control.is_visible_in_tree():
            continue
        if control is PopupMenu or control.get_parent() is PopupMenu:
            continue
        var parent=control.get_parent()
        var clipped=false
        while parent!=canvas:
            if parent is ScrollContainer:
                clipped=true
                break
            parent=parent.get_parent()
        if clipped:
            continue
        var rect=control.get_global_rect()
        var bounds=canvas.get_global_rect().grow(1)
        if not bounds.encloses(rect):print("OUTSIDE ",control.name," rect ",rect," bounds ",bounds)
        check(bounds.encloses(rect),"Client contains "+str(control.name)+" in "+name)
    var output=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
    if not output.is_empty() and DisplayServer.get_name()!="headless":
        await process_frame
        await RenderingServer.frame_post_draw
        var image=root.get_texture().get_image()
        var rect=Rect2i(dialog.get_global_rect())
        var crop=image.get_region(rect)
        DirAccess.make_dir_recursive_absolute(output)
        check(crop.save_png(output.path_join(TranslationServer.get_locale()+"-"+name+".png"))==OK,"Private rendered view: "+name)

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("native_layout_smoke requires disposable user data")
        quit(1)
        return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Native layout language")
        var planning=value()
        await load_fixture(planning)
        await press_node("ToolbarSTART_GAME_DEVELOPMENT")
        await press_node("PlanningMeeting")
        for page in range(50):
            if main.find_child("ConversationWindow",true,false)==null:
                break
            await advance_page()
        await window("PlanningMeetingWindow",330,390)
        await press_node("CancelProposal")
        await dispose()
        for phase in [-1,0,1,2]:
            var fixture_value=value()
            var p: Dictionary=stress_fixture().State.DevelopmentProjects[0].duplicate(true)
            p.Quality={"Fun":115,"Novelty":41,"Graphics":30,"Music":35}
            p.Focus={"Fun":44,"Novelty":18,"Graphics":17,"Music":21}
            p.RequiredPoints=500
            p.Bugs=22
            p.DataSize=13
            p.Recognition=81
            p.GameTypeSourceIndex=null
            p.GenreSourceIndex=null
            if phase==-1:
                p.OriginalStage=0;p.Progress=95
            else:
                p.PendingDecision=phase;p.DecisionIntroductionAcknowledged=phase
                p.OriginalStage=0 if phase==0 else 1 if phase==1 else 3
                p.Progress=250 if phase==0 else 405 if phase==1 else 500
                if phase<2:
                    p.CheckpointMeeting={"Milestone":phase,"Lines":[],"MessageIndex":0}
                    if phase==0:
                        p.TypeCandidates=[0,1,2,3,4]
                if phase==1:
                    p.GameTypeSourceIndex=0
                    p.CheckpointMeeting={"Milestone":1,"Lines":[{"Message":7},{"Message":10},{"Message":11},{"Message":12,"TypeSourceIndex":0}],"MessageIndex":4}
                if phase==2:
                    p.GameTypeSourceIndex=0;p.GenreSourceIndex=0
            fixture_value.State.DevelopmentProjects=[p]
            await load_fixture(fixture_value)
            if phase==-1:
                await press("DEVELOPMENT_PLANNING")
                # The room report is opened by the actual office room button.
                await press_node("OpenProject0")
            var target=["DevelopmentProjectWindow","TypeMeetingWindow","GenreMeetingWindow","CompletionWindow"][phase+1]
            await window(target,660 if phase in [0,1] else 385 if phase==2 else 310,365 if phase==2 else 250)
            if phase==-1:
                check(main.find_children("ProjectSummaryProgress","ProgressBar",true,false).size()==1 and main.find_child("DevelopmentProgress",true,false)==null,"One project progress display")
                await press_node("ProjectMedia")
                await window("MediaInformationWindow",350,290)
                check(main.find_child("MediaCapacityTable",true,false).size.y>=100,"Capacity table exposes all four rows")
            elif phase==2:
                await press_node("CompletionRelease")
                var copies=main.find_child("ReleaseCopies",true,false)
                var price=main.find_child("ReleasePrice",true,false)
                check(copies.get_parent().name=="ReleaseQuantityField" and price.get_parent().name=="ReleasePriceField","Held controls share their actual fields")
            await dispose()
        var reviews=value()
        var p=product()
        p.ReviewDisplayCallbacks=90
        reviews.State.ReleasedGames=[p]
        await load_fixture(reviews)
        await window("MagazineReviewsWindow",440,345)
        check(main.find_child("ReviewComment0",true,false).get_theme_color("default_color")==Color.BLACK,"Review comments have black text")
        check(main.find_child("ReviewCommentPanel0",true,false).get_theme_stylebox("panel").bg_color==Color.WHITE,"Review comments have a bordered white panel")
        await dispose()
        var sales=value()
        p=product();p.ReviewPending=false;p.ReviewDisplayCallbacks=90;p.ReviewScores=[8,8,8,8]
        p.HallOfFame=true;p.HallCelebration={"Frame":1,"Ticks":4}
        sales.State.ReleasedGames=[p]
        await load_fixture(sales)
        await window("HallCelebrationWindow",285,230)
        await press_node("AcknowledgeHallCelebration")
        await window("CompactProductWindow",250,355)
        check(main.find_child("CompactWeeklySalesChart",true,false)!=null and main.find_child("SalesProduct0",true,false)==null,"Sales defaults to the individual graph")
        await dispose()
        var exhibition=value()
        p.ReviewScores=[4,4,4,4];p.HallOfFame=false;p.HallCelebration=null
        exhibition.State.ReleasedGames=[p]
        exhibition.State.Month=10;exhibition.State.Week=1
        exhibition.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":9,"Week":4},"Phase":7,"Index":0,"PendingEvent":7,"Exhibition":{"EligibleProducts":[0],"SelectedProducts":[]}}
        await load_fixture(exhibition)
        await window("ExhibitionSelectionWindow",370,355)
        await dispose()
        exhibition.State.Calendar.Pending.Exhibition={"EligibleProducts":[0],"SelectedProducts":[0],"Visitors":14000,"ReportedAwarenessGain":28,"DisplayCallbacks":90,"PreviousVisitorRecord":14396}
        await load_fixture(exhibition)
        await window("ExhibitionAttendanceWindow",320,280)
        await dispose()
        var awards=value()
        p=product();p.ReviewPending=false;p.HallOfFame=false;p.HallCelebration=null
        p.OriginalAwardCode=3
        awards.State.ReleasedGames=[p]
        awards.State.Month=12;awards.State.Week=4
        awards.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,"Awards":{"Year":3,"Entries":[0],"CategoryIndex":0,"ResultReady":true,"WinningProduct":0}}
        await load_fixture(awards)
        await window("AwardCeremonyWindow",550,335)
        await dispose()
    print("Native layout smoke failures: ",failures)
    quit(0 if failures==0 else 1)
