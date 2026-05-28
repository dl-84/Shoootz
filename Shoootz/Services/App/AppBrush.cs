using Avalonia;
using Avalonia.Media;

namespace Shoootz.Services.App;

internal static class AppBrush
{
    internal static IBrush AppBackground => Resolve("AppBackgroundBrush");

    internal static IBrush AppBackgroundAlt => Resolve("AppBackgroundAltBrush");

    internal static IBrush AppBackgroundTint => Resolve("AppBackgroundTintBrush");

    internal static IBrush BorderBrand => Resolve("BorderBrandBrush");

    internal static IBrush BorderLight => Resolve("BorderLightBrush");

    internal static IBrush BorderMedium => Resolve("BorderMediumBrush");

    internal static IBrush Error => Resolve("ErrorBrush");

    internal static IBrush FooterBackground => Resolve("FooterBackgroundBrush");

    internal static IBrush FooterForeground => Resolve("FooterForegroundBrush");

    internal static IBrush FooterLink => Resolve("FooterLinkBrush");

    internal static IBrush FooterLinkHover => Resolve("FooterLinkHoverBrush");

    internal static IBrush InputBackground => Resolve("InputBackgroundBrush");

    internal static IBrush InputBorder => Resolve("InputBorderBrush");

    internal static IBrush InputFocusBorder => Resolve("InputFocusBorderBrush");

    internal static IBrush InputForeground => Resolve("InputForegroundBrush");

    internal static IBrush InputPlaceholder => Resolve("InputPlaceholderBrush");

    internal static IBrush Link => Resolve("LinkBrush");

    internal static IBrush LinkHover => Resolve("LinkHoverBrush");

    internal static IBrush NavigationActive => Resolve("NavigationActiveBrush");

    internal static IBrush NavigationActiveText => Resolve("NavigationActiveTextBrush");

    internal static IBrush NavigationBackground => Resolve("NavigationBackgroundBrush");

    internal static IBrush NavigationForeground => Resolve("NavigationForegroundBrush");

    internal static IBrush NavigationHover => Resolve("NavigationHoverBrush");

    internal static IBrush Overlay => Resolve("OverlayBrush");

    internal static IBrush Primary => Resolve("PrimaryBrush");

    internal static IBrush PrimaryForeground => Resolve("PrimaryForegroundBrush");

    internal static IBrush PrimaryHover => Resolve("PrimaryHoverBrush");

    internal static IBrush Secondary => Resolve("SecondaryBrush");

    internal static IBrush SecondaryForeground => Resolve("SecondaryForegroundBrush");

    internal static IBrush SecondaryHover => Resolve("SecondaryHoverBrush");

    internal static IBrush Success => Resolve("SuccessBrush");

    internal static IBrush Surface => Resolve("SurfaceBrush");

    internal static IBrush SurfaceSubtle => Resolve("SurfaceSubtleBrush");

    internal static IBrush Tertiary => Resolve("TertiaryBrush");

    internal static IBrush TertiaryForeground => Resolve("TertiaryForegroundBrush");

    internal static IBrush TertiaryHover => Resolve("TertiaryHoverBrush");

    internal static IBrush TextDisabled => Resolve("TextDisabledBrush");

    internal static IBrush TextMuted => Resolve("TextMutedBrush");

    internal static IBrush TextNearBlack => Resolve("TextNearBlackBrush");

    internal static IBrush TextOnDark => Resolve("TextOnDarkBrush");

    internal static IBrush TextPrimary => Resolve("TextPrimaryBrush");

    internal static IBrush TextSecondary => Resolve("TextSecondaryBrush");

    internal static IBrush TextWhite => Resolve("TextWhiteBrush");

    internal static IBrush Warning => Resolve("WarningBrush");

    private static IBrush Resolve(string key)
    {
        if (Application.Current is null)
        {
            return Brushes.Transparent;
        }

        Application.Current.TryGetResource(key, Application.Current.ActualThemeVariant, out object? resource);
        return resource as IBrush ?? Brushes.Transparent;
    }
}
