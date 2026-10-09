using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenGDS2.Core;
using OpenGDS2.Domain;
using OpenGDS2.Persistence;

namespace OpenGDS2.UI;

public partial class Main
{
    private Control? _desktop;
    private Control _screen = null!;
    private Control? _workspace;
    private DesktopWindow? _dialog;
    private Vector2? _conversationPosition;
    private GameState? _desktopState;
    private string _desktopLocale = string.Empty;
    private Label? _desktopFunds;
    private Label? _desktopStatus;
    private Tree? _staffTable;
    private Tree? _productTable;
    private readonly Dictionary<string, Label> _companyMetrics = new();
    private readonly Dictionary<string, DesktopWindow> _windows = new();
    private readonly List<TreeItem> _staffRows = new();
    private readonly List<TreeItem> _productRows = new();
    private Button? _desktopPause;
    private Button? _desktopLoad;
    private HBoxContainer? _projectButtons;
    private string _projectSignature = string.Empty;
    private int _desktopRefreshFrame;
    private string _toolbarHelp = string.Empty;
    private bool _toolbarArtworkMissing;

    private void DestroyDesktop()
    {
        _music?.SetMeetingPhase(-1);
        RememberConversationPosition();
        CloseConversationScene();
        CloseEmployeeDetails();
        if (_desktop is not null) { RemoveChild(_desktop); _desktop.QueueFree(); }
        _desktop = null; _desktopState = null; _workspace = null; _dialog = null;
        _desktopFunds = null; _desktopStatus = null; _desktopPause = null; _desktopLoad = null;
        _staffTable = null; _productTable = null; _staffRows.Clear(); _productRows.Clear();
        _windows.Clear(); _companyMetrics.Clear(); _projectButtons = null; _projectSignature = string.Empty;
        _toolbarHelp = string.Empty; _toolbarArtworkMissing = false;
        _screen.Show();
    }

    private DesktopWindow ChildWindow(string name, string title, Vector2 position, Vector2 size, bool maximize = true)
    {
        var window = new DesktopWindow { Name = name };
        _workspace!.AddChild(window); window.Initialize(title, position, size, maximize);
        _windows[name] = window;
        return window;
    }

    private void EnsureDesktop(GameState state)
    {
        _music.SetMonth(state.Month ?? 0);
        if (_desktop is not null && ReferenceEquals(state, _desktopState) && _desktopLocale == TranslationServer.GetLocale()) return;
        DestroyDesktop();
        foreach (Node child in _screenContent.GetChildren()) { _screenContent.RemoveChild(child); child.QueueFree(); }
        _screen.Hide(); _desktopState = state; _desktopLocale = TranslationServer.GetLocale();
        _music.SetGameplayActive(true);
        _desktop = new Control { Name = "GameDesktop", MouseFilter = MouseFilterEnum.Ignore };
        _desktop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_desktop);
        var layout = new VBoxContainer(); layout.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        layout.AddThemeConstantOverride("separation", 0); _desktop.AddChild(layout);
        var menus = new HBoxContainer { CustomMinimumSize = new Vector2(0, 24) }; layout.AddChild(menus);
        Menu(menus, "MENU_GAME", [("SAVE_GAME", SaveDesktop), ("LOAD_GAME", LoadSession),
            ("SETTINGS", ShowSettings), ("IMPORT_BUTTON", ShowImporter), ("BACK_TO_TITLE", ShowTitleScreen)]);
        Menu(menus, "MENU_COMPANY", [("COMPANY_ACTIVITIES", () => ShowCompanyActivities(state)),("BANK",()=>ShowBank(state)),
            ("DEVELOPMENT_COMPUTERS", () => ShowComputers(state)), ("HARDWARE_LICENSES", () => ShowLicenses(state)),
            ("TECHNOLOGIES", () => ShowTechnologies(state)),("SECRETARY_ADVICE",()=>ShowSecretaryAdvice(state)),
            ("HIRE_SECRETARY",()=>ShowSecretaryHiring(state)),("SECRETARY_POLICIES",()=>ShowSecretaryPolicies(state))]);
        Menu(menus, "MENU_STAFF", [("STAFF", () => ShowStaff(state)), ("HIRE_EMPLOYEES", ShowRecruitment)]);
        Menu(menus, "MENU_DEVELOPMENT", [("START_GAME_DEVELOPMENT", () => ShowPlanningPreview(state)),
            ("DEVELOPMENT_PLANNING", () => ShowPlanningPreview(state))]);
        Menu(menus, "MENU_PRODUCTS", [("SALES", () => ShowSales(state))]);
        Menu(menus, "MENU_WINDOWS", [("OFFICE_WINDOW", () => _windows["OfficeWindow"].Reopen()),
            ("COMPANY_WINDOW", () => _windows["CompanyWindow"].Reopen()),
            ("EMPLOYEE_WINDOW", () => { OriginalPayrollRules.CaptureStaffList(state); _windows["EmployeeWindow"].Reopen(); }),
            ("PRODUCT_WINDOW", () => _windows["ProductWindow"].Reopen())]);
        Menu(menus, "MENU_HELP", [("CALENDAR_INSPECTOR", () => ShowCalendar(state)),("GLOBAL_RANKINGS",()=>ShowRankings(state)),("SALES_CHART",()=>ShowSalesChart(state))]);

        var toolbar = new HBoxContainer { Name = "DesktopToolbar", CustomMinimumSize = new Vector2(0, 28) };
        toolbar.AddThemeConstantOverride("separation", 1); layout.AddChild(toolbar);
        ToolbarAction(toolbar, "BACK_TO_TITLE", ShowTitleScreen, null);
        ToolbarAction(toolbar, "SAVE_GAME", SaveDesktop, 15);
        _desktopLoad = ToolbarAction(toolbar, "LOAD_GAME", LoadSession, 14, System.IO.File.Exists(SavePath));
        ToolbarAction(toolbar, "DEVELOPMENT_PLANNING", () => ShowPlanningPreview(state), null, _activeCatalog is not null);
        ToolbarAction(toolbar, "COMPANY_ACTIVITIES", () => ShowCompanyActivities(state), 8, state.Company.ActivityCost is not null);
        ToolbarAction(toolbar, "HIRE_EMPLOYEES", ShowRecruitment, 13, state.Recruitment.CandidateDatabaseDecoded);
        ToolbarAction(toolbar, "TECHNOLOGIES", () => ShowTechnologies(state), 12, _activeCatalog?.Technologies.Count > 0);
        ToolbarAction(toolbar, "DEVELOPMENT_COMPUTERS", () => ShowComputers(state), 10, _activeCatalog?.DevelopmentComputers.Count == 4);
        ToolbarAction(toolbar, "START_GAME_DEVELOPMENT", () => ShowPlanningPreview(state), null, _activeCatalog is not null);
        ToolbarAction(toolbar, "SALES", () => ShowSales(state), 6, _activeCatalog is not null);
        ToolbarAction(toolbar, "STAFF", () => ShowStaff(state), 7);
        ToolbarAction(toolbar, "HARDWARE_LICENSES", () => ShowLicenses(state), 9, state.AvailableHardware is not null && state.HardwareLicenses is not null);
        ToolbarAction(toolbar, "IMPORT_BUTTON", ShowImporter, null);
        ToolbarAction(toolbar, "SETTINGS", ShowSettings, null);
        ToolbarAction(toolbar, "CALENDAR_INSPECTOR", () => ShowCalendar(state), null);
        _desktopPause = ToolbarAction(toolbar, _paused ? "RESUME" : "PAUSE", () =>
        {
            _paused = !_paused; _desktopPause!.Text = Tr(_paused ? "RESUME" : "PAUSE");
            _desktopPause.Icon = ToolbarArtwork.CreateFallback(_paused ? "RESUME" : "PAUSE");
            if (!string.IsNullOrEmpty(_toolbarHelp)) _toolbarHelp = _desktopPause.Text;
            _desktopPause.TooltipText = _desktopPause.Text; RefreshDesktop(); _refreshCoreStatus?.Invoke();
        }, null);
        _desktopPause.Name = "TogglePause";
        toolbar.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        _desktopFunds = new Label { Name = "DesktopFunds", CustomMinimumSize = new Vector2(225, 0),
            HorizontalAlignment = HorizontalAlignment.Right }; toolbar.AddChild(_desktopFunds);
        _projectButtons = new HBoxContainer { Name = "DesktopProjects" }; layout.AddChild(_projectButtons);
        _workspace = new Control { Name = "DesktopWorkspace", SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore, ClipContents = true }; layout.AddChild(_workspace);
        var backdrop = OriginalAssetStore.LoadTexture("Graphic/Kabe.bmp");
        if (backdrop is not null)
        {
            var wallpaper = new TextureRect { Name = "OriginalDesktopWallpaper", Texture = backdrop,
                TextureRepeat = TextureRepeatEnum.Enabled, StretchMode = TextureRect.StretchModeEnum.Tile,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, MouseFilter = MouseFilterEnum.Ignore };
            wallpaper.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _workspace.AddChild(wallpaper);
        }
        else
        {
            var wallpaper = new ColorRect { Color = new Color(0.85f, 0.8f, 0.8f), MouseFilter = MouseFilterEnum.Ignore };
            wallpaper.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _workspace.AddChild(wallpaper);
        }
        var office = ChildWindow("OfficeWindow", state.Company.Name, new Vector2(4, 4), new Vector2(472, 318), false);
        _officeRenderer = OriginalAssetStore.LoadOfficeRenderer();
        _officeTexture = _officeRenderer?.CreateTexture(state);
        var texture = (Texture2D?)_officeTexture ?? OriginalAssetStore.LoadTexture("Graphic/Embedded/OfficeBase.bmp");
        if (texture is not null)
        {
            var officeImage = new TextureRect { Name = "OriginalOfficeBackground", Texture = texture,
                TextureFilter = TextureFilterEnum.Nearest, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(466, 290), TooltipText = Tr("OFFICE_EMPLOYEE_HELP") };
            officeImage.GuiInput += input => OnOfficeInput(state,officeImage,input);
            office.Body.AddChild(officeImage);
        }
        else office.Body.AddChild(new Label { Text = Tr("OFFICE_IMPORT_REQUIRED"), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        var company = ChildWindow("CompanyWindow", Tr("COMPANY_WINDOW"), new Vector2(480, 4), new Vector2(186, 318));
        foreach (var group in new[] {
            new[] { "COMPANY_NAME", "FUNDS" }, new[] { "WEEKLY_NET", "MONTHLY_NET", "YEARLY_NET" }, new[] { "COMPANY_FAME", "PRODUCTION_CAPACITY", "GENRE_RESEARCH" },
            new[] { "COMPANY_RANK", "RELEASED_COUNT", "HALL_OF_FAME_COUNT" }, new[] { "DATE", "STAFF" } })
        {
            var panel = new PanelContainer(); company.Body.AddChild(panel);
            Color shade = group[0] switch { "COMPANY_NAME" => new Color(0.78f, 0.76f, 1),
                "COMPANY_FAME" => new Color(1, 0.9f, 0.75f), "COMPANY_RANK" => new Color(1, 1, 0.78f),
                _ => new Color(0.7f, 0.97f, 1) };
            panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = shade,
                ContentMarginLeft = 3, ContentMarginRight = 3 });
            var rows = new VBoxContainer(); rows.AddThemeConstantOverride("separation", 0); panel.AddChild(rows);
            foreach (string key in group)
            {
                var row = new HBoxContainer(); rows.AddChild(row);
                var name = new Label { Text = Tr(key), SizeFlagsHorizontal = SizeFlags.ExpandFill };
                name.AddThemeFontSizeOverride("font_size", 11); row.AddChild(name);
                var value = new Label { Name="CompanyMetric"+key, HorizontalAlignment = HorizontalAlignment.Right };
                value.AddThemeFontSizeOverride("font_size", 11); row.AddChild(value); _companyMetrics[key] = value;
            }
        }
        var facilities=new Button {Name="CompanyFacilities",Text=Tr("COMPANY_FACILITIES")};
        facilities.AddThemeFontSizeOverride("font_size",11);company.Body.AddChild(facilities);
        facilities.Pressed+=()=>{if (_dialog is null) ShowFacilities(state);};
        var staff = ChildWindow("EmployeeWindow", Tr("EMPLOYEE_WINDOW"), new Vector2(720, 130), new Vector2(390, 150));
        OriginalPayrollRules.CaptureStaffList(state);
        _staffTable = CreateTable("DesktopEmployeeTable", ["EMPLOYEE_NAME", "EMPLOYEE_LEVEL", "SHORT_FUN", "SHORT_NOVELTY", "SHORT_GRAPHICS",
            "SHORT_MUSIC", "SHORT_ACCURACY", "SHORT_SPEED", "SHORT_INSPIRATION", "SHORT_EXPERIENCE"], [86, 32, 22, 22, 22, 22, 22, 22, 22, 50]);
        staff.Body.AddChild(_staffTable); _staffTable.ItemActivated += () =>
        {
            if (_dialog is not null) return;
            var selected = _staffTable.GetSelected();
            if (selected is null) return;
            int index = _staffRows.IndexOf(selected);
            if (index >= 0 && index < state.Employees.Count) ShowEmployeeDetails(state,state.Employees[index]);
        };
        var products = ChildWindow("ProductWindow", Tr("PRODUCT_WINDOW"), new Vector2(30, 356), new Vector2(390, 150));
        _productTable = CreateTable("DesktopProductTable", ["GAME_TITLE", "RETAIL_PRICE", "UNITS_SOLD", "STOCK"], [150, 65, 70, 70]);
        products.Body.AddChild(_productTable); _productTable.ItemActivated += () => {if (DesktopActionEnabled("SALES")) ShowSales(state);};
        _desktopStatus = new Label { Name = "OfficeStatus", CustomMinimumSize = new Vector2(0, 22), ClipText = true };
        layout.AddChild(_desktopStatus);
        RefreshDesktop();
    }

    private void OnOfficeInput(GameState state,TextureRect image,InputEvent input)
    {
        if (image.Texture is null || _dialog is not null) return;
        Vector2 position;
        if (input is InputEventMouseButton button) position = button.Position;
        else if (input is InputEventMouseMotion motion) position = motion.Position;
        else return;
        Vector2 native = image.Texture.GetSize();
        float scale = Math.Min(image.Size.X/native.X,image.Size.Y/native.Y);
        if (scale <= 0) return;
        Vector2 offset = (image.Size-native*scale)/2;
        Vector2 pixel = (position-offset)/scale;
        if (pixel.X < 0 || pixel.Y < 0 || pixel.X >= native.X || pixel.Y >= native.Y) return;
        var employee = OfficeInteractionRules.EmployeeAt(state,pixel.X,pixel.Y);
        var room=OfficeInteractionRules.DevelopmentRoomAt(pixel.X,pixel.Y);
        var tenants=OfficeInteractionRules.TenantSlotsAt(pixel.X,pixel.Y);
        bool secretary=OfficeInteractionRules.SecretaryAt(pixel.X,pixel.Y);
        image.MouseDefaultCursorShape = employee is null && room is null && tenants.Count==0 && !secretary ? CursorShape.Arrow : CursorShape.PointingHand;
        if (secretary && input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true})
        {
            image.AcceptEvent();ShowSecretaryAdvice(state);return;
        }
        if (input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true} && (room is not null || tenants.Count>0))
        {
            image.AcceptEvent();
            if (CalendarService.HasPending(state)) {ShowPendingCalendarEvent(state);return;}
            if (tenants.Count>1) ShowTenantPlotChoice(state,tenants);
            else if (tenants.Count==1) ShowFacilities(state,tenants[0]);
            else if (room is int index) ShowDevelopmentRoomActions(state,index);
            return;
        }
        if (employee is not null && input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true})
        {
            image.AcceptEvent();
            ShowEmployeeDetails(state,employee);
        }
        else if (employee is null && input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true}) CloseEmployeeDetails();
    }

    private void Menu(HBoxContainer parent, string title, (string Key, Action Action)[] actions)
    {
        var menu = new MenuButton { Name = title, Text = Tr(title) + " " }; parent.AddChild(menu);
        for (int index = 0; index < actions.Length; index++) menu.GetPopup().AddItem(Tr(actions[index].Key), index);
        menu.GetPopup().AboutToPopup += () =>
        {
            for (int index = 0; index < actions.Length; index++)
                menu.GetPopup().SetItemDisabled(index, !DesktopActionEnabled(actions[index].Key));
        };
        menu.GetPopup().IdPressed += id => { if (DesktopActionEnabled(actions[(int)id].Key)) actions[(int)id].Action(); };
    }

    private bool DesktopActionEnabled(string key)
    {
        // Original weekly forms are modal. Modern save/load/settings remain
        // accessible, but another action cannot mutate a suspended transaction.
        if (_desktopState is GameState current && (current.PendingBankLoanReceipt is not null || CalendarService.HasPending(current) || current.PendingHireGreeting is not null || current.PendingSecretaryAdvice is not null || current.PendingSecretaryReplacement is not null || current.PendingProjectCancellation is not null || current.PendingFacilityReceipt is not null) &&
            key is not ("SAVE_GAME" or "LOAD_GAME" or "SETTINGS" or "BACK_TO_TITLE")) return false;
        return key switch
        {
        "LOAD_GAME" => System.IO.File.Exists(SavePath),
        "START_GAME_DEVELOPMENT" or "DEVELOPMENT_PLANNING" or "SALES" => _activeCatalog is not null,
        "HIRE_EMPLOYEES" => _desktopState?.Recruitment.CandidateDatabaseDecoded == true,
        "DEVELOPMENT_COMPUTERS" => _activeCatalog?.DevelopmentComputers.Count == 4,
        "TECHNOLOGIES" => _activeCatalog?.Technologies.Count > 0,
        "HARDWARE_LICENSES" => _desktopState?.AvailableHardware is not null && _desktopState.HardwareLicenses is not null,
        "BANK" => _desktopState is { } bankState && BankLoanService.CheckBorrow(bankState) is BankLoanResult.Success or BankLoanResult.AllSlotsOccupied,
        "COMPANY_ACTIVITIES" => _desktopState?.Company.ActivityCost is not null,
        _ => true
        };
    }

    private Button ToolbarAction(HBoxContainer parent, string key, Action action, int? icon, bool enabled = true)
    {
        var button = new Button { Name = $"Toolbar{key}", Text = Tr(key), TooltipText = Tr(key), Disabled = !enabled, ClipText = true,
            CustomMinimumSize = new Vector2(29, 26), FocusMode = FocusModeEnum.All };
        button.TextureFilter = TextureFilterEnum.Nearest;
        button.AddThemeStyleboxOverride("normal", ClassicTheme.CompactButton());
        button.AddThemeStyleboxOverride("pressed", ClassicTheme.CompactButton(true));
        button.AddThemeFontSizeOverride("font_size", 1);
        foreach (string color in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color", "font_disabled_color" })
            button.AddThemeColorOverride(color, Colors.Transparent);
        if (icon is int id) button.Icon = OriginalAssetStore.LoadTexture($"Graphic/Embedded/Toolbar{id}.bmp");
        if (icon is not null && button.Icon is null) _toolbarArtworkMissing = true;
        button.Icon ??= ToolbarArtwork.CreateFallback(key);
        button.MouseEntered += () => { _toolbarHelp = button.Text; RefreshDesktop(); };
        button.MouseExited += () => { _toolbarHelp = string.Empty; RefreshDesktop(); };
        button.FocusEntered += () => { _toolbarHelp = button.Text; RefreshDesktop(); };
        button.FocusExited += () => { _toolbarHelp = string.Empty; RefreshDesktop(); };
        button.SetMeta("base_enabled",key=="LOAD_GAME" || enabled);
        button.Pressed += () => {if (DesktopActionEnabled(key)) action();}; parent.AddChild(button); return button;
    }

    private Tree CreateTable(string name, string[] headings, int[] widths)
    {
        var table = new Tree { Name = name, Columns = headings.Length, HideRoot = true, ColumnTitlesVisible = true,
            SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        table.AddThemeFontSizeOverride("font_size", 11);
        for (int column = 0; column < headings.Length; column++)
        {
            table.SetColumnTitle(column, Tr(headings[column])); table.SetColumnExpand(column, column == 0);
            table.SetColumnCustomMinimumWidth(column, widths[column]);
        }
        table.CreateItem(); return table;
    }

    private void RefreshDesktop()
    {
        if (_desktopState is not GameState state || _desktopFunds is null || _desktopStatus is null) return;
        if (_desktopLoad is not null) _desktopLoad.Disabled = !System.IO.File.Exists(SavePath);
        if (_desktopPause is not null) _desktopPause.Disabled=!DesktopActionEnabled("PAUSE");
        foreach (var button in _desktop!.FindChildren("Toolbar*","Button",true,false).OfType<Button>())
            button.Disabled=!button.GetMeta("base_enabled").AsBool() || !DesktopActionEnabled(button.Name.ToString()[7..]);
        _desktopFunds.Text = $"{Tr("FUNDS")}: ¥{state.Money:N0}";
        _companyMetrics["COMPANY_NAME"].Text = state.Company.Name;
        _companyMetrics["FUNDS"].Text = state.Money?.ToString("N0") ?? "?";
        _companyMetrics["WEEKLY_NET"].Text=state.Company.WeeklyNet?.ToString("N0") ?? "?";
        _companyMetrics["MONTHLY_NET"].Text=state.Company.MonthlyNet?.ToString("N0") ?? "?";
        _companyMetrics["YEARLY_NET"].Text=state.Company.YearlyNet?.ToString("N0") ?? "?";
        _companyMetrics["COMPANY_FAME"].Text = state.Company.Fame is int fame ? $"{fame}%" : "?";
        _companyMetrics["PRODUCTION_CAPACITY"].Text = state.Company.ProductionCapacity?.ToString() ?? "?";
        _companyMetrics["GENRE_RESEARCH"].Text = state.Company.OriginalMember90?.ToString() ?? "?";
        _companyMetrics["COMPANY_RANK"].Text = state.Company.Rank is int rank ? Tr($"COMPANY_RANK_{rank}") : "?";
        _companyMetrics["DATE"].Text = $"{state.Year}/{state.Month}/{state.Week}";
        _companyMetrics["STAFF"].Text = state.Employees.Count.ToString();
        _companyMetrics["RELEASED_COUNT"].Text = state.ReleasedGames.Count.ToString();
        _companyMetrics["HALL_OF_FAME_COUNT"].Text = state.ReleasedGames.Count(x => x.HallOfFame).ToString();
        string projects = string.Join(" · ", state.DevelopmentProjects.Select(ProjectSummary).Select(x => x.Replace('\n', ' ')));
        _desktopStatus.Text = !string.IsNullOrEmpty(_toolbarHelp) ? _toolbarHelp :
            (_paused ? Tr("PAUSED") + " · " : "") + (string.IsNullOrEmpty(projects)
                ? Tr(_toolbarArtworkMissing ? "TOOLBAR_ARTWORK_MISSING" : "TOOLBAR_READY") : projects);
        string signature = string.Join("|", state.DevelopmentProjects.Select(x => $"{x.RoomIndex}:{x.Title}"));
        if (signature != _projectSignature && _projectButtons is not null)
        {
            _projectSignature = signature;
            foreach (Node child in _projectButtons.GetChildren()) { _projectButtons.RemoveChild(child); child.QueueFree(); }
            foreach (var project in state.DevelopmentProjects.OrderBy(x => x.RoomIndex))
            {
                var button = new Button { Name = $"OpenProject{project.RoomIndex}", Text = $"{Tr("DEVELOPMENT_ROOM")} {project.RoomIndex + 1}: {project.Title}" };
                button.Pressed += () => {if (DesktopActionEnabled("START_GAME_DEVELOPMENT")) ShowProject(state, project);}; _projectButtons.AddChild(button);
            }
        }
        if (_staffTable is null || _productTable is null) return;
        SyncRows(_staffTable, _staffRows, state.Employees.Count);
        for (int index = 0; index < state.Employees.Count; index++)
        {
            var employee = state.Employees[index];
            string[] values = [EmployeeName(employee), employee.Level?.ToString() ?? "?", employee.Fun?.ToString() ?? "?",
                employee.Novelty?.ToString() ?? "?", employee.Graphics?.ToString() ?? "?", employee.Music?.ToString() ?? "?",
                employee.Accuracy?.ToString() ?? "?", employee.DevelopmentSpeed?.ToString() ?? "?", employee.Inspiration?.ToString() ?? "?",
                employee.Experience?.ToString("N0") ?? "?"];
            for (int col = 0; col < values.Length; col++) _staffRows[index].SetText(col, values[col]);
        }
        SyncRows(_productTable, _productRows, state.ReleasedGames.Count);
        for (int index = 0; index < state.ReleasedGames.Count; index++)
        {
            var game = state.ReleasedGames[index]; string[] values = [game.Title, game.Price.ToString("N0"), game.UnitsSold.ToString("N0"), game.Stock.ToString("N0")];
            for (int col = 0; col < values.Length; col++) _productRows[index].SetText(col, values[col]);
        }
    }

    private static void SyncRows(Tree table, List<TreeItem> rows, int count)
    {
        if (rows.Count == count) return;
        table.Clear(); rows.Clear(); var root = table.CreateItem();
        for (int index = 0; index < count; index++) rows.Add(table.CreateItem(root));
    }

    private void SaveDesktop()
    {
        if (_session.Current is not GameState state) return;
        try { SaveStore.Save(SavePath, state); if (_desktopLoad is not null) _desktopLoad.Disabled = false; if (_desktopStatus is not null) _desktopStatus.Text = Tr("SAVE_OK"); }
        catch (Exception ex) { if (_desktopStatus is not null) _desktopStatus.Text = $"{Tr("SAVE_FAILED")}: {ex.Message}"; }
    }

    private void CloseDesktopDialog()
    {
        _conversationReveal=null;_conversationElapsed=0;
        if (_dialog is null) return;
        RememberConversationPosition();
        _workspace!.RemoveChild(_dialog); _dialog.QueueFree(); _dialog = null;
    }

    private void RememberConversationPosition()
    {
        if (_dialog is not null && UsesConversationPosition(_dialog.Name.ToString()))
            _conversationPosition = _dialog.Position;
    }

    private static bool UsesConversationPosition(string name) => name is
        "ConversationWindow" or "SecretaryAdviceWindow" or "PlanningQuoteWindow" or
        "PlanningMeetingWindow" or "TypeMeetingWindow" or "GenreMeetingWindow" or "MangaRightsWindow";

    private void PrepareDesktopDialog()
    {
        CloseEmployeeDetails();
        CloseDesktopDialog();
        _dialog = new DesktopWindow { Name = "ManagementWindow" }; _workspace!.AddChild(_dialog);
        _dialog.Initialize(Tr("APP_TITLE"), new Vector2(430, 36), new Vector2(670, 520));
        // Close behaves like the screen's existing explicit back action, including pending decisions.
        _dialog.CloseRequested = () => { if (_session.Current is GameState state) ShowOfficeReconstruction(state); };
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _dialog.Body.AddChild(scroll);
        _content = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(620, 0) };
        _content.AddThemeConstantOverride("separation", 5); scroll.AddChild(_content);
    }

    private void CompactManagementWindow(string name, Vector2 size, float contentWidth)
    {
        _content.CustomMinimumSize = new Vector2(contentWidth, 0);
        if (_dialog is null || _workspace is null) return;
        _dialog.Name = name;
        _dialog.Size = size;
        // Prefer the free space beside the office, keeping the office visible.
        _dialog.Position = new Vector2(Math.Min(480, Math.Max(0, _workspace.Size.X - size.X)),
            Math.Min(36, Math.Max(0, _workspace.Size.Y - size.Y)));
        if (UsesConversationPosition(name) && _conversationPosition is Vector2 position)
            _dialog.SetPositionInsideWorkspace(position);
    }
}
