using Godot;
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
        var catalog=_importer.LoadCatalog() ?? throw new InvalidOperationException("No local catalog.");
        ClearContent();CompactManagementWindow("MangaSelectionWindow",new Vector2(450,340),430);
        _dialog!.SetTitle(Tr("MANGA_ADAPTATION"));
        if (catalog.Manga.Count==0) {_content.AddChild(new Label {Text=Tr("MANGA_REIMPORT_REQUIRED"),AutowrapMode=TextServer.AutowrapMode.WordSmart});AddAction("CANCEL",()=>ShowDevelopmentRoomActions(state,room));return;}
        var table=CreateTable("MangaTitles",["GAME_TITLE","POPULARITY","CONTRACT_FEE"],[195,95,110]);
        table.CustomMinimumSize=new Vector2(420,180);_content.AddChild(table);
        foreach (var manga in catalog.Manga)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,manga.SourceIndex);
            row.SetText(0,MangaText(catalog,manga.SourceIndex,"TITLE"));row.SetText(1,MangaText(catalog,manga.SourceIndex,"POPULARITY"));
            row.SetText(2,manga.RightsFee.ToString("N0"));row.SetTextAlignment(2,HorizontalAlignment.Right);
        }
        int selected=catalog.Manga[0].SourceIndex;
        var details=new Label {Name="MangaDetails",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(details);
        void Refresh()=>details.Text=$"{Tr("AUTHOR")}: {MangaText(catalog,selected,"AUTHOR")}\n{Tr("PUBLISHER")}: {MangaText(catalog,selected,"PUBLISHER")}";
        table.ItemSelected+=()=>{selected=table.GetSelected().GetMetadata(0).AsInt32();Refresh();};
        table.GetRoot().GetFirstChild().Select(0);Refresh();
        var confirm=new Button {Name="ConfirmManga",Text=Tr("MANGA_RIGHTS_MEETING")};_content.AddChild(confirm);
        confirm.Pressed+=()=>ShowMangaRightsMeeting(state,catalog,room,selected);
        AddAction("CANCEL",()=>ShowDevelopmentRoomActions(state,room));
    }
    private void ShowMangaRightsMeeting(GameState state,OriginalCatalog catalog,int room,int source)
    {
        string title=MangaText(catalog,source,"TITLE"),publisher=MangaText(catalog,source,"PUBLISHER");
        string representative=DialogueText("MANGA_REPRESENTATIVE").Replace("{publisher}",publisher);
        string Text(string key)=>DialogueText(key).Replace("{publisher}",publisher).Replace("{title}",title);
        ShowConversation(Tr("MANGA_RIGHTS_MEETING"),Enumerable.Range(0,4).Select(i=>new ConversationPage(representative,Text($"MANGA_MEETING_{i}"))).ToList(),Confirm,
            ()=>ShowMangaSelection(state,room));
        void Confirm()
        {
            ClearContent();CompactManagementWindow("MangaRightsWindow",new Vector2(330,180),310);
            _dialog!.SetTitle(Tr("MANGA_RIGHTS_MEETING"));
            _content.AddChild(new Label {Name="MangaRightsQuestion",Text=Text("MANGA_RIGHTS_QUESTION").Replace("{fee}",catalog.Manga.Single(x=>x.SourceIndex==source).RightsFee.ToString("N0")),AutowrapMode=TextServer.AutowrapMode.WordSmart});
            var status=new Label {Name="MangaRightsStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(status);
            var agree=new Button {Name="AgreeMangaRights",Text=Tr("ACCEPT")};_content.AddChild(agree);
            agree.Pressed+=()=>
            {
                var result=DevelopmentProjectService.CheckMangaRights(state,catalog,source);
                if (result!=DevelopmentPlanningResult.Success) {status.Text=DialogueText("MANGA_NO_MONEY");return;}
                ShowConversation(Tr("MANGA_RIGHTS_MEETING"),[new(representative,Text("MANGA_RIGHTS_APPROVED")),
                    new(DialogueText("MEETING_SECRETARY"),Text("MANGA_BEGIN_DEVELOPMENT"))],
                    ()=>ShowPlanningPreview(state,room,null,source),()=>ShowMangaSelection(state,room));
            };
            AddAction("DECLINE",()=>ShowConversation(Tr("MANGA_RIGHTS_MEETING"),[new(representative,Text("MANGA_RIGHTS_DECLINED"))],
                ()=>ShowOfficeReconstruction(state),()=>ShowOfficeReconstruction(state)));
        }
    }
}
