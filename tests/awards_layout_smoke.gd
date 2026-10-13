extends "res://tests/native_layout_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("awards_layout_smoke requires disposable user data")
        quit(1)
        return
    root.size=Vector2i(1280,800)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    setup_catalog()
    var catalog=JSON.parse_string(FileAccess.get_file_as_string("user://original/catalog.json"))
    catalog.RivalCompanies=["Authored publisher with a deliberately long company name"]
    write_json("user://original/catalog.json",integer_tokens(catalog))
    for locale in ["en","ja"]:
        var language=ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Awards locale")
        for variant in ["short","long","rival"]:
            var fixture_value=value()
            var p=product();p.ReviewPending=false;p.ReviewDisplayCallbacks=90;p.OriginalAwardCode=3
            p.Title="Synthetic reviewed game" if variant=="short" else "A very long winning game title that previously hid the entire payment message"
            fixture_value.State.ReleasedGames=[p]
            fixture_value.State.Month=12;fixture_value.State.Week=4
            var ceremony={"Year":3,"Entries":[0],"CategoryIndex":0,"ResultReady":true,"WinningProduct":0}
            if variant=="rival":
                p.OriginalAwardCode=0
                fixture_value.State.Competition={"Products":[{"Title":p.Title,"Owner":0,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":100},{"Title":p.Title,"Owner":-1,"OwnProduct":0,"Hardware":0,"Demand":0,"SalesPower":0,"UnitsSold":0}],"PlatformAges":[0,0,0,0,0,0,0,0]}
                ceremony.WinningProduct=null;ceremony.WinningRival=0
                ceremony.RivalEntries=[];ceremony.AnimationProducts=[]
                for i in range(31):ceremony.RivalEntries.append(0)
                for i in range(13):ceremony.AnimationProducts.append(0)
            fixture_value.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":12,"Week":3},"Phase":0,"Index":0,"PendingEvent":8,"Awards":ceremony}
            await load_fixture(fixture_value)
            await window("AwardCeremonyWindow",550,410)
            var output=OS.get_environment("OPENGDS2_PRIVATE_UI_CAPTURE")
            if not output.is_empty():
                check(DirAccess.copy_absolute(output.path_join(locale+"-AwardCeremonyWindow.png"),output.path_join(locale+"-Awards-"+variant+".png"))==OK,"Private awards capture")
            var result=main.find_child("AwardResult",true,false)
            check(result!=null,"Winner message exists")
            if result==null:
                await dispose()
                continue
            check(result.autowrap_mode==TextServer.AUTOWRAP_WORD_SMART,"Winner message wraps")
            check(result.get_line_count()>1,"Full winner/payment message uses multiple lines")
            check(result.get_visible_line_count()==result.get_line_count(),"Every winner/payment line is visible")
            check(result.get_global_rect().end.y<=main.find_child("ContinueAwards",true,false).get_global_rect().position.y,"Acknowledgment stays below the full message")
            check(result.text.contains(p.Title),"Full winning title remains in the message")
            if not OS.get_environment("OPENGDS2_PRIVATE_NATIVE_ART").is_empty():
                var raw=Image.load_from_file(ProjectSettings.globalize_path("user://original/Graphic/Embedded/AwardsStage.png"))
                var displayed=main.find_child("AwardStagePicture",true,false).texture.get_image()
                raw.convert(Image.FORMAT_RGB8);displayed.convert(Image.FORMAT_RGB8)
                check((raw.get_data()!=displayed.get_data())==(locale=="en"),"English banner patched; Japanese preserved")
                var a=raw.get_data();var b=displayed.get_data();var preserved=true
                for y in range(160):
                    for x in range(256):
                        if x<29 or x>=211 or y<4 or y>=22:
                            var i=(y*256+x)*3
                            preserved=preserved and a.slice(i,i+3)==b.slice(i,i+3)
                check(preserved,"Actual runtime banner preserves stage scenery outside caption")
            await press_node("ToolbarSAVE_GAME")
            var before=saved()
            await press_node("ToolbarLOAD_GAME")
            await press_node("ToolbarSAVE_GAME")
            var after=saved()
            check(before.Money==after.Money and before.Calendar.Pending.Awards==after.Calendar.Pending.Awards,"Rendering/reload never pays or draws again")
            await dispose()
    print("Awards layout smoke failures: ",failures)
    quit(0 if failures==0 else 1)
