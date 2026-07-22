using MudBlazor;

namespace HotelManager.Web.Theming;

/// <summary>
/// Custom MudBlazor theme for Hotel Manager: a deep-navy primary with a warm
/// gold accent, shared by the light and dark palettes.
/// </summary>
public static class HotelTheme
{
    private static readonly string[] Fonts =
        ["Roboto", "Segoe UI", "Helvetica", "Arial", "sans-serif"];

    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1F3A5F",
            Secondary = "#C89B3C",
            Tertiary = "#3E7CB1",
            AppbarBackground = "#1F3A5F",
            AppbarText = "#FFFFFF",
            Background = "#F5F6F8",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#2B2B2B",
            Success = "#2E7D32",
            Info = "#3E7CB1",
            Warning = "#ED8B00",
            Error = "#C62828",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#5B8AC0",
            Secondary = "#D8B15A",
            Tertiary = "#4E93C8",
            AppbarBackground = "#12203A",
            AppbarText = "#FFFFFF",
            Background = "#141A24",
            Surface = "#1C2431",
            DrawerBackground = "#12203A",
            DrawerText = "#E0E4EA",
            Success = "#66BB6A",
            Info = "#5B8AC0",
            Warning = "#FFB74D",
            Error = "#EF5350",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "260px",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = Fonts },
            H4 = new H4Typography { FontFamily = Fonts, FontWeight = "600" },
            H5 = new H5Typography { FontFamily = Fonts, FontWeight = "600" },
            H6 = new H6Typography { FontFamily = Fonts, FontWeight = "600" },
            Subtitle1 = new Subtitle1Typography { FontFamily = Fonts, FontWeight = "500" },
            Button = new ButtonTypography { FontFamily = Fonts, FontWeight = "600", TextTransform = "none" },
        },
    };
}
