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
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog() ?? throw new InvalidOperationException("No local catalog."));
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
            int pictureIndex=hardwareId==11 ? OwnHardwareService.OwnPictureIndex(state.OwnHardware.LaunchedDesign!.Value,state.OwnHardware.LaunchedMedium!.Value) : hardwareId;
            picture.Texture=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Hardware{pictureIndex}.bmp");
            picture.SetMeta("hardware_picture_index",pictureIndex);
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
        var proposal=DevelopmentFocusRules.GenerateProposal(_session.Random);
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
        ClearContent(keepMeetingMusic:true);
        _music.SetMeetingPhase((int)MeetingMusicPhase.Planning);
        var canvas=NativeWindow("PlanningMeetingWindow",Tr("PLANNING_MEETING"),new(310,350));
        var defaultTitle=predecessorProductIndex is int parent ? SequelDevelopmentRules.SuggestedTitle(state.ReleasedGames.Single(x=>x.ProductIndex==parent)) : mangaSourceIndex is int manga ? MangaText(catalog,manga,"TITLE") : Tr("NEW_PROJECT_TITLE");
        var illustration=OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningProposal.bmp");
        NativePlace(canvas,Picture("ProposalIllustration",illustration,new(256,160)),new(27,4,256,160));
        NativePlace(canvas,Picture("ProposalHardwarePicture",HardwarePicture(state,hardwareId),new(64,64)),new(4,176,64,64));
        NativeText(canvas,"ProposalHardwareName",CatalogName(catalog,"HARDWARE",hardwareId,catalog.Hardware.Single(x=>x.SourceIndex==hardwareId).Name),new(2,238,72,18),10,HorizontalAlignment.Center);
        NativeFill(canvas,"ProposalTerms",new(78,174,228,74),new(.77f,1,.77f));
        NativeText(canvas,"ProposalHardware",Tr("TARGET_HARDWARE")+": "+CatalogName(catalog,"HARDWARE",hardwareId,catalog.Hardware.Single(x=>x.SourceIndex==hardwareId).Name),new(80,176,224,18));
        NativeText(canvas,"ProposalAudience",Tr("PROJECT_AUDIENCE")+": "+CatalogName(catalog,"AUDIENCE",audienceId,catalog.Audiences.Single(x=>x.SourceIndex==audienceId).Name),new(80,194,224,18));
        var quote=NativeText(canvas,"PlanningQuote","",new(80,213,224,32),10);quote.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var gate=NativeText(canvas,"PlanningGate","",new(4,329,144,18),10);
        var grid=new Control {Name="PlanningFocus"};NativePlace(canvas,grid,new(4,260,140,64));
        var start=NativeAction(canvas,"AcceptPlan","START_GAME_DEVELOPMENT",new(156,259,150,25),()=>{});
        void ShowFocus()
        {
            string[] keys=["STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"];int[] values=[focus.Fun,focus.Novelty,focus.Graphics,focus.Music];
            Color[] colors=[Colors.Blue,Colors.Yellow,new(.7f,0,.8f),Colors.Cyan];
            for(int i=0;i<4;i++)
            {
                var dot=NativeText(grid,"ProposalDot"+i,"●",new(0,i*16,12,16));dot.AddThemeColorOverride("font_color",colors[i]);
                NativeText(grid,"ProposalFocusCaption"+i,Tr(keys[i]),new(14,i*16,82,16));
                var value=NativeText(grid,"ProposalFocus"+i,values[i]+"%",new(94,i*16,42,16),11,HorizontalAlignment.Right);value.AddThemeColorOverride("font_color",Colors.Red);
            }
        }
        void RefreshQuote()
        {
            var result=CheckPlanning(state,catalog,roomIndex,hardwareId,audienceId,predecessorProductIndex,out var value,mangaSourceIndex);
            quote.Text=$"{Tr("PLANNING_COST")}: ¥{value.Cost:N0}\n{Tr("DEVELOPMENT_POINTS")}: {value.Points}";
            gate.Text=Tr($"PLANNING_RESULT_{result}");gate.Visible=result!=DevelopmentPlanningResult.Success;
            start.Disabled=result!=DevelopmentPlanningResult.Success;
        }
        RefreshQuote();ShowFocus();
        NativeAction(canvas,"AnotherProposal","PLANNING_NEW_PROPOSAL",new(156,287,150,25),()=>ShowPlanningMeeting(state,catalog,roomIndex,hardwareId,audienceId,predecessorProductIndex,mangaSourceIndex));
        start.Pressed+=()=>
        {
            var result=predecessorProductIndex is int predecessor
                ? DevelopmentProjectService.TryAcceptSequelPlan(state,catalog,roomIndex,hardwareId,audienceId,predecessor,defaultTitle,focus)
                : mangaSourceIndex is int manga ? DevelopmentProjectService.TryAcceptMangaPlan(state,catalog,roomIndex,hardwareId,audienceId,manga,defaultTitle,focus)
                : DevelopmentProjectService.TryAcceptNewPlan(state,catalog,roomIndex,hardwareId,audienceId,defaultTitle,focus);
            if (result==DevelopmentPlanningResult.Success) ShowProject(state,state.DevelopmentProjects.Single(x=>x.RoomIndex==roomIndex));
            else gate.Text=Tr($"PLANNING_RESULT_{result}");
        };
        NativeAction(canvas,"CancelProposal","CANCEL",new(156,315,150,25),()=>ShowOfficeReconstruction(state));
    }
}
