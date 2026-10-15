using System.Globalization;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;

// Presentation preference only: never changes a saved number or simulation rule.
public static class DisplayNumbers
{
    public static NumberGrouping Grouping {get;set;}=NumberGrouping.Thousands;
    public static IFormatProvider Provider {get;}=new NumberProvider();
    public static string Format(object? value)
    {
        if(value is null)return "";
        if(value is not IFormattable number)return value.ToString()??"";
        string digits=number.ToString("0",CultureInfo.InvariantCulture);
        return OriginalNumberFormatter.FormatDigits(digits,Grouping);
    }
    public static string ToDisplayNumber(this IFormattable value)=>Format(value);
    private sealed class NumberProvider : IFormatProvider,ICustomFormatter
    {
        public object? GetFormat(Type? type)=>type==typeof(ICustomFormatter)?this:CultureInfo.CurrentCulture.GetFormat(type!);
        public string Format(string? format,object? argument,IFormatProvider? provider)=>format is "N0" or "n0"?DisplayNumbers.Format(argument):
            argument is IFormattable numeric?numeric.ToString(format,CultureInfo.CurrentCulture):argument?.ToString()??"";
    }
}
