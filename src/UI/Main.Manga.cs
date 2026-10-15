using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string MangaText(OriginalCatalog catalog,int source,string field)
    {
        var manga=catalog.Manga.Single(x=>x.SourceIndex==source);
        string original=field switch {"TITLE"=>manga.Title,"AUTHOR"=>manga.Author,"PUBLISHER"=>manga.Publisher,_=>manga.PopularityDescription};
        return CatalogName(catalog,$"MANGA_{field}",source,original);
    }
    private void ShowMangaSelection(GameState state,int room)
    {
        var catalog=_importer.LoadCatalog()??throw new InvalidOperationException("No local catalog.");
        ClearContent();var canvas=NativeWindow("MangaSelectionWindow",Tr("MANGA_ADAPTATION"),new(408,278));
        NativeFill(canvas,"MangaQuestionStrip",new(4,3,400,19),Colors.White,true);
        NativeText(canvas,"MangaQuestion",Tr("MANGA_WHICH_TITLE"),new(6,3,396,19),10,HorizontalAlignment.Center);
        if(catalog.Manga.Count==0)
        {
            NativeText(canvas,"MangaMissing",Tr("MANGA_REIMPORT_REQUIRED"),new(4,28,400,170),11).AutowrapMode=TextServer.AutowrapMode.WordSmart;
            NativeAction(canvas,"CancelManga","CANCEL",new(302,247,102,25),()=>ShowDevelopmentRoomActions(state,room));return;
        }
        var table=ManagementTable(canvas,"MangaTitles",["GAME_TITLE","POPULARITY","CONTRACT_FEE"],[190,88,106],new(4,27,400,148));
        StyleSelection(table,new(1,.7f,1));
        foreach(var manga in catalog.Manga)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,manga.SourceIndex);
            row.SetText(0,MangaText(catalog,manga.SourceIndex,"TITLE"));row.SetText(1,MangaText(catalog,manga.SourceIndex,"POPULARITY"));
            row.SetText(2,manga.RightsFee.ToDisplayNumber());row.SetTextAlignment(2,HorizontalAlignment.Right);
        }
        int selected=catalog.Manga[0].SourceIndex;
        string[] keys=["GAME_TITLE","POPULARITY","CONTRACT_FEE","AUTHOR","PUBLISHER"];
        var values=new Label[5];
        for(int i=0;i<5;i++)
        {
            int x=i<3?4:216,y=181+(i<3?i:i-3)*19;
            NativeText(canvas,"MangaCaption"+i,Tr(keys[i]),new(x,y,i<3?72:64,18),10);
            NativeFill(canvas,"MangaValueCell"+i,new(x+(i<3?74:66),y,i<3?134:122,18),i<3?ClassicTheme.Face:new(.65f,1,1));
            values[i]=NativeText(canvas,"MangaDetails"+i,"",new(x+(i<3?76:68),y,i<3?130:118,18),10);
        }
        void Refresh()
        {
            values[0].Text=MangaText(catalog,selected,"TITLE");values[1].Text=MangaText(catalog,selected,"POPULARITY");
            values[2].Text="¥"+DisplayNumbers.Format(catalog.Manga.Single(x=>x.SourceIndex==selected).RightsFee);
            values[3].Text=MangaText(catalog,selected,"AUTHOR");values[4].Text=MangaText(catalog,selected,"PUBLISHER");
            foreach(var label in values)label.TooltipText=label.Text;
        }
        table.ItemSelected+=()=>{selected=table.GetSelected().GetMetadata(0).AsInt32();Refresh();};table.GetRoot().GetFirstChild().Select(0);Refresh();
        NativeAction(canvas,"ConfirmManga","MANGA_RIGHTS_MEETING",new(140,247,156,25),()=>ShowMangaRightsMeeting(state,catalog,room,selected));
        NativeAction(canvas,"CancelManga","CANCEL",new(302,247,102,25),()=>ShowDevelopmentRoomActions(state,room));
    }
    private void ShowMangaRightsMeeting(GameState state,OriginalCatalog catalog,int room,int source)
    {
        string title=MangaText(catalog,source,"TITLE"),publisher=MangaText(catalog,source,"PUBLISHER");
        string representative=DialogueText("MANGA_REPRESENTATIVE").Replace("{publisher}",publisher);
        string Text(string key)=>DialogueText(key).Replace("{publisher}",publisher).Replace("{title}",title);
        ShowConversation(Tr("MANGA_RIGHTS_MEETING"),Enumerable.Range(0,4).Select(i=>new ConversationPage(representative,Text($"MANGA_MEETING_{i}"),RandomDialogueSpeakerRules.Pick(state,_session.Random)?.PortraitId)).ToList(),Confirm,
            ()=>ShowMangaSelection(state,room),scene:OriginalSceneArtwork.Publisher());
        void Confirm()
        {
            ClearContent();CompactManagementWindow("MangaRightsWindow",new Vector2(330,180),310);
            _dialog!.SetTitle(Tr("MANGA_RIGHTS_MEETING"));
            _content.AddChild(new Label {Name="MangaRightsQuestion",Text=Text("MANGA_RIGHTS_QUESTION").Replace("{fee}",catalog.Manga.Single(x=>x.SourceIndex==source).RightsFee.ToDisplayNumber()),AutowrapMode=TextServer.AutowrapMode.WordSmart});
            var status=new Label {Name="MangaRightsStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(status);
            var agree=new Button {Name="AgreeMangaRights",Text=Tr("ACCEPT")};_content.AddChild(agree);
            agree.Pressed+=()=>
            {
                var result=DevelopmentProjectService.CheckMangaRights(state,catalog,source);
                if (result!=DevelopmentPlanningResult.Success) {status.Text=DialogueText("MANGA_NO_MONEY");return;}
                ShowConversation(Tr("MANGA_RIGHTS_MEETING"),[new(representative,Text("MANGA_RIGHTS_APPROVED"),RandomDialogueSpeakerRules.Pick(state,_session.Random)?.PortraitId),
                    new(DialogueText("MEETING_SECRETARY"),Text("MANGA_BEGIN_DEVELOPMENT"),SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex))],
                    ()=>ShowPlanningPreview(state,room,null,source),()=>ShowMangaSelection(state,room),scene:OriginalSceneArtwork.Publisher());
            };
            ShowMeetingScene(Tr("MANGA_RIGHTS_MEETING"),OriginalSceneArtwork.Publisher(),()=>ShowMangaSelection(state,room));
            AddAction("DECLINE",()=>ShowConversation(Tr("MANGA_RIGHTS_MEETING"),[new(representative,Text("MANGA_RIGHTS_DECLINED"),RandomDialogueSpeakerRules.Pick(state,_session.Random)?.PortraitId)],
                ()=>ShowOfficeReconstruction(state),()=>ShowOfficeReconstruction(state),scene:OriginalSceneArtwork.Publisher()));
        }
    }
}
