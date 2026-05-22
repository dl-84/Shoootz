using System.Collections.Generic;
using Shoootz.Models;

namespace Shoootz.Services.License;

internal interface ILicenseService
{
    List<PackageModel> GetAppPackages();

    List<PackageModel> GetThirdPartyPackages();
}
