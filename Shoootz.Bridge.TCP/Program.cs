using System;
using System.Threading;
using Avalonia;

namespace Shoootz.Bridge.TCP;

internal static class Program
{
    private static Mutex? _singleInstanceMutex;

    [STAThread]
    public static void Main(string[] args)
    {
        _singleInstanceMutex = new Mutex(true, "Shoootz.Bridge.TCP", out bool createdNew);

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
