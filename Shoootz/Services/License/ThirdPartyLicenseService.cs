using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Shoootz.Models;

namespace Shoootz.Services.License;

internal class ThirdPartyLicenseService : IThirdPartyLicenseService
{
    private const string LicensesFileName = "licenses.json";

    public List<ThirdPartyPackageModel> GetPackages()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(LicensesFileName));

        if (resourceName is null)
        {
            return [];
        }

        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        List<ThirdPartyPackageModel>? packages = JsonSerializer.Deserialize<List<ThirdPartyPackageModel>>(stream);

        return packages
                ?.Where(package => !string.IsNullOrEmpty(package.LicenseType))
                .OrderBy(p => p.PackageName)
                .ToList()
            ?? [];
    }
}
