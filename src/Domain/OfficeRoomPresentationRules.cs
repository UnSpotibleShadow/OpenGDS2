namespace OpenGDS2.Domain;

/// <summary>Saved presentation phase; native room counters are independent of actor effects.</summary>
public sealed class OfficeRoomPresentationState
{
    public int[] PanelTicks {get;set;}=new int[2];
    public int[] PanelSourceX {get;set;}=[259,259];
    public int CompletionTicks {get;set;}
    public int[] CompletionSourceX {get;set;}=[948,948];
}

/// <summary>00A0283C +0368..0A36; room label, progress digits and 62x31 panel.</summary>
public static class OfficeRoomPresentationRules
{
    public static void Tick(GameState state)
    {
        var phases=state.OfficeRoomPresentation;
        foreach(var p in state.DevelopmentProjects.OrderBy(p=>p.RoomIndex))
        {
            int room=p.RoomIndex;
            if(p.OriginalRoomPaused) continue;
            if(phases.PanelTicks[room]==6) phases.PanelSourceX[room]=259;
            else if(phases.PanelTicks[room]==12)
            {phases.PanelSourceX[room]=321;phases.PanelTicks[room]=0;}
            phases.PanelTicks[room]++;
            if(p.OriginalRoomState==1 && p.OriginalStage==3 && p.Bugs==0)
            {
                if(++phases.CompletionTicks==1) phases.CompletionSourceX[room]=948;
                else if(phases.CompletionTicks==8) phases.CompletionSourceX[room]=1005;
                else if(phases.CompletionTicks>=16) phases.CompletionTicks=0;
            }
        }
    }

    public static OfficeSpriteDefinition Project(OfficeSpriteDefinition sprite,GameState state)
    {
        if(sprite.Id is <87 or >92) return sprite;
        int room=sprite.Id switch {87 or 88 or 91=>0,_=>1};
        var p=state.DevelopmentProjects.SingleOrDefault(p=>p.RoomIndex==room);
        if(p is null) return sprite with {X=-100,Y=0};
        if(sprite.Id is 91 or 92)
            return sprite with {X=162+96*room,Y=131+48*room,
                SourceX=state.OfficeRoomPresentation.PanelSourceX[room],SourceY=226,Width=62,Height=31};
        bool digits=sprite.Id is 88 or 90;
        if(digits)
        {
            if(p.OriginalRoomPaused || p.OriginalRoomState==1 && p.OriginalStage==3)
                return sprite with {X=-100,Y=0};
            int percent=Percentage(p);
            return sprite with {X=194+96*room,Y=129+48*room,
                SourceX=383+17*(percent/10),SourceY=226+12*(percent%10)};
        }
        (int x,int y)=p.OriginalRoomPaused ? (569,357) : p.OriginalRoomState==2 ? (792,369) :
            p.OriginalStage!=3 ? (261,359) : p.Bugs>0 ? (459,380) :
            (state.OfficeRoomPresentation.CompletionSourceX[room],360);
        return sprite with {X=164+96*room,Y=127+48*room,SourceX=x,SourceY=y};
    }

    public static int Percentage(DevelopmentProjectState p)
    {
        double numerator=p.Progress,denominator=p.RequiredPoints;
        if(p.OriginalRoomState==2 && p.PolishingTargets is { } t && p.HardwareDesign is { } h)
        {
            numerator+=p.Quality.Fun+p.Quality.Novelty+p.Quality.Graphics+p.Quality.Music+
                OwnHardwareService.RequiredTechnologies.Sum(id=>h.TechnologyFlags.GetValueOrDefault(id));
            denominator+=t.Fun+t.Novelty+t.Graphics+t.Music+4;
        }
        if(denominator<=0 || !double.IsFinite(numerator/denominator)) return 0;
        return Math.Clamp((int)Math.Floor(numerator/denominator*100),0,100);
    }

    public static void Validate(GameState state)
    {
        if(state.OfficeRoomPresentation is not {PanelTicks.Length:2,PanelSourceX.Length:2,
            CompletionSourceX.Length:2} p || p.PanelTicks.Any(x=>x is <0 or >12) ||
            p.PanelSourceX.Any(x=>x is not (259 or 321)) || p.CompletionTicks is <0 or >15 ||
            p.CompletionSourceX.Any(x=>x is not (948 or 1005)))
            throw new InvalidDataException("Invalid office room presentation phase.");
    }
}
