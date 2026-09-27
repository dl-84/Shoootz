using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Result.Types;
using Shoootz.Models.Club;
using Shoootz.Models.Error;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;
using Shoootz.Services.Store;

namespace Shoootz.ViewModels.Competition;

internal partial class ClubsViewModel : ViewModelBase
{
    private readonly List<ClubModel> _allClubs = [];

    private readonly IStoreService _storeService;

    public ClubsViewModel(IStoreService storeService)
    {
        _storeService = storeService;

        _ = LoadClubsAsync();
    }

    public event Action<string>? SaveClubFailed;

    public ObservableCollection<ClubModel> Clubs { get; } = [];

    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    public bool IsClubSelected => SelectedClub is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchText))]
    public partial string? SearchText { get; set; }

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

    private void ApplyFilter()
    {
        IEnumerable<ClubModel> filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allClubs
            : _allClubs.Where(club =>
                club.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || club.ShooterCount.ToString(CultureInfo.InvariantCulture)
                    .Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            );

        Clubs.Clear();

        foreach (ClubModel club in filtered)
        {
            Clubs.Add(club);
        }
    }

    private async Task LoadClubsAsync()
    {
        Result<List<ClubModel>, StoreReadError> result = await _storeService.GetClubsAsync();

        result.Match(
            clubs =>
            {
                _allClubs.Clear();
                _allClubs.AddRange(clubs);
                ApplyFilter();
            },
            error => SaveClubFailed?.Invoke(error.Value.Message)
        );
    }

    partial void OnSearchTextChanged(string? value)
    {
        ApplyFilter();
    }
}
