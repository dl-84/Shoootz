using System.Collections.Generic;
using Controls.LicenseTable.Models;

namespace Shoootz.Common.Services.License;

public interface ILicenseService
{
    List<PackageModel> GetAppPackages();

    List<PackageModel> GetThirdPartyPackages();
}
