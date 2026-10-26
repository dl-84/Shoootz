using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Controls.LicenseTable.Models;

namespace Shoootz.Common.Services.License;

public class LicenseService(Assembly assembly, Func<IEnumerable<PackageModel>>? additionalAppPackages = null)
    : ILicenseService
{
    public const string Copyright = "© 2026 Daniel Lindner";

    public const string LicenseType = "MIT";

    private const string LicensesFileName = "licenses.json";

    private static readonly HashSet<string> _excludedPackages = ["AvaloniaUI.DiagnosticsSupport", "StyleCop.Analyzers"];

    private readonly Lazy<string> _licenseContent = new(() =>
    {
        using (Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name!}.LICENSE")!)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    });

    public List<PackageModel> GetAppPackages()
    {
        List<PackageModel> packages =
        [
            new(
                assembly.GetName().Name!,
                assembly.GetName().Version?.ToString(3) ?? "n/a",
                LicenseType,
                null,
                _licenseContent.Value,
                Copyright,
                "https://github.com/dl-84/Shoootz"
            ),
        ];

        if (additionalAppPackages is not null)
        {
            packages.AddRange(additionalAppPackages());
        }

        return packages;
    }

    public List<PackageModel> GetThirdPartyPackages()
    {
        string? resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(LicensesFileName));

        if (resourceName is null)
        {
            return [];
        }

        using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
        {
            List<PackageModel>? packages = JsonSerializer.Deserialize<List<PackageModel>>(stream);

            return packages?.Where(package => !_excludedPackages.Contains(package.Name)).OrderBy(p => p.Name).ToList()
                ?? [];
        }
    }
}
