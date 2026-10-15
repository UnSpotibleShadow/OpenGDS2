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
    private void OriginalGameMenu(HBoxContainer parent,GameState state)
    {
        Menu(parent,"MENU_GAME",[("SAVE_GAME",()=>ShowSaveSlots(state,false)),("LOAD_GAME",()=>ShowSaveSlots(state,true)),
            ("NEW_GAME",()=>ShowCompanySetup(state)),("QUIT",()=>GetTree().Quit()),("BACK_TO_TITLE",ShowTitleScreen)]);
    }
    private void DirectCasino(GameState state,string game)
    {
        var result=game switch {"CASINO_POKER"=>CasinoPokerService.Open(state),"CASINO_BLACKJACK"=>CasinoBlackjackService.Open(state),_=>_session.OpenHorse(_activeCatalog)};
        if(result==CasinoResult.Success)
        {if(game=="CASINO_POKER")ShowPoker(state);else if(game=="CASINO_BLACKJACK")ShowBlackjack(state);else ShowHorseRacing(state);}
        else ShowConversation(Tr(game),[new(SecretaryCaption(state.SecretarySourceIndex),Tr("CASINO_RESULT_"+result))],()=>ShowOfficeReconstruction(state));
    }
    private void OriginalGoOutMenu(HBoxContainer parent,GameState state)
    {
        var company=parent.GetChildren().OfType<MenuButton>().Single(m=>m.Name=="MENU_COMPANY").GetPopup();
        var menu=new PopupMenu {Name="OriginalGoOutMenu"};company.AddChild(menu);company.AddSubmenuNodeItem(Tr("GO_OUT"),menu);
        foreach(string key in new[]{"CASINO_HORSE","CASINO_POKER","CASINO_BLACKJACK"})
        {int id=menu.ItemCount;menu.AddItem(Tr(key),id);}
        menu.IdPressed+=id=>{string key=new[]{"CASINO_HORSE","CASINO_POKER","CASINO_BLACKJACK"}[(int)id];if(DesktopActionEnabled(key))DirectCasino(state,key);};
        menu.AboutToPopup+=()=>{for(int i=0;i<3;i++)menu.SetItemDisabled(i,!DesktopActionEnabled("CASINO_POKER"));};
    }
}
