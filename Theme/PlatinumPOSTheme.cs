using MudBlazor;

namespace PlatinumPOS.Theme
{
    public static class PlatinumPOSTheme
    {
        public static readonly MudTheme Theme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0B57D0",          // Google M3 Primary Blue
                PrimaryDarken = "#0842A0",
                PrimaryLighten = "#D3E3FD",   // Tonal light blue
                Secondary = "#00639B",
                SecondaryDarken = "#004A75",
                SecondaryLighten = "#CDE5FF",
                Tertiary = "#146C2E",         // Google M3 Green
                TertiaryDarken = "#0F5223",
                TertiaryLighten = "#C4EED0",
                Success = "#146C2E",
                SuccessDarken = "#0F5223",
                SuccessLighten = "#C4EED0",
                Info = "#0B57D0",
                InfoDarken = "#0842A0",
                InfoLighten = "#D3E3FD",
                Warning = "#E37400",          // Google amber — distinct from Error red
                WarningDarken = "#B45309",
                WarningLighten = "#FEF3C7",
                Error = "#B3261E",
                ErrorDarken = "#8C1D18",
                ErrorLighten = "#F9DEDC",
                Dark = "#1F1F1F",
                Background = "#F8F9FA",       // Very light gray/white background
                BackgroundGray = "#F0F4F9",   // Google Workspace surface color
                Surface = "#FFFFFF",
                AppbarBackground = "#F8F9FA",
                AppbarText = "#1F1F1F",
                DrawerBackground = "#F8F9FA",
                DrawerText = "#444746",
                DrawerIcon = "#444746",
                TextPrimary = "#1F1F1F",
                TextSecondary = "#444746",
                TextDisabled = "#C4C7C5",
                ActionDefault = "#444746",
                LinesDefault = "#E3E3E3",
                LinesInputs = "#747775",
                Divider = "#E3E3E3"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#A8C7FA",          // Google M3 Dark Primary Blue
                PrimaryDarken = "#82B1FF",
                PrimaryLighten = "#004A77",
                Secondary = "#93C5FD",
                SecondaryDarken = "#60A5FA",
                SecondaryLighten = "#1E3A8A",
                Tertiary = "#6DD58C",         // Google M3 Dark Green
                TertiaryDarken = "#4ADE80",
                TertiaryLighten = "#064E3B",
                Success = "#6DD58C",
                SuccessDarken = "#4ADE80",
                SuccessLighten = "#064E3B",
                Info = "#A8C7FA",
                InfoDarken = "#82B1FF",
                InfoLighten = "#004A77",
                Warning = "#FDD663",          // Google amber (dark) — distinct from Error red
                WarningDarken = "#FBBF24",
                WarningLighten = "#78350F",
                Error = "#F2B8B5",
                ErrorDarken = "#F87171",
                ErrorLighten = "#7F1D1D",
                Dark = "#E3E3E3",
                Background = "#202124",       // Google Dark background
                BackgroundGray = "#292A2D",
                Surface = "#292A2D",
                AppbarBackground = "#202124",
                AppbarText = "#E3E3E3",
                DrawerBackground = "#202124",
                DrawerText = "#E3E3E3",
                DrawerIcon = "#E3E3E3",
                TextPrimary = "#E3E3E3",
                TextSecondary = "#C4C7C5",
                TextDisabled = "#8E918F",
                ActionDefault = "#C4C7C5",
                LinesDefault = "#444746",
                LinesInputs = "#8E918F",
                Divider = "#444746"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "16px", // Google M3 typical rounded corners
                DrawerWidthLeft = "280px"
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = new[] { "Outfit", "Inter", "Roboto", "Helvetica", "Arial", "sans-serif" },
                    FontSize = "0.875rem",
                    LineHeight = "1.5"
                },
                H3 = new H3Typography { FontFamily = new[] { "Outfit", "Inter", "Roboto", "sans-serif" }, FontWeight = "500", LetterSpacing = "0" },
                H4 = new H4Typography { FontFamily = new[] { "Outfit", "Inter", "Roboto", "sans-serif" }, FontWeight = "400", LetterSpacing = "0" },
                H5 = new H5Typography { FontFamily = new[] { "Outfit", "Inter", "Roboto", "sans-serif" }, FontWeight = "400", LetterSpacing = "0" },
                H6 = new H6Typography { FontFamily = new[] { "Outfit", "Inter", "Roboto", "sans-serif" }, FontWeight = "500", LetterSpacing = "0.0125em" },
                Subtitle1 = new Subtitle1Typography { FontWeight = "500", LetterSpacing = "0.00938em" },
                Subtitle2 = new Subtitle2Typography { FontWeight = "500", LetterSpacing = "0.00714em" },
                Button = new ButtonTypography { FontWeight = "500", LetterSpacing = "0.02857em", TextTransform = "none" },
                Body1 = new Body1Typography { FontWeight = "400", LetterSpacing = "0.03125em" },
                Body2 = new Body2Typography { FontWeight = "400", LetterSpacing = "0.01786em" }
            }
        };
    }
}
