namespace OpenGDS2.Domain;

// Stable save IDs. Do not reorder; English wording is outside the domain.
public enum ShipmentMessage
{
    AdaptationHuge=0,AdaptationPopular=1,AdaptationReaders=2,AdaptationUnpopular=3,
    BalanceHuge=4,BalanceHit=5,BalancePoor=6,NoveltyHuge=7,NoveltyHit=8,NoveltyRejected=9,
    FunHuge=10,FunHit=11,MusicNotice=12,PriceCheap=13,PriceAbsurd=14,PriceVeryHigh=15,
    PriceHigh=16,PriceSlightlyHigh=17,Reviews=18
}
public static class ShipmentDialogueRules
{
    public static ShipmentMessage Adaptation(int popularity)=>popularity switch
    {>=10=>ShipmentMessage.AdaptationHuge,>=6=>ShipmentMessage.AdaptationPopular,>=3=>ShipmentMessage.AdaptationReaders,_=>ShipmentMessage.AdaptationUnpopular};
    public static ShipmentMessage? Price(long price)=>price switch
    {<4000=>ShipmentMessage.PriceCheap,>=20000=>ShipmentMessage.PriceAbsurd,>=10000=>ShipmentMessage.PriceVeryHigh,>=8000=>ShipmentMessage.PriceHigh,>=7000=>ShipmentMessage.PriceSlightlyHigh,_=>null};
    public static string MessageKey(ShipmentMessage message)
        =>Enum.IsDefined(message) ? $"SHIPMENT_{message}" : throw new ArgumentOutOfRangeException(nameof(message));
    public static bool HasPending(ReleasedGameState product)
        =>product.ShipmentMessages is not null && product.ShipmentMessageIndex<product.ShipmentMessages.Count;
    public static bool Acknowledge(GameState state,ReleasedGameState product)
    {
        if (!state.ReleasedGames.Contains(product) || !product.ReviewPending || !HasPending(product)) return false;
        product.ShipmentMessageIndex++;return true;
    }
    private static int Order(ShipmentMessage message)=>(int)message switch
    {<=3=>0,<=6=>1,<=9=>2,<=11=>3,12=>4,<=17=>5,18=>6,_=>throw new InvalidDataException("Unknown shipment message.")};
    public static void Validate(ReleasedGameState product)
    {
        var messages=product.ShipmentMessages;
        if (messages is null) {if (product.ShipmentMessageIndex!=0) throw new InvalidDataException("Missing shipment queue.");return;}
        if (messages.Count is <1 or >7 || product.ShipmentMessageIndex<0 || product.ShipmentMessageIndex>messages.Count ||
            messages.Any(x=>!Enum.IsDefined(x)) || messages[^1]!=ShipmentMessage.Reviews ||
            messages.Zip(messages.Skip(1)).Any(x=>Order(x.First)>=Order(x.Second)) ||
            (product.OriginalMode==2)!=(Order(messages[0])==0) || HasPending(product) && !product.ReviewPending)
            throw new InvalidDataException("Invalid shipment dialogue queue.");
    }
}
