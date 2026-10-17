using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private int _officeGraphicScale=1;
    private void SaveDisplayPreferences()
    {
        var config=new ConfigFile();config.Load(InterfaceSettingsPath);
        config.SetValue("display","office_scale",_officeGraphicScale);
        config.SetValue("display","number_grouping",(int)DisplayNumbers.Grouping);
        if(config.Save(InterfaceSettingsPath)!=Error.Ok)GD.PushWarning("Display preferences could not be saved.");
    }
    private void OriginalSettingsMenu(HBoxContainer parent)
    {
        var menu=new MenuButton {Name="MENU_SETTINGS",Text=Tr("SETTINGS")+" "};parent.AddChild(menu);
        var popup=menu.GetPopup();
        PopupMenu Section(string name,string key)
        {var child=new PopupMenu {Name=name};popup.AddChild(child);popup.AddSubmenuNodeItem(Tr(key),child);return child;}
        var sound=Section("OriginalSoundMenu","ORIGINAL_SOUND");sound.AddRadioCheckItem(Tr("SOUND_ON"),0);sound.AddRadioCheckItem(Tr("SOUND_OFF"),1);
        sound.AboutToPopup+=()=>{sound.SetItemChecked(0,_effects.Enabled);sound.SetItemChecked(1,!_effects.Enabled);};
        sound.IdPressed+=id=>_effects.SetEnabled(id==0);
        var graphics=Section("OriginalGraphicsMenu","ORIGINAL_GRAPHIC_SIZE");graphics.AddRadioCheckItem(Tr("GRAPHICS_NORMAL"),1);graphics.AddRadioCheckItem(Tr("GRAPHICS_ENLARGED"),2);
        graphics.AboutToPopup+=()=>{graphics.SetItemChecked(0,_officeGraphicScale==1);graphics.SetItemChecked(1,_officeGraphicScale==2);};
        graphics.IdPressed+=id=>{_officeGraphicScale=id==2?2:1;if(_desktopState is { } state)state.OfficeGraphicsEnlarged=_officeGraphicScale==2;SaveDisplayPreferences();ApplyOfficeGraphicScale();};
        var numbers=Section("OriginalNumberMenu","ORIGINAL_NUMBER_DISPLAY");numbers.AddRadioCheckItem(Tr("NUMBER_JAPANESE"),0);numbers.AddRadioCheckItem(Tr("NUMBER_THOUSANDS"),1);
        numbers.AboutToPopup+=()=>{numbers.SetItemChecked(0,DisplayNumbers.Grouping==NumberGrouping.Japanese);numbers.SetItemChecked(1,DisplayNumbers.Grouping==NumberGrouping.Thousands);};
        numbers.IdPressed+=id=>{DisplayNumbers.Grouping=id==0?NumberGrouping.Japanese:NumberGrouping.Thousands;if(_desktopState is { } state)state.NumberDisplayGrouping=DisplayNumbers.Grouping;SaveDisplayPreferences();RefreshDesktop();};
    }
    private void ApplyOfficeGraphicScale()
    {
        if(_desktop?.FindChild("OriginalOfficeBackground",true,false) is TextureRect image)
        {
            image.CustomMinimumSize=new(466*_officeGraphicScale,290*_officeGraphicScale);
            image.Size=image.CustomMinimumSize;
        }
        if(_desktop?.FindChild("OfficeViewport",true,false) is ScrollContainer viewport)
        {viewport.ScrollHorizontal=0;viewport.ScrollVertical=0;}
    }
    private void OriginalDesktopMenus(HBoxContainer parent,GameState state)
    {
        var separator=("",(Action)(()=>{}));
        Menu(parent,"MENU_GAME",[("NEW_GAME",()=>ShowCompanySetup(state)),separator,
            ("SAVE_GAME",()=>ShowSaveSlots(state,false)),("LOAD_GAME",()=>ShowSaveSlots(state,true)),separator,
            ("QUIT",()=>GetTree().Quit()),separator,("BACK_TO_TITLE",ShowTitleScreen)]);
        var actions=Menu(parent,"MENU_ACTIONS",[("COMPANY_DEVELOPMENT",()=>{}),separator,
            ("HIRE_EMPLOYEES",ShowRecruitment),("POACH_EMPLOYEE",()=>ShowPoachingSelection(state)),separator,
            ("SECRETARY_POLICIES",()=>ShowSecretaryPolicies(state)),("HIRE_SECRETARY",()=>ShowSecretaryHiring(state)),separator,
            ("BANK",()=>ShowBank(state)),("DEVELOPMENT_COMPUTERS",()=>ShowComputers(state)),("BUY_BACKUP_DEVICE",()=>BeginBackupDevice(state)),
            ("HARDWARE_LICENSES",()=>ShowLicenses(state)),("TECHNOLOGIES",()=>ShowTechnologies(state)),("GO_OUT",()=>{}),separator,("ADDITIONAL_ACTIONS",()=>{})]);
        MenuChild(actions,0,"OriginalCompanyDevelopmentMenu",[("PUBLIC_RELATIONS",()=>BeginCompanyActivity(state,CompanyActivity.PublicRelations)),
            ("GENRE_RESEARCH",()=>BeginCompanyActivity(state,CompanyActivity.GenreResearch)),("EXPAND_PRODUCTION",()=>BeginCompanyActivity(state,CompanyActivity.ProductionExpansion))]);
        MenuChild(actions,13,"OriginalGoOutMenu",[("CASINO_HORSE",()=>DirectCasino(state,"CASINO_HORSE")),
            ("CASINO_POKER",()=>DirectCasino(state,"CASINO_POKER")),("CASINO_BLACKJACK",()=>DirectCasino(state,"CASINO_BLACKJACK"))]);
        // Retain existing port navigation separately from the recovered native entries.
        MenuChild(actions,15,"AdditionalActionsMenu",[("START_GAME_DEVELOPMENT",()=>ShowPlanningPreview(state)),("SECRETARY_ADVICE",()=>ShowSecretaryAdvice(state))]);
        var information=Menu(parent,"MENU_INFORMATION",[("STAFF",()=>ShowStaff(state)),("SALES",()=>ShowProductList(state)),
            ("SALES_CHART",()=>ShowSalesChart(state)),("HARDWARE_MARKET",()=>ShowHardwareMarket(state)),separator,
            ("OWN_HARDWARE_REPORT",()=>ShowOwnHardwareReport(state)),separator,("GLOBAL_RANKINGS",()=>ShowRankings(state)),separator,("PRODUCT_DETAILS",()=>{})]);
        MenuChild(information,9,"ProductDetailsMenu",[("SALES_HISTORY",()=>ShowSelectedProductHistory(state)),("MAGAZINE_REVIEWS",()=>ShowSelectedProductReviews(state))]);
        var windows=Menu(parent,"MENU_WINDOWS",[("OFFICE_WINDOW",()=>_windows["OfficeWindow"].Reopen()),
            ("COMPANY_WINDOW",()=>_windows["CompanyWindow"].Reopen()),("OTHER_WINDOWS",()=>{})]);
        MenuChild(windows,2,"OtherWindowsMenu",[("EMPLOYEE_WINDOW",()=>{OriginalPayrollRules.CaptureStaffList(state);_windows["EmployeeWindow"].Reopen();}),
            ("PRODUCT_WINDOW",()=>_windows["ProductWindow"].Reopen())]);
        OriginalSettingsMenu(parent);
        Menu(parent,"MENU_HELP",[("GAME_HELP",()=>ShowHelp(state)),separator,("ABOUT_GAME",()=>ShowAbout(state))]);
    }
    private void DirectCasino(GameState state,string game)
    {
        var result=game switch {"CASINO_POKER"=>CasinoPokerService.Open(state),"CASINO_BLACKJACK"=>CasinoBlackjackService.Open(state),_=>_session.OpenHorse(_activeCatalog)};
        if(result==CasinoResult.Success)
        {if(game=="CASINO_POKER")ShowPoker(state);else if(game=="CASINO_BLACKJACK")ShowBlackjack(state);else ShowHorseRacing(state);}
        else ShowConversation(Tr(game),[new(SecretaryCaption(state.SecretarySourceIndex),Tr("CASINO_RESULT_"+result))],()=>ShowOfficeReconstruction(state));
    }
}
