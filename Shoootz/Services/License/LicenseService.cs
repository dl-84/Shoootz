using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Shoootz.Models;
using Shoootz.Services.Localization;

namespace Shoootz.Services.License;

internal class LicenseService(ILocalizationService localizationService) : ILicenseService
{
    private const string CopyrightInternal = "© 2026 Daniel Lindner";

    private const string LicensesFileName = "licenses.json";

    private const string LicenseTypeInternal = "MIT";

    private static readonly HashSet<string> _excludedPackages =
    [
        "AvaloniaUI.DiagnosticsSupport", // Debug-only, no license in generated JSON
        "StyleCop.Analyzers", // Build-time analyzer, Apache-2.0, PrivateAssets=all
    ];

    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    private string GetLicenseContent
    {
        get
        {
            using Stream stream = _assembly.GetManifestResourceStream($"{_assembly.GetName().Name!}.LICENSE")!;
            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            return result;
        }
    }

    public List<PackageModel> GetAppPackages()
    {
        return
        [
            // Application
            new PackageModel(
                _assembly.GetName().Name!,
                _assembly.GetName().Version?.ToString(3) ?? "n/a",
                LicenseTypeInternal,
                null,
                GetLicenseContent,
                CopyrightInternal,
                null
            ),
            // Datebase
            new PackageModel(
                localizationService["Database"],
                "0.0.1",
                LicenseTypeInternal,
                null,
                null,
                CopyrightInternal,
                null
            ),
        ];
    }

    public List<PackageModel> GetThirdPartyPackages()
    {
        string? resourceName = _assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(LicensesFileName));

        if (resourceName is null)
        {
            return [];
        }

        using Stream stream = _assembly.GetManifestResourceStream(resourceName)!;
        List<PackageModel>? packages = JsonSerializer.Deserialize<List<PackageModel>>(stream);

        return packages?.Where(package => !_excludedPackages.Contains(package.Name)).OrderBy(p => p.Name).ToList()
            ?? [];
    }
}
