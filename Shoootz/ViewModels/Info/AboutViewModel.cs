using System.Threading.Tasks;
using Shoootz.Common.Services.License;
using Shoootz.Common.Services.Localization;
using Shoootz.Common.ViewModels.Info;
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
