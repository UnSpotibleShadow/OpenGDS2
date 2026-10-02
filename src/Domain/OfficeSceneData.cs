namespace OpenGDS2.Domain;

/// <summary>Local original initializer data; never bundled with the project.</summary>
public sealed class OfficeSceneData
{
    public string SourceHash { get; set; } = string.Empty;
    public List<OfficeSpriteDefinition> Sprites { get; set; } = [];
}

public sealed record OfficeSpriteDefinition(int Id, int X, int Y, int SourceX, int SourceY,
    int Width, int Height, int DepthOffset)
{
    // VB6 CI2Var uses round-to-even, not truncation.
    public int SortKey => checked((int)Math.Round(Y + DepthOffset + Id / 1000.0, MidpointRounding.ToEven));
}
