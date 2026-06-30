using MudBlazor;

namespace PlatinumPOS.Theme
{
    public static class PlatinumPOSTheme
    {
        public static readonly MudTheme Theme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0B57D0",
                PrimaryDarken = "#0842A0",
                PrimaryLighten = "#D3E3FD",
                Secondary = "#444746",
                SecondaryDarken = "#1F1F1F",
                SecondaryLighten = "#C4C7C5",
                Tertiary = "#146C2E",
                TertiaryDarken = "#0F5223",
                TertiaryLighten = "#C4EED0",
                Success = "#146C2E",
                SuccessDarken = "#0F5223",
                SuccessLighten = "#C4EED0",
                Info = "#0B57D0",
                InfoDarken = "#0842A0",
                InfoLighten = "#D3E3FD",
                Warning = "#E37400",
                WarningDarken = "#B45309",
                WarningLighten = "#FEF3C7",
                Error = "#B3261E",
                ErrorDarken = "#8C1D18",
                ErrorLighten = "#F9DEDC",
                Dark = "#1F1F1F",
                Background = "#F8F9FA",
                BackgroundGray = "#F0F4F9",
                Surface = "#FFFFFF",
                AppbarBackground = "#F8F9FA",
                AppbarText = "#1F1F1F",
                DrawerBackground = "#F0F4F9",
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
                Primary = "#2563EB",
                PrimaryDarken = "#1D4ED8",
                PrimaryLighten = "#1E3A8A",
                Secondary = "#E3E3E3",
                SecondaryDarken = "#C4C7C5",
                SecondaryLighten = "#1E3A8A",
                Tertiary = "#059669",
                TertiaryDarken = "#047857",
                TertiaryLighten = "#A7F3D0",
                Success = "#16A34A",
                SuccessDarken = "#15803D",
                SuccessLighten = "#BBF7D0",
                Info = "#0284C7",
                InfoDarken = "#0369A1",
                InfoLighten = "#BAE6FD",
                Warning = "#D97706",
                WarningDarken = "#B45309",
                WarningLighten = "#FDE68A",
                Error = "#DC2626",
                ErrorDarken = "#B91C1C",
                ErrorLighten = "#FECACA",
                Dark = "#E3E3E3",
                Background = "#0F111A",
                BackgroundGray = "#161B27",
                Surface = "#161B27",
                AppbarBackground = "#0F111A",
                AppbarText = "#E3E3E3",
                DrawerBackground = "#0F111A",
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
                DefaultBorderRadius = "24px",
                DrawerWidthLeft = "260px"
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = ["Inter", "Segoe UI", "system-ui", "Roboto", "Helvetica", "Arial", "sans-serif"],
                    FontSize = "0.875rem",
                    LineHeight = "1.5"
                },
                H3 = new H3Typography { FontFamily = ["Inter", "Segoe UI", "system-ui", "Roboto", "Helvetica", "Arial", "sans-serif"], FontWeight = "800", LetterSpacing = "-0.75px" },
                H4 = new H4Typography { FontFamily = ["Inter", "Segoe UI", "system-ui", "Roboto", "Helvetica", "Arial", "sans-serif"], FontWeight = "700", LetterSpacing = "-0.5px" },
                H5 = new H5Typography { FontFamily = ["Inter", "Segoe UI", "system-ui", "Roboto", "Helvetica", "Arial", "sans-serif"], FontWeight = "700", LetterSpacing = "-0.3px" },
                H6 = new H6Typography { FontFamily = ["Inter", "Segoe UI", "system-ui", "Roboto", "Helvetica", "Arial", "sans-serif"], FontWeight = "600", LetterSpacing = "-0.2px" },
                Subtitle1 = new Subtitle1Typography { FontWeight = "500", LetterSpacing = "0.00938em" },
                Subtitle2 = new Subtitle2Typography { FontWeight = "500", LetterSpacing = "0.00714em" },
                Button = new ButtonTypography { FontWeight = "600", LetterSpacing = "0.3px", TextTransform = "none" },
                Body1 = new Body1Typography { FontWeight = "400", LetterSpacing = "0.03125em" },
                Body2 = new Body2Typography { FontWeight = "400", LetterSpacing = "0.01786em" }
            }
        };
    }
}
