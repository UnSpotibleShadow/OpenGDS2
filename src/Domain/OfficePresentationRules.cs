namespace OpenGDS2.Domain;

public static class OfficePresentationRules
{
    public static IEnumerable<OfficeSpriteDefinition> ForState(OfficeSceneData scene,GameState state)
    {
        if (state.DevelopmentComputerIndex is <0 or >3) throw new InvalidDataException("Invalid development computer.");
        var sprites=state.Year is int year && state.Month is int month && state.Week is int week
            ? ForDate(scene,new(year,month,week))
            : scene.Sprites.Where(x=>x.Id!=144 && x.Id!=145 && (long)x.X+x.Width>0 && x.Y>0);
        foreach (var sprite in sprites)
            yield return sprite.Id==140 && state.DevelopmentComputerIndex is int computer
                ? sprite with {X=91,Y=96,SourceX=571+computer*74,SourceY=318} : sprite;
    }

    public static IEnumerable<OfficeSpriteDefinition> ForDate(OfficeSceneData scene, GameDate date)
    {
        CalendarRules.Validate(date);
        foreach (var original in scene.Sprites)
        {
            // Initializer 5231/527F stores these allocated IDs in members E0/F0.
            var sprite = original.Id switch
            {
                144 => original with { SourceX = date.Month <= 6 ? 949 : 978,
                    SourceY = 228 + 22 * (date.Month <= 6 ? date.Month - 1 : date.Month - 7) },
                145 => original with { SourceX = 949, SourceY = 228 + 22 * (date.Week - 1) },
                _ => original
            };
            // Module2 11+D6D2: only X+Width>0 and Y>0 enter the draw list.
            if ((long)sprite.X + sprite.Width > 0 && sprite.Y > 0) yield return sprite;
        }
    }
}
