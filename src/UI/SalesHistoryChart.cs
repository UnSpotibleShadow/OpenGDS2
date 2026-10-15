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
        var font=GetThemeDefaultFont();int fontSize=Compact?8:14;
        float left=Compact?41:76,top=Compact?10:12;
        float right=Size.X-(Compact?12:16),bottom=Size.Y-(Compact?20:32);
        if (right<=left || bottom<=top) return;
        if(Compact){DrawRect(new Rect2(Vector2.Zero,Size),Colors.White);DrawRect(new Rect2(Vector2.Zero,Size),new Color(.6f,.6f,.6f),false);}
        var area=new Rect2(left,top,right-left,bottom-top);
        DrawRect(area,Colors.White);
        long ceiling=Ceiling;
        for (int i=0;i<=4;i++)
        {
            float y=bottom-(bottom-top)*i/4;
            DrawLine(new(left,y),new(right,y),new Color(.75f,.75f,.75f));
            DrawString(font,new(0,y+5),Compact?(i==0?"0":$"{ceiling*i/4000.0:0.#}k"):$"{DisplayNumbers.Format(ceiling*i/4)}",HorizontalAlignment.Right,left-8,fontSize,Colors.Black);
        }
        if(!Compact)for (int i=0;i<SalesHistoryRules.CellCount;i++)
        {
            float x=left+(right-left)*i/(SalesHistoryRules.CellCount-1);
            DrawLine(new(x,top),new(x,bottom),new Color(.9f,.9f,.9f));
        }
        Vector2 Point(int i)=>new(left+(right-left)*i/(SalesHistoryRules.CellCount-1),
            bottom-(float)((double)_history[SalesHistoryRules.CellCount-1-i]/ceiling)*(bottom-top));
        float cell=(right-left)/(SalesHistoryRules.CellCount-1);
        for(int i=1;i<SalesHistoryRules.CellCount;i++)
        {
            if(_history[SalesHistoryRules.CellCount-1-i]==0)continue;
            var point=Point(i);float width=Mathf.Max(3,Mathf.Floor(cell*.65f));
            float depth=Mathf.Min(Compact?2:4,Mathf.Floor((bottom-point.Y)/2));
            float x=Mathf.Floor(point.X-width-depth),y=Mathf.Max(top+depth,Mathf.Floor(point.Y));
            var front=Compact?new Color(0,1,0):new Color(.8f,.1f,.1f);
            DrawRect(new Rect2(x,y,width,Mathf.Max(1,bottom-y)),front);
            if(depth>0)
            {
                DrawColoredPolygon([new(x,y),new(x+depth,y-depth),new(x+width+depth,y-depth),new(x+width,y)],Compact?new Color(.55f,1,.55f):new Color(1,.5f,.5f));
                DrawColoredPolygon([new(x+width,y),new(x+width+depth,y-depth),new(x+width+depth,bottom-depth),new(x+width,bottom)],Compact?new Color(0,.5f,0):new Color(.45f,0,0));
            }
        }
        if(Compact){DrawLine(new(left,top),new(left,bottom),Colors.Black);DrawLine(new(left,bottom),new(right,bottom),Colors.Black);}
        else DrawRect(area,Colors.Black,false);
        DrawString(font,new(left,bottom+(Compact?16:22)),Tr(Compact ? "COMPACT_HISTORY_OLDEST" : "SALES_HISTORY_OLDEST"),HorizontalAlignment.Left,(right-left)/2,fontSize,Colors.Black);
        DrawString(font,new(left+(right-left)/2,bottom+(Compact?16:22)),Tr(Compact ? "COMPACT_HISTORY_NEWEST" : "SALES_HISTORY_CURRENT"),HorizontalAlignment.Right,(right-left)/2,fontSize,Colors.Black);
        if(Compact) DrawString(font,new(left,bottom+(Compact?16:22)),Tr("COMPACT_HISTORY_MIDDLE"),HorizontalAlignment.Center,right-left,fontSize,Colors.Black);
    }
}
