using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowCompanyActivities(GameState state)
    {
        ClearContent();AddHeading(Tr("COMPANY_ACTIVITIES"),28);
        AddAction("BANK",()=>ShowBank(state),BankLoanService.CheckBorrow(state) is BankLoanResult.Success or BankLoanResult.AllSlotsOccupied);
        AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",20);
        AddHeading($"{Tr("COMPANY_ACTIVITY_COST")}: ¥{state.Company.ActivityCost:N0}",18);
        var grid=new GridContainer {Columns=2};_content.AddChild(grid);
        AddMetric(grid,Tr("COMPANY_FAME"),$"{state.Company.Fame}%");
        AddMetric(grid,Tr("GENRE_RESEARCH"),$"{state.Company.OriginalMember90}");
        AddMetric(grid,Tr("PRODUCTION_CAPACITY"),$"{state.Company.ProductionCapacity}");
        if (state.CompanyActivityPerformed==true) AddHeading(Tr("COMPANY_ACTIVITY_ALREADY_USED"),18);
        foreach (var (kind,key) in new[] {(CompanyActivity.PublicRelations,"PUBLIC_RELATIONS"),
            (CompanyActivity.GenreResearch,"GENRE_RESEARCH"),(CompanyActivity.ProductionExpansion,"EXPAND_PRODUCTION")})
        {
            var button=new Button {Name=$"CompanyActivity{(int)kind}",Text=Tr(key),Disabled=state.CompanyActivityPerformed!=false};_content.AddChild(button);
            button.Pressed+=()=>
            {
                var result=CompanyActivityService.TryPerform(state,kind,new Random());
                ShowCompanyActivities(state);AddHeading(Tr($"COMPANY_ACTIVITY_RESULT_{result}"),18);
            };
        }
        var catalog=_importer.LoadCatalog();
        if (catalog is not null && state.Company.OriginalMember90 is int research)
        {
            AddHeading(Tr("AVAILABLE_GENRES"),20);
            foreach (var genre in catalog.Genres.Where(x=>DevelopmentSelectionRules.IsGenreEligible(x,research)))
                AddHeading(CatalogName(catalog,"GENRE",genre.SourceIndex,genre.Name),16);
        }
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
}
