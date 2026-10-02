namespace OpenGDS2.Domain;

/// <summary>Planning meeting mode 0, 009EFA40 +00BB..0D86.</summary>
public static class DevelopmentFocusRules
{
    public static DevelopmentFocus Generate(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        int[] values=random.Next(7) switch
        {
            0=>[40,16,16,16],1=>[16,40,16,16],2=>[16,16,40,16],
            3=>[16,16,16,40],4=>[22,22,22,22],5=>[34,34,15,15],_=>[15,15,34,34]
        };
        int remaining=100-values.Sum(),index=0,draws=0;
        while (remaining>0)
        {
            if (++draws>100_000) throw new InvalidOperationException("Planning focus generation stalled.");
            if (random.Next(3)==0) {values[index]++;remaining--;}
            index=(index+1)%4;
        }
        return new(values[0],values[1],values[2],values[3]);
    }
}
