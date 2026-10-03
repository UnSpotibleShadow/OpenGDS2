using Godot;

namespace OpenGDS2.UI;

/// <summary>Authored fallback pictograms; original imported bitmaps take priority.</summary>
public static class ToolbarArtwork
{
    public static Texture2D CreateFallback(string action)
    {
        string drawing = action switch
        {
            "BACK_TO_TITLE" => "<path d='M2 10L11 3L20 10M5 9V20H17V9M9 20V13H13V20'/>",
            "SAVE_GAME" => "<path d='M3 2H17L20 5V20H3Z'/><path d='M7 2V8H15V2M7 20V12H16V20M12 3V6'/>",
            "LOAD_GAME" => "<path d='M2 6H9L11 9H21L18 20H2ZM2 6V3H9L11 6H19V9'/>",
            "DEVELOPMENT_PLANNING" => "<path d='M4 2H14L18 6V20H4ZM14 2V6H18M7 10H14M7 14H12M7 17H11'/>",
            "COMPANY_ACTIVITIES" => "<rect x='2' y='7' width='19' height='13' rx='1'/><path d='M8 7V3H15V7M2 12H21M10 11V14H13V11'/>",
            "HIRE_EMPLOYEES" => "<circle cx='8' cy='6' r='3'/><path d='M2 20V16Q8 9 14 16V20M18 6V14M14 10H22'/>",
            "TECHNOLOGIES" => "<path d='M8 2H15M10 2V9L4 18Q3 20 6 20H17Q20 20 19 18L13 9V2M7 14H16'/>",
            "DEVELOPMENT_COMPUTERS" => "<rect x='2' y='3' width='19' height='13'/><path d='M8 20H15M11 16V20M5 6H18V13H5Z'/>",
            "START_GAME_DEVELOPMENT" => "<path d='M3 7Q11 3 20 7L21 17Q19 20 15 15H8Q4 20 2 17ZM6 8V13M3 11H9'/><circle cx='16' cy='9' r='1'/><circle cx='18' cy='12' r='1'/>",
            "SALES" => "<path d='M3 2V20H21M6 16L10 12L14 14L20 5M16 5H20V9'/>",
            "STAFF" => "<circle cx='8' cy='6' r='3'/><circle cx='17' cy='8' r='2'/><path d='M2 20V16Q8 9 14 16V20M15 14Q21 13 21 18V20'/>",
            "HARDWARE_LICENSES" => "<path d='M3 2H17V11M3 2V20H9M6 6H14M6 10H12'/><circle cx='13' cy='15' r='3'/><path d='M16 15H22M19 15V18M21 15V17'/>",
            "IMPORT_BUTTON" => "<path d='M11 2V13M6 8L11 13L16 8M3 14V20H20V14'/>",
            "SETTINGS" => "<path d='M9 2H14L15 6L19 7L21 11L18 14V18L14 20L11 18L7 20L3 17L4 13L2 10L4 6H8Z'/><circle cx='11.5' cy='11' r='3'/>",
            "CALENDAR_INSPECTOR" => "<rect x='3' y='4' width='17' height='16'/><path d='M3 9H20M7 2V6M16 2V6M7 13H9M13 13H16M7 17H9M13 17H16'/>",
            "PAUSE" => "<path d='M6 3H9V19H6ZM14 3H17V19H14Z'/>",
            "RESUME" => "<path d='M6 3L19 11L6 19Z'/>",
            _ => "<circle cx='11' cy='11' r='9'/><path d='M11 6V12M11 16H11.1'/>"
        };
        using var image = new Image();
        image.LoadSvgFromString($"<svg xmlns='http://www.w3.org/2000/svg' width='23' height='22' viewBox='0 0 23 22'><g fill='none' stroke='#25313b' stroke-width='1.5' stroke-linecap='round' stroke-linejoin='round'>{drawing}</g></svg>");
        return ImageTexture.CreateFromImage(image);
    }
}
