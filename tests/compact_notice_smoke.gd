extends "res://tests/company_summary_smoke.gd"

func finish_recall_dialogue() -> Array[String]:
    var texts: Array[String]=[]
    for page in range(12):
        var message=main.find_child("ConversationMessage",true,false)
        if message==null:return texts
        texts.append(message.text)
        check(not message.text.contains("RECALL_DIALOGUE_"),"Recall dialogue is translated")
        for parameter in ["{title}","{quantity}","{price}","{refund}"]:
            check(not message.text.contains(parameter),"Recall parameter replaced: "+parameter)
        var enter=InputEventKey.new();enter.keycode=KEY_ENTER;enter.pressed=true
        message.gui_input.emit(enter);await process_frame
        if is_instance_valid(message) and message.is_inside_tree():
            await create_timer(.03).timeout;message.gui_input.emit(enter);await process_frame
        check(main.find_child("RecallNoticeWindow",true,false)==null,"No extra receipt between pages")
    check(false,"Recall dialogue terminates")
    return texts

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,720)
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        main=load("res://scenes/Main.tscn").instantiate();root.add_child(main);await process_frame
        for phase in [0,1,2,3,4,5]:
            var value=fixture(7);value.State.Company.HallOfFameCount=0;value.State.SecretarySourceIndex=1
            var product=value.State.ReleasedGames[0]
            product.Title="Authored recall title";product.Demand=0;product.Price=0 if phase==5 else 3000
            product.Stock=7 if phase in [0,3] else 0
            product.Recall={"OriginalState":1 if phase==0 else 3 if phase==3 else 2,
                "WeeksRemaining":5 if phase==0 else 0,"PendingEvent":0 if phase==0 else 1,
                "OutcomeRoll":null if phase==0 else 3 if phase==3 else 0 if phase in [1,5] else 1 if phase==2 else 2,
                "RefundAmount":null if phase==0 else 0 if phase==3 else 9000}
            await load_summary(value)
            check(main.find_child("RecallNoticeWindow",true,false)==null,"Recall opens directly as conversation")
            main.call("SaveDesktop");await settle()
            check(saved().Money==value.State.Money and saved().ReleasedGames[0].Recall.PendingEvent!=null,"Saving unread dialogue neither acknowledges nor charges")
            main.call("LoadSession");await settle()
            check(main.find_child("ConversationMessage",true,false)!=null,"Load resumes unread dialogue")
            var texts=await finish_recall_dialogue();await settle()
            check(texts.size()==(3 if phase==0 else 2 if phase==3 else 8),"Recovered page count for outcome "+str(phase))
            if phase not in [0,3]:
                var rate="0" if phase==5 else "3,000" if phase==1 else "1,000"
                var quantity=tr("RECALL_QUANTITY_UNKNOWN") if phase==5 else "3" if phase==1 else "9"
                check(texts[4]==tr("RECALL_DIALOGUE_QUANTITY").replace("{quantity}",quantity).replace("{price}",rate),"Quantity uses immutable refund or explicit unknown")
                check(texts[5]==tr("RECALL_DIALOGUE_COST").replace("{refund}","9,000"),"Saved total is explained")
                check(texts[6]==tr("RECALL_DIALOGUE_GROAN"),"Original separate secretary reaction is restored")
            elif phase==0:
                check(texts[2]==tr("RECALL_DIALOGUE_WAIT"),"Investigation keeps original month warning")
            check(main.find_child("RecallNoticeWindow",true,false)==null and main.find_child("ConversationWindow",true,false)==null,"Last page completes without a second confirmation screen")
            main.call("SaveDesktop");await settle()
            check(saved().ReleasedGames[0].Recall.PendingEvent==null and saved().Money==value.State.Money,"Final page acknowledges exactly once without accounting changes")
            main.call("LoadSession");await settle()
            check(main.find_child("ConversationWindow",true,false)==null,"Completed recall stays complete after reload")
        main.queue_free();await process_frame;await process_frame
    print("Recall dialogue smoke failures: ",failures);quit(0 if failures==0 else 1)
