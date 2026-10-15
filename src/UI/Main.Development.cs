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
        details.AddChild(new Label {Text=$"{Tr("CONTRACT_FEE")}: ¥{DisplayNumbers.Format(employee.ContractFee)}"});
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
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog()!);
        ClearContent();var canvas=NativeWindow("SequelSelectionWindow",Tr("SEQUEL_DEVELOPMENT"),new(408,306));
        NativeFill(canvas,"SequelQuestionStrip",new(4,3,400,19),Colors.White,true);
        NativeText(canvas,"SequelQuestion",Tr("SEQUEL_WHICH_GAME"),new(6,3,396,19),10,HorizontalAlignment.Center);
        var table=ManagementTable(canvas,"SequelPredecessors",["GAME_TITLE","SERIES_INSTALLMENT","UNITS_SOLD"],[220,70,90],new(4,27,400,140));
        StyleSelection(table,new(1,.7f,1));
        var candidates=SequelDevelopmentRules.Candidates(state).ToList();int? selected=null;
        foreach(var product in candidates)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,product.ProductIndex);
            row.SetText(0,product.Title);row.SetText(1,(product.OriginalSequelCounter+1).ToString());row.SetText(2,product.UnitsSold.ToDisplayNumber());
            row.SetTextAlignment(1,HorizontalAlignment.Right);row.SetTextAlignment(2,HorizontalAlignment.Right);
        }
        var picture=NativePlace(canvas,Picture("SequelHardwarePicture",null,new(64,64)),new(4,177,64,64));
        var hardware=NativeText(canvas,"SequelHardwareName","",new(4,243,68,20),10,HorizontalAlignment.Center);
        string[] captions=["PROJECT_AUDIENCE","PROJECT_GENRE","PROJECT_CONTENT"];
        var detail=new Label[3];
        for(int i=0;i<3;i++)
        {
            NativeText(canvas,"SequelDetailCaption"+i,Tr(captions[i]),new(80,176+i*15,91,15),10);
            NativeFill(canvas,"SequelDetailCell"+i,new(174,176+i*15,124,15),new(1,.72f,1));
            detail[i]=NativeText(canvas,"SequelDetail"+i,"—",new(176,176+i*15,120,15),10);
        }
        string[] qualityKeys=["STAT_FUN","STAT_NOVELTY","STAT_GRAPHICS","STAT_MUSIC"];
        Color[] colors=[Colors.Blue,Colors.Yellow,new(.7f,0,.8f),Colors.Cyan];var quality=new Label[4];
        for(int i=0;i<4;i++)
        {
            int x=80+i%2*116,y=225+i/2*31;
            NativeText(canvas,"SequelDot"+i,"●",new(x,y,11,15),10).AddThemeColorOverride("font_color",colors[i]);
            NativeText(canvas,"SequelQualityCaption"+i,Tr(qualityKeys[i]),new(x+12,y,101,15),10);
            quality[i]=NativeText(canvas,"SequelQuality"+i,"0",new(x+12,y+14,101,17),13);
        }
        var status=NativeText(canvas,"SequelSelectionStatus",Tr("SEQUEL_ELIGIBILITY_NOTE"),new(4,285,297,18),9);
        var confirm=NativeAction(canvas,"ConfirmSequelPredecessor","CONFIRM",new(306,232,98,25),()=>{},false);
        NativeAction(canvas,"CancelSequelSelection","CANCEL",new(306,263,98,25),()=>ShowDevelopmentRoomActions(state,room));
        void Refresh()
        {
            var result=selected is int parent?SequelDevelopmentRules.CheckPredecessor(state,parent):DevelopmentPlanningResult.PredecessorUnavailable;
            confirm.Disabled=result!=DevelopmentPlanningResult.Success;
            if(selected is int source&&state.ReleasedGames.FirstOrDefault(p=>p.ProductIndex==source) is { } product)
            {
                picture.Texture=HardwarePicture(state,product.HardwareSourceIndex);
                hardware.Text=CatalogName(catalog,"HARDWARE",product.HardwareSourceIndex,catalog.Hardware.Single(h=>h.SourceIndex==product.HardwareSourceIndex).Name);
                detail[0].Text=CatalogName(catalog,"AUDIENCE",product.AudienceSourceIndex,catalog.Audiences.Single(x=>x.SourceIndex==product.AudienceSourceIndex).Name);
                detail[1].Text=CatalogName(catalog,"GENRE",product.GenreSourceIndex,catalog.Genres.Single(x=>x.SourceIndex==product.GenreSourceIndex).Name);
                detail[2].Text=CatalogName(catalog,"TYPE",product.GameTypeSourceIndex,catalog.GameTypes.Single(x=>x.SourceIndex==product.GameTypeSourceIndex).Name);
                double[] values=[product.Quality.Fun,product.Quality.Novelty,product.Quality.Graphics,product.Quality.Music];
                for(int i=0;i<4;i++)quality[i].Text=DisplayNumbers.Format(values[i]);
            }
            status.Text=candidates.Count==0?Tr("SEQUEL_NO_CANDIDATES"):Tr("SEQUEL_ELIGIBILITY_NOTE");
        }
        table.ItemSelected+=()=>{selected=table.GetSelected().GetMetadata(0).AsInt32();Refresh();};
        if(candidates.Count>0){selected=candidates[0].ProductIndex;table.GetRoot().GetFirstChild().Select(0);}Refresh();
        confirm.Pressed+=()=>{if(!confirm.Disabled)ShowPlanningPreview(state,room,selected);};
    }

    private void ShowPlanningPreview(GameState state,int? selectedRoom=null,int? predecessorProductIndex=null,int? mangaSourceIndex=null)
    {
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog() ?? throw new InvalidOperationException("No local catalog."));
        ClearContent();CompactManagementWindow("PlanningQuoteWindow",new Vector2(430,360),410);
        _dialog!.SetTitle($"{Tr("DEVELOPMENT_PLANNING")} · ¥{DisplayNumbers.Format(state.Money)}");
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
            string[] values=[CatalogName(catalog,"HARDWARE",item.SourceIndex,item.Name),item.PlanningPoints.ToDisplayNumber(),item.PlanningCost.ToDisplayNumber(),
                market is double m ? (Math.Floor(m)*10_000).ToDisplayNumber(): "?"];
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
            string[] values=[CatalogName(catalog,"AUDIENCE",item.SourceIndex,item.Name),item.RawMembers["1454"].ToDisplayNumber(),item.RawMembers["148C"].ToDisplayNumber()];
            for (int column=0;column<values.Length;column++) {row.SetText(column,values[column]);row.SetTextAlignment(column,column==0 ? HorizontalAlignment.Left : HorizontalAlignment.Right);}
            if (item.SourceIndex==audienceId) row.Select(0);
        }
        void RefreshQuote()
        {
            if (hardwareId<0 || audienceId<0) {quote.Text=Tr("PLANNING_NO_HARDWARE");meeting.Disabled=true;return;}
            var result=CheckPlanning(state,catalog,room.GetSelectedId(),hardwareId,audienceId,predecessorProductIndex,out var value,mangaSourceIndex);
            quote.Text=$"{Tr("DEVELOPMENT_POINTS")}\n{DisplayNumbers.Format(value.Points)}\n{Tr("PLANNING_COST")}\n¥{DisplayNumbers.Format(value.Cost)}";
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
            quote.Text=$"{Tr("PLANNING_COST")}: ¥{DisplayNumbers.Format(value.Cost)}\n{Tr("DEVELOPMENT_POINTS")}: {value.Points}";
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
