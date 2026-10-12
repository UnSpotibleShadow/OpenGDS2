extends "res://tests/stress_relief_smoke.gd"

# Author-created blank bitmap fixtures; no original artwork is required.
func blank_bmp(path: String, width: int, height: int, white: bool) -> void:
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var stride: int=(width*3+3)&~3
    var pixels := PackedByteArray()
    pixels.resize(54+stride*height)
    pixels.fill(255 if white else 0)
    for i in range(54):
        pixels[i]=0
    pixels[0]=66
    pixels[1]=77
    pixels.encode_u32(2,pixels.size())
    pixels.encode_u32(10,54)
    pixels.encode_u32(14,40)
    pixels.encode_u32(18,width)
    pixels.encode_u32(22,height)
    pixels.encode_u16(26,1)
    pixels.encode_u16(28,24)
    pixels.encode_u32(34,stride*height)
    DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(path.get_base_dir()))
    var file := FileAccess.open(path,FileAccess.WRITE)
    file.store_buffer(pixels)

func click_office(button_index: int, pixel: Vector2) -> void:
    var office=main.find_child("OriginalOfficeBackground",true,false)
    check(office!=null,"Office input surface exists")
    if office==null:
        return
    var scale: float=min(office.size.x/466.0,office.size.y/290.0)
    var offset: Vector2=(office.size-Vector2(466,290)*scale)/2.0
    var click := InputEventMouseButton.new()
    click.button_index=button_index
    click.pressed=true
    click.position=offset+pixel*scale
    office.gui_input.emit(click)
    await process_frame

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("office_presentation_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    blank_bmp("user://original/Graphic/Embedded/OfficeSprites.bmp",1600,530,true)
    blank_bmp("user://original/Graphic/Embedded/OfficeMasks.bmp",1600,530,false)
    var sprites: Array=[]
    for id in range(23,151):
        sprites.append({"Id":id,"X":-1000,"Y":-1000,"SourceX":0,"SourceY":0,"Width":1,"Height":1,"DepthOffset":0})
    write_json("user://original/office.json",{"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6","Sprites":sprites})
    write_json("user://original/catalog.json",{"SchemaVersion":1,"SourceHash":"4820bc85529709a128a4812f1d1ff06b2ea16899f08a67f04d0431c8b04fdfe6"})
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Office locale "+locale)
        var value := stress_fixture()
        value.State.Employees[0].Office={"SlotIndex":0,"X":88,"Y":50,"Action":0,"SeatedAction":0,"MovementState":0,"WorkEngaged":false,"WorkTicks":17}
        value.State.Employees[1].Office={"SlotIndex":1,"X":184,"Y":106,"Action":6,"SeatedAction":6,"MovementState":0,"WorkEngaged":true}
        value.State.EmployeeSources[0].Employee=value.State.Employees[0]
        value.State.EmployeeSources[1].Employee=value.State.Employees[1]
        await load_fixture(value)
        if main.find_child("TogglePause",true,false)==null:
            for label in main.find_children("*","Label",true,false):
                if label.text.to_lower().contains("fail") or label.text.to_lower().contains("invalid"):
                    print("Fixture diagnostic: ",label.text)
            quit(1)
            return
        await press_node("TogglePause")
        await click_office(MOUSE_BUTTON_LEFT,Vector2(97,66))
        check(main.find_child("EmployeeDetailsWindow",true,false)!=null,"Left click selects an employee")
        await click_office(MOUSE_BUTTON_RIGHT,Vector2(232,149.5))
        await press_node("ToolbarSAVE_GAME")
        var state := saved()
        check(state.Employees[0].Office.Action==7 and state.Employees[0].Office.MovementState==3,"Right click assigns the chosen native desk")
        check(state.Employees[0].Office.X==88 and state.Employees[0].Office.Y==50,"Assignment starts walking without teleporting")
        check(state.Employees[1].Office.Action==6,"Other employee remains assigned")
        check(state.has("OfficeRoomPresentation"),"Room animation phase is saved")
        await press_node("ToolbarLOAD_GAME")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Employees[0].Office.Action==7,"Desk assignment survives reload")
        await dispose()
    print("Office presentation smoke failures: ",failures)
    quit(0 if failures==0 else 1)
