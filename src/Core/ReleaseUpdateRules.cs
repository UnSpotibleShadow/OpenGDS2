using System.Text.Json;
using System.Text.RegularExpressions;
namespace OpenGDS2.Core;

public sealed record ReleaseNotice(string Version,string Url);

/// <summary>Only release metadata is read. Never downloads or executes an update.</summary>
public static class ReleaseUpdateRules
{
    private sealed record VersionParts(int Major,int Minor,int Patch,string[] Pre);
    private static VersionParts? Parse(string text)
    {
        var match=Regex.Match(text,@"^v?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$");
        if(!match.Success||!int.TryParse(match.Groups[1].Value,out int a)||!int.TryParse(match.Groups[2].Value,out int b)||!int.TryParse(match.Groups[3].Value,out int c))return null;
        string[] pre=match.Groups[4].Success?match.Groups[4].Value.Split('.'):[];
        if(pre.Any(s=>s.All(char.IsAsciiDigit)&&s.Length>1&&s[0]=='0'))return null;
        return new(a,b,c,pre);
    }
    public static int? Compare(string left,string right)
    {
        var a=Parse(left);var b=Parse(right);if(a is null||b is null)return null;
        int n=a.Major.CompareTo(b.Major);if(n!=0)return n;
        n=a.Minor.CompareTo(b.Minor);if(n!=0)return n;n=a.Patch.CompareTo(b.Patch);if(n!=0)return n;
        if(a.Pre.Length==0||b.Pre.Length==0)return a.Pre.Length==b.Pre.Length?0:a.Pre.Length==0?1:-1;
        for(int i=0;i<Math.Min(a.Pre.Length,b.Pre.Length);i++)
        {
            string x=a.Pre[i],y=b.Pre[i];bool xn=x.All(char.IsAsciiDigit),yn=y.All(char.IsAsciiDigit);
            n=xn&&yn?(x.Length==y.Length?string.CompareOrdinal(x,y):x.Length.CompareTo(y.Length)):xn!=yn?(xn?-1:1):string.CompareOrdinal(x,y);
            if(n!=0)return n;
        }
        return a.Pre.Length.CompareTo(b.Pre.Length);
    }
    public static ReleaseNotice? Select(string json,string installed)
    {
        if(Parse(installed) is null)return null;
        try
        {
            using var document=JsonDocument.Parse(json);if(document.RootElement.ValueKind!=JsonValueKind.Array)return null;
            ReleaseNotice? newest=null;
            foreach(var release in document.RootElement.EnumerateArray())
            {
                if(release.ValueKind!=JsonValueKind.Object||!release.TryGetProperty("draft",out var draft)||draft.ValueKind!=JsonValueKind.False||
                    !release.TryGetProperty("tag_name",out var tag)||tag.ValueKind!=JsonValueKind.String||
                    !release.TryGetProperty("html_url",out var url)||url.ValueKind!=JsonValueKind.String)continue;
                string version=tag.GetString()!,link=url.GetString()!;
                // The action button can open only this repository's release pages.
                if(!Uri.TryCreate(link,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.Host!="github.com"||!string.IsNullOrEmpty(uri.UserInfo)||!uri.IsDefaultPort||
                    !uri.AbsolutePath.StartsWith("/UnSpotibleShadow/OpenGDS2/releases/tag/",StringComparison.Ordinal))continue;
                if(Compare(version,installed) is >0&&(newest is null||Compare(version,newest.Version) is >0))newest=new(version,link);
            }
            return newest;
        }
        catch(JsonException){return null;}
    }
}
