using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Result.Types;
using Shoootz.Models.Club;
using Shoootz.Models.Error;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.Services.Store;
using Shoootz.ViewModels.Services;

namespace Shoootz.ViewModels.Competition;

internal partial class ClubsViewModel : ListFilterService<ClubModel>
{
    private readonly IStoreService _storeService;

    public ClubsViewModel(IStoreService storeService)
    {
        _storeService = storeService;

        _ = LoadClubsAsync();
    }

    public event Action<string>? SaveClubFailed;

    public ObservableCollection<ClubModel> Clubs => Items;

    public bool IsClubSelected => SelectedClub is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClubSelected))]
    public partial ClubModel? SelectedClub { get; set; }

    public async Task ExecuteCreateClubAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        Result<Unit, StoreSaveError> result = await _storeService.SaveClubAsync(new ClubModel { Name = name });

        await result.Match(
            async _ => await LoadClubsAsync(),
            error =>
            {
                SaveClubFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    public async Task ExecuteDeleteClubAsync(ClubModel club)
    {
        if (club.ShooterCount > 0)
        {
            SaveClubFailed?.Invoke(LocalizationService.Instance[nameof(Messages.ClubHasShootersMessage)]);
            return;
        }

        Result<bool, StoreReadError> hasShootersResult = await _storeService.ClubHasShootersAsync(club.Id);

        await hasShootersResult.Match(
            async hasShooters =>
            {
                if (hasShooters)
                {
                    SaveClubFailed?.Invoke(LocalizationService.Instance[nameof(Messages.ClubHasShootersMessage)]);
                    return;
                }

                Result<Unit, StoreSaveError> result = await _storeService.DeleteClubAsync(club.Id);

                await result.Match(
                    async _ =>
                    {
                        SelectedClub = null;
                        await LoadClubsAsync();
                    },
                    error =>
                    {
                        SaveClubFailed?.Invoke(error.Value.Message);
                        return Task.CompletedTask;
                    }
                );
            },
            error =>
            {
                SaveClubFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    public async Task ExecuteRenameClubAsync(ClubModel club, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || newName == club.Name)
        {
            return;
        }

        Result<Unit, StoreSaveError> result = await _storeService.SaveClubAsync(
            new ClubModel { Id = club.Id, Name = newName }
        );

        await result.Match(
            async _ =>
            {
                await LoadClubsAsync();
                SelectedClub = null;
            },
            error =>
            {
                SaveClubFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    protected override bool Matches(ClubModel item, string searchText)
    {
        return item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || item.ShooterCount.ToString(CultureInfo.InvariantCulture)
                .Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadClubsAsync()
    {
        Result<List<ClubModel>, StoreReadError> result = await _storeService.GetClubsAsync();

        result.Match(SetItems, error => SaveClubFailed?.Invoke(error.Value.Message));
    }
}
