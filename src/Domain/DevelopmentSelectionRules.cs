namespace OpenGDS2.Domain;

public enum DevelopmentSelectionResult { Success, WrongStage, MoneyUnknown, InsufficientFunds, CompanyFameUnknown }

/// <summary>Verified confirmation effects; original candidate-list generation is separate.</summary>
public static class DevelopmentSelectionRules
{
    public static bool MatchesAudience(GameTypeDefinition type,AudienceDefinition audience)
    {
        int code=audience.Identifier;
        if (code is <1 or >7) throw new InvalidDataException("Unsupported audience identifier.");
        if (code==1) return true;
        return type.AudienceAffinityCode switch
        {
            >=1 and <=7=>type.AudienceAffinityCode==code,
            8=>code is 2 or 3 or 4,9=>code is 6 or 7,10=>code is 4 or 5,
            11=>code is 2 or 3 or 4 or 5,12=>code is 4 or 5 or 6 or 7,
            _=>throw new InvalidDataException("Unsupported type affinity.")
        };
    }
    public static bool IsGenreEligible(GenreDefinition genre,int originalCompanyMember90)
        =>genre.RequiredCompanyValue<=originalCompanyMember90;
    public static DevelopmentSelectionResult TrySelectType(GameState state,DevelopmentProjectState project,
        OriginalCatalog catalog,int selectedSourceIndex,IReadOnlyDictionary<int,int> originalRosterMap)
    {
        // +0795 uses points[rosterMap[selectedSourceIndex]], after the source was
        // already mapped by the list handler. Require the actual original map.
        ArgumentNullException.ThrowIfNull(originalRosterMap);
        if (project.OriginalMode!=0 || DevelopmentWorkRules.PendingMilestone(project)!=DevelopmentMilestone.SelectType)
            return DevelopmentSelectionResult.WrongStage;
        var selected=catalog.GameTypes.SingleOrDefault(x=>x.SourceIndex==selectedSourceIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(selectedSourceIndex));
        if (!originalRosterMap.TryGetValue(selectedSourceIndex,out int pointsSource))
            throw new InvalidOperationException("Original type roster map is required at the selected source index.");
        var pointsType=catalog.GameTypes.SingleOrDefault(x=>x.SourceIndex==pointsSource)
            ?? throw new InvalidDataException("Invalid original type roster entry.");
        if (selected.SelectionFee<0 || pointsType.SelectionPoints<0 || !double.IsFinite(project.Recognition) || project.Recognition<0)
            throw new InvalidDataException("Invalid type selection data.");
        if (state.Company.Fame is not int fame) return DevelopmentSelectionResult.CompanyFameUnknown;
        if (fame<0) throw new InvalidDataException("Invalid company recognition.");
        if (state.Money is null) return DevelopmentSelectionResult.MoneyUnknown;
        if (state.Money<selected.SelectionFee) return DevelopmentSelectionResult.InsufficientFunds;
        double points=project.RequiredPoints+pointsType.SelectionPoints;
        if (!double.IsFinite(points)) throw new InvalidDataException("Invalid development points.");
        FinancialRules.Apply(state,-selected.SelectionFee);
        project.GameTypeSourceIndex=selectedSourceIndex;project.RequiredPoints=points;project.OriginalStage=1;
        project.Recognition+=Math.Floor(fame/3.0);
        project.PendingDecision=null;
        return DevelopmentSelectionResult.Success;
    }
    public static DevelopmentSelectionResult TrySelectGenre(GameState state,DevelopmentProjectState project,
        OriginalCatalog catalog,int selectedSourceIndex)
    {
        // Original eligibility is checked while constructing the selection list.
        if (project.OriginalMode!=0 || DevelopmentWorkRules.PendingMilestone(project)!=DevelopmentMilestone.SelectGenre)
            return DevelopmentSelectionResult.WrongStage;
        var selected=catalog.Genres.SingleOrDefault(x=>x.SourceIndex==selectedSourceIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(selectedSourceIndex));
        var type=catalog.GameTypes.SingleOrDefault(x=>x.SourceIndex==project.GameTypeSourceIndex)
            ?? throw new InvalidDataException("Missing selected game type.");
        var audience=catalog.Audiences.SingleOrDefault(x=>x.SourceIndex==project.AudienceSourceIndex)
            ?? throw new InvalidDataException("Missing selected audience.");
        DevelopmentWorkRules.ValidateQuality(project.Quality);
        if (selected.SelectionFee<0 || selected.SelectionPoints<0 || type.RawTable13CC.Count!=15)
            throw new InvalidDataException("Invalid genre selection data.");
        bool affinity=audience.Identifier>1 && type.RawTable13CC.Contains(selected.Identifier);
        if (state.Money is null) return DevelopmentSelectionResult.MoneyUnknown;
        if (state.Money<selected.SelectionFee) return DevelopmentSelectionResult.InsufficientFunds;
        double points=project.RequiredPoints+selected.SelectionPoints;
        if (!double.IsFinite(points)) throw new InvalidDataException("Invalid development points.");
        FinancialRules.Apply(state,-selected.SelectionFee);
        project.GenreSourceIndex=selectedSourceIndex;project.RequiredPoints=points;project.OriginalStage=2;
        if (affinity) project.Quality=project.Quality with {Fun=project.Quality.Fun+20};
        project.PendingDecision=null;
        return DevelopmentSelectionResult.Success;
    }
}
