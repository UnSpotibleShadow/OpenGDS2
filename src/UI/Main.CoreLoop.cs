using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowDismissEmployee(GameState state,EmployeeState employee)
    {
        ClearContent();AddHeading(Tr("DISMISS_EMPLOYEE"),28);AddHeading(EmployeeName(employee),22);
        AddHeading(Tr("DISMISS_EMPLOYEE_CONFIRM"),18);
        var confirm=new Button {Name="ConfirmDismissal",Text=Tr("CONFIRM")};_content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            var result=EmployeeDismissalService.TryDismiss(state,employee);
            ShowStaff(state);
            if (result!=DismissalResult.Success) AddHeading(Tr($"DISMISS_RESULT_{result}"),16);
        };
        AddAction("CANCEL",()=>ShowStaff(state));
    }
    private string ProjectSummary(DevelopmentProjectState project)
        =>$"{Tr("DEVELOPMENT_ROOM")} {project.RoomIndex+1}: {project.Title}\n{project.Progress/project.RequiredPoints:P0} · {Tr("BUGS")}: {project.Bugs}";
    private void AddProjectMetrics(DevelopmentProjectState project)
    {
        var metrics=new GridContainer {Columns=2};_content.AddChild(metrics);
        AddMetric(metrics,Tr("STAT_FUN"),$"{project.Quality.Fun:N0}");
        AddMetric(metrics,Tr("STAT_NOVELTY"),$"{project.Quality.Novelty:N0}");
        AddMetric(metrics,Tr("STAT_GRAPHICS"),$"{project.Quality.Graphics:N0}");
        AddMetric(metrics,Tr("STAT_MUSIC"),$"{project.Quality.Music:N0}");
        AddMetric(metrics,Tr("BUGS"),project.Bugs.ToString());
        AddMetric(metrics,Tr("GAME_RECOGNITION"),$"{project.Recognition:N0}%");
    }
    private void ShowProject(GameState state,DevelopmentProjectState project)
    {
        if (ShowPendingPromotion(state)) return;
        if (project.OriginalRoomState==2) {ShowHardwareProject(state,project);return;}
        if (project.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(project)) {ShowProjectDecision(state,project);return;}
        ClearContent();AddHeading(project.Title,28);
        CompactManagementWindow("DevelopmentProjectWindow",new Vector2(430,470),410);
        var refreshPanel=ProjectPanel(_content,state,project,OwnHardwareService.Catalog(state,_importer.LoadCatalog()!));
        if (project.OriginalMode==1)
        {
            AddHeading(Tr("SEQUEL_DEVELOPMENT"),16);
            AddHeading(Tr("SEQUEL_INHERITANCE_NOTE"),14);
        }
        var progress=new ProgressBar {Name="DevelopmentProgress",MinValue=0,MaxValue=100};_content.AddChild(progress);
        var report=new Label {Name="DevelopmentReport",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(report);
        AddHeading(Tr("DEVELOPMENT_STAFF_NOTE"),16);
        AddAction("STAFF",()=>ShowStaff(state));
        if (project.OriginalStage==3) AddAction("RELEASE_GAME",()=>ShowRelease(state,project));
        AddAction("MEDIA_INFORMATION",()=>ShowMediaInformation(state,project.HardwareSourceIndex,project.DataSize,()=>ShowProject(state,project)));
        AddAction("PAID_PROMOTION",()=>ShowPromotionSelector(state,project));
        AddAction("CANCEL_PROJECT",()=>ShowProjectCancellation(state,project));
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            refreshPanel();
            progress.Value=project.Progress/project.RequiredPoints*100;
            report.Text=$"{Tr("DEVELOPMENT_POINTS")}: {project.Progress:N1} / {project.RequiredPoints:N0}\n"+
                string.Join("\n",state.Employees.Where(x=>x.Office?.Action is >=6 and <=17 && (x.Office.Action-6)/6==project.RoomIndex)
                    .Select(x=>$"{EmployeeName(x)} · {Tr("STRESS")}: {x.Stress:N0}%"));
        };
        _refreshCoreStatus();
    }
    private void ShowProjectCancellation(GameState state,DevelopmentProjectState project)
    {
        ClearContent();CompactManagementWindow("ProjectCancellationWindow",new Vector2(390,230),370);
        _dialog?.SetTitle(Tr("CANCEL_PROJECT"));
        AddHeading(project.Title,18);AddHeading(Tr("CANCEL_PROJECT_CONFIRM"),16);
        _status=new Label {Name="CancellationStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        var availability=ProjectCancellationService.Check(state,project);
        var confirm=new Button {Name="ConfirmProjectCancellation",Text=Tr("CONFIRM"),Disabled=availability!=ProjectCancellationResult.Success};
        _content.AddChild(confirm);
        if (availability!=ProjectCancellationResult.Success) _status.Text=Tr($"CANCELLATION_RESULT_{availability}");
        confirm.Pressed+=()=>
        {
            var result=_session.CancelProject(project);
            if (result!=ProjectCancellationResult.Success) {_status.Text=Tr($"CANCELLATION_RESULT_{result}");return;}
            ShowOfficeReconstruction(state);
        };
        AddAction("KEEP_PROJECT",()=>ShowProject(state,project));
    }
    private bool ShowPendingProjectCancellation(GameState state)
    {
        if (state.PendingProjectCancellation is not { } pending) return false;
        int cursor=pending.MessageIndex;
        bool english=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        var imported=english ? null : _importer.LoadDialogue();
        string Text(string key)=>english ? Tr(key) : imported?.Entries.GetValueOrDefault(key) ?? Tr("CANCELLATION_REIMPORT_REQUIRED");
        string heading,speaker,body;int? portrait=null;
        if (cursor<pending.Staff.Count)
        {
            var reaction=pending.Staff[cursor];
            var employee=new EmployeeState {Name=reaction.Name,SourceIndex=reaction.SourceIndex};
            heading=english || imported?.Entries.ContainsKey("CANCELLATION_EMPLOYEE_NAMED")==true
                ? Text("CANCELLATION_EMPLOYEE_NAMED").Replace("{name}",EmployeeName(employee)) : Tr("CANCEL_PROJECT");
            speaker=DialogueText("CORE_EMPLOYEE");body=Text(ProjectCancellationDialogueRules.ReactionKey(reaction.Variant));
            portrait=reaction.PortraitId;
        }
        else
        {
            heading=speaker=DialogueText("CORE_SECRETARY");
            body=Text(cursor==pending.Staff.Count ? "CANCELLATION_STRESS_SUMMARY" : "CANCELLATION_DISCARD_SUMMARY")
                .Replace("{room}",(pending.RoomIndex+1).ToString());
        }
        ShowConversation(heading,[new(speaker,body,portrait)],()=>
        {
            if (!ReferenceEquals(_session.Current,state) || !ReferenceEquals(state.PendingProjectCancellation,pending) || pending.MessageIndex!=cursor) return;
            ProjectCancellationDialogueRules.Acknowledge(state);ShowOfficeReconstruction(state);
        });
        return true;
    }
    private void ShowProjectDecision(GameState state,DevelopmentProjectState project)
    {
        if (CheckpointDialogueRules.ShouldIntroduce(state,project))
        {
            var milestone=project.PendingDecision!.Value;
            ShowConversation(Tr("DEVELOPMENT_ROOM")+" "+(project.RoomIndex+1),
                [new(DialogueText("CORE_SECRETARY"),DialogueText(CheckpointDialogueRules.MessageKey(milestone)).Replace("{room}",(project.RoomIndex+1).ToString()))],
                ()=>{CheckpointDialogueRules.Acknowledge(state,project,milestone);ShowProjectDecision(state,project);},musicPhase:MeetingMusicRules.ForMilestone(milestone));
            return;
        }
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog() ?? throw new InvalidOperationException("No imported catalog."));
        if (project.PendingDecision is DevelopmentMilestone.SelectType or DevelopmentMilestone.SelectGenre)
        {
            try {CheckpointMeetingRules.Prepare(state,project,catalog,_session.Random);}
            catch (InvalidOperationException) {ClearContent();AddHeading(Tr("LEGACY_ROSTER_UNKNOWN"),16);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));return;}
        }
        if (CheckpointMeetingRules.HasPending(project)) {ShowCheckpointMeeting(state,project,catalog);return;}
        ClearContent(keepMeetingMusic:true);
        if (project.PendingDecision is DevelopmentMilestone phase)
            _music.SetMeetingPhase((int)MeetingMusicRules.ForMilestone(phase));
        AddHeading(project.Title,18);
        _status=new Label {AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        if (project.PendingDecision==DevelopmentMilestone.SelectType)
        {
            CheckpointPanel(state,project,catalog,"TypeMeetingWindow",DialogueText("CHECKPOINT_ALPHA_MEETING"));
            _dialog?.SetTitle(DialogueText("CHECKPOINT_ALPHA_MEETING"));
            AddHeading(Tr("SELECT_GAME_TYPE"),16);
            try
            {
                var candidates=DevelopmentSimulation.PrepareTypeCandidates(state,project,catalog,_session.Random);
                int? selected=null;
                var chosen=new Label {Name="CheckpointChoice",Text=Tr("CHECKPOINT_NO_CHOICE"),AutowrapMode=TextServer.AutowrapMode.WordSmart};
                var confirm=new Button {Name="ConfirmTypeChoice",Text=Tr("CONFIRM"),Disabled=true};
                var group=new ButtonGroup();
                var options=CheckpointOptions(includeExamples:true);
                int listIndex=0;
                foreach (int source in candidates)
                {
                    var type=catalog.GameTypes.Single(x=>x.SourceIndex==source);
                    int rowIndex=listIndex++;
                    var button=AddCheckpointOption(options,$"SelectType{rowIndex}",CatalogName(catalog,"TYPE",type.SourceIndex,type.Name),
                        type.SelectionFee,type.SelectionPoints,group,type.ExampleTitles,$"TypeExamples{rowIndex}");
                    button.Pressed+=()=>
                    {
                        selected=source;chosen.Text=CatalogName(catalog,"TYPE",type.SourceIndex,type.Name);
                        confirm.Disabled=state.Money is null || state.Money<type.SelectionFee;
                        _status.Text=confirm.Disabled ? Tr(state.Money is null ? "SELECTION_RESULT_MoneyUnknown" : "SELECTION_RESULT_InsufficientFunds") : string.Empty;
                    };
                }
                _content.AddChild(chosen);_content.AddChild(confirm);
                confirm.Pressed+=()=>
                {
                    if (!selected.HasValue) return;
                    var result=CheckpointMeetingRules.TrySelectType(state,project,catalog,selected.Value);
                    if (result==DevelopmentSelectionResult.Success) ShowProjectDecision(state,project);
                    else _status.Text=Tr($"SELECTION_RESULT_{result}");
                };
            }
            catch (InvalidOperationException) {_status.Text=Tr("LEGACY_ROSTER_UNKNOWN");}
        }
        else if (project.PendingDecision==DevelopmentMilestone.SelectGenre)
        {
            CheckpointPanel(state,project,catalog,"GenreMeetingWindow",DialogueText("CHECKPOINT_BETA_MEETING"));
            _dialog?.SetTitle(DialogueText("CHECKPOINT_BETA_MEETING"));
            AddHeading(Tr("SELECT_GENRE"),16);
            if (state.Company.OriginalMember90 is not int companyValue) {_status.Text=Tr("LEGACY_ROSTER_UNKNOWN");return;}
            int? selected=null;
            var chosen=new Label {Name="CheckpointChoice",Text=Tr("CHECKPOINT_NO_CHOICE"),AutowrapMode=TextServer.AutowrapMode.WordSmart};
            var confirm=new Button {Name="ConfirmGenreChoice",Text=Tr("CONFIRM"),Disabled=true};
            var group=new ButtonGroup();
            var options=CheckpointOptions();
            foreach (var genre in catalog.Genres.Where(x=>DevelopmentSelectionRules.IsGenreEligible(x,companyValue)))
            {
                var button=AddCheckpointOption(options,$"SelectGenre{genre.SourceIndex}",CatalogName(catalog,"GENRE",genre.SourceIndex,genre.Name),
                    genre.SelectionFee,genre.SelectionPoints,group);
                button.Pressed+=()=>
                {
                    selected=genre.SourceIndex;chosen.Text=CatalogName(catalog,"GENRE",genre.SourceIndex,genre.Name);
                    confirm.Disabled=state.Money is null || state.Money<genre.SelectionFee;
                    _status.Text=confirm.Disabled ? Tr(state.Money is null ? "SELECTION_RESULT_MoneyUnknown" : "SELECTION_RESULT_InsufficientFunds") : string.Empty;
                };
            }
            _content.AddChild(chosen);_content.AddChild(confirm);
            confirm.Pressed+=()=>
            {
                if (!selected.HasValue) return;
                var result=CheckpointMeetingRules.TrySelectGenre(state,project,catalog,selected.Value);
                if (result==DevelopmentSelectionResult.Success) ShowProjectDecision(state,project);
                else _status.Text=Tr($"SELECTION_RESULT_{result}");
            };
        }
        else
        {
            CompactManagementWindow("CompletionWindow",new Vector2(450,510),430);
            AddHeading(Tr("DEVELOPMENT_COMPLETE"),22);
            _content.AddChild(Picture("CompletionIllustration",OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),new(256,160)));
            ProjectPanel(_content,state,project,catalog);
            if (project.Bugs>=60) AddHeading(Tr("RELEASE_BUG_WARNING"),16);
            AddAction("RELEASE_GAME",()=>ShowRelease(state,project));
            AddAction("CONTINUE_DEBUGGING",()=>{project.PendingDecision=null;ShowProject(state,project);});
        }
        if (project.PendingDecision==DevelopmentMilestone.StartDebugging) AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        if (project.PendingDecision==DevelopmentMilestone.StartDebugging)
        {
            AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
            AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
        }
        if (project.PendingDecision is DevelopmentMilestone.SelectType or DevelopmentMilestone.SelectGenre)
            ShowMeetingScene(DialogueText(project.PendingDecision==DevelopmentMilestone.SelectType ? "CHECKPOINT_ALPHA_MEETING" : "CHECKPOINT_BETA_MEETING"),OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),()=>ShowOfficeReconstruction(state));
    }
    private VBoxContainer CheckpointOptions(bool includeExamples=false)
    {
        var headings=new HBoxContainer {Name="CheckpointColumnHeadings"};
        headings.AddThemeConstantOverride("separation",4);
        headings.AddChild(new Label {Text=Tr(includeExamples ? "SELECT_GAME_TYPE" : "SELECT_GENRE"),
            ClipText=true,CustomMinimumSize=new Vector2(120,0),SizeFlagsHorizontal=SizeFlags.ExpandFill});
        headings.AddChild(new Label {Text=Tr("CHECKPOINT_POINTS"),CustomMinimumSize=new Vector2(100,0),HorizontalAlignment=HorizontalAlignment.Right});
        headings.AddChild(new Label {Name="CheckpointFeeHeading",Text=Tr("CHECKPOINT_FEE"),CustomMinimumSize=new Vector2(90,0),HorizontalAlignment=HorizontalAlignment.Right});
        if (includeExamples) headings.AddChild(new Label {Text=Tr("CHECKPOINT_EXAMPLES"),
            ClipText=true,CustomMinimumSize=new Vector2(130,0),SizeFlagsHorizontal=SizeFlags.ExpandFill});
        var scroll=new ScrollContainer {Name="CheckpointOptions",CustomMinimumSize=new Vector2(0,175),
            HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled};_content.AddChild(scroll);
        var options=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};options.AddThemeConstantOverride("separation",1);
        scroll.AddChild(options);options.AddChild(headings);return options;
    }
    private Button AddCheckpointOption(VBoxContainer options,string nodeName,string caption,long fee,int points,ButtonGroup group,
        System.Collections.Generic.IReadOnlyList<string>? examples=null,string? examplesNodeName=null)
    {
        var row=new HBoxContainer();row.AddThemeConstantOverride("separation",4);options.AddChild(row);
        string feeText=$"¥{fee:N0}";
        var button=new Button {Name=nodeName,Text=caption,ToggleMode=true,ButtonGroup=group,ClipText=true,
            CustomMinimumSize=new Vector2(120,28),SizeFlagsHorizontal=SizeFlags.ExpandFill,TooltipText=$"{caption}\n{Tr("CHECKPOINT_POINTS")}: +{points:N0}\n{Tr("CHECKPOINT_FEE")}: {feeText}"};
        row.AddChild(button);
        // Explicitly retain the preview selection even when a test/accessibility caller emits Pressed directly.
        button.Pressed+=()=>button.ButtonPressed=true;
        row.AddChild(new Label {Name=$"{nodeName}Points",Text=$"+{points:N0}",CustomMinimumSize=new Vector2(100,0),HorizontalAlignment=HorizontalAlignment.Right});
        row.AddChild(new Label {Name=$"{nodeName}Fee",Text=feeText,CustomMinimumSize=new Vector2(90,0),HorizontalAlignment=HorizontalAlignment.Right});
        if (examples is not null)
        {
            // Show all locally imported examples without drawing from the simulation's random stream.
            string text=string.Join("\n",examples.Where(x=>!string.IsNullOrWhiteSpace(x)));
            if (text.Length==0) text="—";
            row.AddChild(new Label {Name=examplesNodeName!,Text=text,TooltipText=text,CustomMinimumSize=new Vector2(130,0),
                SizeFlagsHorizontal=SizeFlags.ExpandFill,AutowrapMode=TextServer.AutowrapMode.WordSmart});
        }
        if (options.GetChildCount()==2) button.CallDeferred(Control.MethodName.GrabFocus);
        return button;
    }
    private void ShowCheckpointMeeting(GameState state,DevelopmentProjectState project,OriginalCatalog catalog)
    {
        var meeting=project.CheckpointMeeting!;var line=meeting.Lines[meeting.MessageIndex];
        string body=DialogueText(CheckpointMeetingRules.MessageKey(line.Message));
        if (line.TypeSourceIndex is int typeSource)
        {
            var type=catalog.GameTypes.Single(x=>x.SourceIndex==typeSource);
            body=body.Replace("{type}",CatalogName(catalog,"TYPE",type.SourceIndex,type.Name));
        }
        var audience=catalog.Audiences.Single(x=>x.SourceIndex==project.AudienceSourceIndex);
        body=body.Replace("{audience}",CatalogName(catalog,"AUDIENCE",audience.SourceIndex,audience.Name))
            .Replace("{recognition}",project.Recognition.ToString("0.############",System.Globalization.CultureInfo.InvariantCulture));
        if (project.GenreSourceIndex is int genreSource)
        {
            var genre=catalog.Genres.Single(x=>x.SourceIndex==genreSource);
            body=body.Replace("{genre}",CatalogName(catalog,"GENRE",genre.SourceIndex,genre.Name));
        }
        bool secretary=CheckpointMeetingRules.IsSecretary(line.Message);
        var speaker=line.EmployeeSourceIndex is int speakerSource
            ? state.Employees.FirstOrDefault(x=>x.SourceIndex==speakerSource) ?? state.EmployeeSources.FirstOrDefault(x=>x.SourceIndex==speakerSource)?.Employee : null;
        string speakerName=secretary ? DialogueText("CORE_SECRETARY") : speaker is null ? DialogueText("CORE_EMPLOYEE") :
            string.Format(Tr("CHECKPOINT_EMPLOYEE_NAMED"),EmployeeName(speaker));
        int? portrait=secretary ? SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex) : line.EmployeePortraitId;
        ShowConversation(DialogueText(meeting.Milestone==DevelopmentMilestone.SelectType ? "CHECKPOINT_ALPHA_MEETING" : "CHECKPOINT_BETA_MEETING"),[new(speakerName,body,portrait)],()=>
        {
            CheckpointMeetingRules.Acknowledge(state,project);
            if (project.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(project)) ShowProjectDecision(state,project);
            else ShowOfficeReconstruction(state);
        },scene:OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),musicPhase:MeetingMusicRules.ForMilestone(meeting.Milestone));
    }
    private void ShowRelease(GameState state,DevelopmentProjectState project,string? draftTitle=null,long? draftPrice=null,long? draftCopies=null)
    {
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog() ?? throw new InvalidOperationException("No imported catalog."));
        // Release controls can follow the final meeting. Keep its recording
        // through this form; opening release from ordinary work has no phase.
        ClearContent(keepMeetingMusic:true);CompactManagementWindow("ShipmentWindow",new Vector2(460,490),440);
        _dialog?.SetTitle(Tr("RELEASE_GAME"));
        if (state.Company.ProductionCapacity is null || state.SalesFee1 is null || state.SalesFee2 is null)
        {
            AddHeading(Tr("LEGACY_ROSTER_UNKNOWN"),16);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));return;
        }
        var defaultTitle=project.OriginalMode==1 && project.PredecessorProductIndex is int predecessor
            ? SequelDevelopmentRules.SuggestedTitle(state.ReleasedGames.Single(x=>x.ProductIndex==predecessor)) : project.Title;
        var title=new LineEdit {Name="ReleaseTitle",Text=draftTitle ?? defaultTitle,MaxLength=128};
        var price=new SpinBox {Name="ReleasePrice",MinValue=0,MaxValue=20000,AllowGreater=true,Step=10,Value=draftPrice ?? ReleaseService.DefaultPrice};
        HeldShipmentButtons(price,false);
        var copies=ProductionQuantity(state,"ReleaseCopies");if(draftCopies.HasValue)copies.Value=draftCopies.Value;_content.RemoveChild(copies);
        long unit=ReleaseService.UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex),project.DataSize);
        ShipmentBreakdown(state,unit,price,copies);
        var controls=new HBoxContainer();_content.AddChild(controls);
        controls.AddChild(Picture("ShipmentHardwarePicture",HardwarePicture(state,project.HardwareSourceIndex),new(64,64)));
        var fields=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};controls.AddChild(fields);
        fields.AddChild(new Label {Text=Tr("GAME_TITLE")});fields.AddChild(title);
        var numbers=new GridContainer {Columns=2};fields.AddChild(numbers);
        numbers.AddChild(new Label {Text=Tr("PRODUCTION_COPIES")});numbers.AddChild(copies);
        numbers.AddChild(new Label {Text=Tr("RETAIL_PRICE")});numbers.AddChild(price);
        _status=new Label();_content.AddChild(_status);
        var confirm=new Button {Name="ConfirmRelease",Text=Tr("RELEASE_GAME")};_content.AddChild(confirm);
        title.TextChanged+=value=>confirm.Disabled=string.IsNullOrWhiteSpace(value);
        confirm.Pressed+=()=>
        {
            var result=ReleaseService.TryRelease(state,project,catalog,title.Text,(long)price.Value,(long)copies.Value,_session.Random);
            if (result==ReleaseResult.Success) ShowReviews(state,state.ReleasedGames[^1]);
            else _status.Text=Tr($"RELEASE_RESULT_{result}");
        };
        AddAction("MEDIA_INFORMATION",()=>
        {
            string savedTitle=title.Text;long savedPrice=(long)price.Value,savedCopies=(long)copies.Value;
            ShowMediaInformation(state,project.HardwareSourceIndex,project.DataSize,()=>ShowRelease(state,project,savedTitle,savedPrice,savedCopies));
        });
        AddAction("CANCEL",()=>ShowProject(state,project));
    }
    private SpinBox ProductionQuantity(GameState state,string name)
    {
        // Same 100-copy floor and capacity×100 ceiling as the original spinner.
        var copies=new SpinBox {Name=name,MinValue=100,MaxValue=Math.Max(100,(state.Company.ProductionCapacity ?? throw new InvalidOperationException("Unknown production capacity."))*100L),
            Step=100,Value=ReleaseService.DefaultCopies};_content.AddChild(copies);HeldShipmentButtons(copies,true);return copies;
    }
    private void ShowSales(GameState state)
    {
        if (ShowPendingProductEvent(state)) return;
        ClearContent();AddHeading(Tr("SALES"),28);
        var funds=new Label {Name="SalesFunds"};_content.AddChild(funds);
        var budget=new Label {Name="ProductionAvailability"};_content.AddChild(budget);
        var rows=new List<(ReleasedGameState Product,Label Report,Button Production)>();
        foreach (var product in state.ReleasedGames.OrderByDescending(x=>x.ProductIndex))
        {
            AddHeading(product.Title,22);
            var individual=new Button {Name=$"OpenProduct{product.ProductIndex}",Text=Tr("PRODUCT_DETAILS")};
            individual.Pressed+=()=>ShowCompactProduct(state,product);_content.AddChild(individual);
            var report=new Label {Name=$"SalesProduct{product.ProductIndex}",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(report);
            var button=new Button {Name=$"ProduceProduct{product.ProductIndex}",Text=Tr("PRODUCE_MORE"),Disabled=state.Company.ProductionCapacity is null};
            button.Pressed+=()=>ShowProduction(state,product);_content.AddChild(button);
            rows.Add((product,report,button));
            var history=new Button {Name=$"SalesHistoryProduct{product.ProductIndex}",Text=Tr("SALES_HISTORY")};
            history.Pressed+=()=>ShowSalesHistory(state,product);_content.AddChild(history);
            if (product.ReviewScores is not null)
            {
                var reviews=new Button {Name=$"ReviewsProduct{product.ProductIndex}",Text=Tr("MAGAZINE_REVIEWS")};
                reviews.Pressed+=()=>ShowReviews(state,product);_content.AddChild(reviews);
            }
        }
        if (rows.Count==0) AddHeading(Tr("NO_RELEASED_GAMES"),18);
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            funds.Text=$"{Tr("FUNDS")}: ¥{state.Money:N0}";
            var availability=ReleaseService.CheckProduction(state);
            budget.Text=availability==ReleaseResult.Success ? Tr("PRODUCTION_READY") : Tr($"RELEASE_RESULT_{availability}");
            foreach (var (p,label,production) in rows)
            {
                production.Disabled=state.Company.ProductionCapacity is null || ReleaseService.CheckProduction(state,p)!=ReleaseResult.Success;
                label.Text=$"{Tr("RETAIL_PRICE")}: ¥{p.Price:N0} · {Tr("STOCK")}: {p.Stock:N0}\n"+
                $"{Tr("UNITS_SOLD")}: {p.UnitsSold:N0} · {Tr("LAST_SALES_BATCH")}: {p.LastBatchSold:N0}\n"+
                $"{Tr("NET_SALES_RECEIPTS")}: ¥{p.NetSalesReceipts:N0}\n{Tr("PRODUCTION_COST")}: ¥{p.ProductionSpending:N0}\n{Tr("DEMAND")}: {p.Demand}";
                var result=ReleaseService.CheckProduction(state,p);
                if (result is ReleaseResult.Recalled or ReleaseResult.StateUnknown) label.Text+="\n"+Tr($"RELEASE_RESULT_{result}");
                if (p.Recall?.OriginalState==1 && p.Recall.WeeksRemaining>0)
                    label.Text+=$"\n{Tr("RECALL_WEEKS_REMAINING")}: {p.Recall.WeeksRemaining}";
                if (p.Recall?.RefundAmount is long refund) label.Text+=$"\n{Tr("RECALL_REFUND")}: ¥{refund:N0}";
            }
        };
        _refreshCoreStatus();
    }
    private void ShowReviews(GameState state,ReleasedGameState product)
    {
        if (ShipmentDialogueRules.HasPending(product))
        {
            ShowConversation(Tr("RELEASE_GAME"),[new(DialogueText("CORE_SECRETARY"),
                DialogueText(ShipmentDialogueRules.MessageKey(product.ShipmentMessages![product.ShipmentMessageIndex])))],
                ()=>{ShipmentDialogueRules.Acknowledge(state,product);ShowReviews(state,product);});
            return;
        }
        ClearContent();AddHeading(Tr("MAGAZINE_REVIEWS"),28);AddHeading(product.Title,22);
        if(product.ReviewScores is not {Count:4})throw new InvalidOperationException("Reviews are unknown for this product.");
        ReviewRows(state,product);
    }
    private bool ShowPendingProductEvent(GameState state)
    {
        var celebration=state.ReleasedGames.FirstOrDefault(x=>x.HallCelebration is not null);
        if(celebration is not null){ShowHallCelebration(state,celebration);return true;}
        var summary=state.ReleasedGames.FirstOrDefault(p=>p.ReviewSummaryPage is not null);
        if(summary is not null){ShowReviewSummary(state,summary);return true;}
        var review=state.ReleasedGames.FirstOrDefault(x=>x.ReviewPending);
        if (review is not null) {ShowReviews(state,review);return true;}
        foreach (var product in state.ReleasedGames.OrderBy(x=>x.ProductIndex))
        {
            if (product.PendingSalesMilestones.Count>0) {ShowSalesMilestone(state,product);return true;}
            if (product.Recall?.PendingEvent is not null) {ShowRecall(state,product);return true;}
        }
        return false;
    }
    private void ShowSalesMilestone(GameState state,ReleasedGameState product)
    {
        ShowSalesAchievement(state,product);
    }
    private void ShowProduction(GameState state,ReleasedGameState product,long? draftCopies=null)
    {
        var availability=ReleaseService.CheckProduction(state,product);
        if (availability!=ReleaseResult.Success)
        {
            ClearContent();AddHeading(Tr($"RELEASE_RESULT_{availability}"),20);
            AddAction("CANCEL",()=>ShowSales(state));return;
        }
        var catalog=OwnHardwareService.Catalog(state,_importer.LoadCatalog() ?? throw new InvalidOperationException("No imported catalog."));
        ClearContent();AddHeading(product.Title,28);AddHeading(Tr("PRODUCE_MORE"),22);
        var copies=ProductionQuantity(state,"ProductionCopies");if(draftCopies.HasValue)copies.Value=draftCopies.Value;
        long unit=ReleaseService.UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==product.HardwareSourceIndex),product.DataSize);
        ReproductionBreakdown(state,product,unit,copies);
        var quote=new Label();_content.AddChild(quote);quote.Text=$"{Tr("PRODUCTION_COST")}: ¥{unit*(long)copies.Value:N0}";
        copies.ValueChanged+=_=>quote.Text=$"{Tr("PRODUCTION_COST")}: ¥{unit*(long)copies.Value:N0}";
        _status=new Label();_content.AddChild(_status);
        var confirm=new Button {Name="ConfirmProduction",Text=Tr("CONFIRM")};_content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            var result=ReleaseService.TryProduce(state,product,catalog,(long)copies.Value);
            if (result==ReleaseResult.Success) ShowSales(state);else _status.Text=Tr($"RELEASE_RESULT_{result}");
        };
        AddAction("CANCEL",()=>ShowSales(state));
    }
}
