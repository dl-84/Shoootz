using System.Threading.Tasks;
using Shoootz.Services.License;
using Shoootz.Services.Localization;
using Shoootz.Services.Store;

namespace Shoootz.ViewModels.Info;

internal class AboutViewModel : InfoViewModelBase
{
    private readonly ILicenseService _licenseService;

    private readonly IStoreService _storeService;

    public AboutViewModel(
        ILicenseService licenseService,
        ILocalizationService localizationService,
        IStoreService storeService
    )
        : base(localizationService, licenseService.GetAppPackages())
    {
        _licenseService = licenseService;
        _storeService = storeService;

        _ = RefreshDbVersionAsync();
    }

    private async Task RefreshDbVersionAsync()
    {
        await _storeService.GetDbStatusAsync();
        SetPackages(_licenseService.GetAppPackages());
    }
}
