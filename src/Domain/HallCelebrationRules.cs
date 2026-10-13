namespace OpenGDS2.Domain;

public sealed class HallCelebrationState
{
    public int Ticks {get;set;}
    public int Frame {get;set;}
}

/// <summary>fGMCrossRDen 00982814: copy frame 1 at 3, frame 2 at 5, reset.</summary>
public static class HallCelebrationRules
{
    public static bool Advance(GameState state,ReleasedGameState product)
    {
        if(product.HallCelebration is not { } scene)return false;
        // The native callback tests the retained counter before incrementing it.
        if(scene.Ticks==3)scene.Frame=1;
        else if(scene.Ticks>=5){scene.Frame=2;scene.Ticks=0;}
        scene.Ticks++;state.HallCelebrationTicks=scene.Ticks;return true;
    }
    public static bool Acknowledge(ReleasedGameState product)
    {
        if(product.HallCelebration is null)return false;
        product.HallCelebration=null;return true;
    }
    public static void Validate(ReleasedGameState product)
    {
        if(product.HallCelebration is not { } scene)return;
        if(scene.Ticks is <0 or >5 || scene.Frame is <0 or >2 || !product.HallOfFame || product.ReviewPending ||
            product.ReviewScores is not {Count:4} scores || scores.Sum()<32)
            throw new InvalidDataException("Invalid Hall of Fame celebration.");
    }
}
