using System.Collections.Generic;
using Shoootz.Models;
using Shoootz.Services.License;

namespace Shoootz.ViewModels.Info;

internal partial class LicensesViewModel(IThirdPartyLicenseService thirdPartyLicenseService) : ViewModelBase
{
    public List<ThirdPartyPackageModel> Packages { get; } = thirdPartyLicenseService.GetPackages();
}
