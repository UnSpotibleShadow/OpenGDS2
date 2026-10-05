namespace OpenGDS2.Domain;

/// <summary>fGMSyukka mode 2, +0455..067B. Applied before common release rolls/clamp.</summary>
public static class MangaDevelopmentRules
{
    public static double MarketModifier(int popularity)
    {
        if (popularity<0) throw new ArgumentOutOfRangeException(nameof(popularity));
        return popularity>=10 ? .7 : popularity>=6 ? .5 : popularity>=3 ? .2 : -.2;
    }
}
