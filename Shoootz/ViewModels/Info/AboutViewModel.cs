using System;
using System.Collections.Generic;
using Controls.LicenseTable;
using Shoootz.Services.License;
using Shoootz.Services.Localization;

namespace Shoootz.ViewModels.Info;

internal partial class AboutViewModel : ViewModelBase, IDisposable
{
    private readonly ILocalizationService _localizationService;

    public AboutViewModel(ILicenseService licenseService, ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        Packages = licenseService.GetAppPackages();
        localizationService.LanguageChanged += OnLanguageChanged;
    }

    public IReadOnlyList<string> ColumnHeaders =>
        [
            _localizationService["Name"],
            _localizationService["Version"],
            _localizationService["License"],
            _localizationService["Link"],
        ];

    public List<PackageModel> Packages { get; }

    public void Dispose()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(ColumnHeaders));
    }
}
