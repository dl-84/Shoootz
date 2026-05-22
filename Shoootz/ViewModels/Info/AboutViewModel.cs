using System.Collections.Generic;
using System.Linq;
using Shoootz.Models;
using Shoootz.Services.License;

namespace Shoootz.ViewModels.Info;

internal partial class AboutViewModel(ILicenseService licenseService) : ViewModelBase
{
    public List<PackageModel> Packages { get; } = licenseService.GetAppPackages();

    public string AppVersion => Packages.First().Version;

    public string DatabaseVersion => Packages.Skip(1).First().Version;
}
