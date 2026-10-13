namespace OpenGDS2.Domain;
/// <summary>fGMSyukka 009A4D78 and fGMSyukkaSai 00997D6C.</summary>
public static class HeldShipmentRules
{
    public static long Step(int ticks,bool quantity)
    {
        if(ticks<1)throw new ArgumentOutOfRangeException(nameof(ticks));
        if(!quantity)return ticks<=20?10:100;
        return ticks<=10?100:ticks<=20?1000:ticks<=30?10000:ticks<=40?30000:ticks<=60?100000:1000000;
    }
}
