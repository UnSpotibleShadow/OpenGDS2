namespace OpenGDS2.Domain;

/// <summary>fGMZokuhen, fGMKikakusyo and the mode-1 work/shipment branches.</summary>
public static class SequelDevelopmentRules
{
    public static IEnumerable<ReleasedGameState> Candidates(GameState state)
        =>state.ReleasedGames.Where(x=>x.HallOfFame && !x.HasReleasedSequel).OrderByDescending(x=>x.ProductIndex);

    public static DevelopmentPlanningResult CheckPredecessor(GameState state,int productIndex)
    {
        var product=state.ReleasedGames.SingleOrDefault(x=>x.ProductIndex==productIndex);
        if (product is null || !product.HallOfFame || product.HasReleasedSequel)
            return DevelopmentPlanningResult.PredecessorUnavailable;
        return state.DevelopmentProjects.Any(x=>x.OriginalRoomState>0 && x.OriginalMode==1 && x.PredecessorProductIndex==productIndex)
            ? DevelopmentPlanningResult.SequelAlreadyDeveloping : DevelopmentPlanningResult.Success;
    }

    public static string SuggestedTitle(ReleasedGameState predecessor)
        =>(predecessor.SeriesBaseTitle ?? predecessor.Title)+(predecessor.OriginalSequelCounter+2).ToString(System.Globalization.CultureInfo.InvariantCulture);

    public static void Initialize(DevelopmentProjectState project,ReleasedGameState predecessor)
    {
        DevelopmentWorkRules.ValidateQuality(predecessor.Quality);
        if (!double.IsFinite(predecessor.Recognition) || predecessor.Recognition<0) throw new InvalidDataException("Invalid predecessor recognition.");
        var q=predecessor.Quality;
        project.OriginalMode=1;project.PredecessorProductIndex=predecessor.ProductIndex;
        project.Quality=new(Math.Floor(q.Fun*.3),Math.Floor(q.Novelty*.3),Math.Floor(q.Graphics*.3),Math.Floor(q.Music*.3));
        project.Recognition=Math.Floor(predecessor.Recognition*.3);
    }

    // Called in the same employee pass as the original checkpoints. Does not
    // charge selection fees, apply fame/affinity bonuses or build a type roster.
    public static void InheritCheckpoint(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,DevelopmentMilestone milestone)
    {
        if (project.OriginalMode!=1 || project.PredecessorProductIndex is not int index ||
            DevelopmentWorkRules.PendingMilestone(project)!=milestone || milestone==DevelopmentMilestone.StartDebugging)
            throw new InvalidOperationException("No automatic sequel checkpoint.");
        var predecessor=state.ReleasedGames.Single(x=>x.ProductIndex==index);
        if (milestone==DevelopmentMilestone.SelectType)
        {
            var type=catalog.GameTypes.Single(x=>x.SourceIndex==predecessor.GameTypeSourceIndex);
            if (type.SelectionPoints<0 || !double.IsFinite(project.RequiredPoints+type.SelectionPoints)) throw new InvalidDataException("Invalid inherited type points.");
            project.RequiredPoints+=type.SelectionPoints;project.GameTypeSourceIndex=type.SourceIndex;project.OriginalStage=1;
        }
        else
        {
            var genre=catalog.Genres.Single(x=>x.SourceIndex==predecessor.GenreSourceIndex);
            if (genre.SelectionPoints<0 || !double.IsFinite(project.RequiredPoints+genre.SelectionPoints)) throw new InvalidDataException("Invalid inherited genre points.");
            project.RequiredPoints+=genre.SelectionPoints;project.GenreSourceIndex=genre.SourceIndex;project.OriginalStage=2;
        }
    }

    public static void ValidateLineage(GameState state)
    {
        foreach (var product in state.ReleasedGames)
        {
            if (product.OriginalMode is not (0 or 1 or 2) || product.OriginalSequelCounter!=(product.OriginalMode==1 ? 1 : 0) ||
                product.SeriesBaseTitle is not null && (string.IsNullOrWhiteSpace(product.SeriesBaseTitle) || product.SeriesBaseTitle.Length>128))
                throw new InvalidDataException("Invalid series record.");
            if (product.OriginalMode!=1 && (product.PredecessorProductIndex.HasValue || product.SeriesBaseTitle is not null && product.SeriesBaseTitle!=product.Title))
                throw new InvalidDataException("Original game has a predecessor.");
            ValidateManga(product.OriginalMode,product.MangaSourceIndex);
            if (product.OriginalMode==1)
            {
                if (product.PredecessorProductIndex is not int index || index<0 || index>=product.ProductIndex)
                    throw new InvalidDataException("Invalid sequel predecessor.");
                var parent=state.ReleasedGames[index];
                if (!parent.HallOfFame || !parent.HasReleasedSequel || product.SeriesBaseTitle!=(parent.SeriesBaseTitle ?? parent.Title) ||
                    product.GameTypeSourceIndex!=parent.GameTypeSourceIndex || product.GenreSourceIndex!=parent.GenreSourceIndex)
                    throw new InvalidDataException("Inconsistent sequel lineage.");
            }
            if (state.ReleasedGames.Count(x=>x.OriginalMode==1 && x.PredecessorProductIndex==product.ProductIndex)!=(product.HasReleasedSequel ? 1 : 0))
                throw new InvalidDataException("Invalid predecessor consumption.");
        }
        foreach (var project in state.DevelopmentProjects.Where(p=>p.OriginalRoomState==1))
        {
            ValidateManga(project.OriginalMode,project.MangaSourceIndex);
            if (project.OriginalMode!=1 && project.PredecessorProductIndex.HasValue)
                throw new InvalidDataException("Original project has a predecessor.");
            if (project.OriginalMode!=1) continue;
            var parent=state.ReleasedGames.SingleOrDefault(x=>x.ProductIndex==project.PredecessorProductIndex);
            if (parent is null || !parent.HallOfFame || parent.HasReleasedSequel ||
                project.PendingDecision is DevelopmentMilestone.SelectType or DevelopmentMilestone.SelectGenre || project.TypeCandidates is not null ||
                project.OriginalStage==0 && project.GameTypeSourceIndex.HasValue || project.OriginalStage<2 && project.GenreSourceIndex.HasValue ||
                project.OriginalStage>=1 && project.GameTypeSourceIndex!=parent.GameTypeSourceIndex ||
                project.OriginalStage>=2 && project.GenreSourceIndex!=parent.GenreSourceIndex ||
                state.DevelopmentProjects.Count(x=>x.OriginalMode==1 && x.PredecessorProductIndex==project.PredecessorProductIndex)!=1)
                throw new InvalidDataException("Invalid active sequel lineage.");
        }
    }
    private static void ValidateManga(int mode,int? source)
    {
        if (mode==2 ? source is null or <0 or >30 : source.HasValue)
            throw new InvalidDataException("Invalid manga lineage.");
    }
}
