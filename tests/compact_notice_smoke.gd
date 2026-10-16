extends "res://tests/company_summary_smoke.gd"

func action(key: String) -> Button:
    var row=main.find_child("NoticeActions",true,false)
    for button in row.get_children():
        if button.text==tr(key):return button
    return null

func finish_recall_dialogue() -> Array[String]:
    var texts: Array[String]=[]
    for page in range(8):
        var message=main.find_child("ConversationMessage",true,false)
        if message==null:return texts
        texts.append(message.text)
        check(not message.text.contains("RECALL_DIALOGUE_"),"Recall dialogue translated")
        check(not message.text.contains("{quantity}") and not message.text.contains("{price}") and not message.text.contains("{refund}"),"Recall parameters replaced")
        var enter=InputEventKey.new();enter.keycode=KEY_ENTER;enter.pressed=true
        message.gui_input.emit(enter);await process_frame
        if is_instance_valid(message) and message.is_inside_tree() and message.text.contains("9,000"):
            var captures=OS.get_environment("OPENGDS2_NOTICE_CAPTURE")
            if not captures.is_empty():
                await RenderingServer.frame_post_draw
                var image=root.get_texture().get_image()
                image.save_png(captures+"/recall-conversation-"+TranslationServer.get_locale()+".png")
                var conversation=main.find_child("ConversationWindow",true,false)
                image.get_region(Rect2i(conversation.get_global_rect())).save_png(captures+"/recall-conversation-"+TranslationServer.get_locale()+"-dialog.png")
        if is_instance_valid(message) and message.is_inside_tree():
            await create_timer(.03).timeout
            message.gui_input.emit(enter);await process_frame
    check(false,"Recall dialogue terminates")
    return texts

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await process_frame
        for phase in [0,1,2,3,4]:
            var value=fixture(7)
            value.State.Company.HallOfFameCount=0
            value.State.SecretarySourceIndex=1
            var product=value.State.ReleasedGames[0]
            product.Title="Northern Capital Province"
            product.Demand=0
            product.Price=3000
            product.Stock=7 if phase in [0,3] else 0
            product.Recall={"OriginalState":1 if phase==0 else (3 if phase==3 else 2),
                "WeeksRemaining":5 if phase==0 else 0,"PendingEvent":0 if phase==0 else 1,
                "OutcomeRoll":null if phase==0 else (3 if phase==3 else (0 if phase==1 else (1 if phase==2 else 2))),
                "RefundAmount":null if phase==0 else (0 if phase==3 else 9000)}
            await load_summary(value)
            var texts=await finish_recall_dialogue();await settle()
            check(texts.size()==(2 if phase==0 else (1 if phase==3 else 6)),"Original explanatory stages")
            if phase not in [0,3]:
                var rate="3,000" if phase==1 else "1,000"
                var quantity="3" if phase==1 else "9"
                check(texts[3]==tr("RECALL_DIALOGUE_QUANTITY").replace("{quantity}",quantity).replace("{price}",rate),"Quantity uses saved refund before withdrawn stock")
                check(texts[4]==tr("RECALL_DIALOGUE_COST").replace("{refund}","9,000"),"Saved total explained")
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
            await finish_recall_dialogue();await settle()
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
