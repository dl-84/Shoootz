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

namespace Shoootz.ViewModels.Competition;

internal partial class ShootersViewModel : ViewModelBase
{
    private readonly List<ShooterModel> _allShooters = [];

    private readonly IStoreService _storeService;

    public ShootersViewModel(IStoreService storeService)
    {
        _storeService = storeService;

        _ = LoadShootersAsync();
    }

    public event Action<string>? SaveShooterFailed;

    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    public bool IsShooterSelected => SelectedShooter is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchText))]
    public partial string? SearchText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShooterSelected))]
    public partial ShooterModel? SelectedShooter { get; set; }

    public ObservableCollection<ShooterModel> Shooters { get; } = [];

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

    private void ApplyFilter()
    {
        IEnumerable<ShooterModel> filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allShooters
            : _allShooters.Where(shooter =>
                (shooter.Lastname?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || (shooter.Firstname?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || (shooter.Club?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                || shooter
                    .Birthyear.ToString(CultureInfo.InvariantCulture)
                    .Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || EnumConverter.ToDisplayText(shooter.Gender).Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            );

        Shooters.Clear();

        foreach (ShooterModel shooter in filtered)
        {
            Shooters.Add(shooter);
        }
    }

    private async Task LoadShootersAsync()
    {
        Result<List<ShooterModel>, StoreReadError> result = await _storeService.GetShootersAsync();

        result.Match(
            shooters =>
            {
                _allShooters.Clear();
                _allShooters.AddRange(shooters);
                ApplyFilter();
            },
            error => SaveShooterFailed?.Invoke(error.Value.Message)
        );
    }

    partial void OnSearchTextChanged(string? value)
    {
        ApplyFilter();
    }
}
