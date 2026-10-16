using Godot;
using System;
using System.IO;
using OpenGDS2.Core;
using OpenGDS2.Persistence;

namespace OpenGDS2.UI;

public partial class Main
{
    private void BuildTitleMenu()
    {
        _content.CustomMinimumSize=new Vector2(600,0);
        _content.AddThemeConstantOverride("separation",8);
        var texture=OriginalAssetStore.LoadTexture("Graphic/Title.bmp");
        if(texture is not null)
            _content.AddChild(new TextureRect {Name="TitleArtwork",Texture=texture,
                ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,
                SizeFlagsHorizontal=SizeFlags.ShrinkCenter,CustomMinimumSize=new Vector2(558,385)});
        else AddHeading(Tr("APP_TITLE"),32);

        var primary=new HBoxContainer {Name="TitlePrimaryActions"};
        primary.AddThemeConstantOverride("separation",8);_content.AddChild(primary);
        TitleButton(primary,"NEW_GAME",()=>{if(_importer.HasImportedAssets())ShowCompanySetup();else ShowImporter();},height:30);
        TitleButton(primary,"LOAD_GAME",LoadSession,File.Exists(SavePath),30);
        TitleButton(primary,"SETTINGS",ShowSettings,height:30);

        var secondary=new GridContainer {Name="TitleSecondaryActions",Columns=2};
        secondary.AddThemeConstantOverride("h_separation",8);
        secondary.AddThemeConstantOverride("v_separation",6);_content.AddChild(secondary);
        TitleButton(secondary,"LOAD_SAVE_SLOTS",()=>ShowSaveSlots(null,true));
        TitleButton(secondary,"LOAD_PREVIOUS_SAVE",LoadPreviousSession,File.Exists(SaveStore.BackupPath(SavePath)),name:"LoadPreviousSave")
            .TooltipText=Tr("LOAD_PREVIOUS_SAVE_NOTE");
        TitleButton(secondary,"GAME_HELP",()=>ShowHelp());
        TitleButton(secondary,"GLOBAL_RANKINGS",()=>ShowRankings());
        TitleButton(secondary,"EARLY_ACCESS_SCOPE",ShowEarlyAccessScope);
        TitleButton(secondary,"IMPORT_BUTTON",ShowImporter);

        var footer=new HBoxContainer {Name="TitleFooter"};_content.AddChild(footer);
        var quit=TitleButton(footer,"QUIT",()=>GetTree().Quit());
        quit.SizeFlagsHorizontal=SizeFlags.ShrinkBegin;quit.CustomMinimumSize=new Vector2(120,26);
        footer.AddChild(new Label {Name="BuildVersion",Text=$"{Tr("EARLY_ACCESS_LABEL")} — {BuildInfo.Version}",
            SizeFlagsHorizontal=SizeFlags.ExpandFill,HorizontalAlignment=HorizontalAlignment.Right,
            VerticalAlignment=VerticalAlignment.Center});
        _content.AddChild(new Label {Name="EarlyAccessBoundary",Text=Tr("EARLY_ACCESS_BOUNDARY"),
            HorizontalAlignment=HorizontalAlignment.Center,AutowrapMode=TextServer.AutowrapMode.WordSmart});
        _status=new Label {Name="TitleStatus",HorizontalAlignment=HorizontalAlignment.Center,
            AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
    }

    private Button TitleButton(Container parent,string key,Action action,bool enabled=true,int height=26,string? name=null)
    {
        var button=new Button {Name=name??"Title"+key,Text=Tr(key),Disabled=!enabled,
            SizeFlagsHorizontal=SizeFlags.ExpandFill,CustomMinimumSize=new Vector2(0,height)};
        button.Pressed+=action;parent.AddChild(button);return button;
    }
}
