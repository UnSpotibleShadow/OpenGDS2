namespace OpenGDS2.Domain;

/// <summary>fGMKaiwa timer/click handlers, 00998AB0 and 00981674.</summary>
public sealed class ConversationRevealState(string text)
{
    public const double TickSeconds=.020;
    public string Text {get;}=text ?? throw new ArgumentNullException(nameof(text));
    public int Cursor {get;private set;}
    public int MouthCycle {get;private set;}
    public int PortraitFrame {get;private set;}
    public bool Finished=>Cursor>Text.Length;
    public void Tick()
    {
        if (Finished) {PortraitFrame=0;return;}
        Cursor++;MouthCycle++;
        if (MouthCycle==2) PortraitFrame=0;
        if (MouthCycle is 4 or 8) PortraitFrame=1;
        if (MouthCycle==6) PortraitFrame=2;
        if (MouthCycle>=10) MouthCycle=0;
    }
    // First click reveals the current page; later clicks advance the queue.
    public bool Advance()
    {
        if (Finished) return true;
        Cursor=Text.Length;return false;
    }
    public int VisibleCharacters
    {
        get
        {
            int end=Math.Min(Cursor,Text.Length);
            if (end>0 && end<Text.Length && char.IsHighSurrogate(Text[end-1]) && char.IsLowSurrogate(Text[end])) end--;
            int count=0;foreach (var rune in Text.AsSpan(0,end).EnumerateRunes()) count++;return count;
        }
    }
}
