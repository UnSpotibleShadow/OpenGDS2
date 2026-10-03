using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string ProjectSummary(DevelopmentProjectState project)
        =>$"{Tr("DEVELOPMENT_ROOM")} {project.RoomIndex+1}: {project.Title}\n{project.Progress/project.RequiredPoints:P0} · {Tr("BUGS")}: {project.Bugs}";
    private void AddProjectMetrics(DevelopmentProjectState project)
    {
        var metrics=new GridContainer {Columns=2};_content.AddChild(metrics);
        AddMetric(metrics,Tr("STAT_FUN"),$"{project.Quality.Fun:N0}");
        AddMetric(metrics,Tr("STAT_NOVELTY"),$"{project.Quality.Novelty:N0}");
        AddMetric(metrics,Tr("STAT_GRAPHICS"),$"{project.Quality.Graphics:N0}");
        AddMetric(metrics,Tr("STAT_MUSIC"),$"{project.Quality.Music:N0}");
        AddMetric(metrics,Tr("BUGS"),project.Bugs.ToString());
        AddMetric(metrics,Tr("GAME_RECOGNITION"),$"{project.Recognition:N0}%");
    }
    private void ShowProject(GameState state,DevelopmentProjectState project)
    {
        if (project.PendingDecision.HasValue) {ShowProjectDecision(state,project);return;}
        ClearContent();AddHeading(project.Title,28);
        var progress=new ProgressBar {Name="DevelopmentProgress",MinValue=0,MaxValue=100};_content.AddChild(progress);
        var report=new Label {Name="DevelopmentReport",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(report);
        AddHeading(Tr("DEVELOPMENT_STAFF_NOTE"),16);
        AddAction("STAFF",()=>ShowStaff(state));
        if (project.OriginalStage==3) AddAction("RELEASE_GAME",()=>ShowRelease(state,project));
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            progress.Value=project.Progress/project.RequiredPoints*100;
            var q=project.Quality;
            report.Text=$"{Tr("DEVELOPMENT_POINTS")}: {project.Progress:N1} / {project.RequiredPoints:N0}\n"+
                $"{Tr("STAT_FUN")}: {q.Fun:N0} · {Tr("STAT_NOVELTY")}: {q.Novelty:N0}\n"+
                $"{Tr("STAT_GRAPHICS")}: {q.Graphics:N0} · {Tr("STAT_MUSIC")}: {q.Music:N0}\n"+
                $"{Tr("BUGS")}: {project.Bugs}\n"+
                string.Join("\n",state.Employees.Where(x=>x.Office?.Action is >=6 and <=17 && (x.Office.Action-6)/6==project.RoomIndex)
                    .Select(x=>$"{x.Name} · {Tr("STRESS")}: {x.Stress:N0}%"));
        };
        _refreshCoreStatus();
    }
    private void ShowProjectDecision(GameState state,DevelopmentProjectState project)
    {
        var catalog=_importer.LoadCatalog() ?? throw new InvalidOperationException("No imported catalog.");
        ClearContent();AddHeading(project.Title,28);AddProjectMetrics(project);
        _status=new Label {AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        if (project.PendingDecision==DevelopmentMilestone.SelectType)
        {
            AddHeading(Tr("SELECT_GAME_TYPE"),22);
            try
            {
                var candidates=DevelopmentSimulation.PrepareTypeCandidates(state,project,catalog,new Random());
                int listIndex=0;
                foreach (int source in candidates)
                {
                    var type=catalog.GameTypes.Single(x=>x.SourceIndex==source);
                    var button=new Button {Name=$"SelectType{listIndex++}",Text=$"{type.Name} · ¥{type.SelectionFee:N0}",
                        Disabled=state.Money is null || state.Money<type.SelectionFee};
                    button.Pressed+=()=>
                    {
                        var result=DevelopmentSelectionRules.TrySelectType(state,project,catalog,source,state.OriginalTypeRosterMap!);
                        if (result==DevelopmentSelectionResult.Success) ShowOfficeReconstruction(state);
                        else _status.Text=Tr($"SELECTION_RESULT_{result}");
                    };
                    _content.AddChild(button);
                }
            }
            catch (InvalidOperationException) {_status.Text=Tr("LEGACY_ROSTER_UNKNOWN");}
        }
        else if (project.PendingDecision==DevelopmentMilestone.SelectGenre)
        {
            AddHeading(Tr("SELECT_GENRE"),22);
            if (state.Company.OriginalMember90 is not int companyValue) {_status.Text=Tr("LEGACY_ROSTER_UNKNOWN");return;}
            foreach (var genre in catalog.Genres.Where(x=>DevelopmentSelectionRules.IsGenreEligible(x,companyValue)))
            {
                var button=new Button {Name=$"SelectGenre{genre.SourceIndex}",Text=$"{genre.Name} · ¥{genre.SelectionFee:N0}",
                    Disabled=state.Money is null || state.Money<genre.SelectionFee};
                button.Pressed+=()=>
                {
                    var result=DevelopmentSelectionRules.TrySelectGenre(state,project,catalog,genre.SourceIndex);
                    if (result==DevelopmentSelectionResult.Success) ShowOfficeReconstruction(state);
                    else _status.Text=Tr($"SELECTION_RESULT_{result}");
                };
                _content.AddChild(button);
            }
        }
        else
        {
            AddHeading(Tr("DEVELOPMENT_COMPLETE"),22);
            if (project.Bugs>=60) AddHeading(Tr("RELEASE_BUG_WARNING"),16);
            AddAction("RELEASE_GAME",()=>ShowRelease(state,project));
            AddAction("CONTINUE_DEBUGGING",()=>{project.PendingDecision=null;ShowProject(state,project);});
        }
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private void ShowRelease(GameState state,DevelopmentProjectState project)
    {
        var catalog=_importer.LoadCatalog() ?? throw new InvalidOperationException("No imported catalog.");
        ClearContent();AddHeading(Tr("RELEASE_GAME"),28);AddProjectMetrics(project);
        if (state.Company.ProductionCapacity is null || state.SalesFee1 is null || state.SalesFee2 is null)
        {
            AddHeading(Tr("LEGACY_ROSTER_UNKNOWN"),16);AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));return;
        }
        var title=AddField(Tr("GAME_TITLE"),project.Title,128);title.Name="ReleaseTitle";
        AddHeading(Tr("RETAIL_PRICE"),18);
        var price=new SpinBox {Name="ReleasePrice",MinValue=0,MaxValue=20000,AllowGreater=true,Step=10,Value=ReleaseService.DefaultPrice};_content.AddChild(price);
        AddHeading(Tr("PRODUCTION_COPIES"),18);
        var copies=ProductionQuantity(state,"ReleaseCopies");
        long unit=ReleaseService.UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex),project.DataSize);
        var quote=new Label {Name="ReleaseQuote",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(quote);
        _status=new Label();_content.AddChild(_status);
        void Quote()=>quote.Text=$"{Tr("PRODUCTION_COST")}: ¥{unit*(long)copies.Value:N0}\n"+
            $"{Tr("NET_PER_COPY")}: ¥{(long)price.Value-state.SalesFee1-state.SalesFee2:N0}";
        price.ValueChanged+=_=>Quote();copies.ValueChanged+=_=>Quote();Quote();
        var confirm=new Button {Name="ConfirmRelease",Text=Tr("RELEASE_GAME")};_content.AddChild(confirm);
        title.TextChanged+=value=>confirm.Disabled=string.IsNullOrWhiteSpace(value);
        confirm.Pressed+=()=>
        {
            var result=ReleaseService.TryRelease(state,project,catalog,title.Text,(long)price.Value,(long)copies.Value,new Random());
            if (result==ReleaseResult.Success) ShowReviews(state,state.ReleasedGames[^1]);
            else _status.Text=Tr($"RELEASE_RESULT_{result}");
        };
        AddAction("CANCEL",()=>ShowProject(state,project));
    }
    private SpinBox ProductionQuantity(GameState state,string name)
    {
        // Same 100-copy floor and capacity×100 ceiling as the original spinner.
        var copies=new SpinBox {Name=name,MinValue=100,MaxValue=Math.Max(100,(state.Company.ProductionCapacity ?? throw new InvalidOperationException("Unknown production capacity."))*100L),
            Step=100,Value=ReleaseService.DefaultCopies};_content.AddChild(copies);return copies;
    }
    private void ShowSales(GameState state)
    {
        if (ShowPendingProductEvent(state)) return;
        ClearContent();AddHeading(Tr("SALES"),28);
        var funds=new Label {Name="SalesFunds"};_content.AddChild(funds);
        var rows=new List<(ReleasedGameState Product,Label Report)>();
        foreach (var product in state.ReleasedGames.OrderByDescending(x=>x.ProductIndex))
        {
            AddHeading(product.Title,22);
            var report=new Label {Name=$"SalesProduct{product.ProductIndex}",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(report);rows.Add((product,report));
            var button=new Button {Name=$"ProduceProduct{product.ProductIndex}",Text=Tr("PRODUCE_MORE"),Disabled=state.Company.ProductionCapacity is null};
            button.Pressed+=()=>ShowProduction(state,product);_content.AddChild(button);
            if (product.ReviewScores is not null)
            {
                var reviews=new Button {Name=$"ReviewsProduct{product.ProductIndex}",Text=Tr("MAGAZINE_REVIEWS")};
                reviews.Pressed+=()=>ShowReviews(state,product);_content.AddChild(reviews);
            }
        }
        if (rows.Count==0) AddHeading(Tr("NO_RELEASED_GAMES"),18);
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
        _activeCatalog=_importer.LoadCatalog();_clockRunning=true;
        _refreshCoreStatus=()=>
        {
            funds.Text=$"{Tr("FUNDS")}: ¥{state.Money:N0}";
            foreach (var (p,label) in rows) label.Text=$"{Tr("RETAIL_PRICE")}: ¥{p.Price:N0} · {Tr("STOCK")}: {p.Stock:N0}\n"+
                $"{Tr("UNITS_SOLD")}: {p.UnitsSold:N0} · {Tr("LAST_SALES_BATCH")}: {p.LastBatchSold:N0}\n"+
                $"{Tr("NET_SALES_RECEIPTS")}: ¥{p.NetSalesReceipts:N0}\n{Tr("PRODUCTION_COST")}: ¥{p.ProductionSpending:N0}\n{Tr("DEMAND")}: {p.Demand}";
        };
        _refreshCoreStatus();
    }
    private void ShowReviews(GameState state,ReleasedGameState product)
    {
        ClearContent();AddHeading(Tr("MAGAZINE_REVIEWS"),28);AddHeading(product.Title,22);
        var scores=product.ReviewScores ?? throw new InvalidOperationException("Reviews are unknown for this product.");
        var row=new HBoxContainer();row.AddThemeConstantOverride("separation",24);_content.AddChild(row);
        for (int i=0;i<scores.Count;i++)
        {
            var column=new VBoxContainer();row.AddChild(column);
            column.AddChild(new Label {Text=$"{Tr("REVIEWER")} {i+1}"});
            var score=new Label {Name=$"ReviewScore{i}",Text=$"{scores[i]} / 10"};score.AddThemeFontSizeOverride("font_size",32);column.AddChild(score);
        }
        int total=scores.Sum();
        var result=new Label {Name="ReviewTotal",Text=$"{Tr("REVIEW_TOTAL")}: {total} / 40"};_content.AddChild(result);
        AddHeading(Tr(total>=32 ? "REVIEW_HALL_OF_FAME" : total>=24 ? "REVIEW_GOOD" : total>=12 ? "REVIEW_LOW" : "REVIEW_POOR"),20);
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
        var acknowledge=new Button {Name="AcknowledgeReviews",Text=Tr("CONTINUE")};_content.AddChild(acknowledge);
        acknowledge.Pressed+=()=>{ReviewService.Acknowledge(state,product);ShowSales(state);};
    }
    private bool ShowPendingProductEvent(GameState state)
    {
        var review=state.ReleasedGames.FirstOrDefault(x=>x.ReviewPending);
        if (review is not null) {ShowReviews(state,review);return true;}
        var milestone=state.ReleasedGames.FirstOrDefault(x=>x.PendingSalesMilestones.Count>0);
        if (milestone is null) return false;
        ShowSalesMilestone(state,milestone);return true;
    }
    private void ShowSalesMilestone(GameState state,ReleasedGameState product)
    {
        ClearContent();AddHeading(Tr("SALES_MILESTONE"),28);AddHeading(product.Title,22);
        int stage=product.PendingSalesMilestones[0];
        AddHeading($"{Tr("UNITS_SOLD")}: {SalesMilestoneService.Threshold(stage):N0}",24);
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
        var acknowledge=new Button {Name="AcknowledgeSalesMilestone",Text=Tr("CONTINUE")};_content.AddChild(acknowledge);
        acknowledge.Pressed+=()=>{SalesMilestoneService.Acknowledge(state,product);ShowSales(state);};
    }
    private void ShowProduction(GameState state,ReleasedGameState product)
    {
        var catalog=_importer.LoadCatalog() ?? throw new InvalidOperationException("No imported catalog.");
        ClearContent();AddHeading(product.Title,28);AddHeading(Tr("PRODUCE_MORE"),22);
        var copies=ProductionQuantity(state,"ProductionCopies");
        long unit=ReleaseService.UnitProductionCost(catalog.Hardware.Single(x=>x.SourceIndex==product.HardwareSourceIndex),product.DataSize);
        var quote=new Label();_content.AddChild(quote);quote.Text=$"{Tr("PRODUCTION_COST")}: ¥{unit*(long)copies.Value:N0}";
        copies.ValueChanged+=_=>quote.Text=$"{Tr("PRODUCTION_COST")}: ¥{unit*(long)copies.Value:N0}";
        _status=new Label();_content.AddChild(_status);
        var confirm=new Button {Name="ConfirmProduction",Text=Tr("CONFIRM")};_content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            var result=ReleaseService.TryProduce(state,product,catalog,(long)copies.Value);
            if (result==ReleaseResult.Success) ShowSales(state);else _status.Text=Tr($"RELEASE_RESULT_{result}");
        };
        AddAction("CANCEL",()=>ShowSales(state));
    }
}
