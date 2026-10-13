namespace OpenGDS2.Domain;
public sealed class CompanyActivityPresentation
{
    public CompanyActivity Activity {get;set;}
    public bool Confirmed {get;set;}
    public int? Before {get;set;}
    public int? After {get;set;}
    public long? Cost {get;set;}
    public List<int> UnlockedGenres {get;set;}=[];
    public int Page {get;set;}
    public static int? Value(GameState state,CompanyActivity kind)=>kind switch
    {CompanyActivity.PublicRelations=>state.Company.Fame,CompanyActivity.GenreResearch=>state.Company.OriginalMember90,_=>state.Company.ProductionCapacity};
    public static CompanyActivityResult Confirm(GameState state,OriginalCatalog? catalog,Random random)
    {
        var scene=state.PendingCompanyActivity ?? throw new InvalidOperationException("No confirmation.");
        if(scene.Confirmed)return CompanyActivityResult.AlreadyActed;
        int? before=Value(state,scene.Activity);long? cost=state.Company.ActivityCost;
        var result=CompanyActivityService.TryPerform(state,scene.Activity,random);if(result!=CompanyActivityResult.Success)return result;
        scene.Before=before;scene.After=Value(state,scene.Activity);scene.Cost=cost;scene.Confirmed=true;
        if(scene.Activity==CompanyActivity.GenreResearch && catalog is not null)
            scene.UnlockedGenres=catalog.Genres.Where(g=>!DevelopmentSelectionRules.IsGenreEligible(g,before!.Value) &&
                DevelopmentSelectionRules.IsGenreEligible(g,scene.After!.Value)).Select(g=>g.SourceIndex).ToList();
        return result;
    }
    public static void Validate(GameState state)
    {
        if(state.PendingCompanyActivity is not { } scene)return;
        if(!Enum.IsDefined(scene.Activity)||scene.Page is <0 or >1 ||scene.UnlockedGenres is null ||scene.UnlockedGenres.Any(g=>g is <0 or >17)||
            scene.Confirmed && (scene.Before is null or <0 ||scene.After is null or <0 ||scene.Cost is null or <0 ||Value(state,scene.Activity)!=scene.After ||state.CompanyActivityPerformed!=true)||
            !scene.Confirmed && (scene.Before is not null ||scene.After is not null ||scene.Cost is not null ||scene.Page!=0 ||scene.UnlockedGenres.Count!=0))
            throw new InvalidDataException("Invalid activity presentation.");
    }
}
