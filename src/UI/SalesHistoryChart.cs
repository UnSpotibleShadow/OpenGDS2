using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

/// <summary>Community-drawn classic chart using the recovered original scale.</summary>
public partial class SalesHistoryChart : Control
{
    private long[] _history=[];
    public bool Compact {get;set;}
    public long Ceiling=>_history.Length==SalesHistoryRules.CellCount
        ? Compact ? SalesHistoryRules.CompactChartCeiling(_history) : SalesHistoryRules.ChartCeiling(_history) : 0;
    public void SetHistory(IReadOnlyList<long> history)
    {
        SalesHistoryRules.Validate(history);_history=history.ToArray();QueueRedraw();
    }
    public override void _Draw()
    {
        if (_history.Length!=SalesHistoryRules.CellCount) return;
        var font=GetThemeDefaultFont();const int fontSize=14;
        const float left=76,top=12;
        float right=Size.X-16,bottom=Size.Y-32;
        if (right<=left || bottom<=top) return;
        var area=new Rect2(left,top,right-left,bottom-top);
        DrawRect(area,Colors.White);
        long ceiling=Ceiling;
        for (int i=0;i<=4;i++)
        {
            float y=bottom-(bottom-top)*i/4;
            DrawLine(new(left,y),new(right,y),new Color(.75f,.75f,.75f));
            DrawString(font,new(0,y+5),$"{ceiling*i/4:N0}",HorizontalAlignment.Right,left-8,fontSize,Colors.Black);
        }
        for (int i=0;i<SalesHistoryRules.CellCount;i++)
        {
            float x=left+(right-left)*i/(SalesHistoryRules.CellCount-1);
            DrawLine(new(x,top),new(x,bottom),new Color(.9f,.9f,.9f));
        }
        Vector2 Point(int i)=>new(left+(right-left)*i/(SalesHistoryRules.CellCount-1),
            bottom-(float)((double)_history[SalesHistoryRules.CellCount-1-i]/ceiling)*(bottom-top));
        for (int i=1;i<SalesHistoryRules.CellCount;i++)
            DrawLine(Point(i-1),Point(i),new Color(.65f,.05f,.05f),2);
        DrawRect(area,Colors.Black,false);
        DrawString(font,new(left,bottom+22),Tr(Compact ? "COMPACT_HISTORY_OLDEST" : "SALES_HISTORY_OLDEST"),HorizontalAlignment.Left,(right-left)/2,fontSize,Colors.Black);
        DrawString(font,new(left+(right-left)/2,bottom+22),Tr(Compact ? "COMPACT_HISTORY_NEWEST" : "SALES_HISTORY_CURRENT"),HorizontalAlignment.Right,(right-left)/2,fontSize,Colors.Black);
        if(Compact) DrawString(font,new(left,bottom+22),Tr("COMPACT_HISTORY_MIDDLE"),HorizontalAlignment.Center,right-left,fontSize,Colors.Black);
    }
}
