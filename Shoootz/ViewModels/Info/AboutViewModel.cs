using System.Reflection;

namespace Shoootz.ViewModels.Info;

internal partial class AboutViewModel : ViewModelBase
{
    public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "n/a";

    public static string DatabaseVersion => "0.0.1";
}
