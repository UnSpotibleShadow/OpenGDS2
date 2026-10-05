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
    private Label _status = null!;
    private Button _importButton = null!;
    private FileDialog _fileDialog = null!;
    private LineEdit? _companyName;
    private LineEdit? _presidentName;
    private OriginalOfficeRenderer? _officeRenderer;
    private ImageTexture? _officeTexture;
    private double _officeElapsed;
    private OriginalCatalog? _activeCatalog;
    private bool _clockRunning;
    private bool _paused;
    private Action? _refreshCoreStatus;

    public override void _Process(double delta)
    {
        TickConversation(delta);
        if (++_desktopRefreshFrame % 6 == 0) {RefreshDesktop();RefreshEmployeeDetails();}
        RefreshEmployeeSelectionArrow();
        if (!_clockRunning || _paused || _session.Current is not GameState state) return;
        _officeElapsed += delta;
        // Keep at most one second of backlog after a suspended window.
        _officeElapsed = Math.Min(_officeElapsed, 1);
        bool updated = false;
        while (_officeElapsed >= OfficeStaffRules.TickSeconds)
        {
            _session.TickOffice(_activeCatalog);
            _music.SetMonth(state.Month ?? 0);
            PersistPendingRankings(state);
            _officeElapsed -= OfficeStaffRules.TickSeconds;
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
        BuildShell();
        _music = new BackgroundMusic { Name = "BackgroundMusic" };
        AddChild(_music);
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
        AddAction("GLOBAL_RANKINGS",()=>ShowRankings());
        var import = new Button { Text = Tr("IMPORT_BUTTON"), CustomMinimumSize = new Vector2(0, 46) };
        import.Pressed += ShowImporter;
        _content.AddChild(import);
        _content.AddChild(_status);
        var quit = new Button { Text = Tr("QUIT"), CustomMinimumSize = new Vector2(0, 46) };
        quit.Pressed += () => GetTree().Quit();
        _content.AddChild(quit);
        var note = new Label { Text = Tr("DEV_BUILD"), HorizontalAlignment = HorizontalAlignment.Center };
        _content.AddChild(note);
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

        var beginner = new Button { Text = Tr("BEGINNER_MODE"), CustomMinimumSize = new Vector2(0, 52) };
        beginner.Pressed += () => StartReconstructedGame(companyName, presidentName, GameMode.Beginner);
        _content.AddChild(beginner);

        var normal = new Button { Text = Tr("NORMAL_MODE"), CustomMinimumSize = new Vector2(0, 52) };
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

    private string SavePath => ProjectSettings.GlobalizePath("user://saves/slot1.json");

    private void ShowOfficeReconstruction(GameState state)
    {
        CloseConversationScene();
        _activeCatalog = _importer.LoadCatalog();
        _activeEmployeeDatabase = _importer.LoadEmployeeDatabase();
        PersistPendingRankings(state);
        EnsureDesktop(state);
        // M35 persisted these two stops before their form behavior was mapped.
        // Resume the existing cursor, without replaying a week boundary.
        if (state.Calendar?.Pending?.Block is CalendarBlock.Exhibition or CalendarBlock.BeginnerEnding)
        {
            _session.TickOffice(_activeCatalog);
            PersistPendingRankings(state);
        }
        if (ShowPendingHireGreeting(state)) return;
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
        ClearContent();
        AddHeading(Tr("HIRE_EMPLOYEES"), 18);
        CompactManagementWindow("RecruitmentWindow", new Vector2(500,440), 480);
        _content.AddChild(new Label { Name="RecruitmentFunds",Text=$"{Tr("FUNDS")}: ¥{state.Money:N0}" });
        var table=CreateTable("RecruitmentTable",
            ["EMPLOYEE_NAME","EMPLOYEE_LEVEL","SHORT_FUN","SHORT_GRAPHICS","SHORT_NOVELTY","SHORT_MUSIC","SHORT_ACCURACY","SHORT_CONTRACT_FEE"],
            [105,35,28,28,28,28,28,110]);
        table.CustomMinimumSize=new Vector2(0,140);_content.AddChild(table);
        var root=table.GetRoot();
        for (int i=0;i<roster.Candidates.Count;i++)
        {
            var candidate=roster.Candidates[i];var row=table.CreateItem(root);row.SetMetadata(0,i);
            string[] values=[EmployeeName(candidate),candidate.Level?.ToString() ?? "?",candidate.Fun?.ToString() ?? "?",
                candidate.Graphics?.ToString() ?? "?",candidate.Novelty?.ToString() ?? "?",candidate.Music?.ToString() ?? "?",
                candidate.Accuracy?.ToString() ?? "?",candidate.ContractFee?.ToString("N0") ?? "?"];
            for (int col=0;col<values.Length;col++) {row.SetText(col,values[col]);if (col>0) row.SetTextAlignment(col,HorizontalAlignment.Right);}
        }
        var details=new HBoxContainer {Name="RecruitmentDetails"};details.AddThemeConstantOverride("separation",5);_content.AddChild(details);
        _status = new Label { Name="RecruitmentStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart };
        _content.AddChild(_status);
        var actions=new HBoxContainer();_content.AddChild(actions);
        var hire=new Button {Name="HireSelectedCandidate",Text=Tr("HIRE_CONFIRM"),Disabled=true,SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(hire);
        var cancel=new Button {Text=Tr("CANCEL"),SizeFlagsHorizontal=SizeFlags.ExpandFill};actions.AddChild(cancel);
        cancel.Pressed+=()=>ShowOfficeReconstruction(state);
        EmployeeState? selected=null;
        void SelectCandidate()
        {
            var row=table.GetSelected();if (row is null) return;
            int index=row.GetMetadata(0).AsInt32();if (index<0 || index>=roster.Candidates.Count) return;
            selected=roster.Candidates[index];
            foreach (Node child in details.GetChildren()) {details.RemoveChild(child);child.QueueFree();}
            var picture=new VBoxContainer();details.AddChild(picture);
            picture.AddChild(new TextureRect {Name="EmployeePortrait",Texture=OriginalAssetStore.LoadEmployeePortrait(selected.PortraitId),
                TextureFilter=TextureFilterEnum.Nearest,ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,CustomMinimumSize=new Vector2(64,64)});
            picture.AddChild(new Label {Text=$"{Tr("LEVEL")} {selected.Level}/10"});
            details.AddChild(new EmployeeAbilityChart {Name="RecruitmentAbilityChart",Employee=selected,
                CustomMinimumSize=new Vector2(210,144),ClipContents=true,MouseFilter=MouseFilterEnum.Ignore});
            var skills=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill};details.AddChild(skills);
            AddRecruitmentBar(skills,"STAT_DEVELOPMENT_SPEED",selected.DevelopmentSpeed);
            AddRecruitmentBar(skills,"STAT_INSPIRATION",selected.Inspiration);
            var technology=new Label {Name="RecruitmentTechnologies",AutowrapMode=TextServer.AutowrapMode.WordSmart};skills.AddChild(technology);
            var learned=selected.TechnologyLevels?.Where(x=>x.Value>0).OrderBy(x=>x.Key).ToList();
            technology.Text=learned is null ? Tr("NOT_YET_DECODED") : learned.Count==0 ? Tr("EMPLOYEE_NO_TECHNOLOGIES") :
                string.Join("\n",learned.Select(x=>CatalogName(_activeCatalog,"TECHNOLOGY",x.Key,_activeCatalog?.Technologies.SingleOrDefault(t=>t.SourceIndex==x.Key)?.Name ?? "?")));
            hire.Disabled=selected.ContractFee is not long fee || state.Money is not long money || money<fee;
        }
        table.ItemSelected+=SelectCandidate;
        hire.Pressed+=()=>
        {
            if (selected is null) return;
            var result=_session.Hire(selected);
            if (result==HireResult.Success) ShowOfficeReconstruction(state);
            else _status.Text=Tr("HIRE_FAILED")+" "+Tr($"HIRE_RESULT_{result}");
        };
        root.GetFirstChild()?.Select(0);
        if (roster.Candidates.Count == 0) AddHeading(Tr("NO_APPLICANTS"), 18);
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
        ClearContent(); AddHeading(Tr("HARDWARE_LICENSES"), 30);
        var catalog = _importer.LoadCatalog();
        if (catalog is not null)
            foreach (var hardware in catalog.Hardware.Where(x => state.AvailableHardware?.Contains(x.SourceIndex) == true))
            {
                AddHeading($"{CatalogName(catalog,"HARDWARE",hardware.SourceIndex,hardware.Name)} · {hardware.BitWidth} {Tr("BIT_UNIT")} · {hardware.LicenseFee:N0}", 18);
                if (state.HardwareLicenses?.Contains(hardware.SourceIndex) == true) AddHeading(Tr("LICENSE_OWNED"), 14);
                else AddAction("LICENSE_BUY", () =>
                {
                    var result = HardwareLicenseService.TryPurchase(state, catalog, hardware.SourceIndex);
                    ShowLicenses(state); AddHeading(Tr($"LICENSE_RESULT_{result}"), 16);
                });
            }
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private void ShowComputers(GameState state)
    {
        ClearContent(); AddHeading(Tr("DEVELOPMENT_COMPUTERS"), 30);
        var catalog = _importer.LoadCatalog();
        if (catalog is not null)
            foreach (var computer in catalog.DevelopmentComputers)
            {
                AddHeading($"{CatalogName(catalog,"COMPUTER",computer.SourceIndex,computer.Name)} · {computer.BitWidth} {Tr("BIT_UNIT")} · {computer.PurchasePrice:N0}", 18);
                if (computer.SourceIndex == state.DevelopmentComputerIndex) AddHeading(Tr("COMPUTER_CURRENT"), 14);
                else AddAction("COMPUTER_BUY", () =>
                {
                    var result = DevelopmentComputerService.TryPurchase(state, catalog, computer.SourceIndex);
                    ShowComputers(state); AddHeading(Tr($"COMPUTER_RESULT_{result}"), 16);
                });
            }
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private void ShowStaff(GameState state)
    {
        ClearContent(); AddHeading(Tr("STAFF"), 30);
        bool completeSources=OriginalPayrollRules.CaptureStaffList(state);
        long? salaryTotal=null;
        // Malformed large monetary fields must not crash this information screen.
        try
        {
            if (completeSources)
            {
                salaryTotal=OriginalPayrollRules.StaffSalaryTotal(state.EmployeeSources);
            }
        }
        catch (OverflowException) { salaryTotal=null; }
        _content.AddChild(new Label {Name="StaffSalaryTotal",
            Text=$"{Tr("MONTHLY_SALARY_TOTAL")}: {salaryTotal?.ToString("N0") ?? Tr("NOT_YET_DECODED")}"});
        foreach (var employee in state.Employees)
        {
            if (CalendarService.HasPending(state)) AddHeading($"{EmployeeName(employee)}: {employee.MonthlySalary:N0}",18);
            else AddStaffEmployeeControls(state,employee);
        }
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private VBoxContainer AddStaffEmployeeControls(GameState state,EmployeeState employee)
    {
        var panel = AddEmployeeCard(employee);
        panel.AddChild(new Label {Name=$"MonthlySalary{employee.SourceIndex}",
            Text=$"{Tr("MONTHLY_SALARY")}: {employee.MonthlySalary?.ToString("N0") ?? Tr("NOT_YET_DECODED")}"});
        var dismiss=new Button {Name=$"DismissEmployee{employee.SourceIndex}",Text=Tr("DISMISS_EMPLOYEE"),Disabled=employee.SourceIndex is null};
        dismiss.Pressed+=()=>ShowDismissEmployee(state,employee);panel.AddChild(dismiss);
        string threshold = employee.Level is >= 1 and <= 10
            ? EmployeeLevelUpService.RequiredExperience(employee.Level.Value).ToString() : Tr("NOT_YET_DECODED");
        panel.AddChild(new Label {Text=$"{Tr("EXPERIENCE")}: {employee.Experience?.ToString("0.0") ?? Tr("NOT_YET_DECODED")} / {threshold}"});
        panel.AddChild(new Label {Text=$"{Tr("STRESS")}: {employee.Stress?.ToString("0.0") ?? Tr("NOT_YET_DECODED")}"});
        if (employee.Office is OfficeEmployeeState workspace)
        {
            panel.AddChild(new Label {Text=workspace.Action is >=6 and <=17
                ? $"{Tr("DEVELOPMENT_ROOM")} {(workspace.Action-6)/6+1} / {Tr("DESK")} {(workspace.Action-6)%6+1}"
                : workspace.Action is >=1 and <=5 ? $"{Tr("STUDY_SEAT")} {workspace.Action}" : Tr("LOUNGE")});
            AddAction("ASSIGN_DESK",()=>ShowDeskAssignment(state,employee));
            AddAction("TRAINING",()=>ShowTraining(state,employee),employee.TechnologyLevels is not null && _importer.LoadCatalog()?.Technologies.Count>0);
            AddAction("USE_TECHNOLOGY",()=>ShowTechnologyUse(state,employee),employee.TechnologyLevels?.Any(x=>x.Value>0)==true);
            AddAction("TAKE_BREAK",()=>
            {
                var result=OfficeAssignmentService.TryTakeBreak(state,employee,new Random());
                if (result==OfficeAssignmentResult.Success) ShowOfficeReconstruction(state);
                else { ShowStaff(state); AddHeading(Tr($"ASSIGNMENT_RESULT_{result}"),16); }
            },workspace.Action is >=1 and <=17);
        }
        if (employee.SourceIndex is int source)
            AddAction("LEVEL_UP", () =>
            {
                var random = new Random();
                var result = EmployeeLevelUpService.TryLevelUp(state, source, Enumerable.Range(0, 8).Select(_ => random.NextSingle()).ToArray());
                ShowStaff(state);
                if (result != LevelUpResult.Success) AddHeading(Tr("LEVEL_UP_FAILED") + " " + Tr($"LEVEL_UP_RESULT_{result}"), 16);
            }, employee.Level is >= 1 and < 10 && employee.Experience >= EmployeeLevelUpService.RequiredExperience(employee.Level.Value));
        return panel;
    }

    private void ShowCalendar(GameState state)
    {
        // Separate inspector: advancing an incomplete simulation would skip payroll/events.
        ClearContent(); AddHeading(Tr("CALENDAR_INSPECTOR"), 28);
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

    private void LoadSession()
    {
        try { var state = SaveStore.Load(SavePath); _session.Restore(state); ShowOfficeReconstruction(state); }
        catch (Exception ex) { ShowTitleScreen(); _status.Text = $"{Tr("LOAD_FAILED")}: {ex.Message}"; }
    }

    private void ShowSettings()
    {
        ClearContent(); AddHeading(Tr("SETTINGS"), 30);
        foreach (var (locale, name) in new[] { ("en", "English"), ("ja", "日本語") })
        {
            var button = new Button { Text = name };
            button.Pressed += () => { SaveInterfaceLanguage(locale); ShowSettings(); };
            _content.AddChild(button);
        }
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

    private void ClearContent(bool keepConversationScene=false)
    {
        if (!keepConversationScene) CloseConversationScene();
        _conversationReveal=null;_conversationElapsed=0;
        _clockRunning=false;
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
