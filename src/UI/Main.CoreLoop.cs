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
        ClearContent();var canvas=NativeWindow("DevelopmentProjectWindow",project.Title,new(290,212));
        var refreshPanel=ProjectPanel(canvas,state,project,OwnHardwareService.Catalog(state,_importer.LoadCatalog()!));
        NativeAction(canvas,"ProjectPromotion","PAID_PROMOTION",new(4,103,70,25),()=>ShowPromotionSelector(state,project));
        NativeAction(canvas,"ProjectCancel","CANCEL",new(4,132,70,25),()=>ShowProjectCancellation(state,project));
        NativeAction(canvas,"ProjectRelease","PRODUCT_SHIP",new(4,161,70,25),()=>ShowRelease(state,project),project.OriginalStage==3);
        NativeAction(canvas,"ProjectMedia","NATIVE_MEDIA",new(4,188,70,20),()=>ShowMediaInformation(state,project.HardwareSourceIndex,project.DataSize,()=>ShowProject(state,project)));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;_refreshCoreStatus=refreshPanel;
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
        _status=new Label {AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        if (project.PendingDecision==DevelopmentMilestone.SelectType)
        {
            CheckpointPanel(state,project,catalog,"TypeMeetingWindow",DialogueText("CHECKPOINT_ALPHA_MEETING"));
            _dialog?.SetTitle(DialogueText("CHECKPOINT_ALPHA_MEETING"));

            try
            {
                var candidates=DevelopmentSimulation.PrepareTypeCandidates(state,project,catalog,_session.Random);
                int? selected=null;
                var chosen=new Label {Name="CheckpointChoice",Text=Tr("CHECKPOINT_NO_CHOICE"),Visible=false};
                var confirm=new Button {Name="ConfirmTypeChoice",Text=Tr("CONFIRM"),Disabled=true,CustomMinimumSize=new(86,24),SizeFlagsHorizontal=SizeFlags.ShrinkEnd};
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

            if (state.Company.OriginalMember90 is not int companyValue) {_status.Text=Tr("LEGACY_ROSTER_UNKNOWN");return;}
            int? selected=null;
            var chosen=new Label {Name="CheckpointChoice",Text=Tr("CHECKPOINT_NO_CHOICE"),Visible=false};
            var confirm=new Button {Name="ConfirmGenreChoice",Text=Tr("CONFIRM"),Disabled=true,CustomMinimumSize=new(86,24),SizeFlagsHorizontal=SizeFlags.ShrinkEnd};
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
            var canvas=NativeWindow("CompletionWindow",Tr("DEVELOPMENT_COMPLETE"),new(368,316));
            NativeText(canvas,"CompletionCaption",Tr("DEVELOPMENT_COMPLETE"),new(4,3,360,18),12,HorizontalAlignment.Center);
            NativePlace(canvas,Picture("CompletionIllustration",OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),new(256,160)),new(56,26,256,160));
            NativeCompletionSummary(canvas,state,project,catalog);
            NativeAction(canvas,"CompletionRelease","RELEASE_GAME",new(254,245,110,27),()=>ShowRelease(state,project));
            NativeAction(canvas,"CompletionDebug","NATIVE_DEBUG",new(254,278,110,27),()=>{project.PendingDecision=null;ShowProject(state,project);});
            if(project.Bugs>=60)NativeText(canvas,"CompletionBugWarning",Tr("RELEASE_BUG_WARNING"),new(4,298,240,16),9);
        }
        if (project.PendingDecision is DevelopmentMilestone.SelectType or DevelopmentMilestone.SelectGenre)
            ShowMeetingScene(DialogueText(project.PendingDecision==DevelopmentMilestone.SelectType ? "CHECKPOINT_ALPHA_MEETING" : "CHECKPOINT_BETA_MEETING"),OriginalAssetStore.LoadTexture("Graphic/Embedded/PlanningMeeting.bmp"),()=>ShowOfficeReconstruction(state));
    }
    private VBoxContainer CheckpointOptions(bool includeExamples=false)
    {
        var scroll=new ScrollContainer {Name="CheckpointOptions",CustomMinimumSize=new(264,158),HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled};
        _content.AddChild(scroll);var frame=new PanelContainer();frame.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=Colors.White});scroll.AddChild(frame);
        var options=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};options.AddThemeConstantOverride("separation",0);frame.AddChild(options);
        var headings=new HBoxContainer {Name="CheckpointColumnHeadings"};headings.AddThemeConstantOverride("separation",4);options.AddChild(headings);
        foreach(var (key,width,name) in new[]{(includeExamples?"PROJECT_CONTENT":"PROJECT_GENRE",102,"CheckpointNameHeading"),("CHECKPOINT_POINTS",50,"CheckpointPointsHeading"),("CHECKPOINT_FEE",84,"CheckpointFeeHeading")})
        {var label=new Label {Name=name,Text=Tr(key),ClipText=true,CustomMinimumSize=new(width,18),HorizontalAlignment=key=="CHECKPOINT_FEE"?HorizontalAlignment.Right:HorizontalAlignment.Left};label.AddThemeFontSizeOverride("font_size",10);headings.AddChild(label);}
        return options;
    }
    private Button AddCheckpointOption(VBoxContainer options,string nodeName,string caption,long fee,int points,ButtonGroup group,
        System.Collections.Generic.IReadOnlyList<string>? examples=null,string? examplesNodeName=null)
    {
        var row=new HBoxContainer {Name=nodeName+"Row"};row.AddThemeConstantOverride("separation",4);options.AddChild(row);
        var button=new Button {Name=nodeName,Text=caption,ToggleMode=true,ButtonGroup=group,ClipText=true,CustomMinimumSize=new(102,19)};
        button.AddThemeFontSizeOverride("font_size",11);
        foreach(string mode in new[]{"normal","hover","pressed"})button.AddThemeStyleboxOverride(mode,new StyleBoxFlat {BgColor=mode=="pressed"?new(.7f,1,.7f):Colors.White,ContentMarginTop=0,ContentMarginBottom=0,ContentMarginLeft=2});
        string exampleText=examples is null?"":string.Join("\n",examples.Where(x=>!string.IsNullOrWhiteSpace(x)));
        button.TooltipText=caption+(exampleText.Length==0?"":"\n"+exampleText);row.AddChild(button);
        var point=new Label {Name=nodeName+"Points",Text=$"+{points:N0}",CustomMinimumSize=new(50,19),HorizontalAlignment=HorizontalAlignment.Right};point.AddThemeFontSizeOverride("font_size",11);row.AddChild(point);
        var price=new Label {Name=nodeName+"Fee",Text=$"¥{fee:N0}",CustomMinimumSize=new(84,19),HorizontalAlignment=HorizontalAlignment.Right};price.AddThemeFontSizeOverride("font_size",11);row.AddChild(price);
        row.GuiInput+=e=>{if(e is InputEventMouseButton {Pressed:true,ButtonIndex:MouseButton.Left}){button.ButtonPressed=true;button.EmitSignal(Button.SignalName.Pressed);}};
        if(options.GetChildCount()==2)button.CallDeferred(Control.MethodName.GrabFocus);return button;
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

        var copies=ProductionQuantity(state,"ReleaseCopies");if(draftCopies.HasValue)copies.Value=draftCopies.Value;_content.RemoveChild(copies);
        long unit=ReleaseService.UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex),project.DataSize);
        ShipmentBreakdown(state,unit,price,copies);
        var controls=new HBoxContainer();_content.AddChild(controls);
        controls.AddChild(Picture("ShipmentHardwarePicture",HardwarePicture(state,project.HardwareSourceIndex),new(64,64)));
        var fields=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};controls.AddChild(fields);
        fields.AddChild(new Label {Text=Tr("GAME_TITLE")});fields.AddChild(title);
        var numbers=new GridContainer {Columns=2};fields.AddChild(numbers);
        numbers.AddChild(new Label {Text=Tr("PRODUCTION_COPIES")});
        var quantityField=new HBoxContainer {Name="ReleaseQuantityField"};quantityField.AddChild(copies);quantityField.AddChild(HeldShipmentButtons(copies,true));numbers.AddChild(quantityField);
        numbers.AddChild(new Label {Text=Tr("RETAIL_PRICE")});
        var priceField=new HBoxContainer {Name="ReleasePriceField"};priceField.AddChild(price);priceField.AddChild(HeldShipmentButtons(price,false));numbers.AddChild(priceField);
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
            Step=100,Value=ReleaseService.DefaultCopies};_content.AddChild(copies);return copies;
    }
    private void ShowSales(GameState state)
    {
        if(ShowPendingProductEvent(state))return;
        var product=state.ReleasedGames.OrderByDescending(p=>p.ProductIndex).FirstOrDefault();
        if(product is not null){ShowCompactProduct(state,product);return;}
        ClearContent();var canvas=NativeWindow("NoReleasedGamesWindow",Tr("SALES"),new(260,94));
        NativeText(canvas,"NoReleasedGames",Tr("NO_RELEASED_GAMES"),new(4,8,252,24));
        NativeAction(canvas,"CloseEmptySales","BACK_TO_OFFICE",new(80,51,176,26),()=>ShowOfficeReconstruction(state));
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
        _content.RemoveChild(copies);var productionField=new HBoxContainer {Name="ProductionQuantityField"};_content.AddChild(productionField);productionField.AddChild(copies);productionField.AddChild(HeldShipmentButtons(copies,true));
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
