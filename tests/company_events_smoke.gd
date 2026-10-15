extends "res://tests/review_round_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Company event locale")
        var v=complete_value();v.State.Money=200000000
        await load_fixture(v);main.call("SetSimulationPaused",true)
        await press("BUY_BACKUP_DEVICE")
        check(main.find_child("ConversationWindow",true,false)!=null,"Backup introduction is a secretary conversation")
        await award_page()
        await window("BackupDeviceConfirmation",340,155)
        await press_node("CancelBackupDevice")
        await press_node("ToolbarSAVE_GAME")
        check(not saved().BackupDeviceOwned and saved().Money==200000000,"Cancel preserves money and ownership")
        await press("BUY_BACKUP_DEVICE");await award_page()
        await press_node("ConfirmBackupDevice")
        await press_node("ToolbarSAVE_GAME")
        check(saved().BackupDeviceOwned and saved().Money==100000000,"Confirmed device charged once")
        check(saved().PendingBackupDevice.Page==2,"Receipt remains unread in saved state")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("ConversationWindow",true,false)!=null,"Reload restores backup result conversation")
        await award_page();await press_node("ToolbarSAVE_GAME")
        check(saved().PendingBackupDevice==null and saved().Money==100000000,"Reading the saved receipt never charges again")
        await dispose()
        for protected in [false,true]:
            v=complete_value();var p=stress_fixture().State.DevelopmentProjects[0]
            p.Title="Authored project";p.PendingDecision=null;p.CheckpointMeeting=null
            p.OriginalRoomState=1;p.OriginalStage=0;p.Progress=0;v.State.DevelopmentProjects=[p]
            v.State.BackupDeviceOwned=protected
            v.State.CompanyEventContinuation={"NextVirusRoom":1}
            v.State.PendingVirusEvent={"RoomIndex":0,"Title":p.Title,"Protected":protected,"Page":0,"StaffIndex":0,"Staff":[]}
            await load_fixture(v);main.call("SetSimulationPaused",true)
            check(main.find_child("ConversationWindow",true,false)!=null,"Virus introduction appears")
            await award_page()
            if protected:
                await award_page()
            else:
                await award_page();await press_node("ToolbarSAVE_GAME")
                check(saved().DevelopmentProjects.size()==1,"Project survives until the last loss notice is acknowledged")
                await press_node("ToolbarLOAD_GAME");await award_page()
            await press_node("ToolbarSAVE_GAME")
            check(saved().PendingVirusEvent==null,"Virus conversation completes")
            check(saved().DevelopmentProjects.size()==(1 if protected else 0),"Backup ownership controls project survival")
            check(saved().CompanyEventContinuation.NextVirusRoom==1,"Saved company pass retains its next room")
            await dispose()
    print("Company event smoke failures: ",failures);quit(0 if failures==0 else 1)
