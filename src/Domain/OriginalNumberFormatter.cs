using System.Globalization;
using System.Text;
namespace OpenGDS2.Domain;

public enum NumberGrouping { Japanese, Thousands }

/// <summary>009E8AEC groups integer digits in fours (万/億/兆) or threes (comma), retaining zero-filled lower groups.</summary>
public static class OriginalNumberFormatter
{
    public static string Format(long value,NumberGrouping grouping)=>FormatDigits(value.ToString(CultureInfo.InvariantCulture),grouping);
    public static string FormatDigits(string integer,NumberGrouping grouping)
    {
        if(!Enum.IsDefined(grouping) || string.IsNullOrEmpty(integer))throw new ArgumentException("Invalid integer display.");
        bool negative=integer[0]=='-';string digits=negative?integer[1..]:integer;
        if(digits.Length==0 || digits.Any(c=>c is <'0' or >'9'))throw new ArgumentException("Only integer digits can be grouped.");
        int width=grouping==NumberGrouping.Japanese?4:3;
        string[] units=["","万","億","兆","京"];
        var text=new StringBuilder(negative?"-":"");int first=(digits.Length-1)%width+1;
        for(int offset=0;offset<digits.Length;)
        {
            int count=offset==0?first:width;text.Append(digits.AsSpan(offset,count));offset+=count;
            if(offset==digits.Length)break;
            int remaining=(digits.Length-offset)/width;
            // 京 extends the native long-number display for modern Int64 values.
            text.Append(grouping==NumberGrouping.Thousands?",":remaining<units.Length?units[remaining]:"×10^"+(remaining*4)+" ");
        }
        return text.ToString();
    }
}
