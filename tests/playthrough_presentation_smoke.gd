extends "res://tests/native_layout_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("playthrough_presentation_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Playthrough presentation locale")
        var development=value()
        var p: Dictionary=stress_fixture().State.DevelopmentProjects[0].duplicate(true)
        p.RequiredPoints=300;p.Progress=149;p.OriginalStage=0;p.GameTypeSourceIndex=null;p.GenreSourceIndex=null;p.Bugs=4
        p.DataSize=9;p.Recognition=0;p.PendingDecision=null;p.TypeCandidates=[0,1,2,3,4]
        development.State.DevelopmentProjects=[p]
        development.State.RetainedDevelopmentEligibility=0
        for employee in development.State.Employees:
            employee.DevelopmentSpeed=80;employee.Inspiration=80;employee.Fun=80;employee.Novelty=80;employee.Graphics=80;employee.Music=80;employee.Accuracy=80;employee.Stress=0
        development.State.Employees[1].Office.WorkCategory=0
        development.State.Employees[1].Office.WorkTier=1
        development.State.Employees[1].Office.WorkTicks=1
        await load_fixture(development)
        await press("DEVELOPMENT_PLANNING")
        await press_node("OpenProject0")
        var report=main.find_child("DevelopmentProjectWindow",true,false)
        report.position=Vector2(740,370)
        var moved=report.position
        await press_node("ProjectMedia")
        await window("MediaInformationWindow",310,290)
        check(is_instance_valid(report) and report.visible and report.position==moved,"Report stays behind ancillary windows at its moved position")
        var media=main.find_child("MediaCapacityTable",true,false)
        var horizontal=false
        for bar in media.find_children("*","HScrollBar",true,false):horizontal=horizontal or bar.is_visible_in_tree()
        check(not horizontal,"Media table has no horizontal scrollbar")
        await press_node("CloseMediaInformation")
        report=main.find_child("DevelopmentProjectWindow",true,false)
        check(report.position==moved,"Returning from media retains per-room report position")
        for frame in range(600):
            if main.find_child("ConversationWindow",true,false)!=null or main.find_child("TypeMeetingWindow",true,false)!=null:break
            await process_frame
        check(is_instance_valid(report) and report.visible and report.position==moved,"Development meeting retains the existing report")
        for page in range(60):
            if main.find_child("TypeMeetingWindow",true,false)!=null:break
            if main.find_child("ConversationWindow",true,false)!=null:await advance_page()
            else:await process_frame
        var type_window=main.find_child("TypeMeetingWindow",true,false)
        check(type_window!=null,"Real office work reaches alpha selection")
        if type_window!=null:
            check(main.find_child("ProjectCancel",true,false).disabled,"Underlying report actions are blocked during the decision")
            var selected=main.find_child("SelectType0",true,false)
            selected.button_pressed=true;selected.pressed.emit()
            await press_node("ConfirmTypeChoice")
            for page in range(60):
                if main.find_child("ConversationWindow",true,false)==null:break
                await advance_page()
            for frame in range(5):await process_frame
            check(is_instance_valid(report) and report.visible and report.position==moved,"Same report remains after alpha meeting")
            check(not main.find_child("ProjectCancel",true,false).disabled,"Report actions resume after the meeting")
        await dispose()
        var completed=value()
        p.OriginalStage=3;p.Progress=300;p.GameTypeSourceIndex=0;p.GenreSourceIndex=0;p.PendingDecision=2;p.DecisionIntroductionAcknowledged=2
        completed.State.DevelopmentProjects=[p]
        completed.State.Company.ProductionCapacity=200
        await load_fixture(completed)
        await press_node("CompletionRelease")
        var copies=main.find_child("ReleaseCopies",true,false)
        copies.value=10100
        await process_frame
        check(copies.size.x>=110 and copies.get_line_edit().text.replace(",","").contains("10100"),"Five-digit quantities fit their numeric field")
        var plus=main.find_child("ReleaseCopiesIncreaseHeld",true,false)
        plus.button_down.emit();plus.button_up.emit();plus.pressed.emit()
        check(copies.value==10200,"Ordinary quantity click changes exactly 100 copies")
        plus.button_down.emit()
        await create_timer(.19).timeout
        plus.button_up.emit();plus.pressed.emit()
        check(copies.value>=10300 and copies.value<=10400,"Held quantity uses the recovered 80ms cadence without release duplication")
        await dispose()
        var reviewed=value()
        var product_value=product();product_value.ReviewDisplayCallbacks=90
        reviewed.State.ReleasedGames=[product_value]
        await load_fixture(reviewed)
        await window("MagazineReviewsWindow",440,345)
        check(main.find_child("ReviewReactionFrame0",true,false).get_script()!=null,"Reviewer reactions use a speech-bubble outline")
        check(main.find_child("ReviewScoreImage0",true,false).size==Vector2(56,56),"Review digits have padding inside their box")
        if not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty():
            var raw=Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/Reviewer0.bmp"))
            var displayed=main.find_child("ReviewerPortrait0",true,false).texture.get_image()
            raw.convert(Image.FORMAT_RGB8);displayed.convert(Image.FORMAT_RGB8)
            check((raw.get_data()!=displayed.get_data())==(locale=="en"),"English portrait labels patched; Japanese preserved")
            check(raw.get_data().slice(0,48*64*3)==displayed.get_data().slice(0,48*64*3),"Reviewer face remains identical")
        await dispose()
        var sales=value()
        product_value.ReviewPending=false;product_value.ReviewDisplayCallbacks=90
        var other=product_value.duplicate(true);other.ProductIndex=1;other.Title="Second authored product"
        sales.State.ReleasedGames=[product_value,other]
        await load_fixture(sales)
        await press("SALES")
        var sales_window=main.find_child("CompactProductWindow",true,false)
        sales_window.position=Vector2(700,300);moved=sales_window.position
        main.find_child("ProductSelector",true,false).item_selected.emit(1)
        await window("CompactProductWindow",195,355)
        check(main.find_child("CompactProductWindow",true,false).position==moved,"Switching products retains report position")
        var toolbar=main.find_child("ToolbarSAVE_GAME",true,false)
        var normal=toolbar.get_theme_stylebox("normal");var hover=toolbar.get_theme_stylebox("hover")
        check(normal.get_content_margin(SIDE_LEFT)==hover.get_content_margin(SIDE_LEFT) and normal.get_content_margin(SIDE_TOP)==hover.get_content_margin(SIDE_TOP),"Toolbar hover keeps identical icon placement")
        await dispose()
        var awards=value();awards.State.ReleasedGames=[product_value]
        awards.State.Month=12;awards.State.Week=4
        awards.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,"Awards":{"Year":3,"Entries":[0],"CategoryIndex":0}}
        await load_fixture(awards)
        var award_window=main.find_child("AwardCeremonyWindow",true,false);award_window.position=Vector2(600,280);moved=award_window.position
        await press_node("RevealAward")
        await window("AwardCeremonyWindow",550,390)
        check(main.find_child("AwardCeremonyWindow",true,false).position==moved,"Announce winner retains award window position")
        check(main.find_child("AwardTrophyFrameMusic",true,false)!=null,"Trophy has its own bordered box")
        await press_node("ContinueAwards")
        check(main.find_child("AwardCeremonyWindow",true,false).position==moved,"Continue retains award window position")
        await dispose()
        var renewal=value();renewal.State.Year=3;renewal.State.Month=3;renewal.State.Week=4
        renewal.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":3,"Week":4},"Phase":1,"Index":0,"PendingEvent":0}
        renewal.State.Employees[0].ContractFee=10;renewal.State.Employees[1].ContractFee=20
        await load_fixture(renewal)
        await window("AnnualRenewalWindow",525,410)
        check(main.find_child("RenewalSelectedTotal",true,false).text.ends_with("¥30"),"Compact renewal retains selected fee sum")
        main.find_child("RenewEmployee1",true,false).button_pressed=false
        check(main.find_child("RenewalSelectedTotal",true,false).text.ends_with("¥10"),"Renewal selection updates total")
        await dispose()
    print("Playthrough presentation smoke failures: ",failures)
    quit(0 if failures==0 else 1)
