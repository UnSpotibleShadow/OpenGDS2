using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void BuildCompanySummary(DesktopWindow window)
    {
        // The compact original Company page groups identity, capacity, games,
        // profits and tenants. Its rank controls sit below the visible window.
        Section("Identity",new Color("c0c0ff"),["COMPANY_NAME","FUNDS"]);
        Section("Capacity",new Color("ffe0c0"),["COMPANY_FAME","PRODUCTION_CAPACITY","GENRE_RESEARCH"]);
        Section("Games",new Color("ffffc0"),["RELEASED_COUNT","HALL_OF_FAME_COUNT","HIGHEST_SALES","HIGHEST_SALES_TITLE"]);
        Section("Profit",new Color("c0ffc0"),["WEEKLY_NET","MONTHLY_NET","YEARLY_NET"]);
        Section("Tenants",new Color("c0ffff"),["TENANT_1","TENANT_2","TENANT_3"]);

        void Section(string name,Color color,string[] keys)
        {
            var panel=new PanelContainer {Name="CompanySection"+name};window.Body.AddChild(panel);
            panel.AddThemeStyleboxOverride("panel",new StyleBoxFlat {BgColor=color,ContentMarginLeft=3,ContentMarginRight=3});
            var rows=new VBoxContainer();rows.AddThemeConstantOverride("separation",0);panel.AddChild(rows);
            foreach(string key in keys)
            {
                var row=new HBoxContainer();row.AddThemeConstantOverride("separation",4);rows.AddChild(row);
                if(key!="HIGHEST_SALES_TITLE")
                {
                    string caption=key switch {
                        "PRODUCTION_CAPACITY"=>Tr("PRODUCTION_CAPACITY"),
                        "GENRE_RESEARCH"=>Tr("COMPANY_SUMMARY_LIBRARY"),
                        "HIGHEST_SALES"=>Tr("COMPANY_SUMMARY_HIGHEST_SALES"),
                        "WEEKLY_NET"=>Tr("COMPANY_SUMMARY_WEEKLY_PROFIT"),
                        "MONTHLY_NET"=>Tr("COMPANY_SUMMARY_MONTHLY_PROFIT"),
                        "YEARLY_NET"=>Tr("COMPANY_SUMMARY_YEARLY_PROFIT"),
                        _ when key.StartsWith("TENANT_")=>string.Format(DisplayNumbers.Provider,Tr("COMPANY_SUMMARY_TENANT"),key[^1]),
                        _=>Tr(key)};
                    var label=new Label {Text=caption};label.AddThemeFontSizeOverride("font_size",11);row.AddChild(label);
                }
                var value=new Label {Name="CompanyMetric"+key,HorizontalAlignment=HorizontalAlignment.Right,
                    SizeFlagsHorizontal=SizeFlags.ExpandFill,TextOverrunBehavior=TextServer.OverrunBehavior.TrimEllipsis};
                value.AddThemeFontSizeOverride("font_size",11);row.AddChild(value);_companyMetrics[key]=value;
            }
        }
    }

    private void RefreshCompanySummary(GameState state)
    {
        string Number(long? number)=>number?.ToDisplayNumber()??"?";
        string Money(long? amount)=>amount is null?"?":string.Format(DisplayNumbers.Provider,Tr("COMPANY_SUMMARY_MONEY"),amount.Value.ToDisplayNumber());
        void Set(string key,string text){_companyMetrics[key].Text=text;_companyMetrics[key].TooltipText=text;}
        Set("COMPANY_NAME",state.Company.Name);
        Set("FUNDS",Money(state.Money));
        Set("COMPANY_FAME",state.Company.Fame is int fame?$"{fame}%":"?");
        Set("PRODUCTION_CAPACITY",state.Company.ProductionCapacity is int capacity
            ?string.Format(DisplayNumbers.Provider,Tr("COMPANY_SUMMARY_CAPACITY"),capacity.ToDisplayNumber(),((long)capacity*100).ToDisplayNumber()):"?");
        Set("GENRE_RESEARCH",state.Company.OriginalMember90 is int library?$"{library}%":"?");
        Set("RELEASED_COUNT",state.ReleasedGames.Count.ToString());
        Set("HALL_OF_FAME_COUNT",Number(state.Company.HallOfFameCount));
        var record=state.ReleasedGames.OrderByDescending(game=>game.UnitsSold).FirstOrDefault();
        Set("HIGHEST_SALES",Number(record?.UnitsSold??0));
        Set("HIGHEST_SALES_TITLE",record?.Title??"—");
        Set("WEEKLY_NET",Money(state.Company.WeeklyNet));
        Set("MONTHLY_NET",Money(state.Company.MonthlyNet));
        Set("YEARLY_NET",Money(state.Company.YearlyNet));
        for(int slot=0;slot<3;slot++)
        {
            int? kind=state.Calendar?.OriginalSlots0444.Count>slot?state.Calendar.OriginalSlots0444[slot]:null;
            Set("TENANT_"+(slot+1),kind is null?"?":kind==-1?Tr("COMPANY_SUMMARY_NO_TENANT"):FacilityName(kind.Value));
        }
    }
}
