extends "res://tests/facility_promotion_smoke.gd"

func order_case(kind: int) -> void:
    var v=facility_value(kind);v.State.Money=100000000;v.State.Company.ProductionCapacity=50
    if kind==2:v.State.ReleasedGames=[{"Title":"Authored recycling game","Stock":7,"UnitsSold":9,"HardwareSourceIndex":0,"Price":5800}]
    for employee in v.State.Employees:employee.Stress=80
    for row in v.State.EmployeeSources:
        if row.GateValue==1:row.Employee.Stress=80
    await pause_load(v);await facility_input()
    var action="UseHotSpring0" if kind==1 else "RecycleStock0" if kind==2 else "ExpandProduction0"
    await press_node(action);await award_page();await press_node("ConfirmFacilityService")
    await press_node("ToolbarSAVE_GAME");var pending=saved()
    check(pending.PendingFacilityReceipt.EffectsPending,"Result records unread effects")
    if kind==1:
        check(pending.Money==92000000 and pending.Employees[0].Stress==80,"Bath is paid before the secretary; stress waits")
    elif kind==2:
        check(pending.Money==100000000 and pending.ReleasedGames[0].Stock==0,"Recycling removes stock before reporting but waits to credit")
    else:
        check(pending.Money==100000000 and pending.Company.ProductionCapacity==50,"Production report precedes capacity/debit")
    var body=main.find_child("ConversationMessage",true,false).text
    await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
    check(saved().Money==pending.Money and saved().PendingFacilityReceipt.Gain==pending.PendingFacilityReceipt.Gain,"Reload does not commit or reroll the unread result")
    check(main.find_child("ConversationMessage",true,false).text==body,"Reload preserves exact result text")
    await award_page();await press_node("ToolbarSAVE_GAME");var applied=saved()
    if kind==1:
        check(applied.Money==92000000 and applied.Employees[0].Stress==0 and not applied.PendingFacilityReceipt.EffectsPending,"Secretary acknowledgement clears stress before staff reactions")
        for page in range(applied.PendingFacilityReceipt.Reactions.size()):await award_page()
    elif kind==2:
        check(applied.Money==100003500 and applied.ReleasedGames[0].Stock==0 and applied.ReleasedGames[0].UnitsSold==9,"Acknowledgement credits recycling exactly once")
    else:
        check(applied.Money==90000000 and applied.Company.ProductionCapacity==50+pending.PendingFacilityReceipt.Gain,"Acknowledgement applies the saved production outcome")
        check(applied.Company.WeeklyNet==pending.Company.WeeklyNet,"Production still bypasses the profit accumulator")
    await press_node("ToolbarSAVE_GAME");var complete=saved()
    check(complete.PendingFacilityReceipt==null,"Facility sequence completes")
    await press_node("ToolbarLOAD_GAME");await press_node("ToolbarSAVE_GAME")
    check(saved().Money==complete.Money and saved().Company.ProductionCapacity==complete.Company.ProductionCapacity,"Completed reload never applies effects again")
    await dispose()

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    setup_issue118()
    for locale in ["en","ja"]:
        var settings=ConfigFile.new();settings.set_value("language","locale",locale);settings.save("user://interface.cfg")
        for kind in [1,2,7]:await order_case(kind)
    print("Facility order smoke failures: ",failures);quit(0 if failures==0 else 1)
