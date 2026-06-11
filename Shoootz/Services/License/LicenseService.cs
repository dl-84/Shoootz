using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Controls.LicenseTable.Models;
using Microsoft.Extensions.DependencyInjection;
using Shoootz.Services.Localization;
using Shoootz.Services.Store;

namespace Shoootz.Services.License;

internal class LicenseService(ILocalizationService localizationService, IServiceProvider serviceProvider)
    : ILicenseService
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

    private readonly Lazy<string> _licenseContent = new Lazy<string>(() =>
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

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
        IStoreService? storeService = serviceProvider.GetService<IStoreService>();

        return
        [
            new PackageModel(
                _assembly.GetName().Name!,
                _assembly.GetName().Version?.ToString(3) ?? "n/a",
                LicenseTypeInternal,
                null,
                _licenseContent.Value,
                CopyrightInternal,
                "https://github.com/dl-84/Shoootz"
            ),
            new PackageModel(
                localizationService["Database"],
                storeService?.DbVersion ?? "n/a",
                LicenseTypeInternal,
                null,
                null,
                CopyrightInternal
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

        using (Stream stream = _assembly.GetManifestResourceStream(resourceName)!)
        {
            List<PackageModel>? packages = JsonSerializer.Deserialize<List<PackageModel>>(stream);

            return packages?.Where(package => !_excludedPackages.Contains(package.Name)).OrderBy(p => p.Name).ToList()
                ?? [];
        }
    }
}
