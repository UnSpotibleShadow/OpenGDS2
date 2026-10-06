using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private VBoxContainer AddEmployeeCard(EmployeeState employee)
    {
        var frame=new PanelContainer();_content.AddChild(frame);
        var card=new VBoxContainer();frame.AddChild(card);
        var row=new HBoxContainer();row.AddThemeConstantOverride("separation",12);card.AddChild(row);
        var texture=OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId);
        if (texture is not null)
            row.AddChild(new TextureRect
            {
                Name="EmployeePortrait",Texture=texture,TextureFilter=TextureFilterEnum.Nearest,
                ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize=new Vector2(64,64)
            });
        var details=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};row.AddChild(details);
        details.AddChild(new Label {Text=$"{EmployeeName(employee)}   {Tr("LEVEL")} {employee.Level}"});
        details.AddChild(new Label {Text=$"{Tr("CONTRACT_FEE")}: ¥{employee.ContractFee:N0}"});
        details.AddChild(new Label {Text=$"{Tr("STAT_FUN")}: {employee.Fun}   {Tr("STAT_NOVELTY")}: {employee.Novelty}   {Tr("STAT_ACCURACY")}: {employee.Accuracy}"});
        details.AddChild(new Label {Text=$"{Tr("STAT_GRAPHICS")}: {employee.Graphics}   {Tr("STAT_MUSIC")}: {employee.Music}   {Tr("STAT_DEVELOPMENT_SPEED")}: {employee.DevelopmentSpeed}   {Tr("STAT_INSPIRATION")}: {employee.Inspiration}"});
        return card;
    }
    private static DevelopmentPlanningResult CheckPlanning(GameState state,OriginalCatalog catalog,int room,int hardware,int audience,
        int? predecessor,out DevelopmentQuote quote,int? mangaSourceIndex=null)
        =>predecessor is int parent ? DevelopmentProjectService.CheckSequelPlan(state,catalog,room,hardware,audience,parent,out quote)
            : mangaSourceIndex is int manga ? DevelopmentProjectService.CheckMangaPlan(state,catalog,room,hardware,audience,manga,out quote)
            : DevelopmentProjectService.CheckNewPlan(state,catalog,room,hardware,audience,out quote);

    private void ShowSequelSelection(GameState state,int room)
    {
        ClearContent();CompactManagementWindow("SequelSelectionWindow",new Vector2(440,310),420);
        _dialog!.SetTitle(Tr("SEQUEL_DEVELOPMENT"));
        _content.AddChild(new Label {Text=Tr("SEQUEL_ELIGIBILITY_NOTE"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        var table=CreateTable("SequelPredecessors",["GAME_TITLE","SERIES_INSTALLMENT","REVIEW_TOTAL","UNITS_SOLD"],[180,55,60,90]);
        table.CustomMinimumSize=new Vector2(410,150);_content.AddChild(table);
        var candidates=SequelDevelopmentRules.Candidates(state).ToList();
        int? selected=null;
        foreach (var product in candidates)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,product.ProductIndex);
            string[] values=[product.Title,(product.OriginalSequelCounter+1).ToString(),product.ReviewScores?.Sum().ToString() ?? "?",product.UnitsSold.ToString("N0")];
            for (int column=0;column<values.Length;column++)
            {
                row.SetText(column,values[column]);row.SetTextAlignment(column,column==0 ? HorizontalAlignment.Left : HorizontalAlignment.Right);
            }
        }
        var status=new Label {Name="SequelSelectionStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(status);
        var confirm=new Button {Name="ConfirmSequelPredecessor",Text=Tr("PLANNING_MEETING"),Disabled=true};_content.AddChild(confirm);
        void Refresh()
        {
            var result=selected is int parent ? SequelDevelopmentRules.CheckPredecessor(state,parent) : DevelopmentPlanningResult.PredecessorUnavailable;
            confirm.Disabled=result!=DevelopmentPlanningResult.Success;
            status.Text=candidates.Count==0 ? Tr("SEQUEL_NO_CANDIDATES") : result==DevelopmentPlanningResult.Success ? Tr("SEQUEL_INHERITANCE_NOTE") : Tr($"PLANNING_RESULT_{result}");
        }
        table.ItemSelected+=()=>{selected=table.GetSelected().GetMetadata(0).AsInt32();Refresh();};
        if (candidates.Count>0) {selected=candidates[0].ProductIndex;table.GetRoot().GetFirstChild().Select(0);}
        Refresh();
        confirm.Pressed+=()=>{if (!confirm.Disabled) ShowPlanningPreview(state,room,selected);};
        AddAction("CANCEL",()=>ShowDevelopmentRoomActions(state,room));
    }

    private void ShowPlanningPreview(GameState state,int? selectedRoom=null,int? predecessorProductIndex=null,int? mangaSourceIndex=null)
    {
        var catalog=_importer.LoadCatalog() ?? throw new InvalidOperationException("No local catalog.");
        ClearContent();CompactManagementWindow("PlanningQuoteWindow",new Vector2(430,360),410);
        _dialog!.SetTitle($"{Tr("DEVELOPMENT_PLANNING")} · ¥{state.Money:N0}");
        var room=new OptionButton {Name="PlanningRoom"};
        for (int i=0;i<2;i++) room.AddItem($"{Tr("DEVELOPMENT_ROOM")} {i+1}",i);
        if (selectedRoom is int chosen) {room.Select(chosen);room.Visible=false;}
        _content.AddChild(room);
        var top=new HBoxContainer();_content.AddChild(top);
        var hardware=CreateTable("PlanningHardware",["TARGET_HARDWARE","PLANNING_POINTS_COLUMN","PLANNING_COST_COLUMN","HARDWARE_SHIPMENTS"],[104,45,75,62]);
        hardware.CustomMinimumSize=new Vector2(320,100);top.AddChild(hardware);
        var hardwareDetails=new VBoxContainer {CustomMinimumSize=new Vector2(68,0)};top.AddChild(hardwareDetails);
        var picture=new TextureRect {Name="PlanningHardwarePicture",CustomMinimumSize=new Vector2(64,64),
            ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
            TextureFilter=TextureFilterEnum.Nearest};hardwareDetails.AddChild(picture);
        var bits=new Label {Name="PlanningHardwareBits",HorizontalAlignment=HorizontalAlignment.Center};hardwareDetails.AddChild(bits);
        var lower=new HBoxContainer();_content.AddChild(lower);
        var audience=CreateTable("PlanningAudience",["TARGET_AUDIENCE","PLANNING_POINTS_COLUMN","PLANNING_COST_COLUMN"],[105,65,85]);
        audience.CustomMinimumSize=new Vector2(270,150);lower.AddChild(audience);
        var totals=new VBoxContainer {CustomMinimumSize=new Vector2(120,0)};lower.AddChild(totals);
        var quote=new Label {Name="PlanningQuote",HorizontalAlignment=HorizontalAlignment.Center,AutowrapMode=TextServer.AutowrapMode.WordSmart};
        quote.AddThemeFontSizeOverride("font_size",11);totals.AddChild(quote);
        var gate=new Label {Name="PlanningGate",AutowrapMode=TextServer.AutowrapMode.WordSmart};gate.AddThemeFontSizeOverride("font_size",11);totals.AddChild(gate);
        var meeting=new Button {Name="PlanningMeeting",Text=Tr("PLANNING_MEETING")};totals.AddChild(meeting);
        var cancel=new Button {Text=Tr("CANCEL")};totals.AddChild(cancel);cancel.Pressed+=()=>ShowOfficeReconstruction(state);
        var available=catalog.Hardware.Where(x=>state.AvailableHardware?.Contains(x.SourceIndex)==true).ToList();
        int hardwareId=available.FirstOrDefault(x=>state.HardwareLicenses?.Contains(x.SourceIndex)==true)?.SourceIndex ?? available.FirstOrDefault()?.SourceIndex ?? -1;
        int audienceId=catalog.Audiences.FirstOrDefault()?.SourceIndex ?? -1;
        foreach (var item in available)
        {
            var row=hardware.CreateItem(hardware.GetRoot());row.SetMetadata(0,item.SourceIndex);
            double? market=state.Calendar?.Hardware.SingleOrDefault(x=>x.SourceIndex==item.SourceIndex)?.Market;
            string[] values=[CatalogName(catalog,"HARDWARE",item.SourceIndex,item.Name),item.PlanningPoints.ToString("N0"),item.PlanningCost.ToString("N0"),
                market is double m ? (Math.Floor(m)*10_000).ToString("N0") : "?"];
            bool licensed=state.HardwareLicenses?.Contains(item.SourceIndex)==true;
            for (int column=0;column<values.Length;column++)
            {
                row.SetText(column,values[column]);row.SetTextAlignment(column,column==0 ? HorizontalAlignment.Left : HorizontalAlignment.Right);
                row.SetSelectable(column,licensed);
                if (!licensed) {row.SetCustomColor(column,new Color(.6f,.6f,.6f));row.SetTooltipText(column,Tr("PLANNING_RESULT_LicenseRequired"));}
            }
            if (item.SourceIndex==hardwareId && licensed) row.Select(0);
        }
        foreach (var item in catalog.Audiences)
        {
            var row=audience.CreateItem(audience.GetRoot());row.SetMetadata(0,item.SourceIndex);
            string[] values=[CatalogName(catalog,"AUDIENCE",item.SourceIndex,item.Name),item.RawMembers["1454"].ToString("N0"),item.RawMembers["148C"].ToString("N0")];
            for (int column=0;column<values.Length;column++) {row.SetText(column,values[column]);row.SetTextAlignment(column,column==0 ? HorizontalAlignment.Left : HorizontalAlignment.Right);}
            if (item.SourceIndex==audienceId) row.Select(0);
        }
        void RefreshQuote()
        {
            if (hardwareId<0 || audienceId<0) {quote.Text=Tr("PLANNING_NO_HARDWARE");meeting.Disabled=true;return;}
            var result=CheckPlanning(state,catalog,room.GetSelectedId(),hardwareId,audienceId,predecessorProductIndex,out var value,mangaSourceIndex);
            quote.Text=$"{Tr("DEVELOPMENT_POINTS")}\n{value.Points:N0}\n{Tr("PLANNING_COST")}\n¥{value.Cost:N0}";
            bits.Text=$"{catalog.Hardware.Single(x=>x.SourceIndex==hardwareId).BitWidth} {Tr("BIT_UNIT")}";
            picture.Texture=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Hardware{hardwareId}.bmp");
            gate.Text=result==DevelopmentPlanningResult.Success ? "" : Tr($"PLANNING_RESULT_{result}");meeting.Disabled=result!=DevelopmentPlanningResult.Success;
        }
        hardware.ItemSelected+=()=> {var row=hardware.GetSelected();if (row is not null && state.HardwareLicenses?.Contains(row.GetMetadata(0).AsInt32())==true) hardwareId=row.GetMetadata(0).AsInt32();RefreshQuote();};
        audience.ItemSelected+=()=> {audienceId=audience.GetSelected().GetMetadata(0).AsInt32();RefreshQuote();};
        room.ItemSelected+=_=>RefreshQuote();RefreshQuote();
        meeting.Pressed+=()=> {if (!meeting.Disabled) ShowPlanningMeeting(state,catalog,room.GetSelectedId(),hardwareId,audienceId,predecessorProductIndex,mangaSourceIndex);};
    }
    private void ShowPlanningMeeting(GameState state,OriginalCatalog catalog,int roomIndex,int hardwareId,int audienceId,
        int? predecessorProductIndex=null,int? mangaSourceIndex=null)
    {
        var proposal=DevelopmentFocusRules.GenerateProposal(new Random());
        string hardware=CatalogName(catalog,"HARDWARE",hardwareId,catalog.Hardware.Single(x=>x.SourceIndex==hardwareId).Name);
        string audience=CatalogName(catalog,"AUDIENCE",audienceId,catalog.Audiences.Single(x=>x.SourceIndex==audienceId).Name);
        var pages=DevelopmentFocusRules.DialogueKeys(proposal.Profile,state.ReleasedGames.Count).Select(key=>new ConversationPage(
            DialogueText("MEETING_SECRETARY"),DialogueText(key).Replace("{hardware}",hardware).Replace("{audience}",audience))).ToList();
        ShowConversation(Tr("PLANNING_MEETING"),pages,
            ()=>ShowPlanningProposal(state,catalog,roomIndex,hardwareId,audienceId,predecessorProductIndex,mangaSourceIndex,proposal.Focus),
            ()=>ShowOfficeReconstruction(state),OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),MeetingMusicPhase.Planning);
    }
    private void ShowPlanningProposal(GameState state,OriginalCatalog catalog,int roomIndex,int hardwareId,int audienceId,int? predecessorProductIndex,int? mangaSourceIndex,DevelopmentFocus focus)
    {
        ClearContent();CompactManagementWindow("PlanningMeetingWindow",new Vector2(430,350),410);
        _dialog!.SetTitle(Tr("PLANNING_MEETING"));
        var defaultTitle=predecessorProductIndex is int parent ? SequelDevelopmentRules.SuggestedTitle(state.ReleasedGames.Single(x=>x.ProductIndex==parent)) : mangaSourceIndex is int manga ? MangaText(catalog,manga,"TITLE") : Tr("NEW_PROJECT_TITLE");
        var title=AddField(Tr("GAME_TITLE"),defaultTitle,128);title.Name="PlanningTitle";
        AddHeading(CatalogName(catalog,"HARDWARE",hardwareId,catalog.Hardware.Single(x=>x.SourceIndex==hardwareId).Name),16);
        AddHeading(CatalogName(catalog,"AUDIENCE",audienceId,catalog.Audiences.Single(x=>x.SourceIndex==audienceId).Name),16);
        var quote=new Label {Name="PlanningQuote",HorizontalAlignment=HorizontalAlignment.Center};_content.AddChild(quote);
        var gate=new Label {Name="PlanningGate",HorizontalAlignment=HorizontalAlignment.Center};_content.AddChild(gate);
        var grid=new GridContainer {Name="PlanningFocus",Columns=2};_content.AddChild(grid);
        var start=new Button {Name="AcceptPlan",Text=Tr("START_GAME_DEVELOPMENT"),CustomMinimumSize=new Vector2(0,42)};
        void ShowFocus()
        {
            foreach (Node child in grid.GetChildren()) {grid.RemoveChild(child);child.QueueFree();}
            foreach (var item in new[] {("STAT_FUN",focus.Fun),("STAT_NOVELTY",focus.Novelty),("STAT_GRAPHICS",focus.Graphics),("STAT_MUSIC",focus.Music)})
                AddMetric(grid,Tr(item.Item1),$"{item.Item2}%");
        }
        void RefreshQuote()
        {
            var result=CheckPlanning(state,catalog,roomIndex,hardwareId,audienceId,predecessorProductIndex,out var value,mangaSourceIndex);
            quote.Text=$"{Tr("PLANNING_COST")}: ¥{value.Cost:N0}   {Tr("DEVELOPMENT_POINTS")}: {value.Points}";
            gate.Text=Tr($"PLANNING_RESULT_{result}");
            start.Disabled=result!=DevelopmentPlanningResult.Success || string.IsNullOrWhiteSpace(title.Text);
        }
        title.TextChanged+=_=>RefreshQuote();
        RefreshQuote();ShowFocus();
        AddAction("PLANNING_NEW_PROPOSAL",()=>ShowPlanningMeeting(state,catalog,roomIndex,hardwareId,audienceId,predecessorProductIndex,mangaSourceIndex));
        start.Pressed+=()=>
        {
            var result=predecessorProductIndex is int predecessor
                ? DevelopmentProjectService.TryAcceptSequelPlan(state,catalog,roomIndex,hardwareId,audienceId,predecessor,title.Text,focus)
                : mangaSourceIndex is int manga ? DevelopmentProjectService.TryAcceptMangaPlan(state,catalog,roomIndex,hardwareId,audienceId,manga,title.Text,focus)
                : DevelopmentProjectService.TryAcceptNewPlan(state,catalog,roomIndex,hardwareId,audienceId,title.Text,focus);
            if (result==DevelopmentPlanningResult.Success) ShowProject(state,state.DevelopmentProjects.Single(x=>x.RoomIndex==roomIndex));
            else gate.Text=Tr($"PLANNING_RESULT_{result}");
        };
        _content.AddChild(start);
        AddAction("CANCEL",()=>ShowOfficeReconstruction(state));
    }
}
