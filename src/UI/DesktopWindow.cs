using Godot;
using System;

namespace OpenGDS2.UI;

/// <summary>Community-written child-window chrome. Coordinates are desktop pixels.</summary>
public partial class DesktopWindow : PanelContainer
{
    public VBoxContainer Body { get; private set; } = null!;
    public Action? CloseRequested { get; set; }
    private Control _caption = null!;
    private Label _title = null!;
    private Vector2 _dragOrigin;
    private Vector2 _mouseOrigin;
    private bool _dragging;
    private bool _resizing;
    private float _resizeStartHeight, _resizeStartMouseY, _minimumHeight;
    public bool IsDragging=>_dragging || _resizing;

    public void EnableVerticalResize()
    {
        _minimumHeight=Size.Y;
        var grip=new Control {Name="VerticalResizeHandle",CustomMinimumSize=new(0,6),MouseDefaultCursorShape=CursorShape.Vsize,TooltipText=""};
        Body.GetParent().AddChild(grip);
        grip.GuiInput+=input=>
        {
            if(input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:true} && !_minimized && _restore is null)
            {
                _resizing=true;_resizeStartHeight=Size.Y;_resizeStartMouseY=GetGlobalMousePosition().Y;MoveToFront();grip.AcceptEvent();
            }
        };
    }
    private TextureRect? _icon;
    public void SetNativeIcon(string windowName)
    {
        var texture=NativeWindowArtwork.Load(windowName);
        if(_icon is null){_icon=new TextureRect {Name="WindowTitleIcon",Position=new(2,3),Size=new(16,16),ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,TextureFilter=TextureFilterEnum.Nearest,MouseFilter=MouseFilterEnum.Ignore};_caption.AddChild(_icon);}
        _icon.Texture=texture;_icon.Visible=texture is not null;_title.Position=new(texture is null?5:21,1);
        _title.Size=new(Math.Max(0,Size.X-_title.Position.X-_captionReserve),20);
    }
    public override void _Input(InputEvent input)
    {
        if(input is InputEventMouseButton {ButtonIndex:MouseButton.Left,Pressed:false}) {_dragging=false;_resizing=false;}
        if(_resizing && input is InputEventMouseMotion && _bounds is not null)
        {
            float maximum=Math.Max(_minimumHeight,_bounds.Size.Y-Position.Y);
            Size=new(Size.X,Math.Clamp(_resizeStartHeight+GetGlobalMousePosition().Y-_resizeStartMouseY,_minimumHeight,maximum));
            _expandedSize=Size;
        }
    }
    public override void _Notification(int what)
    {if(what==NotificationApplicationFocusOut||what==NotificationVisibilityChanged&&!IsVisibleInTree()){_dragging=false;_resizing=false;}}
    private bool _minimized;
    private Rect2? _restore;
    private Vector2 _expandedSize;
    private Control? _bounds;
    private int _captionReserve=70;

    public void Initialize(string title, Vector2 position, Vector2 size, bool canMaximize = true)
    {
        _captionReserve=canMaximize?70:48;AddToGroup("GameplayWindows");Position = position; Size = size; _expandedSize = size;
        AddThemeStyleboxOverride("panel", ClassicTheme.WindowFrame());
        var layout = new VBoxContainer(); layout.AddThemeConstantOverride("separation", 2); AddChild(layout);
        _caption = new Control { Name="WindowCaption",CustomMinimumSize = new Vector2(0, 22), MouseFilter = MouseFilterEnum.Stop };
        layout.AddChild(_caption);
        var tint = new ColorRect { Color = new Color(0.68f, 0.77f, 0.85f), MouseFilter = MouseFilterEnum.Ignore };
        tint.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); _caption.AddChild(tint);
        _title = new Label { Text = title, Position = new Vector2(5, 1), MouseFilter = MouseFilterEnum.Ignore };
        _title.AddThemeFontSizeOverride("font_size", 13); _caption.AddChild(_title);
        var controls = new HBoxContainer { AnchorLeft = 1, AnchorRight = 1, OffsetLeft = canMaximize ? -66 : -44,
            OffsetRight = -2, OffsetTop = 2, OffsetBottom = 20 };
        controls.AddThemeConstantOverride("separation", 2); _caption.AddChild(controls);
        CaptionButton(controls, "MinimizeWindow", "−", () =>
        {
            _minimized = !_minimized;
            if (_minimized) { _expandedSize = Size; Body.Hide(); CustomMinimumSize = Vector2.Zero; Size = new Vector2(Size.X, 28); }
            else { Body.Show(); Size = _expandedSize; }
        });
        if (canMaximize) CaptionButton(controls, "MaximizeWindow", "□", () =>
        {
            if (_minimized) { _minimized = false; Body.Show(); Size = _expandedSize; }
            if (_restore is Rect2 saved) { Position = saved.Position; Size = saved.Size; _restore = null; FitInsideWorkspace(); }
            else { _restore = new Rect2(Position, Size); Position = Vector2.Zero; Size = ((Control)GetParent()).Size; }
        });
        CaptionButton(controls, "CloseWindow", "×", () => { if (CloseRequested is not null) CloseRequested(); else Hide(); });
        Body = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        Body.AddThemeConstantOverride("separation", 2); layout.AddChild(Body);
        _caption.GuiInput += OnCaptionInput;
        GuiInput += e => { if (e is InputEventMouseButton { Pressed: true }) MoveToFront(); };
        Resized += () => _title.Size = new Vector2(Math.Max(0, Size.X - _title.Position.X - _captionReserve), 20);
        _title.ClipText = true;
        _title.Size = new Vector2(Math.Max(0, Size.X - _title.Position.X - _captionReserve), 20);
        _bounds = GetParent() as Control;
        if (_bounds is not null) _bounds.Resized += FitInsideWorkspace;
        FitInsideWorkspace();SetNativeIcon(Name.ToString());
    }

    public override void _ExitTree()
    {
        if (_bounds is not null && GodotObject.IsInstanceValid(_bounds)) _bounds.Resized -= FitInsideWorkspace;
        _bounds = null;_dragging=false;_resizing=false;
    }

    private void FitInsideWorkspace()
    {
        if (_bounds is null || _bounds.Size.X <= 0 || _bounds.Size.Y <= 0) return;
        if (_restore is not null) { Position = Vector2.Zero; Size = _bounds.Size; }
        else Position = new Vector2(Math.Clamp(Position.X, 0, Math.Max(0, _bounds.Size.X - Size.X)),
            Math.Clamp(Position.Y, 0, Math.Max(0, _bounds.Size.Y - 24)));
    }

    public void SetTitle(string title) => _title.Text = title;

    public void SetPositionInsideWorkspace(Vector2 position)
    {
        Position = position;
        FitInsideWorkspace();
    }

    public void Reopen()
    {
        if (_minimized) { _minimized = false; Body.Show(); Size = _expandedSize; }
        FitInsideWorkspace();
        Show(); MoveToFront();
    }

    private static void CaptionButton(HBoxContainer parent, string name, string text, Action action)
    {
        var button = new Button { Name = name, Text = text, CustomMinimumSize = new Vector2(20, 18), FocusMode = FocusModeEnum.None };
        button.AddThemeFontSizeOverride("font_size", 12);
        button.AddThemeStyleboxOverride("normal", ClassicTheme.CompactButton());
        button.AddThemeStyleboxOverride("pressed", ClassicTheme.CompactButton(true));
        button.Pressed += () => action(); parent.AddChild(button);
    }

    private void OnCaptionInput(InputEvent input)
    {
        if (input is InputEventMouseButton { ButtonIndex: MouseButton.Left } button)
        {
            _dragging = button.Pressed; _dragOrigin = Position; _mouseOrigin = GetGlobalMousePosition();
            if (button.Pressed) MoveToFront();
        }
        if (_dragging && input is InputEventMouseMotion && _restore is null)
        {
            var requested = _dragOrigin + GetGlobalMousePosition() - _mouseOrigin;
            var bounds = ((Control)GetParent()).Size;
            Position = new Vector2(Math.Clamp(requested.X, 0, Math.Max(0, bounds.X - Size.X)),
                Math.Clamp(requested.Y, 0, Math.Max(0, bounds.Y - 24)));
            AcceptEvent();
        }
    }
}
