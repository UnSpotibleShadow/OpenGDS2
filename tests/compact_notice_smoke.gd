extends "res://tests/company_summary_smoke.gd"

func action(key: String) -> Button:
    var row=main.find_child("NoticeActions",true,false)
    for button in row.get_children():
        if button.text==tr(key):return button
    return null

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await process_frame
        for phase in [0,1,3]:
            var value=fixture(7)
            value.State.Company.HallOfFameCount=0
            var product=value.State.ReleasedGames[0]
            product.Title="Northern Capital Province"
            product.Demand=0
            product.Stock=0 if phase==1 else 7
            product.Recall={"OriginalState":1 if phase==0 else (2 if phase==1 else 3),
                "WeeksRemaining":5 if phase==0 else 0,"PendingEvent":0 if phase==0 else 1,
                "OutcomeRoll":null if phase==0 else (0 if phase==1 else 3),
                "RefundAmount":null if phase==0 else (9000 if phase==1 else 0)}
            await load_summary(value)
            var window=main.find_child("RecallNoticeWindow",true,false)
            check(window != null,"Recall notice opens " + locale)
            check(window.size.x<=440 and window.size.y<300,"Notice fits its contents " + locale)
            var row=main.find_child("NoticeActions",true,false)
            check(row.get_child_count()==3,"Save, load and continue share one row")
            for button in row.get_children():
                check(button.size.y<=26 and button.size.x<160,"Compact notice button " + button.text)
                check(window.get_global_rect().encloses(button.get_global_rect()),"Notice action remains inside frame")
            action("SAVE_GAME").pressed.emit();await settle()
            check(saved().Money==value.State.Money,"Save does not repeat a refund")
            action("LOAD_GAME").pressed.emit();await settle()
            window=main.find_child("RecallNoticeWindow",true,false)
            check(window.size.y<300,"Reload restores compact notice")
            action("SAVE_GAME").pressed.emit();await settle()
            var status: Label=null
            for label in window.find_children("*","Label",true,false):
                if label.text==tr("SAVE_OK"):status=label
            check(status != null,"Notice save feedback preserved")
            if phase==1:
                var original_size=window.size
                status.text=(tr("SAVE_FAILED")+" ").repeat(45)
                await settle()
                check(window.size.y>original_size.y,"Long status grows the notice")
                check(window.size.y<=660,"Long notice stays bounded by workspace")
                status.text=tr("SAVE_OK");await settle()
                check(window.size.y<300,"Short feedback restores compact height")
                var captures=OS.get_environment("OPENGDS2_NOTICE_CAPTURE")
                if not captures.is_empty():
                    window.move_to_front();await RenderingServer.frame_post_draw
                    var image=root.get_texture().get_image()
                    image.save_png(captures+"/recall-"+locale+".png")
                    image.get_region(Rect2i(window.get_global_rect())).save_png(captures+"/recall-"+locale+"-dialog.png")
            action("CONTINUE").pressed.emit();await settle()
            check(main.find_child("RecallNoticeWindow",true,false)==null,"Continue closes the acknowledged notice")
            main.call("SaveDesktop");await process_frame
            check(saved().ReleasedGames[0].Recall.PendingEvent==null,"Acknowledgement persists exactly once")
            check(saved().Money==value.State.Money,"Acknowledgement changes no funds")
        main.queue_free();await process_frame;await process_frame
    print("Compact notice checks complete; failures=",failures)
    quit(0 if failures==0 else 1)
