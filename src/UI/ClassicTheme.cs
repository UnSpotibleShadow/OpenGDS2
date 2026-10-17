using Godot;

namespace OpenGDS2.UI;

/// <summary>Community-authored classic desktop controls surrounding imported graphics.</summary>
public static class ClassicTheme
{
    public static readonly Color Face = new(0.83f, 0.83f, 0.83f);
    public static Theme Create()
    {
        var theme = new Theme { DefaultFontSize = 13 };
        foreach (string type in new[] { "Button", "MenuButton", "Label", "LineEdit", "OptionButton", "CheckButton", "CheckBox", "PopupMenu" })
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
        theme.SetColor("default_color", "RichTextLabel", Colors.Black);
        theme.SetStylebox("panel", "PopupMenu", Bevel(Face, false));
        theme.SetStylebox("normal", "LineEdit", Bevel(Colors.White, true));
        // The engine's default caret is pale against this classic white input.
        foreach (string type in new[] { "LineEdit", "TextEdit" })
        {
            theme.SetColor("caret_color", type, Colors.Black);
            theme.SetColor("selection_color", type, new Color(0.12f, 0.24f, 0.48f));
            theme.SetColor("font_selected_color", type, Colors.White);
        }
        var inputFocus = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(0.12f, 0.24f, 0.48f) };
        inputFocus.SetBorderWidthAll(2);
        theme.SetStylebox("focus", "LineEdit", inputFocus);
        theme.SetStylebox("panel", "PanelContainer", Bevel(Face, false));
        theme.SetStylebox("panel", "Tree", Bevel(Colors.White, true));
        theme.SetColor("font_color", "Tree", Colors.Black);
        theme.SetColor("font_selected_color", "Tree", Colors.Black);
        foreach (string state in new[] { "selected", "selected_focus" })
            theme.SetStylebox(state, "Tree", new StyleBoxFlat { BgColor = new Color(.68f, 1, .68f) });
        theme.SetColor("title_button_color", "Tree", Colors.Black);
        theme.SetStylebox("title_button_normal", "Tree", CompactButton());
        theme.SetStylebox("title_button_hover", "Tree", CompactButton());
        theme.SetStylebox("title_button_pressed", "Tree", CompactButton(true));
        theme.SetStylebox("panel","TooltipPanel",new StyleBoxFlat {BgColor=new Color(1,1,.88f),BorderColor=Colors.Black,
            BorderWidthLeft=1,BorderWidthRight=1,BorderWidthTop=1,BorderWidthBottom=1,
            ContentMarginLeft=6,ContentMarginRight=6,ContentMarginTop=4,ContentMarginBottom=4});
        theme.SetColor("font_color","TooltipLabel",Colors.Black);
        theme.SetColor("font_shadow_color","TooltipLabel",Colors.Transparent);
        theme.SetFontSize("font_size","TooltipLabel",13);
        return theme;
    }

    public static StyleBoxFlat WindowFrame()
    {
        var box = new StyleBoxFlat { BgColor = Face, BorderColor = new Color(0.4f, 0.5f, 0.6f),
            ContentMarginLeft = 3, ContentMarginRight = 3, ContentMarginTop = 3, ContentMarginBottom = 3 };
        box.SetBorderWidthAll(1); return box;
    }

    public static StyleBoxTexture CompactButton(bool pressed = false)
    {
        var box = Bevel(Face, pressed);
        box.ContentMarginLeft = 3; box.ContentMarginRight = 3;
        box.ContentMarginTop = 1; box.ContentMarginBottom = 1;
        return box;
    }

    public static StyleBoxTexture CompactInput()
    {
        var box=Bevel(Colors.White,true);
        box.ContentMarginLeft=3;box.ContentMarginRight=3;
        box.ContentMarginTop=1;box.ContentMarginBottom=1;
        return box;
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
