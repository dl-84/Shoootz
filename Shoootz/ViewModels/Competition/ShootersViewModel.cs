using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Result.Types;
using Shoootz.Extensions;
using Shoootz.Models.Club;
using Shoootz.Models.Error;
using Shoootz.Models.Shooter;
using Shoootz.Services.Store;
using Shoootz.Utils.Classes;

namespace Shoootz.ViewModels.Competition;

internal partial class ShootersViewModel : FilterableListViewModelBase<ShooterModel>
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
    public partial ShooterModel? SelectedShooter { get; set; }

    public ObservableCollection<ShooterModel> Shooters => Items;

    public async Task ExecuteDeleteShooterAsync(ShooterModel shooter, bool deleteShots)
    {
        Result<Unit, StoreSaveError> result = await _storeService.DeleteShooterAsync(shooter.Id!, deleteShots);

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

    public async Task ExecuteSaveShooterAsync(ShooterModel shooter)
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
                SelectedShooter = null;
            },
            error =>
            {
                SaveShooterFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    public async Task<string> GenerateShooterIdAsync()
    {
        return await _storeService.GenerateShooterIdAsync();
    }

    public async Task<List<string>> GetClubNamesAsync()
    {
        Result<List<ClubModel>, StoreReadError> result = await _storeService.GetClubsAsync();

        return result.Match(
            clubs => clubs.Select(club => club.Name).ToList(),
            error =>
            {
                SaveShooterFailed?.Invoke(error.Value.Message);
                return [];
            }
        );
    }

    protected override bool Matches(ShooterModel item, string searchText)
    {
        return (item.Lastname?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || (item.Firstname?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || (item.Club?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || item.Birthyear.ToString(CultureInfo.InvariantCulture)
                .Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || EnumConverter.ToDisplayText(item.Gender).Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadShootersAsync()
    {
        Result<List<ShooterModel>, StoreReadError> result = await _storeService.GetShootersAsync();

        result.Match(SetItems, error => SaveShooterFailed?.Invoke(error.Value.Message));
    }
}
