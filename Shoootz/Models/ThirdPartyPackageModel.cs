namespace Shoootz.Models;

internal record ThirdPartyPackageModel(
    string PackageName,
    string PackageVersion,
    string LicenseType,
    string LicenseUrl,
    string Copyright,
    string PackageUrl
);
