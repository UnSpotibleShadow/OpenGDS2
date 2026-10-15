using Godot;
using System;
using OpenGDS2.Core;
using OpenGDS2.Domain;
using OpenGDS2.Import;
using OpenGDS2.Persistence;

namespace OpenGDS2.UI;

public partial class Main : Control
{
    private readonly OriginalGameImporter _importer = new();
    private readonly GameSession _session = new();
    private VBoxContainer _content = null!;
    private VBoxContainer _screenContent = null!;
    private BackgroundMusic _music = null!;
    private OriginalSoundEffects _effects = null!;
    private Label _status = null!;
    private Button _importButton = null!;
    private FileDialog _fileDialog = null!;
    private LineEdit? _companyName;
    private LineEdit? _presidentName;
    private OriginalOfficeRenderer? _officeRenderer;
    private ImageTexture? _officeTexture;
    private double _officeElapsed;
    private OriginalCatalog? _baseCatalog;
    private OriginalCatalog? _activeCatalog
    {get=>_baseCatalog is { } c && _session.Current is { } s ? OwnHardwareService.Catalog(s,c) : _baseCatalog;set=>_baseCatalog=value;}
    private bool _clockRunning;
    private bool _paused;
    private Action? _refreshCoreStatus;

    public override void _Process(double delta)
    {
        TickConversation(delta);
        TickPokerInterface(delta);
        if (++_desktopRefreshFrame % 6 == 0) {RefreshDesktop();RefreshEmployeeDetails();}
        RefreshEmployeeSelectionArrow();
        if (GetTree().GetNodesInGroup("GameplayWindows").OfType<DesktopWindow>().Any(w=>w.IsDragging&&w.IsVisibleInTree())){_officeElapsed=0;return;}
        if (!_clockRunning || _paused || _session.Current is not GameState state) return;
        bool updated = false;
        if (OfficeTickScheduler.Advance(ref _officeElapsed,delta))
        {
            _session.TickOffice(_activeCatalog);
            _music.SetMonth(state.Month ?? 0);
            PersistPendingRankings(state);
            if(ShowPendingLevelUp(state))return;
        if(ShowPendingCompanyActivity(state))return;
            if(ShowPendingProgrammingChallenge(state))return;
            if (ShowPendingFinancialFailure(state)) return;
            if (ShowPendingPromotion(state)) return;
        if (ShowPendingMysteryLabResult(state)) return;
            if (ShowPendingOwnHardware(state)) return;
            if (ShowPendingCalendarEvent(state)) return;
            if (ShowPendingProductEvent(state)) return;
            updated = true;
            var pending=state.DevelopmentProjects.FirstOrDefault(x=>x.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(x));
            if (pending is not null) {ShowProjectDecision(state,pending);return;}
        }
        if (updated && _officeRenderer is not null && _officeTexture is not null) _officeRenderer.UpdateTexture(_officeTexture, state);
        RefreshEmployeeSelectionArrow();
        _refreshCoreStatus?.Invoke();
    }

    public override void _Ready()
    {
        LoadInterfaceLanguage();
        GetWindow().Title = $"OpenGDS2 {BuildInfo.Version}";
        BuildShell();
        _music = new BackgroundMusic { Name = "BackgroundMusic" };
        AddChild(_music);
        _effects=new OriginalSoundEffects {Name="OriginalSoundEffects"};AddChild(_effects);
        _session.SoundRequested+=_effects.Play;
        _session.SoundResetRequested+=_effects.Stop;
        ShowTitleScreen();
    }

    public override void _ExitTree()
    {
        _officeTexture?.Dispose(); _officeTexture = null; _officeRenderer = null;
    }

    private void BuildShell()
    {
        Theme = ClassicTheme.Create();
        var background = new ColorRect { Color = ClassicTheme.Face, MouseFilter = MouseFilterEnum.Ignore };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(background);

        var scroll = new ScrollContainer();
        scroll.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(scroll);
        _screen = scroll;
        var center = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        scroll.AddChild(center);

        _content = new VBoxContainer { CustomMinimumSize = new Vector2(620, 0) };
        _content.AddThemeConstantOverride("separation", 14);
        center.AddChild(_content);
        _screenContent = _content;

        _fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            UseNativeDialog = true,
            Title = Tr("IMPORT_DIALOG_TITLE")
        };
        _fileDialog.Filters = [$"*.lzh, *.LZH ; {Tr("LZH_ARCHIVES")}"];
        _fileDialog.FileSelected += ImportSelected;
        AddChild(_fileDialog);
    }

    private void ShowImporter()
    {
        DestroyDesktop();
        _music?.SetGameplayActive(false);
        ClearContent();
        AddHeading(Tr("APP_TITLE"), 42);
        AddHeading(Tr("APP_SUBTITLE"), 18);
        AddSpacer(18);
        var prompt = new Label { Text = Tr("IMPORT_PROMPT"), HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _content.AddChild(prompt);
        _importButton = new Button { Text = Tr("IMPORT_BUTTON"), CustomMinimumSize = new Vector2(0, 52) };
        _importButton.Pressed += () =>
        {
            _fileDialog.Title = Tr("IMPORT_DIALOG_TITLE");
            _fileDialog.Filters = [$"*.lzh, *.LZH ; {Tr("LZH_ARCHIVES")}"];
            _fileDialog.PopupCenteredRatio(0.75f);
        };
        _content.AddChild(_importButton);
        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _content.AddChild(_status);
    }

    private void ImportSelected(string path)
    {
        _importButton.Disabled = true;
        _status.Text = Tr("IMPORTING");
        try
        {
            var result = _importer.ImportArchive(path);
            _status.Text = $"{Tr("IMPORT_OK")} ({result.FilesImported} {Tr("FILES_IMPORTED")})";
            ShowTitleScreen();
        }
        catch (Exception ex)
        {
            GD.PushError(ex.ToString());
            _status.Text = $"{Tr("IMPORT_FAILED")}: {ex.Message}";
            _importButton.Disabled = false;
        }
    }

    private void ShowTitleScreen()
    {
        _effects?.Stop();
        if (_session.Current is { } old) {CasinoPokerService.Close(old);CasinoBlackjackService.Close(old);CasinoCardChainService.Close(old);CasinoHorseService.Close(old);}
        DestroyDesktop();
        _music?.SetTitleActive();
        ClearContent();
        var titleTexture = OriginalAssetStore.LoadTexture("Graphic/Title.bmp");
        if (titleTexture is not null)
        {
            var rect = new TextureRect
            {
                Texture = titleTexture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(558, 300)
            };
            _content.AddChild(rect);
        }
        else AddHeading(Tr("APP_TITLE"), 42);
        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };

        foreach (var (key, enabled) in new[] { ("NEW_GAME", true), ("LOAD_GAME", System.IO.File.Exists(SavePath)), ("SETTINGS", true) })
        {
            var button = new Button { Text = Tr(key), Disabled = !enabled, CustomMinimumSize = new Vector2(0, 46) };
            if (key == "NEW_GAME") button.Pressed += () =>
            {
                if (_importer.HasImportedAssets()) ShowCompanySetup();
                else ShowImporter();
            };
            if (key == "LOAD_GAME") button.Pressed += LoadSession;
            if (key == "SETTINGS") button.Pressed += ShowSettings;
            _content.AddChild(button);
        }
        AddAction("GAME_HELP",()=>ShowHelp());
        AddAction("LOAD_SAVE_SLOTS",()=>ShowSaveSlots(null,true));
        AddAction("GLOBAL_RANKINGS",()=>ShowRankings());
        var previous = new Button { Name = "LoadPreviousSave", Text = Tr("LOAD_PREVIOUS_SAVE"),
            TooltipText = Tr("LOAD_PREVIOUS_SAVE_NOTE"), Disabled = !System.IO.File.Exists(SaveStore.BackupPath(SavePath)),
            CustomMinimumSize = new Vector2(0, 46) };
        previous.Pressed += LoadPreviousSession;
        _content.AddChild(previous);
        AddAction("EARLY_ACCESS_SCOPE", ShowEarlyAccessScope);
        var import = new Button { Text = Tr("IMPORT_BUTTON"), CustomMinimumSize = new Vector2(0, 46) };
        import.Pressed += ShowImporter;
        _content.AddChild(import);
        _content.AddChild(_status);
        var quit = new Button { Text = Tr("QUIT"), CustomMinimumSize = new Vector2(0, 46) };
        quit.Pressed += () => GetTree().Quit();
        _content.AddChild(quit);
        var note = new Label { Name = "BuildVersion", Text = $"{Tr("EARLY_ACCESS_CANDIDATE")} — {BuildInfo.Version}", HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _content.AddChild(note);
        _content.AddChild(new Label { Name = "EarlyAccessBoundary", Text = Tr("EARLY_ACCESS_BOUNDARY"),
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart });
    }

    /// <summary>
    /// Mirrors the original VB6 fGMGameNew form recovered from the executable:
    /// Company Name, President Name, Confirm, Cancel.
    /// Original default personal names must come from a local import.
    /// </summary>
    private void ShowCompanySetup()
    {
        ClearContent();
        AddHeading(Tr("NEW_GAME"), 34);

        _companyName = AddField(Tr("COMPANY_NAME"), string.Empty, 32);
        _presidentName = AddField(Tr("PRESIDENT_NAME"), string.Empty, 32);

        var validation = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _content.AddChild(validation);

        var confirm = new Button { Text = Tr("CONFIRM"), CustomMinimumSize = new Vector2(0, 46) };
        confirm.Pressed += () =>
        {
            var company = _companyName.Text.Trim();
            var president = _presidentName.Text.Trim();
            if (company.Length == 0 || president.Length == 0)
            {
                validation.Text = Tr("NAME_REQUIRED");
                return;
            }
            ShowModeSelection(company, president);
        };
        _content.AddChild(confirm);

        var cancel = new Button { Text = Tr("CANCEL"), CustomMinimumSize = new Vector2(0, 46) };
        cancel.Pressed += ShowTitleScreen;
        _content.AddChild(cancel);
    }

    /// <summary>
    /// Mirrors the original fGMGameKaishi form: Beginner Mode, Normal Mode, Cancel.
    /// Mode funds are statically verified; remaining mode-specific simulation differences stay unresolved.
    /// </summary>
    private void ShowModeSelection(string companyName, string presidentName)
    {
        ClearContent();
        AddHeading(Tr("GAME_START"), 34);

        var summary = new Label
        {
            Text = $"{Tr("COMPANY_NAME")}: {companyName}\n{Tr("PRESIDENT_NAME")}: {presidentName}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _content.AddChild(summary);
        AddSpacer(8);

        _content.AddChild(new Label { Name = "EarlyAccessModeBoundary", Text = Tr("EARLY_ACCESS_BOUNDARY"),
            HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart });

        var beginner = new Button { Text = Tr("BEGINNER_MODE"), TooltipText = Tr("BEGINNER_RULES_VERIFIED"), CustomMinimumSize = new Vector2(0, 52) };
        beginner.Pressed += () => StartReconstructedGame(companyName, presidentName, GameMode.Beginner);
        _content.AddChild(beginner);

        var normal = new Button { Text = Tr("NORMAL_MODE"), TooltipText = Tr("NORMAL_RULES_VERIFIED"), CustomMinimumSize = new Vector2(0, 52) };
        normal.Pressed += () => StartReconstructedGame(companyName, presidentName, GameMode.Normal);
        _content.AddChild(normal);

        var cancel = new Button { Text = Tr("CANCEL"), CustomMinimumSize = new Vector2(0, 46) };
        cancel.Pressed += ShowCompanySetup;
        _content.AddChild(cancel);
    }

    private void StartReconstructedGame(string companyName, string presidentName, GameMode mode)
    {
        try
        {
            _paused=false;
            var state = _session.StartNewGame(companyName, presidentName, mode, _importer.LoadEmployeeDatabase(), _importer.LoadCatalog());
            ShowOfficeReconstruction(state);
        }
        catch (Exception ex)
        {
            ShowTitleScreen(); _status.Text = $"{Tr("IMPORT_FAILED")}: {ex.Message}";
        }
    }

    private int _activeSaveSlot=1;
    private string SavePath => ProjectSettings.GlobalizePath($"user://saves/slot{_activeSaveSlot}.json");

    private void ShowOfficeReconstruction(GameState state)
    {
        if (FinancialFailureService.IsComplete(state)) {ShowTitleScreen();return;}
        _music.SetMeetingPhase(-1);
        CloseConversationScene();
        _activeCatalog = _importer.LoadCatalog();
        _activeEmployeeDatabase = _importer.LoadEmployeeDatabase();
        PersistPendingRankings(state);
        EnsureDesktop(state);
        if(ShowPendingLevelUp(state))return;
        if(ShowPendingCompanyActivity(state))return;
            if(ShowPendingProgrammingChallenge(state))return;
            if (ShowPendingFinancialFailure(state)) return;
        if (state.Casino.PokerOpen) {ShowPoker(state);return;}
        if (state.Casino.BlackjackOpen) {ShowBlackjack(state);return;}
        if (state.Casino.CardChainOpen) {ShowCardChain(state);return;}
        if (state.Casino.HorseOpen) {ShowHorseRacing(state);return;}
        // M35 persisted these two stops before their form behavior was mapped.
        // Resume the existing cursor, without replaying a week boundary.
        if (state.Calendar?.Pending?.Block is CalendarBlock.Exhibition or CalendarBlock.BeginnerEnding)
        {
            _session.TickOffice(_activeCatalog);
            PersistPendingRankings(state);
        }
        if (ShowPendingPromotion(state)) return;
        if (ShowPendingMysteryLabResult(state)) return;
            if (ShowPendingOwnHardware(state)) return;
        if (ShowPendingProjectCancellation(state)) return;
        if (ShowPendingFeeNegotiation(state)) return;
        if (ShowPendingFacilityReceipt(state)) return;
        if (ShowPendingBankLoanReceipt(state)) return;
        if (ShowPendingHireGreeting(state)) return;
        if (ShowPendingSecretaryReplacement(state)) return;
        if (ShowPendingSecretaryAdvice(state)) return;
        if (ShowPendingCalendarEvent(state)) return;
        if (ShowPendingProductEvent(state)) return;
        var pendingProject=state.DevelopmentProjects.FirstOrDefault(x=>x.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(x));
        if (pendingProject is not null) {ShowProjectDecision(state,pendingProject);return;}
        CloseDesktopDialog();
        _refreshCoreStatus = null;
        _officeElapsed = 0;
        _clockRunning = true;
        RefreshDesktop();
        _status = _desktopStatus!;
        _music.SetGameplayActive(true);
    }

    private void ShowRecruitment()
    {
        var state = _session.Current ?? throw new InvalidOperationException("No session.");
        var roster = _session.OpenRecruitment();
        ShowNativeRecruitment(state,roster);
    }

    private void AddRecruitmentBar(VBoxContainer parent,string caption,int? value)
    {
        parent.AddChild(new Label {Text=$"{Tr(caption)}: {value?.ToString() ?? "?"}",AutowrapMode=TextServer.AutowrapMode.WordSmart});
        var bar=new ProgressBar {MaxValue=99,Value=value ?? 0,ShowPercentage=false,CustomMinimumSize=new Vector2(140,9)};
        bar.AddThemeStyleboxOverride("background",new StyleBoxFlat {BgColor=new Color(0,0,.7f)});
        bar.AddThemeStyleboxOverride("fill",new StyleBoxFlat {BgColor=new Color(0,1,0)});parent.AddChild(bar);
    }

    private void ShowLicenses(GameState state)
    {
        ShowLicenceSelection(state);
    }

    private void ShowComputers(GameState state)=>ShowComputerPurchase(state);

    private void ShowStaff(GameState state)
    {
        ShowStaffSelection(state);
    }

    private void ShowCalendar(GameState state)
    {
        // Separate inspector: advancing an incomplete simulation would skip payroll/events.
        ClearContent();IdentifyManagementWindow("CalendarInspectorWindow"); AddHeading(Tr("CALENDAR_INSPECTOR"), 28);
        var date = new GameDate(state.Year ?? 1, state.Month ?? 4, state.Week ?? 1);
        var label = new Label { Text = $"{date.Year} / {date.Month} / {date.Week}", HorizontalAlignment = HorizontalAlignment.Center };
        _content.AddChild(label); AddHeading(Tr("CALENDAR_INSPECTOR_NOTE"), 14);
        AddHeading(Tr(state.Calendar is null ? "CALENDAR_LEGACY_SAVE" : "CALENDAR_LIVE"),14);
        long? payrollDebit=null;
        try { payrollDebit=OriginalPayrollRules.MonthlyDebit(state); }
        catch (OverflowException) { }
        _content.AddChild(new Label {Name="OriginalPayrollPreview",
            Text=$"{Tr("ORIGINAL_PAYROLL_PREVIEW")}: {payrollDebit?.ToString("N0") ?? Tr("NOT_YET_DECODED")}"});
        AddHeading(Tr("PAYROLL_PREVIEW_NOTE"),14);
        AddAction("NEXT_WEEK", () => { date = CalendarRules.AdvanceWeek(date); label.Text = $"{date.Year} / {date.Month} / {date.Week}"; });
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private bool SaveSlotsExist()=>System.IO.Directory.Exists(ProjectSettings.GlobalizePath("user://saves")) &&
        System.IO.Directory.EnumerateFiles(ProjectSettings.GlobalizePath("user://saves"),"slot*.json").Any();
    private void LoadSession()
    {
        if(!System.IO.File.Exists(SavePath)){ShowSaveSlots(_session.Current,true);return;}
        try { var state = SaveStore.Load(SavePath); _session.Restore(state); ShowOfficeReconstruction(state); }
        catch (Exception ex) { ShowTitleScreen(); _status.Text = $"{Tr("LOAD_FAILED")}: {ex.Message}"; }
    }

    private void LoadPreviousSession()
    {
        try { var state = SaveStore.LoadBackup(SavePath); _session.Restore(state); ShowOfficeReconstruction(state); }
        catch (Exception ex) { ShowTitleScreen(); _status.Text = $"{Tr("LOAD_FAILED")}: {ex.Message}"; }
    }

    private void ShowEarlyAccessScope()
    {
        DestroyDesktop();
        ClearContent(); AddHeading(Tr("EARLY_ACCESS_SCOPE"), 30);
        foreach (var key in new[] { "EARLY_ACCESS_BOUNDARY", "EARLY_ACCESS_FEATURES", "EARLY_ACCESS_DEFERRED", "LOAD_PREVIOUS_SAVE_NOTE" })
            _content.AddChild(new Label { Text = Tr(key), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        AddAction("BACK_TO_TITLE", ShowTitleScreen);
    }

    private void ShowSettings()
    {
        ClearContent();IdentifyManagementWindow("SettingsWindow"); AddHeading(Tr("SETTINGS"), 30);
        foreach (var (locale, name) in new[] { ("en", "English"), ("ja", "日本語") })
        {
            var button = new Button { Text = name };
            button.Pressed += () => { SaveInterfaceLanguage(locale); ShowSettings(); };
            _content.AddChild(button);
        }
        AddHeading(Tr("ORIGINAL_EFFECTS_SETTINGS"),18);
        var effectsEnabled=new CheckButton {Name="EffectsEnabled",Text=Tr("ORIGINAL_EFFECTS_ENABLED"),ButtonPressed=_effects.Enabled};effectsEnabled.Toggled+=_effects.SetEnabled;_content.AddChild(effectsEnabled);
        var effectsLabel=new Label {Text=$"{Tr("EFFECTS_VOLUME")}: {_effects.VolumePercent:N0}%"};_content.AddChild(effectsLabel);
        var effectsVolume=new HSlider {Name="EffectsVolume",MinValue=0,MaxValue=100,Step=1,Value=_effects.VolumePercent};effectsVolume.ValueChanged+=value=>{_effects.SetVolume((float)value);effectsLabel.Text=$"{Tr("EFFECTS_VOLUME")}: {value:N0}%";};_content.AddChild(effectsVolume);
        AddHeading(Tr("MUSIC_SETTINGS"), 18);
        _content.AddChild(new Label { Text = $"{Tr("MUSIC_DIRECTORY")}: {ProjectSettings.GlobalizePath(BackgroundMusic.DirectoryPath)}", AutowrapMode = TextServer.AutowrapMode.WordSmart });
        _content.AddChild(new Label { Text = Tr("MUSIC_LOCAL_NOTE"), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        var enabled = new CheckButton { Name = "MusicEnabled", Text = Tr("MUSIC_ENABLED"), ButtonPressed = _music.Enabled };
        enabled.Toggled += value => _music.SetEnabled(value); _content.AddChild(enabled);
        var seasonal = new CheckButton { Name = "SeasonalMusicEnabled", Text = Tr("MUSIC_SEASONAL_ENABLED"), ButtonPressed = _music.SeasonalEnabled };
        seasonal.Toggled += value => _music.SetSeasonalEnabled(value); _content.AddChild(seasonal);
        _content.AddChild(new Label { Text = Tr("MUSIC_SEASONAL_NOTE"), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        var volumeLabel = new Label { Text = $"{Tr("MUSIC_VOLUME")}: {_music.VolumePercent:N0}%" }; _content.AddChild(volumeLabel);
        var volume = new HSlider { Name = "MusicVolume", MinValue = 0, MaxValue = 100, Step = 1, Value = _music.VolumePercent };
        volume.ValueChanged += value => { _music.SetVolume((float)value); volumeLabel.Text = $"{Tr("MUSIC_VOLUME")}: {value:N0}%"; }; _content.AddChild(volume);
        AddAction("NEXT_MUSIC_TRACK", _music.NextTrack, _music.TrackCount > 0);
        if (_desktopState is GameState adviceState)
        {
            AddAction("SECRETARY_ADVICE",()=>ShowSecretaryAdvice(adviceState),DesktopActionEnabled("SECRETARY_ADVICE"));
            AddAction("HIRE_SECRETARY",()=>ShowSecretaryHiring(adviceState),DesktopActionEnabled("HIRE_SECRETARY"));
            AddAction("SECRETARY_POLICIES",()=>ShowSecretaryPolicies(adviceState),DesktopActionEnabled("SECRETARY_POLICIES"));
        }
        if (_desktopState is GameState state) AddAction("BACK_TO_OFFICE", () => ShowOfficeReconstruction(state));
        AddAction("BACK_TO_TITLE", ShowTitleScreen);
    }

    private void AddAction(string key, Action action, bool enabled = true)
    {
        var button = new Button { Text = Tr(key), Disabled = !enabled, CustomMinimumSize = new Vector2(0, _desktop is null ? 42 : 26) };
        button.Pressed += () => action(); _content.AddChild(button);
    }

    private void AddMetric(GridContainer grid, string label, string value)
    {
        var left = new Label { Text = label };
        left.AddThemeFontSizeOverride("font_size", _desktop is null ? 17 : 13);
        grid.AddChild(left);
        var right = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Right };
        right.AddThemeFontSizeOverride("font_size", 13);
        grid.AddChild(right);
    }

    private LineEdit AddField(string labelText, string initialValue, int maxLength)
    {
        var label = new Label { Text = labelText };
        label.AddThemeFontSizeOverride("font_size", 18);
        _content.AddChild(label);

        var edit = new LineEdit
        {
            Text = initialValue,
            MaxLength = maxLength,
            CustomMinimumSize = new Vector2(0, 44)
        };
        _content.AddChild(edit);
        return edit;
    }

    private void AddHeading(string text, int size)
    {
        var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeFontSizeOverride("font_size", _desktop is null ? size : Math.Min(size, 18));
        if (_dialog is not null && _content.GetChildCount() == 0) _dialog.SetTitle(text);
        _content.AddChild(label);
    }

    private void AddSpacer(float height) => _content.AddChild(new Control { CustomMinimumSize = new Vector2(0, height) });

    private void ClearContent(bool keepConversationScene=false,bool keepMeetingMusic=false)
    {
        if (!keepMeetingMusic) _music?.SetMeetingPhase(-1);
        if (!keepConversationScene) CloseConversationScene();
        _conversationReveal=null;_conversationElapsed=0;
        _clockRunning=false;
        _pokerInterfaceVisible=false;
        _refreshCoreStatus=null;
        if (_desktop is null)
        {
            _officeRenderer = null;
            _officeTexture = null;
        }
        _officeElapsed = 0;
        if (_desktop is not null) PrepareDesktopDialog();
        else _content = _screenContent;
        _content.CustomMinimumSize = new Vector2(620, 0);
        _companyName = null;
        _presidentName = null;
        foreach (Node child in _content.GetChildren()) { _content.RemoveChild(child); child.QueueFree(); }
    }
}
