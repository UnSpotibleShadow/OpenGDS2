extends "res://tests/project_panels_smoke.gd"

func normalize(values: Array) -> Array:
    return values.map(func(value): return int(value))

func artwork_fixture() -> void:
    for i in range(4):
        blank_bmp("user://original/Graphic/Embedded/Reviewer%d.bmp" % i,64,64,false)
    for i in range(10):
        blank_bmp("user://original/Graphic/Embedded/ReviewReaction%d.bmp" % i,18,64,false)
    for i in range(5):
        blank_bmp("user://original/Graphic/Embedded/Trophy%d.bmp" % i,40,40,false)
    for item in [["AwardsStage",256,160],["ExhibitionBooth",256,160],["EmptyBadge",40,40]]:
        var image := Image.create(item[1],item[2],false,Image.FORMAT_RGBA8)
        image.fill(Color.WHITE)
        check(image.save_png("user://original/Graphic/Embedded/%s.png" % item[0])==OK,"Authored event image")
    for i in range(1,11):
        var image := Image.create(64,64,false,Image.FORMAT_RGBA8)
        image.fill(Color.LIGHT_BLUE)
        check(image.save_png("user://original/Graphic/Embedded/ReviewScore%d.png" % i)==OK,"Authored score image")

func product() -> Dictionary:
    return {"ProductIndex":0,"Title":"Synthetic reviewed game","HardwareSourceIndex":0,"AudienceSourceIndex":0,
        "GenreSourceIndex":0,"GameTypeSourceIndex":0,"Stock":1000,"Price":5800,"OriginalAwardCode":0,"ReleasedOn":{"Year":3,"Month":1,"Week":1},
        "Quality":{"Fun":30,"Novelty":80,"Music":140,"Graphics":80},"WeeklySalesHistory":[0,0,0,0,0,0,0,0,0,0,0,0,0],
        "ReviewScores":[4,2,9,10],"ReviewPending":true,"ReviewCommentVariants":[1,2,3,0],"ReviewDisplayCallbacks":19}

func run() -> void:
    if OS.get_environment("XDG_DATA_HOME").is_empty():
        push_error("review_event_smoke requires disposable data")
        quit(1)
        return
    root.size=Vector2i(1280,720)
    blank_bmp("user://original/Graphic/Embedded/OfficeBase.bmp",466,290,true)
    panel_catalog()
    artwork_fixture()
    for locale in ["en","ja"]:
        var language := ConfigFile.new()
        language.set_value("language","locale",locale)
        check(language.save("user://interface.cfg")==OK,"Review/event locale " + locale)
        var value := stress_fixture()
        value.State.DevelopmentProjects=[]
        value.State.ReleasedGames=[product()]
        Engine.time_scale=0
        await load_fixture(value)
        check(main.find_child("ReviewerRow0",true,false).position.y < main.find_child("ReviewerRow3",true,false).position.y,"Reviewers stack vertically")
        check(main.find_child("ReviewerPortrait0",true,false).texture != null,"Reviewer portrait loads locally")
        check(main.find_child("AcknowledgeReviews",true,false).disabled and main.find_child("ReviewScore0",true,false).text=="?","Partial review waits for the native final-score callback")
        await press_node("ToolbarSAVE_GAME")
        var partial := saved()
        check(partial.ReleasedGames[0].ReviewDisplayCallbacks==19 and normalize(partial.ReleasedGames[0].ReviewCommentVariants)==[1,2,3,0],"Saving retains partial reveal and comment choices")
        await press_node("ToolbarLOAD_GAME")
        check(main.find_child("AcknowledgeReviews",true,false).disabled,"Reload resumes the partial review")
        Engine.time_scale=100
        for frame in range(200):
            if not main.find_child("AcknowledgeReviews",true,false).disabled:
                break
            await process_frame
        Engine.time_scale=1
        check(main.find_child("ReviewScoreImage3",true,false).visible,"Final review uses its local score artwork")
        if locale=="en":
            check(main.find_child("ReviewReaction2",true,false).text=="…","Score nine retains the initial reaction")
            check(not main.find_child("ReviewComment0",true,false).text.is_empty() and not main.find_child("ReviewComment0",true,false).text.contains("REVIEW_COMMENT_"),"Individual English comments are shown")
        else:
            check(main.find_child("ReviewReactionImage2",true,false).texture != null,"Japanese reaction remains locally imported")
        await press_node("ToolbarSAVE_GAME")
        check(normalize(saved().ReleasedGames[0].ReviewScores)==[4,2,9,10] and saved().ReleasedGames[0].ReviewDisplayCallbacks==90,"Presentation never changes the saved scores")
        await dispose()
        var exhibition := stress_fixture()
        exhibition.State.DevelopmentProjects=[]
        var shown := product()
        shown.ReviewPending=false
        shown.ReviewDisplayCallbacks=90
        exhibition.State.ReleasedGames=[shown]
        exhibition.State.Month=10
        exhibition.State.Week=1
        exhibition.State.Calendar.Pending={"ClosingDate":{"Year":3,"Month":9,"Week":4},"Phase":7,"Index":0,"PendingEvent":7,
            "Exhibition":{"EligibleProducts":[0],"SelectedProducts":[0],"Visitors":9900,"ReportedAwarenessGain":19,"VisitorSteps":[450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450,450],"DisplayCallbacks":4,"PreviousVisitorRecord":123}}
        Engine.time_scale=0
        await load_fixture(exhibition)
        check(main.find_child("ExhibitionBoothPicture",true,false).texture != null,"Exhibition loads the booth scene")
        check(main.find_child("ExhibitionVisitors",true,false).text.ends_with("450") and main.find_child("ExhibitionPreviousRecord",true,false).text.ends_with("123"),"Attendance and previous record are distinct")
        await press_node("ToolbarSAVE_GAME")
        check(saved().Calendar.Pending.Exhibition.DisplayCallbacks==4,"Attendance phase saves without repeating fees or draws")
        await dispose()
        Engine.time_scale=1
    print("Review/event smoke failures: ",failures)
    quit(0 if failures==0 else 1)
