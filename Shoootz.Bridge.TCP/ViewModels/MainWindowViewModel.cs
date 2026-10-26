using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Shoootz.Bridge.TCP.Models.Settings;
using Shoootz.Bridge.TCP.ViewModels.General;
using Shoootz.Bridge.TCP.ViewModels.Info;
using Shoootz.Bridge.TCP.ViewModels.Settings;
using Shoootz.Bridge.TCP.Views.Types;
using Shoootz.Common.Models.Language.Error;
using Shoootz.Common.Services.Dialog;
using Shoootz.Common.Services.Settings;
using Shoootz.Common.ViewModels;

namespace Shoootz.Bridge.TCP.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase, IDialogHost
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ISettingsWriter<SettingsModel> _settingsWriter;

    public MainWindowViewModel(IServiceProvider serviceProvider, ISettingsWriter<SettingsModel> settingsWriter)
    {
        _serviceProvider = serviceProvider;
        _settingsWriter = settingsWriter;

        CurrentPage = CreateView(NavigationPageType.General)!;
        CurrentNavigationPageType = NavigationPageType.General;
    }

    [ObservableProperty]
    public partial NavigationPageType CurrentNavigationPageType { get; set; }

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    public void NavigateTo(NavigationPageType pageType)
    {
        ViewModelBase? next = CreateView(pageType);

        if (next is null)
        {
            return;
        }

        ViewModelBase previous = CurrentPage;
        CurrentPage = next;
        CurrentNavigationPageType = pageType;

        if (!ReferenceEquals(previous, CurrentPage) && previous is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public void RedirectToSettingsError(List<SettingsError> settingsErrors, string settings)
    {
        CurrentPage = new Common.ViewModels.Error.SettingsErrorViewModel<SettingsModel>(
            _settingsWriter,
            typeof(MainWindowViewModel).Assembly,
            "Shoootz.Bridge.TCP.Resources.JsonSchemas.Settings.schema.json"
        )
        {
            EditorContent = settings,
            ErrorMessages = settingsErrors.Select(settingsError => settingsError.Message).ToList(),
        };
    }

    private ViewModelBase? CreateView(NavigationPageType pageType)
    {
        return pageType switch
        {
            NavigationPageType.About => _serviceProvider.GetRequiredService<AboutViewModel>(),
            NavigationPageType.App => _serviceProvider.GetRequiredService<AppViewModel>(),
            NavigationPageType.General => _serviceProvider.GetRequiredService<GeneralViewModel>(),
            NavigationPageType.Licenses => _serviceProvider.GetRequiredService<LicensesViewModel>(),
            _ => null,
        };
    }
}
