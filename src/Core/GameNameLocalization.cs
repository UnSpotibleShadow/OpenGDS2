using OpenGDS2.Domain;
using OpenGDS2.Import;

namespace OpenGDS2.Core;

/// <summary>Display-only English names; imported catalogs and saved titles stay original.</summary>
public static class GameNameLocalization
{
    public static string RivalTitle(RivalProduct product,OriginalCatalog? catalog,bool english,Func<string,string> translate)
    {
        if(!english||catalog?.SourceHash!=EmployeeDatabaseImporter.ExpectedHash||product.Owner<0||product.OwnProduct is not null)
            return product.Title;
        // Saved chart positions change when the player releases games. Match
        // the imported identity, never the runtime chart/award index or sales.
        int initial=catalog.InitialRivalProducts.FindIndex(p=>p.Owner==product.Owner&&p.Title==product.Title);
        if(initial>=0)return Read($"CATALOG_RIVAL_INITIAL_TITLE_{initial}",product.Title,translate);
        int staged=catalog.StagedRivalProducts.FindIndex(p=>p.Product.Owner==product.Owner&&p.Product.Title==product.Title);
        return staged>=0?Read($"CATALOG_RIVAL_STAGED_TITLE_{staged}",product.Title,translate):product.Title;
    }

    public static string RivalCompany(int owner,OriginalCatalog? catalog,bool english,Func<string,string> translate,string unknown)
    {
        string original=catalog?.RivalCompanies.ElementAtOrDefault(owner)??unknown;
        return english&&catalog?.SourceHash==EmployeeDatabaseImporter.ExpectedHash&&catalog.RivalCompanies.Count==10&&owner is >=0 and <10
            ?Read($"CATALOG_RIVAL_COMPANY_{owner}",original,translate):original;
    }

    public static IReadOnlyList<string> Examples(GameTypeDefinition type,OriginalCatalog catalog,bool english,Func<string,string> translate)
    {
        if(!english||catalog.SourceHash!=EmployeeDatabaseImporter.ExpectedHash||catalog.GameTypes.Count!=51)
            return type.ExampleTitles;
        return type.ExampleTitles.Select((title,index)=>Read($"CATALOG_TYPE_EXAMPLE_{type.SourceIndex}_{index}",title,translate)).ToArray();
    }

    private static string Read(string key,string original,Func<string,string> translate)
    {
        string text=translate(key);
        return string.IsNullOrWhiteSpace(text)||text==key?original:text;
    }
}
