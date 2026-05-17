using System.Collections.Generic;
using System.Reflection;
using Sektionsliga.Models;

namespace Sektionsliga.ViewModels.Info;

public partial class VersionsViewModel : ViewModelBase
{
    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public List<ThirdPartyPackageModel> Packages { get; } =
    [
        new("Avalonia", "12.0.3", "MIT"),
        new("Avalonia.Desktop", "12.0.3", "MIT"),
        new("Avalonia.Themes.Fluent", "12.0.3", "MIT"),
        new("Avalonia.Fonts.Inter", "12.0.3", "MIT"),
        new("CommunityToolkit.Mvvm", "8.4.1", "MIT"),
        new("Microsoft.Extensions.DependencyInjection", "10.0.8", "MIT"),
    ];
}
