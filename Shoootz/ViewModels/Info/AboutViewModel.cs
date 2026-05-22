using System.Collections.Generic;
using System.Reflection;
using Shoootz.Models;
using Shoootz.Services.License;

namespace Shoootz.ViewModels.Info;

internal partial class AboutViewModel(ILicenseService licenseService) : ViewModelBase
{
    public List<PackageModel> Packages { get; } = licenseService.GetAppPackages();
}
