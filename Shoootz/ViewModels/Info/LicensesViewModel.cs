using System.Collections.Generic;
using Shoootz.Models;
using Shoootz.Services.License;

namespace Shoootz.ViewModels.Info;

internal partial class LicensesViewModel(ILicenseService licenseService) : ViewModelBase
{
    public List<PackageModel> Packages { get; } = licenseService.GetThirdPartyPackages();
}
