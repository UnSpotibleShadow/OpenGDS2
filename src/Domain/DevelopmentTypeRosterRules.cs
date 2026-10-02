namespace OpenGDS2.Domain;

/// <summary>fGMalpha +0184..05F3 roster phase, before cosmetic example-title RNG.</summary>
public static class DevelopmentTypeRosterRules
{
    public static IReadOnlyList<int> Build(OriginalCatalog catalog,AudienceDefinition audience,Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var types=catalog.GameTypes.OrderBy(x=>x.SourceIndex).ToArray();
        if (types.Length<2 || !types.Select(x=>x.SourceIndex).SequenceEqual(Enumerable.Range(0,types.Length)))
            throw new InvalidDataException("Original type definitions must be contiguous from zero.");
        var compatible=types.Select(x=>DevelopmentSelectionRules.MatchesAudience(x,audience)).ToArray();
        var result=new List<int>();
        for (int source=0;source<types.Length;source++)
            if (random.Next(6)==0 && compatible[source]) result.Add(source);
        int draws=0;
        while (result.Count<5)
            for (int source=0;source<types.Length;source++)
            {
                if (++draws>100_000) throw new InvalidOperationException("Original type roster generation stalled.");
                if (random.Next(10)!=0) continue;
                // +0551..057A: ANY different old entry permits appending.
                if (result.Count==0 || result.Any(x=>x!=source)) result.Add(source);
                if (result.Count>=5) break;
            }
        return result;
    }
}
