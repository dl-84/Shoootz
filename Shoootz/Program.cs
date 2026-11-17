using System;
using System.Threading;
using Avalonia;

namespace Shoootz;

internal static class Program
{
    private static Mutex? _singleInstanceMutex;

    [STAThread]
    public static void Main(string[] args)
    {
        _singleInstanceMutex = new Mutex(true, "Shoootz", out bool createdNew);

        if (!createdNew)
        {
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect()
#if DEBUG
        .WithDeveloperTools()
#endif
        .WithInterFont().LogToTrace();
}
