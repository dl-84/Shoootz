using System;
using System.Collections.Generic;
using Controls.LicenseTable.Models;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Tmds.DBus.Protocol;

namespace Shoootz.ViewModels.Info;

internal abstract class InfoViewModelBase : ViewModelBase, IDisposable
{
    private readonly ILocalizationService _localizationService;

    protected InfoViewModelBase(ILocalizationService localizationService, List<PackageModel> packages)
    {
        _localizationService = localizationService;
        Packages = packages;
        ColumnHeaders = BuildColumnHeaders();
        localizationService.LanguageChanged += OnLanguageChanged;
    }

    public IReadOnlyList<string> ColumnHeaders { get; private set; }

    public List<PackageModel> Packages { get; }

    public void Dispose()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private IReadOnlyList<string> BuildColumnHeaders() =>
        [
            _localizationService[nameof(Messages.Database)],
            _localizationService[nameof(Messages.Version)],
            _localizationService[nameof(Messages.License)],
            _localizationService[nameof(Messages.Link)],
        ];

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        ColumnHeaders = BuildColumnHeaders();
        OnPropertyChanged(nameof(ColumnHeaders));
    }
}
