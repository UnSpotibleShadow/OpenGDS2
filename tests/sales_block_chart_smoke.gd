extends "res://tests/window_artwork_smoke.gd"

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():quit(1);return
    root.size=Vector2i(1280,800);blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true);setup_catalog()
    for locale in ["en","ja"]:
        var language=ConfigFile.new();language.set_value("language","locale",locale);language.save("user://interface.cfg")
        var v=complete_value();v.State.DevelopmentProjects=[];var p=product();p.ReviewPending=false;p.ReviewDisplayCallbacks=90;p.Recall={"OriginalState":1,"WeeksRemaining":0};p.WeeklySalesHistory=[0,10000,8000,4000,0,1000,0,6000,0,2000,0,5000,0];v.State.ReleasedGames=[p]
        await load_fixture(v);await press_node("TogglePause");await menu_choice("MENU_PRODUCTS",0)
        var table=main.find_child("DesktopProductTable",true,false);table.get_root().get_first_child().select(0);table.item_activated.emit();await window("CompactProductWindow",195,325)
        var chart=main.find_child("CompactWeeklySalesChart",true,false)
        await RenderingServer.frame_post_draw
        var picture=root.get_texture().get_image().get_region(Rect2i(chart.get_global_rect()));var faces=[0,0,0]
        for y in range(picture.get_height()):
            for x in range(picture.get_width()):
                var c=picture.get_pixel(x,y)
                if c.r<.01 and c.g>.99 and c.b<.01:faces[0]+=1
                elif absf(c.r-.55)<.01 and c.g>.99 and absf(c.b-.55)<.01:faces[1]+=1
                elif c.r<.01 and absf(c.g-.5)<.01 and c.b<.01:faces[2]+=1
        check(faces[0]>200 and faces[1]>15 and faces[2]>50,"Filled block fronts, top and side faces replace hairline strokes")
        await dispose()
    print("Sales block chart failures: ",failures);quit(0 if failures==0 else 1)
