using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Result.Types;
using Shoootz.Models.Club;
using Shoootz.Models.Error;
using Shoootz.Services.Store;

namespace Shoootz.ViewModels.Competition;

internal partial class ClubsViewModel : ViewModelBase
{
    private readonly IStoreService _storeService;

    public ClubsViewModel(IStoreService storeService)
    {
        _storeService = storeService;

        _ = LoadClubsAsync();
    }

    public event Action<string>? SaveClubFailed;

    public ObservableCollection<ClubModel> Clubs { get; } = [];

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
            async _ => await LoadClubsAsync(),
            error =>
            {
                SaveClubFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    private async Task LoadClubsAsync()
    {
        Result<List<ClubModel>, StoreReadError> result = await _storeService.GetClubsAsync();

        result.Match(
            clubs =>
            {
                Clubs.Clear();

                foreach (ClubModel club in clubs)
                {
                    Clubs.Add(club);
                }
            },
            error => SaveClubFailed?.Invoke(error.Value.Message)
        );
    }
}
