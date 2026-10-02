using Godot;

namespace OpenGDS2.UI;

/// <summary>Community-authored classic desktop controls surrounding imported graphics.</summary>
public static class ClassicTheme
{
    public static readonly Color Face = new(0.83f, 0.83f, 0.83f);
    public static Theme Create()
    {
        var theme = new Theme { DefaultFontSize = 16 };
        foreach (string type in new[] { "Button", "Label", "LineEdit", "OptionButton", "CheckButton", "CheckBox", "PopupMenu" })
        {
            theme.SetColor("font_color", type, Colors.Black);
            theme.SetColor("font_hover_color", type, Colors.Black);
            theme.SetColor("font_pressed_color", type, Colors.Black);
            theme.SetColor("font_focus_color", type, Colors.Black);
            theme.SetColor("font_disabled_color", type, new Color(0.42f,0.42f,0.42f));
        }
        foreach (string type in new[] { "Button", "OptionButton" })
        {
            theme.SetStylebox("normal", type, Bevel(Face, false));
            theme.SetStylebox("hover", type, Bevel(new Color(0.88f,0.88f,0.88f), false));
            theme.SetStylebox("pressed", type, Bevel(Face, true));
            theme.SetStylebox("disabled", type, Bevel(Face, false));
            var focus = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(0.15f,0.22f,0.45f) };
            focus.SetBorderWidthAll(1);
            theme.SetStylebox("focus", type, focus);
        }
        theme.SetStylebox("normal", "LineEdit", Bevel(Colors.White, true));
        theme.SetStylebox("panel", "PanelContainer", Bevel(Face, false));
        return theme;
    }

    private static StyleBoxTexture Bevel(Color color, bool pressed)
    {
        using var image = Image.CreateEmpty(8, 8, false, Image.Format.Rgb8);
        image.Fill(color);
        var shadow = new Color(0.35f,0.35f,0.35f);
        for (int x = 0; x < 8; x++)
        for (int y = 0; y < 8; y++)
        {
            if (x >= 6 || y >= 6) image.SetPixel(x, y, pressed ? Colors.White : shadow);
            else if (x < 2 || y < 2) image.SetPixel(x, y, pressed ? shadow : Colors.White);
        }
        var box = new StyleBoxTexture
        {
            Texture = ImageTexture.CreateFromImage(image),
            ContentMarginLeft = 8, ContentMarginRight = 8,
            ContentMarginTop = 4, ContentMarginBottom = 4,
        };
        box.SetTextureMarginAll(2);
        return box;
    }
}
