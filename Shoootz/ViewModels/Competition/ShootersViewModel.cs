using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Shooter;
using Shoootz.Services.Store;

namespace Shoootz.ViewModels.Competition;

internal partial class ShootersViewModel : ViewModelBase
{
    private readonly IStoreService _storeService;

    public ShootersViewModel(IStoreService storeService)
    {
        _storeService = storeService;

        _ = LoadShootersAsync();
    }

    public event Action<string>? SaveShooterFailed;

    public bool IsShooterSelected => SelectedShooter is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShooterSelected))]
    public partial ShooterListModel? SelectedShooter { get; set; }

    public ObservableCollection<ShooterListModel> Shooters { get; } = [];

    public async Task ExecuteDeleteShooterAsync(ShooterListModel shooter, bool deleteShots)
    {
        Result<Unit, StoreSaveError> result = await _storeService.DeleteShooterAsync(shooter.Id, deleteShots);

        await result.Match(
            async _ =>
            {
                SelectedShooter = null;
                await LoadShootersAsync();
            },
            error =>
            {
                SaveShooterFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    public async Task ExecuteSaveShooterAsync(ShooterListModel shooter)
    {
        if (string.IsNullOrWhiteSpace(shooter.Id) || string.IsNullOrWhiteSpace(shooter.Lastname))
        {
            return;
        }

        Result<Unit, StoreSaveError> result = await _storeService.SaveShooterAsync(shooter);

        await result.Match(
            async _ =>
            {
                await LoadShootersAsync();
                SelectedShooter = Shooters.FirstOrDefault(loadedShooter => loadedShooter.Id == shooter.Id);
            },
            error =>
            {
                SaveShooterFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    public async Task<string> GenerateShooterIdAsync() => await _storeService.GenerateShooterIdAsync();

    private async Task LoadShootersAsync()
    {
        Result<List<ShooterListModel>, StoreReadError> result = await _storeService.GetShootersAsync();

        result.Match(
            shooters =>
            {
                Shooters.Clear();

                foreach (ShooterListModel shooter in shooters)
                {
                    Shooters.Add(shooter);
                }
            },
            error => SaveShooterFailed?.Invoke(error.Value.Message)
        );
    }
}
