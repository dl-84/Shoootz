using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;
using Shoootz.Services.Store;
using Shoootz.ViewModels.Services;

namespace Shoootz.ViewModels.Competition;

internal partial class ShotsViewModel : ListFilterService<ShotRowViewModel>
{
    private readonly IStoreService _storeService;

    public ShotsViewModel(IStoreService storeService)
    {
        _storeService = storeService;

        _ = LoadShotsAsync();
    }

    public event Action<string>? DeleteShotsFailed;

    public event Action<string>? LoadShotsFailed;

    public bool IsAnyShotSelected => Shots.Any(row => row.IsSelected);

    [ObservableProperty]
    public partial bool IsAllSelected { get; set; }

    public ObservableCollection<ShotRowViewModel> Shots => Items;

    public async Task ExecuteDeleteSelectedShotsAsync()
    {
        List<int> ids = Shots.Where(row => row.IsSelected).Select(row => row.Shot.Id).ToList();

        if (ids.Count == 0)
        {
            return;
        }

        Result<Unit, StoreSaveErrorModel> result = await _storeService.DeleteShotsAsync(ids);

        await result.Match(
            async _ => await LoadShotsAsync(),
            error =>
            {
                DeleteShotsFailed?.Invoke(error.Value.Message);
                return Task.CompletedTask;
            }
        );
    }

    public async Task ExecuteRefreshAsync()
    {
        await LoadShotsAsync();
    }

    protected override bool Matches(ShotRowViewModel item, string searchText)
    {
        ShotModel shot = item.Shot;

        return (shot.Shooter?.Firstname?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || (shot.Shooter?.Lastname?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || (shot.DiscType?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || shot.DecValue.ToString(CultureInfo.InvariantCulture)
                .Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || shot.ShotDateTime.ToString(CultureInfo.InvariantCulture)
                .Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnIsAllSelectedChanged(bool value)
    {
        foreach (ShotRowViewModel row in Shots)
        {
            row.IsSelected = value;
        }
    }

    private async Task LoadShotsAsync()
    {
        Result<List<ShotModel>, StoreReadErrorModel> result = await _storeService.GetShotsAsync();

        result.Match(
            shots =>
            {
                List<ShotRowViewModel> rows = shots.Select(shot => new ShotRowViewModel(shot)).ToList();

                foreach (ShotRowViewModel row in rows)
                {
                    row.PropertyChanged += OnRowPropertyChanged;
                }

                SetItems(rows);
            },
            error => LoadShotsFailed?.Invoke(error.Value.Message)
        );
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShotRowViewModel.IsSelected))
        {
            OnPropertyChanged(nameof(IsAnyShotSelected));
        }
    }
}
