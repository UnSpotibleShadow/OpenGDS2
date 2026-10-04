using Godot;

namespace OpenGDS2.UI;

/// <summary>Simple community-drawn marker; no original pixels are bundled.</summary>
public partial class EmployeeSelectionArrow : Control
{
    public override void _Draw()
    {
        Vector2[] points=[new(3,0),new(5,0),new(5,5),new(8,5),new(4,10),new(0,5),new(3,5)];
        DrawColoredPolygon(points.Select(p=>p*Size/new Vector2(8,10)).ToArray(),new Color(.9f,.05f,.05f));
    }
}
