using System.Collections.Generic;
using Controls.LicenseTable;

namespace Shoootz.Services.License;

internal interface ILicenseService
{
    List<PackageModel> GetAppPackages();

    List<PackageModel> GetThirdPartyPackages();
}
