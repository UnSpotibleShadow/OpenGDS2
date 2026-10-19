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
    private readonly Dictionary<string,Vector2> _dialogPositions=new();
    private string? _dialogPositionKey;
    private readonly Dictionary<int,(DesktopWindow Window,Action Refresh)> _retainedProjectReports=new();
    private int? _projectReportRoom;
    private Action? _projectReportRefresh;
    private bool _closingProjectReport;
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
    private Button? _desktopLoad;
    private HBoxContainer? _projectButtons;
    private string _projectSignature = string.Empty;
    private int _desktopRefreshFrame;
    private string _toolbarHelp = string.Empty;
    private bool _toolbarArtworkMissing;

    private void DestroyDesktop()
    {
        CloseAwardBoard();
        ClosePromotionSelector();
        CloseExhibitionBoard();
        CloseFacilityOwner();
        _music?.SetMeetingPhase(-1);
        RememberConversationPosition();
        CloseConversationScene();
        CloseCompanyResearch();
        CloseEmployeeDetails();
        if (_desktop is not null) { RemoveChild(_desktop); _desktop.QueueFree(); }
        _desktop = null; _desktopState = null; _workspace = null; _dialog = null;
        _retainedProjectReports.Clear();_projectReportRoom=null;_projectReportRefresh=null;_dialogPositionKey=null;
        _desktopFunds = null; _desktopStatus = null; _desktopLoad = null;
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
        if(state.NumberDisplayGrouping is { } grouping)DisplayNumbers.Grouping=grouping;
        else state.NumberDisplayGrouping=DisplayNumbers.Grouping;
        if(state.OfficeGraphicsEnlarged is { } enlarged)_officeGraphicScale=enlarged?2:1;
        else state.OfficeGraphicsEnlarged=_officeGraphicScale==2;
        DestroyDesktop();
        foreach (Node child in _screenContent.GetChildren()) { _screenContent.RemoveChild(child); child.QueueFree(); }
        _screen.Hide(); _desktopState = state; _desktopLocale = TranslationServer.GetLocale();
        _music.SetGameplayActive(true);
        _desktop = new Control { Name = "GameDesktop", MouseFilter = MouseFilterEnum.Ignore };
        _desktop.Theme=new Theme {DefaultFont=new SystemFont {FontNames=["MS UI Gothic","MS Gothic","Noto Sans CJK JP","sans-serif"]},DefaultFontSize=13};
        _desktop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_desktop);
        var layout = new VBoxContainer(); layout.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        layout.AddThemeConstantOverride("separation", 0); _desktop.AddChild(layout);
        var menus = new HBoxContainer { CustomMinimumSize = new Vector2(0, 24) }; layout.AddChild(menus);
        OriginalDesktopMenus(menus,state);

        var toolbar = new HBoxContainer { Name = "DesktopToolbar", CustomMinimumSize = new Vector2(0,24) };
        toolbar.AddThemeConstantOverride("separation",1);layout.AddChild(toolbar);
        void Gap()=>toolbar.AddChild(new Control {CustomMinimumSize=new(7,0)});
        ToolbarAction(toolbar,"SAVE_GAME",SaveDesktop,15);
        _desktopLoad=ToolbarAction(toolbar,"LOAD_GAME",LoadSession,14,System.IO.File.Exists(SavePath));
        Gap();
        ToolbarAction(toolbar,"PUBLIC_RELATIONS",()=>BeginCompanyActivity(state,CompanyActivity.PublicRelations),2);
        ToolbarAction(toolbar,"GENRE_RESEARCH",()=>BeginCompanyActivity(state,CompanyActivity.GenreResearch),1);
        ToolbarAction(toolbar,"EXPAND_PRODUCTION",()=>BeginCompanyActivity(state,CompanyActivity.ProductionExpansion),0);
        Gap();
        ToolbarAction(toolbar,"HIRE_EMPLOYEES",ShowRecruitment,13,state.Recruitment.CandidateDatabaseDecoded);
        ToolbarAction(toolbar,"TECHNOLOGIES",()=>ShowTechnologies(state),12,_activeCatalog?.Technologies.Count>0);
        ToolbarAction(toolbar,"BANK",()=>ShowBank(state),11);
        ToolbarAction(toolbar,"DEVELOPMENT_COMPUTERS",()=>ShowComputers(state),10,_activeCatalog?.DevelopmentComputers.Count==4);
        ToolbarAction(toolbar,"HARDWARE_LICENSES",()=>ShowLicenses(state),9,state.AvailableHardware is not null && state.HardwareLicenses is not null);
        ToolbarAction(toolbar,"HIRE_SECRETARY",()=>ShowSecretaryHiring(state),8);
        Gap();
        ToolbarAction(toolbar,"STAFF",()=>ShowStaff(state),7);
        ToolbarAction(toolbar,"SALES",()=>ShowProductList(state),6,_activeCatalog is not null);
        ToolbarAction(toolbar,"SALES_CHART",()=>ShowSalesChart(state),5);
        Gap();
        ToolbarAction(toolbar,"GAME_HELP",()=>ShowHelp(state),3);
        toolbar.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        _desktopFunds = new Label { Name = "DesktopFunds", CustomMinimumSize = new Vector2(285, 0),
            HorizontalAlignment = HorizontalAlignment.Right,SizeFlagsVertical=SizeFlags.ShrinkCenter };
        var fundsBox=new StyleBoxFlat {BgColor=Colors.White,BorderColor=new(.5f,.5f,.5f),ContentMarginLeft=4,ContentMarginRight=4,ContentMarginTop=1,ContentMarginBottom=1};fundsBox.SetBorderWidthAll(1);
        _desktopFunds.AddThemeStyleboxOverride("normal",fundsBox);_desktopFunds.AddThemeFontSizeOverride("font_size",12);toolbar.AddChild(_desktopFunds);
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
        var office = ChildWindow("OfficeWindow", state.Company.Name, new Vector2(4, 4), new Vector2(472, 320), false);
        _officeRenderer = OriginalAssetStore.LoadOfficeRenderer();
        _officeTexture = _officeRenderer?.CreateTexture(state);
        var texture = (Texture2D?)_officeTexture ?? OriginalAssetStore.LoadTexture("Graphic/Embedded/OfficeBase.bmp");
        if (texture is not null)
        {
            var officeImage = new TextureRect { Name = "OriginalOfficeBackground", Texture = texture,
                TextureFilter = TextureFilterEnum.Nearest, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(466*_officeGraphicScale,290*_officeGraphicScale), TooltipText = Tr("OFFICE_EMPLOYEE_HELP") };
            officeImage.GuiInput += input => OnOfficeInput(state,officeImage,input);
            var viewport=new ScrollContainer {Name="OfficeViewport",SizeFlagsHorizontal=SizeFlags.ExpandFill,SizeFlagsVertical=SizeFlags.ExpandFill};
            office.Body.AddChild(viewport);viewport.AddChild(officeImage);
        }
        else office.Body.AddChild(new Label { Text = Tr("OFFICE_IMPORT_REQUIRED"), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        var company = ChildWindow("CompanyWindow", Tr("COMPANY_WINDOW"), new Vector2(480, 4), new Vector2(240, 330));
        BuildCompanySummary(company);
        var staff = ChildWindow("EmployeeWindow", Tr("EMPLOYEE_WINDOW"), new Vector2(720, 130), new Vector2(390, 150));
        if (state.FinancialFailure is null && !state.Casino.ActivityOpen) OriginalPayrollRules.CaptureStaffList(state);
        _staffTable = CreateTable("DesktopEmployeeTable", ["EMPLOYEE_NAME", "EMPLOYEE_LEVEL", "SHORT_FUN", "SHORT_NOVELTY", "SHORT_GRAPHICS",
            "SHORT_MUSIC", "SHORT_ACCURACY", "SHORT_SPEED", "SHORT_INSPIRATION", "SHORT_EXPERIENCE"], [86, 32, 22, 22, 22, 22, 22, 22, 22, 50]);
        staff.EnableVerticalResize();
        staff.Body.AddChild(_staffTable); _staffTable.ItemActivated += () =>
        {
            if (_dialog is not null && !_clockRunning) return;
            var selected = _staffTable.GetSelected();
            if (selected is null) return;
            int index = _staffRows.IndexOf(selected);
            if (index >= 0 && index < state.Employees.Count) ShowEmployeeDetails(state,state.Employees[index]);
        };
        var products = ChildWindow("ProductWindow", Tr("PRODUCT_WINDOW"), new Vector2(30, 356), new Vector2(390, 150));
        _productTable = CreateTable("DesktopProductTable", ["GAME_TITLE", "RETAIL_PRICE", "UNITS_SOLD", "STOCK"], [134, 65, 70, 70]);
        products.EnableVerticalResize();
        _productTable.ScrollHorizontalEnabled=false;
        _productTable.AddThemeFontSizeOverride("title_button_font_size",10);
        StyleSelection(_productTable,new(1,.68f,1));
        var instruction=new Label {Text=Tr("PRODUCT_LIST_INSTRUCTION"),HorizontalAlignment=HorizontalAlignment.Center};instruction.AddThemeFontSizeOverride("font_size",10);products.Body.AddChild(instruction);
        products.Body.AddChild(_productTable); _productTable.ItemActivated += () =>
        {
            if(!DesktopActionEnabled("SALES"))return;
            var selected=_productTable.GetSelected();
            var product=selected is null?null:state.ReleasedGames.FirstOrDefault(p=>p.ProductIndex==selected.GetMetadata(0).AsInt32());
            if(product is null)ShowSales(state);else ShowCompactProduct(state,product);
        };
        _desktopStatus = new Label { Name = "OfficeStatus", CustomMinimumSize = new Vector2(0, 22), ClipText = true };
        layout.AddChild(_desktopStatus);
        RefreshDesktop();
    }

    private void OnOfficeInput(GameState state,TextureRect image,InputEvent input)
    {
        if (image.Texture is null || _dialog is not null && !_clockRunning) return;
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
        if(input is InputEventMouseButton {ButtonIndex:MouseButton.Right,Pressed:true})
        {
            if(_selectedEmployee is { } selected && state.Employees.Contains(selected) && !CalendarService.HasPending(state))
            {
                OfficeAssignmentResult? result=null;
                if(OfficeInteractionRules.StudyDeskAt(pixel.X,pixel.Y) is int study)
                    result=OfficeAssignmentService.TryAssignStudySeat(state,selected,study);
                else if(OfficeInteractionRules.DeskAt(pixel.X,pixel.Y) is int desk)
                    result=OfficeAssignmentService.TryAssignDeskFromOffice(state,selected,desk);
                else if(OfficeInteractionRules.VendingAt(pixel.X,pixel.Y))
                    result=OfficeAssignmentService.TryTakeBreak(state,selected,_session.Random);
                if(result is { } outcome)
                {image.AcceptEvent();RefreshDesktop();RefreshEmployeeDetails();if(_officeRenderer is not null&&_officeTexture is not null)_officeRenderer.UpdateTexture(_officeTexture,state);
                    if(_desktopStatus is not null)_desktopStatus.Text=Tr($"ASSIGNMENT_RESULT_{outcome}");}
            }
            return;
        }
        var student=OfficeInteractionRules.StudyComputerEmployeeAt(state,pixel.X,pixel.Y);
        if(student is not null && input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true})
        {
            image.AcceptEvent();
            if(CalendarService.HasPending(state))ShowPendingCalendarEvent(state);
            else if(DesktopActionEnabled("TRAINING"))ShowTraining(state,student);
            return;
        }
        var employee = OfficeInteractionRules.EmployeeAt(state,pixel.X,pixel.Y);
        if(employee is null && input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true} &&
            OfficeInteractionRules.TrainingAreaAt(pixel.X,pixel.Y))
        {image.AcceptEvent();ShowCompanyResearch(state);return;}
        var room=OfficeInteractionRules.DevelopmentRoomAt(pixel.X,pixel.Y);
        var tenants=OfficeInteractionRules.TenantSlotsAt(pixel.X,pixel.Y);
        bool secretary=OfficeInteractionRules.SecretaryAt(pixel.X,pixel.Y);
        bool deskTarget=_selectedEmployee is not null && OfficeInteractionRules.DeskAt(pixel.X,pixel.Y) is not null;
        image.TooltipText=OfficeHoverText(state,pixel,employee,room,tenants);
        image.MouseDefaultCursorShape = student is null && !OfficeInteractionRules.TrainingAreaAt(pixel.X,pixel.Y) && employee is null && room is null && tenants.Count==0 && !secretary && !deskTarget ? CursorShape.Arrow : CursorShape.PointingHand;
        if(employee is not null && input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true})
        {
            image.AcceptEvent();ShowEmployeeDetails(state,employee);return;
        }
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
        if (input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true}) CloseEmployeeDetails();
    }

    private PopupMenu Menu(HBoxContainer parent, string title, (string Key, Action Action)[] actions)
    {
        var menu = new MenuButton { Name = title, Text = Tr(title) + " " }; parent.AddChild(menu);
        FillMenu(menu.GetPopup(),actions);
        return menu.GetPopup();
    }

    private void FillMenu(PopupMenu popup,(string Key,Action Action)[] actions)
    {
        for(int i=0;i<actions.Length;i++)
            if(actions[i].Key.Length==0)popup.AddSeparator("",i);else popup.AddItem(Tr(actions[i].Key),i);
        popup.AboutToPopup+=()=>
        {
            for(int i=0;i<actions.Length;i++)
                if(actions[i].Key.Length>0)popup.SetItemDisabled(i,!DesktopActionEnabled(actions[i].Key));
        };
        popup.IdPressed+=id=>
        {
            if(id>=0 && id<actions.Length && actions[(int)id].Key.Length>0 && DesktopActionEnabled(actions[(int)id].Key))actions[(int)id].Action();
        };
    }

    private void MenuChild(PopupMenu parent,int index,string name,(string Key,Action Action)[] actions)
    {
        var child=new PopupMenu {Name=name};parent.AddChild(child);FillMenu(child,actions);parent.SetItemSubmenuNode(index,child);
    }

    private bool DesktopActionEnabled(string key)
    {
        if((_facilityOwner is not null || _facilityChild is not null) && key is not ("SAVE_GAME" or "LOAD_GAME" or "SETTINGS" or "BACK_TO_TITLE"))return false;
        if (_desktopState?.Casino.ActivityOpen==true && key is not ("SAVE_GAME" or "LOAD_GAME" or "BACK_TO_TITLE")) return false;
        if (_desktopState?.FinancialFailure is not null && key is not ("SAVE_GAME" or "LOAD_GAME" or "BACK_TO_TITLE")) return false;
        if (_desktopState is { } failed && FinancialFailureService.IsComplete(failed) && key=="SAVE_GAME") return false;
        // Original weekly forms are modal. Modern save/load/settings remain
        // accessible, but another action cannot mutate a suspended transaction.
        if (_desktopState is GameState current && (CompanyEventService.HasPending(current) || current.PendingCompanyActivity is not null || ProgrammingChallengeService.HasPending(current) || current.ReleasedGames.Any(p=>p.HallCelebration is not null) || current.PendingPromotion is not null || current.PendingBankLoanReceipt is not null || CalendarService.HasPending(current) || current.PendingHireGreeting is not null || current.PendingSecretaryAdvice is not null || current.PendingSecretaryReplacement is not null || current.PendingProjectCancellation is not null || (current.PendingFacilityReceipt is not null || current.PendingFeeNegotiation is not null || OwnHardwareService.HasPending(current) || current.PendingMysteryLabResult is not null)) &&
            key is not ("SAVE_GAME" or "LOAD_GAME" or "SETTINGS" or "BACK_TO_TITLE")) return false;
        return key switch
        {
        "LOAD_GAME" => SaveSlotsExist(),
        "START_GAME_DEVELOPMENT" or "DEVELOPMENT_PLANNING" or "SALES" => _activeCatalog is not null,
        "HIRE_EMPLOYEES" => _desktopState?.Recruitment.CandidateDatabaseDecoded == true,
        "DEVELOPMENT_COMPUTERS" => _activeCatalog?.DevelopmentComputers.Count == 4,
        "TECHNOLOGIES" => _activeCatalog?.Technologies.Count > 0,
        "HARDWARE_MARKET" => _activeCatalog?.Hardware.Count>0,
        "OWN_HARDWARE_REPORT" or "NATIVE_OWN_CONSOLES" => _desktopState?.OwnHardware.Launched is not null,
        "HARDWARE_LICENSES" => _desktopState?.AvailableHardware is not null && _desktopState.HardwareLicenses is not null,
        "BANK" => _desktopState is { } bankState && BankLoanService.CheckBorrow(bankState) is BankLoanResult.Success or BankLoanResult.AllSlotsOccupied,
        "COMPANY_ACTIVITIES" => _desktopState?.Company.ActivityCost is not null,
        "PUBLIC_RELATIONS" or "GENRE_RESEARCH" or "EXPAND_PRODUCTION" => _desktopState is {Company.ActivityCost:not null,CompanyActivityPerformed:false},
        _ => true
        };
    }

    public bool GetSimulationPaused()=>_paused;
    public bool SetSimulationPaused(bool paused)
    {
        if(!DesktopActionEnabled("PAUSE"))return false;
        _paused=paused;RefreshDesktop();_refreshCoreStatus?.Invoke();return true;
    }

    private Button ToolbarAction(HBoxContainer parent, string key, Action action, int? icon, bool enabled = true)
    {
        var button = new Button { Name = $"Toolbar{key}", Text = "", TooltipText = Tr(key), IconAlignment=HorizontalAlignment.Center, Disabled = !enabled, ClipText = true,
            CustomMinimumSize = new Vector2(23,22), SizeFlagsVertical=SizeFlags.ShrinkCenter, FocusMode = FocusModeEnum.All };
        button.TextureFilter = TextureFilterEnum.Nearest;
        foreach(string mode in new[]{"normal","pressed","hover","disabled"})button.AddThemeStyleboxOverride(mode,new StyleBoxEmpty());
        var focus=new StyleBoxFlat {BgColor=Colors.Transparent,BorderColor=new(.15f,.24f,.45f),ContentMarginLeft=0,ContentMarginRight=0,ContentMarginTop=0,ContentMarginBottom=0};focus.SetBorderWidthAll(1);button.AddThemeStyleboxOverride("focus",focus);
        button.AddThemeFontSizeOverride("font_size", 1);
        foreach (string color in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color", "font_disabled_color" })
            button.AddThemeColorOverride(color, Colors.Transparent);
        if (icon is int id) button.Icon = OriginalAssetStore.LoadTexture($"Graphic/Embedded/Toolbar{id}.bmp");
        if (icon is not null && button.Icon is null) _toolbarArtworkMissing = true;
        button.Icon ??= ToolbarArtwork.CreateFallback(key);
        button.MouseEntered += () => { _toolbarHelp = button.TooltipText; RefreshDesktop(); };
        button.MouseExited += () => { _toolbarHelp = string.Empty; RefreshDesktop(); };
        button.FocusEntered += () => { _toolbarHelp = button.TooltipText; RefreshDesktop(); };
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
        RefreshRetainedProjectReports(state);
        if (_desktopLoad is not null) _desktopLoad.Disabled = !System.IO.File.Exists(SavePath);
        foreach (var button in _desktop!.FindChildren("Toolbar*","Button",true,false).OfType<Button>())
            button.Disabled=!button.GetMeta("base_enabled").AsBool() || !DesktopActionEnabled(button.Name.ToString()[7..]);
        _desktopFunds.Text = $"{string.Format(DisplayNumbers.Provider,Tr("TOOLBAR_YEAR"),state.Year?.ToString()??"?")}   {Tr("FUNDS")}: ¥{DisplayNumbers.Format(state.Money)}";
        RefreshCompanySummary(state);
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
                employee.Experience?.ToDisplayNumber()?? "?"];
            for (int col = 0; col < values.Length; col++) _staffRows[index].SetText(col, values[col]);
        }
        int? selectedProductId=_productTable.GetSelected()?.GetMetadata(0).AsInt32();
        SyncRows(_productTable, _productRows, state.ReleasedGames.Count);
        for (int index = 0; index < state.ReleasedGames.Count; index++)
        {
            var game = state.ReleasedGames[state.ReleasedGames.Count-1-index];_productRows[index].SetMetadata(0,game.ProductIndex); string[] values = [game.Title, game.Price.ToDisplayNumber(), game.UnitsSold.ToDisplayNumber(), game.Stock.ToDisplayNumber()];
            for (int col = 0; col < values.Length; col++) _productRows[index].SetText(col, values[col]);
            if(game.ProductIndex==selectedProductId)_productRows[index].Select(0);
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
        if(!_closingProjectReport && _projectReportRoom is int room && _projectReportRefresh is Action refresh &&
            _session.Current?.DevelopmentProjects.Any(p=>p.RoomIndex==room)==true)
        {
            var report=_dialog;_retainedProjectReports[room]=(report,refresh);
            report.CloseRequested=()=>CloseRetainedProjectReport(room);
            _dialog=null;_projectReportRoom=null;_projectReportRefresh=null;_dialogPositionKey=null;return;
        }
        _workspace!.RemoveChild(_dialog); _dialog.QueueFree(); _dialog = null;
        _projectReportRoom=null;_projectReportRefresh=null;_dialogPositionKey=null;
    }

    private void CloseRetainedProjectReport(int room)
    {
        if(!_retainedProjectReports.Remove(room,out var retained))return;
        _dialogPositions["DevelopmentProjectWindow:"+room]=retained.Window.Position;
        retained.Window.GetParent()?.RemoveChild(retained.Window);retained.Window.QueueFree();
    }
    private void RefreshRetainedProjectReports(GameState state)
    {
        foreach(var pair in _retainedProjectReports.ToArray())
            if(state.DevelopmentProjects.Any(p=>p.RoomIndex==pair.Key))pair.Value.Refresh();
            else CloseRetainedProjectReport(pair.Key);
    }
    private void RememberConversationPosition()
    {
        if(_dialog is not null && _dialogPositionKey is string key)_dialogPositions[key]=_dialog.Position;
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
        if(_session.Current is GameState current)RefreshRetainedProjectReports(current);
        // Close behaves like the screen's existing explicit back action, including pending decisions.
        _dialog.CloseRequested = () => { if (_session.Current is GameState state) ShowOfficeReconstruction(state); };
        var scroll = new ScrollContainer { SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _dialog.Body.AddChild(scroll);
        _content = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(620, 0) };
        _content.AddThemeConstantOverride("separation", 5); scroll.AddChild(_content);
    }

    private void CompactManagementWindow(string name, Vector2 size, float contentWidth,string? positionKey=null)
    {
        _content.CustomMinimumSize = new Vector2(contentWidth, 0);
        if (_dialog is null || _workspace is null) return;
        _dialog.Size = size;
        // Prefer the free space beside the office, keeping the office visible.
        _dialog.Position = new Vector2(Math.Min(480, Math.Max(0, _workspace.Size.X - size.X)),
            Math.Min(36, Math.Max(0, _workspace.Size.Y - size.Y)));
        IdentifyManagementWindow(name,positionKey);
    }

    private void IdentifyManagementWindow(string name,string? positionKey=null)
    {
        if (_dialog is null) return;
        _dialog.Name=name;_dialog.SetNativeIcon(name);
        _dialogPositionKey=positionKey??(UsesConversationPosition(name)?null:name);
        if(_dialogPositionKey is string key && _dialogPositions.TryGetValue(key,out var saved))_dialog.SetPositionInsideWorkspace(saved);
        else if (UsesConversationPosition(name) && _conversationPosition is Vector2 position)
            _dialog.SetPositionInsideWorkspace(position);
        QueueCompactNotice(name);
    }
}
