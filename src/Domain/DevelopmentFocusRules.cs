namespace OpenGDS2.Domain;

/// <summary>Planning meeting mode 0, 009EFA40 +00BB..0D86.</summary>
public static class DevelopmentFocusRules
{
    public static DevelopmentFocus Generate(Random random)
        =>GenerateProposal(random).Focus;

    public static (int Profile,DevelopmentFocus Focus) GenerateProposal(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        int profile=random.Next(7);
        int[] values=profile switch
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
        return (profile,new(values[0],values[1],values[2],values[3]));
    }
    public static IEnumerable<string> DialogueKeys(int profile,int releasedCount)
    {
        if (profile is <0 or >6 || releasedCount<0) throw new ArgumentOutOfRangeException();
        if (releasedCount<=3) yield return "MEETING_INTRO";
        int[][] lines=[[0,1,2],[3,4,5,6],[7,8,9],[10,11,12,13,14],[15,16,17,18],[19,20,21,22],[23,24,25,26]];
        foreach (int line in lines[profile])
        {
            bool earlyOnly=line is 2 or 6 or 9 or 13 or 18 or 22;
            if (!earlyOnly || releasedCount<=2) yield return $"MEETING_LINE_{line}";
        }
    }
}
