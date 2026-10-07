namespace OpenGDS2.Domain;

public static class OfficePresentationRules
{
    public static IEnumerable<OfficeSpriteDefinition> ForState(OfficeSceneData scene,GameState state)
    {
        if (state.DevelopmentComputerIndex is <0 or >3) throw new InvalidDataException("Invalid development computer.");
        GameDate? date=state.Year is int year && state.Month is int month && state.Week is int week ? new(year,month,week) : null;
        if (date is not null) CalendarRules.Validate(date.Value);
        var assigned=state.Employees.Where(x=>x.Office?.Action is >=6 and <=17)
            .Select(x=>x.Office!.Action).ToHashSet();
        foreach (var original in scene.Sprites)
        {
            if (date is null && original.Id is 144 or 145) continue;
            var sprite=date is null ? original : DateSprite(original,date.Value);
            if (sprite.Id==141 && SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex) is int portrait)
                sprite=sprite with {SourceX=48+(portrait-1)*152};
            if (state.DevelopmentComputerIndex is int computer)
            {
                if (sprite.Id==140) sprite=sprite with {X=91,Y=96,SourceX=571+computer*74,SourceY=318};
                else if (sprite.Id is >=50 and <=61)
                {
                    int action=6+(sprite.Id-50);
                    if (assigned.Contains(action)) sprite=OfficeDeskPresentationRules.ForAssignment(sprite,action,computer);
                }
            }
            sprite=OfficeBurstFlameRules.Project(sprite,state);
            if ((long)sprite.X+sprite.Width>0 && sprite.Y>0) yield return sprite;
        }
    }

    public static IEnumerable<OfficeSpriteDefinition> ForDate(OfficeSceneData scene, GameDate date)
    {
        CalendarRules.Validate(date);
        foreach (var original in scene.Sprites)
        {
            // Initializer 5231/527F stores these allocated IDs in members E0/F0.
            var sprite = DateSprite(original,date);
            // Module2 11+D6D2: only X+Width>0 and Y>0 enter the draw list.
            if ((long)sprite.X + sprite.Width > 0 && sprite.Y > 0) yield return sprite;
        }
    }

    private static OfficeSpriteDefinition DateSprite(OfficeSpriteDefinition original,GameDate date)=>original.Id switch
    {
        144=>original with {SourceX=date.Month<=6 ? 949 : 978,SourceY=228+22*(date.Month<=6 ? date.Month-1 : date.Month-7)},
        145=>original with {SourceX=949,SourceY=228+22*(date.Week-1)},
        _=>original
    };
}
