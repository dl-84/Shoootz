using Avalonia;
using Avalonia.Media;

namespace Shoootz.Services.App;

internal static class AppBrush
{
    public static IBrush? BackgroundAlt => GetBrush("AppBackgroundAltBrush");

    public static IBrush? Error => GetBrush("ErrorBrush");

    public static IBrush? PrimaryBrush => GetBrush("PrimaryBrush");

    public static IBrush? PrimaryForeground => GetBrush("PrimaryForegroundBrush");

    private static IBrush? GetBrush(string key)
    {
        if (Application.Current is null)
        {
            return null;
        }

        Application.Current.TryGetResource(key, Application.Current.ActualThemeVariant, out object? resource);
        return resource as IBrush;
    }
}
