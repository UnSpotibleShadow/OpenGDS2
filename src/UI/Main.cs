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
        if (!_clockRunning || _paused || _session.Current is not GameState state) return;
        _officeElapsed += delta;
        // Keep at most one second of backlog after a suspended window.
        _officeElapsed = Math.Min(_officeElapsed, 1);
        bool updated = false;
        while (_officeElapsed >= OfficeStaffRules.TickSeconds)
        {
            _session.TickOffice(_activeCatalog);
            _officeElapsed -= OfficeStaffRules.TickSeconds;
            if (ShowPendingProductEvent(state)) return;
            updated = state.Employees.Any(x => x.Office is not null);
            var pending=state.DevelopmentProjects.FirstOrDefault(x=>x.PendingDecision.HasValue);
            if (pending is not null) {ShowProjectDecision(state,pending);return;}
        }
        if (updated && _officeRenderer is not null && _officeTexture is not null) _officeRenderer.UpdateTexture(_officeTexture, state);
        _refreshCoreStatus?.Invoke();
    }

    public override void _Ready()
    {
        BuildShell();
        TranslationServer.SetLocale("en");
        ShowTitleScreen();
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
        var center = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        scroll.AddChild(center);

        _content = new VBoxContainer { CustomMinimumSize = new Vector2(620, 0) };
        _content.AddThemeConstantOverride("separation", 14);
        center.AddChild(_content);

        _fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            UseNativeDialog = true,
            Title = "Select GameDev2DX.lzh"
        };
        _fileDialog.Filters = ["*.lzh, *.LZH ; LHA/LZH archives"];
        _fileDialog.FileSelected += ImportSelected;
        AddChild(_fileDialog);
    }

    private void ShowImporter()
    {
        ClearContent();
        AddHeading(Tr("APP_TITLE"), 42);
        AddHeading(Tr("APP_SUBTITLE"), 18);
        AddSpacer(18);
        var prompt = new Label { Text = Tr("IMPORT_PROMPT"), HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _content.AddChild(prompt);
        _importButton = new Button { Text = Tr("IMPORT_BUTTON"), CustomMinimumSize = new Vector2(0, 52) };
        _importButton.Pressed += () => _fileDialog.PopupCenteredRatio(0.75f);
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
            _status.Text = $"{Tr("IMPORT_OK")} ({result.FilesImported} files)";
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
    /// Company Name, President Name (default 伊集院 光の助), Confirm, Cancel.
    /// </summary>
    private void ShowCompanySetup()
    {
        ClearContent();
        AddHeading(Tr("NEW_GAME"), 34);

        _companyName = AddField(Tr("COMPANY_NAME"), string.Empty, 32);
        _presidentName = AddField(Tr("PRESIDENT_NAME"), "伊集院 光の助", 32);

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
        if (ShowPendingProductEvent(state)) return;
        ClearContent();
        _activeCatalog=_importer.LoadCatalog();
        _clockRunning=true;
        _content.CustomMinimumSize = new Vector2(1248, 0);
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 16);
        _content.AddChild(row);
        var office = new VBoxContainer { CustomMinimumSize = new Vector2(932, 0) };
        row.AddChild(office);
        var company = new Label { Text = state.Company.Name, HorizontalAlignment = HorizontalAlignment.Center };
        company.AddThemeFontSizeOverride("font_size", 22);
        office.AddChild(company);
        _officeRenderer = OriginalAssetStore.LoadOfficeRenderer();
        _officeTexture = _officeRenderer?.CreateTexture(state);
        var texture = (Texture2D?)_officeTexture ?? OriginalAssetStore.LoadTexture("Graphic/Embedded/OfficeBase.bmp");
        if (texture is not null)
        {
            office.AddChild(new TextureRect
            {
                Name = "OriginalOfficeBackground", Texture = texture,
                TextureFilter = TextureFilterEnum.Nearest,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(932, 580)
            });
        }
        else
        {
            var importNote = new Label { Text = Tr("OFFICE_IMPORT_REQUIRED"),
                AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(932, 100) };
            office.AddChild(importNote);
        }
        var sidebar = new VBoxContainer { CustomMinimumSize = new Vector2(300, 0) };
        sidebar.AddThemeConstantOverride("separation", 5);
        row.AddChild(sidebar);
        var metrics = new GridContainer { Columns = 2 };
        metrics.AddThemeConstantOverride("h_separation", 12);
        AddMetric(metrics, Tr("FUNDS"), state.Money?.ToString("N0") ?? Tr("NOT_YET_DECODED"));
        AddMetric(metrics, Tr("DATE"), $"{state.Year} / {state.Month} / {state.Week}");
        AddMetric(metrics, Tr("STAFF"), state.Employees.Count.ToString());
        AddMetric(metrics, Tr("COMPANY_RANK"), state.Company.Rank is int rank ? Tr($"COMPANY_RANK_{rank}") : Tr("NOT_YET_DECODED"));
        AddMetric(metrics, Tr("COMPANY_FAME"), state.Company.Fame is int fame ? $"{fame}%" : Tr("NOT_YET_DECODED"));
        AddMetric(metrics,Tr("GENRE_RESEARCH"),state.Company.OriginalMember90?.ToString() ?? Tr("NOT_YET_DECODED"));
        AddMetric(metrics,Tr("PRODUCTION_CAPACITY"),state.Company.ProductionCapacity?.ToString() ?? Tr("NOT_YET_DECODED"));
        sidebar.AddChild(metrics);
        void OfficeAction(string key, Action action, bool enabled = true)
        {
            var button = new Button { Text = Tr(key), Disabled = !enabled, CustomMinimumSize = new Vector2(0, 30) };
            button.Pressed += () => action();
            sidebar.AddChild(button);
        }
        OfficeAction("HIRE_EMPLOYEES", ShowRecruitment, state.Recruitment.CandidateDatabaseDecoded);
        OfficeAction("STAFF", () => ShowStaff(state));
        OfficeAction("COMPANY_ACTIVITIES",()=>ShowCompanyActivities(state),state.Company.ActivityCost is not null && state.CompanyActivityPerformed is not null && state.CompanyActivityTicks is not null);
        OfficeAction("DEVELOPMENT_COMPUTERS", () => ShowComputers(state), _importer.LoadCatalog()?.DevelopmentComputers.Count == 4);
        OfficeAction("HARDWARE_LICENSES", () => ShowLicenses(state), state.AvailableHardware is not null && state.HardwareLicenses is not null);
        OfficeAction("SAVE_GAME", () =>
        {
            try { SaveStore.Save(SavePath, state); ShowOfficeReconstruction(state); _status.Text = Tr("SAVE_OK"); }
            catch (Exception ex) { _status.Text = $"{Tr("SAVE_FAILED")}: {ex.Message}"; }
        });
        OfficeAction("LOAD_GAME", LoadSession, System.IO.File.Exists(SavePath));
        OfficeAction("CALENDAR_INSPECTOR", () => ShowCalendar(state));
        OfficeAction("DEVELOPMENT_PLANNING", () => ShowPlanningPreview(state), _importer.LoadCatalog() is not null);
        OfficeAction("START_GAME_DEVELOPMENT", () => ShowPlanningPreview(state), _activeCatalog is not null);
        OfficeAction("SALES", () => ShowSales(state), _activeCatalog is not null);
        OfficeAction("PAUSE", () => _paused=!_paused);
        foreach (var project in state.DevelopmentProjects.OrderBy(x=>x.RoomIndex))
        {
            var button=new Button {Name=$"OpenProject{project.RoomIndex}",Text=$"{Tr("DEVELOPMENT_ROOM")} {project.RoomIndex+1}: {project.Title}",
                CustomMinimumSize=new Vector2(0,30)};
            button.Pressed+=()=>ShowProject(state,project);sidebar.AddChild(button);
        }
        OfficeAction("IMPORT_BUTTON", ShowImporter);
        OfficeAction("BACK_TO_TITLE", ShowTitleScreen);
        _status = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        sidebar.AddChild(_status);
        _refreshCoreStatus=()=>
        {
            ((Label)metrics.GetChild(1)).Text=state.Money?.ToString("N0") ?? Tr("NOT_YET_DECODED");
            ((Label)metrics.GetChild(9)).Text=state.Company.Fame is int currentFame ? $"{currentFame}%" : Tr("NOT_YET_DECODED");
            ((Label)metrics.GetChild(11)).Text=state.Company.OriginalMember90?.ToString() ?? Tr("NOT_YET_DECODED");
            ((Label)metrics.GetChild(13)).Text=state.Company.ProductionCapacity?.ToString() ?? Tr("NOT_YET_DECODED");
            _status.Text=string.Join("\n",state.DevelopmentProjects.OrderBy(x=>x.RoomIndex).Select(ProjectSummary));
            if (_paused) _status.Text+="\n"+Tr("PAUSED");
        };
        _refreshCoreStatus();
        var progress = new Label { Text = Tr("RECONSTRUCTION_STATUS"), AutowrapMode = TextServer.AutowrapMode.WordSmart };
        sidebar.AddChild(progress);
        if (!state.Recruitment.CandidateDatabaseDecoded) _status.Text = Tr("DATABASE_IMPORT_REQUIRED");
    }

    private void ShowRecruitment()
    {
        var state = _session.Current ?? throw new InvalidOperationException("No session.");
        var roster = _session.OpenRecruitment();
        ClearContent();
        AddHeading(Tr("HIRE_EMPLOYEES"), 30);
        _status = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _content.AddChild(_status);
        foreach (var candidate in roster.Candidates)
        {
            var panel = AddEmployeeCard(candidate);
            var hire = new Button { Text = Tr("HIRE_CONFIRM"), Disabled = state.Money < candidate.ContractFee };
            hire.Pressed += () =>
            {
                var result = _session.Hire(candidate);
                if (result == HireResult.Success) ShowOfficeReconstruction(state);
                else _status.Text = Tr("HIRE_FAILED") + " " + Tr($"HIRE_RESULT_{result}");
            };
            panel.AddChild(hire);
        }
        if (roster.Candidates.Count == 0) AddHeading(Tr("NO_APPLICANTS"), 18);
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private void ShowLicenses(GameState state)
    {
        ClearContent(); AddHeading(Tr("HARDWARE_LICENSES"), 30);
        var catalog = _importer.LoadCatalog();
        if (catalog is not null)
            foreach (var hardware in catalog.Hardware.Where(x => state.AvailableHardware?.Contains(x.SourceIndex) == true))
            {
                AddHeading($"{hardware.Name} · {hardware.BitWidth} bit · {hardware.LicenseFee:N0}", 18);
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
                AddHeading($"{computer.Name} · {computer.BitWidth} bit · {computer.PurchasePrice:N0}", 18);
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
        foreach (var employee in state.Employees)
        {
            var panel = AddEmployeeCard(employee);
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
                    : Tr("LOUNGE")});
                AddAction("ASSIGN_DESK",()=>ShowDeskAssignment(state,employee));
                AddAction("TAKE_BREAK",()=>
                {
                    var result=OfficeAssignmentService.TryTakeBreak(state,employee,new Random());
                    if (result==OfficeAssignmentResult.Success) ShowOfficeReconstruction(state);
                    else { ShowStaff(state); AddHeading(Tr($"ASSIGNMENT_RESULT_{result}"),16); }
                },workspace.Action is >=6 and <=17);
            }
            if (employee.SourceIndex is int source)
                AddAction("LEVEL_UP", () =>
                {
                    var random = new Random();
                    var result = EmployeeLevelUpService.TryLevelUp(state, source, Enumerable.Range(0, 8).Select(_ => random.NextSingle()).ToArray());
                    ShowStaff(state);
                    if (result != LevelUpResult.Success) AddHeading(Tr("LEVEL_UP_FAILED") + " " + Tr($"LEVEL_UP_RESULT_{result}"), 16);
                }, employee.Level is >= 1 and < 10 && employee.Experience >= EmployeeLevelUpService.RequiredExperience(employee.Level.Value));
        }
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private void ShowCalendar(GameState state)
    {
        // Separate inspector: advancing an incomplete simulation would skip payroll/events.
        ClearContent(); AddHeading(Tr("CALENDAR_INSPECTOR"), 28);
        var date = new GameDate(state.Year ?? 1, state.Month ?? 4, state.Week ?? 1);
        var label = new Label { Text = $"{date.Year} / {date.Month} / {date.Week}", HorizontalAlignment = HorizontalAlignment.Center };
        _content.AddChild(label); AddHeading(Tr("CALENDAR_INSPECTOR_NOTE"), 14);
        AddAction("NEXT_WEEK", () => { date = CalendarRules.AdvanceWeek(date); label.Text = $"{date.Year} / {date.Month} / {date.Week}"; });
        AddAction("CANCEL", () => ShowOfficeReconstruction(state));
    }

    private void LoadSession()
    {
        try { var state = SaveStore.Load(SavePath); _session.Restore(state); ShowOfficeReconstruction(state); }
        catch (Exception ex) { _status.Text = $"{Tr("LOAD_FAILED")}: {ex.Message}"; }
    }

    private void ShowSettings()
    {
        ClearContent(); AddHeading(Tr("SETTINGS"), 30);
        foreach (var (locale, name) in new[] { ("en", "English"), ("ja", "日本語") })
        {
            var button = new Button { Text = name };
            button.Pressed += () => { TranslationServer.SetLocale(locale); ShowSettings(); };
            _content.AddChild(button);
        }
        AddAction("BACK_TO_TITLE", ShowTitleScreen);
    }

    private void AddAction(string key, Action action, bool enabled = true)
    {
        var button = new Button { Text = Tr(key), Disabled = !enabled, CustomMinimumSize = new Vector2(0, 42) };
        button.Pressed += () => action(); _content.AddChild(button);
    }

    private static void AddMetric(GridContainer grid, string label, string value)
    {
        var left = new Label { Text = label };
        left.AddThemeFontSizeOverride("font_size", 17);
        grid.AddChild(left);
        var right = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Right };
        right.AddThemeFontSizeOverride("font_size", 17);
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
        label.AddThemeFontSizeOverride("font_size", size);
        _content.AddChild(label);
    }

    private void AddSpacer(float height) => _content.AddChild(new Control { CustomMinimumSize = new Vector2(0, height) });

    private void ClearContent()
    {
        _clockRunning=false;
        _refreshCoreStatus=null;
        _officeRenderer = null;
        _officeTexture = null;
        _officeElapsed = 0;
        _content.CustomMinimumSize = new Vector2(620, 0);
        _companyName = null;
        _presidentName = null;
        foreach (Node child in _content.GetChildren()) { _content.RemoveChild(child); child.QueueFree(); }
    }
}
