namespace OpenGDS2.Domain;

/// <summary>VB-family 24-bit Rnd recurrence; seed initialization is a modern policy.</summary>
public sealed class VbSimulationRandom : Random
{
    public const uint Mask=0xFFFFFF;
    public uint State {get;private set;}
    public Action<uint>? StateChanged {get;set;}
    public VbSimulationRandom(uint state)=>Restore(state);
    public void Restore(uint state)
    {if(state>Mask)throw new ArgumentOutOfRangeException(nameof(state));State=state;}
    public override float NextSingle()
    {State=unchecked(State*0x43FD43FD+0xC39EC3)&Mask;StateChanged?.Invoke(State);return State/16777216f;}
    protected override double Sample()=>NextSingle();
    public override double NextDouble()=>NextSingle();
    public override int Next()=>Next(int.MaxValue);
    public override int Next(int maxValue)
    {if(maxValue<0)throw new ArgumentOutOfRangeException(nameof(maxValue));return (int)Math.Floor((double)NextSingle()*maxValue);}
    public override int Next(int minValue,int maxValue)
    {if(minValue>maxValue)throw new ArgumentOutOfRangeException(nameof(minValue));return checked((int)(minValue+(long)Math.Floor(NextDouble()*((long)maxValue-minValue))));}
}
