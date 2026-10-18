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
        sound.AboutToPopup+=()=>{sound.SetItemChecked(0,_effects.Enabled&&_music.Enabled);sound.SetItemChecked(1,!_effects.Enabled&&!_music.Enabled);};
        sound.IdPressed+=id=>{_effects.SetEnabled(id==0);_music.SetEnabled(id==0);};
        var effects=Section("EffectsMenu","ORIGINAL_EFFECTS_SETTINGS");
        effects.AddCheckItem(Tr("ORIGINAL_EFFECTS_ENABLED"),0);
        effects.AboutToPopup+=()=>effects.SetItemChecked(0,_effects.Enabled);
        effects.IdPressed+=_=>_effects.SetEnabled(!_effects.Enabled);
        var music=Section("MusicMenu","MUSIC_SETTINGS");music.AddCheckItem(Tr("MUSIC_ENABLED"),0);
        music.AboutToPopup+=()=>music.SetItemChecked(0,_music.Enabled);
        music.IdPressed+=_=>_music.SetEnabled(!_music.Enabled);
        popup.AddItem(Tr("AUDIO_VOLUME"),100);
        popup.AddCheckItem(Tr("UPDATE_CHECKS_ENABLED"),101);
        popup.AboutToPopup+=()=>popup.SetItemChecked(popup.GetItemIndex(101),UpdateChecksEnabled());
        popup.IdPressed+=id=>{if(id==101)SetUpdateChecksEnabled(!UpdateChecksEnabled());};
        popup.IdPressed+=id=>{if(id==100)ShowAudioVolume();};
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
            ("HARDWARE_LICENSES",()=>ShowLicenses(state)),("TECHNOLOGIES",()=>ShowTechnologies(state)),("GO_OUT",()=>{})]);
        MenuChild(actions,0,"OriginalCompanyDevelopmentMenu",[("PUBLIC_RELATIONS",()=>BeginCompanyActivity(state,CompanyActivity.PublicRelations)),
            ("GENRE_RESEARCH",()=>BeginCompanyActivity(state,CompanyActivity.GenreResearch)),("EXPAND_PRODUCTION",()=>BeginCompanyActivity(state,CompanyActivity.ProductionExpansion))]);
        MenuChild(actions,13,"OriginalGoOutMenu",[("CASINO_HORSE",()=>DirectCasino(state,"CASINO_HORSE")),
            ("CASINO_POKER",()=>DirectCasino(state,"CASINO_POKER")),("CASINO_BLACKJACK",()=>DirectCasino(state,"CASINO_BLACKJACK"))]);
        Menu(parent,"MENU_INFORMATION",[("STAFF",()=>ShowStaff(state)),("NATIVE_PRODUCTS",()=>ShowProductList(state)),
            ("NATIVE_TOP100",()=>ShowSalesChart(state)),("NATIVE_PLATFORMS",()=>ShowHardwareMarket(state)),separator,
            ("NATIVE_OWN_CONSOLES",()=>ShowOwnHardwareReport(state)),separator,("NATIVE_TIME_RECORDS",()=>ShowRankings(state))]);
        var windows=Menu(parent,"MENU_WINDOWS",[("OFFICE_WINDOW",()=>_windows["OfficeWindow"].Reopen()),
            ("COMPANY_WINDOW",()=>_windows["CompanyWindow"].Reopen()),("OTHER_WINDOWS",()=>{})]);
        var other=new PopupMenu {Name="OtherWindowsMenu"};windows.AddChild(other);windows.SetItemSubmenuNode(2,other);
        void ListWindows()
        {
            other.Clear();
            var children=(_workspace?.GetChildren().OfType<DesktopWindow>()??Enumerable.Empty<DesktopWindow>()).ToArray();
            var active=children.LastOrDefault(w=>w.Visible);
            foreach(var window in children.OrderBy(w=>w.CreationOrder))
            {int id=other.ItemCount;other.AddCheckItem($"{id+1} {window.Caption}",id);other.SetItemChecked(id,window==active);other.SetItemMetadata(id,window.GetInstanceId());}
        }
        other.AboutToPopup+=ListWindows;
        other.IdPressed+=id=>{if(id>=0&&id<other.ItemCount&&GodotObject.InstanceFromId(other.GetItemMetadata((int)id).AsUInt64()) is DesktopWindow window)window.Reopen();};
        ListWindows();
        OriginalSettingsMenu(parent);
        Menu(parent,"MENU_HELP",[("GAME_HELP",()=>ShowHelp(state)),separator,("ABOUT_GAME",()=>ShowAbout(state))]);
    }
    private void ShowAudioVolume()
    {
        var canvas=ManagementCanvas("AudioVolumeWindow",Tr("AUDIO_VOLUME"),new(280,160));
        int y=8;
        foreach(var music in new[]{false,true})
        {
            NativeText(canvas,music?"MusicVolumeLabel":"EffectsVolumeLabel",Tr(music?"MUSIC_VOLUME":"EFFECTS_VOLUME"),new(8,y,264,22),11);
            var slider=new HSlider {Name=music?"MusicVolume":"EffectsVolume",MinValue=0,MaxValue=100,Step=1,Value=music?_music.VolumePercent:_effects.VolumePercent};
            NativePlace(canvas,slider,new(8,y+24,264,24));
            slider.ValueChanged+=v=>{if(music)_music.SetVolume((float)v);else _effects.SetVolume((float)v);slider.TooltipText=$"{v:0}%";};
            y+=52;
        }
        NativeAction(canvas,"CloseAudioVolume","CLOSE",new(90,126,100,26),()=>{if(_desktopState is { } state)ShowOfficeReconstruction(state);});
    }
    private void DirectCasino(GameState state,string game)
    {
        var result=game switch {"CASINO_POKER"=>CasinoPokerService.Open(state),"CASINO_BLACKJACK"=>CasinoBlackjackService.Open(state),_=>_session.OpenHorse(_activeCatalog)};
        if(result==CasinoResult.Success)
        {if(game=="CASINO_POKER")ShowPoker(state);else if(game=="CASINO_BLACKJACK")ShowBlackjack(state);else ShowHorseRacing(state);}
        else ShowConversation(Tr(game),[new(SecretaryCaption(state.SecretarySourceIndex),Tr("CASINO_RESULT_"+result))],()=>ShowOfficeReconstruction(state));
    }
}
