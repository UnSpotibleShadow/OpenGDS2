using Godot;
namespace OpenGDS2.UI;
public partial class ReviewSpeechBubble:Control
{
    public override void _Draw()
    {
        Vector2[] outline=[new(4,1),new(Size.X-1,1),new(Size.X-1,Size.Y-2),new(4,Size.Y-2),new(4,38),new(1,34),new(4,30)];
        DrawColoredPolygon(outline,Colors.White);DrawPolyline(outline.Append(outline[0]).ToArray(),Colors.Black,1);
    }
}
