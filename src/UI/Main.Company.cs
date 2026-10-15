using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowCompanyActivities(GameState state)
    {
        if(ShowPendingCompanyActivity(state))return;
        var canvas=ManagementCanvas("CompanyActivitiesWindow",Tr("COMPANY_ACTIVITIES"),new(390,325));
        NativeFill(canvas,"CompanyActivityMetricsBox",new(4,4,382,95),new(.77f,1,1),true);
        string[] labels=["FUNDS","COMPANY_ACTIVITY_COST","COMPANY_FAME","GENRE_RESEARCH","PRODUCTION_CAPACITY"];
        string[] values=[$"¥{state.Money:N0}",$"¥{state.Company.ActivityCost:N0}",$"{state.Company.Fame}%",$"{state.Company.OriginalMember90}",$"{state.Company.ProductionCapacity}"];
        for(int i=0;i<labels.Length;i++){NativeText(canvas,"ActivityMetricLabel"+i,Tr(labels[i]),new(8,6+18*i,265,18));NativeText(canvas,"ActivityMetricValue"+i,values[i],new(275,6+18*i,107,18),11,HorizontalAlignment.Right);}
        int y=105;
        foreach(var (kind,key) in new[] {(CompanyActivity.PublicRelations,"PUBLIC_RELATIONS"),(CompanyActivity.GenreResearch,"GENRE_RESEARCH"),(CompanyActivity.ProductionExpansion,"EXPAND_PRODUCTION")})
        {
            NativeAction(canvas,$"CompanyActivity{(int)kind}",key,new(4,y,382,25),()=>{state.PendingCompanyActivity=new(){Activity=kind};ShowPendingCompanyActivity(state);},state.CompanyActivityPerformed==false);y+=29;
        }
        var catalog=_importer.LoadCatalog();
        string genres=catalog is not null && state.Company.OriginalMember90 is int research ? string.Join(", ",catalog.Genres.Where(x=>DevelopmentSelectionRules.IsGenreEligible(x,research)).Select(g=>CatalogName(catalog,"GENRE",g.SourceIndex,g.Name))) : "—";
        NativeText(canvas,"AvailableGenresLabel",Tr("AVAILABLE_GENRES"),new(4,194,382,19),10);
        var list=new RichTextLabel {Name="AvailableGenres",Text=genres,ScrollActive=true};list.AddThemeFontSizeOverride("normal_font_size",11);NativePlace(canvas,list,new(4,215,382,42));
        _status=NativeText(canvas,"CompanyActivityStatus",state.CompanyActivityPerformed==true?Tr("COMPANY_ACTIVITY_ALREADY_USED"):"",new(4,260,382,25),10);
        NativeAction(canvas,"CompanyActivitiesBank","BANK",new(4,290,185,25),()=>ShowBank(state),BankLoanService.CheckBorrow(state) is BankLoanResult.Success or BankLoanResult.AllSlotsOccupied);
        NativeAction(canvas,"CompanyActivitiesClose","BACK_TO_OFFICE",new(197,290,189,25),()=>ShowOfficeReconstruction(state));
    }
}
