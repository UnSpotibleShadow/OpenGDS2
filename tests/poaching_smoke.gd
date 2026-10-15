extends "res://tests/review_round_smoke.gd"

func outsider_fixture() -> Dictionary:
    var v=complete_value()
    v.State.Money=200000000
    var outsider=v.State.EmployeeSources[1]
    outsider.GateValue=0;outsider.Employee.SpecialFlag=1;outsider.Employee.ContractFee=10000000;outsider.Employee.Office=null
    v.State.Employees=[v.State.Employees[0]]
    return v

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);check(language.save("user://interface.cfg")==OK,"Poaching locale")
        await load_fixture(outsider_fixture());check(main.call("SetSimulationPaused",true),"Freeze poaching fixture")
        await press("POACH_EMPLOYEE");await window("PoachingSelectionWindow",510,380)
        await press_node("ConfirmPoachingTarget");await award_page()
        check(main.find_child("PoachingConfirmationWindow",true,false)!=null,"Poaching confirms after target introduction")
        await press_node("CancelPoachingConfirmation");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==200000000 and saved().PendingPoaching==null,"Declining never starts a negotiation or charges")
        await press("POACH_EMPLOYEE");await press_node("ConfirmPoachingTarget");await award_page();await press_node("StartPoachingNegotiation")
        await window("PoachingWindow",540,400)
        main.find_child("PoachingWindow",true,false).position=Vector2(500,190)
        await press_node("PoachingRaiseOffer");await window("PoachingContractWindow",190,210);await press_node("PoachingAmount0")
        check(main.find_child("PoachingWindow",true,false).position==Vector2(500,190),"Amount selector returns to moved negotiation")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Money==200000000 and saved().PendingPoaching.PlayerOffer==20000000,"Raising an offer does not charge immediately")
        await press_node("PoachingDonate");await press_node("PoachingAmount0");await press_node("ToolbarSAVE_GAME")
        check(saved().Money==200000000 and saved().PendingPoaching.PendingDonation==5000000,"Secretary acknowledgment precedes donation debit")
        await press_node("ToolbarLOAD_GAME");await award_page();await press_node("ToolbarSAVE_GAME")
        check(saved().Money==195000000 and saved().PendingPoaching.PlayerDonations==5000000,"Donation charges once after saved acknowledgment")
        main.find_child("PoachingWindow",true,false).find_child("CloseWindow",true,false).pressed.emit();await process_frame
        await press_node("ToolbarSAVE_GAME")
        check(saved().PendingPoaching==null and saved().Employees.size()==1 and saved().Money==195000000,"Closing cancels employment changes and retains spent donations")
        await dispose()
        var timed=outsider_fixture()
        timed.State.PendingPoaching={"SourceIndex":1,"Defensive":false,"Phase":4,"TimerCallbacks":39,"OriginalFee":10000000,"PlayerOffer":10000000,"RivalOffer":10000000}
        await load_fixture(timed);await create_timer(.3).timeout;await press_node("ToolbarSAVE_GAME")
        var snapshot=saved().PendingPoaching
        check(snapshot.TimerCallbacks>40 and snapshot.RivalDonations>=1000000,"Native 100ms timer reaches rival's first bid")
        await press_node("PoachingDonate");await press_node("ToolbarSAVE_GAME");var frozen=saved().PendingPoaching.TimerCallbacks
        await create_timer(.3).timeout;await press_node("ToolbarSAVE_GAME")
        check(saved().PendingPoaching.TimerCallbacks==frozen,"Amount selector pauses negotiation timer")
        await press_node("CancelPoachingAmount");await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
        check(saved().PendingPoaching.RivalDonations==snapshot.RivalDonations,"Reload retains rival bid without replay")
        await dispose()
    print("Poaching smoke failures: ",failures);quit(0 if failures==0 else 1)
