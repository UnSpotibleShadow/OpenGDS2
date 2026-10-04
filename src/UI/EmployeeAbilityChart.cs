using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

/// <summary>Community-drawn version of fGMInfo's five-axis employee chart.</summary>
public partial class EmployeeAbilityChart : Control
{
    public EmployeeState Employee {get;set;}=null!;
    public override void _Draw()
    {
        if (Employee is null) return;
        var font=GetThemeDefaultFont();var center=new Vector2(Size.X/2,76);
        const float scale=1.6f;
        Vector2[] axes=[new(0,-32),new(32,-8),new(24,24),new(-24,24),new(-32,-8)];
        DrawRect(new Rect2(new Vector2(center.X-62,12),new Vector2(124,124)),new Color(.68f,1,1));
        var outline=axes.Select(v=>center+v*scale).ToArray();
        DrawPolyline(outline.Append(outline[0]).ToArray(),new Color(.25f,.35f,.35f),1);
        foreach (var v in outline) DrawLine(center,v,new Color(.48f,.68f,.68f),1);
        int?[] values=[Employee.Fun,Employee.Novelty,Employee.Accuracy,Employee.Music,Employee.Graphics];
        if (values.All(v=>v.HasValue))
        {
            // 009DF294 +057E..0BAA divides each raw ability by 100. Higher
            // values are not normalized to the employee's highest ability.
            var points=axes.Select((v,i)=>center+v*(values[i]!.Value/100f)*scale).ToArray();
            DrawPolyline(points.Append(points[0]).ToArray(),Colors.Red,1.5f);
            foreach (var point in points) DrawCircle(point,1.5f,Colors.Red);
        }
        string[] keys=["STAT_FUN","STAT_NOVELTY","STAT_ACCURACY","STAT_MUSIC","STAT_GRAPHICS"];
        Vector2[] labels=[new(center.X,10),new(Size.X-32,57),new(Size.X-37,125),new(34,125),new(33,57)];
        for (int i=0;i<5;i++)
        {
            string text=Tr(keys[i]);float width=font.GetStringSize(text,HorizontalAlignment.Left,-1,11).X;
            DrawString(font,labels[i]-new Vector2(width/2,0),text,HorizontalAlignment.Left,-1,11,Colors.Black);
            text=values[i]?.ToString() ?? "?";width=font.GetStringSize(text,HorizontalAlignment.Left,-1,12).X;
            DrawString(font,labels[i]+new Vector2(-width/2,14),text,HorizontalAlignment.Left,-1,12,Colors.Black);
        }
    }
}
