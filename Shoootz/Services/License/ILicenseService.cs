using System.Collections.Generic;
using Controls.LicenseTable.Models;

namespace Shoootz.Services.License;

internal interface ILicenseService
{
    List<PackageModel> GetAppPackages();

    List<PackageModel> GetThirdPartyPackages();
}
