namespace OpenGDS2.Domain;

/// <summary>40 ms office cadence without replaying missed native loop iterations.</summary>
public static class OfficeTickScheduler
{
    public static bool Advance(ref double remainder,double delta)
    {
        if(!double.IsFinite(delta)||delta<0)throw new ArgumentOutOfRangeException(nameof(delta));
        double elapsed=remainder+delta,interval=OfficeStaffRules.TickSeconds;
        if(elapsed+1e-12<interval){remainder=elapsed;return false;}
        // Keep ordinary frame quantization below one tick. A stalled frame
        // discards missed iterations; subsequent frames cannot replay them.
        remainder=elapsed+1e-12>=2*interval?0:Math.Max(0,elapsed-interval);
        return true;
    }
}
