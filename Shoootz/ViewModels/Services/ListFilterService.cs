using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Shoootz.ViewModels.Services;

internal abstract partial class ListFilterService<TItem> : ViewModelBase
{
    private readonly List<TItem> _allItems = [];

    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchText))]
    public partial string? SearchText { get; set; }

    protected ObservableCollection<TItem> Items { get; } = [];

    protected abstract bool Matches(TItem item, string searchText);

    protected void SetItems(IEnumerable<TItem> items)
    {
        _allItems.Clear();
        _allItems.AddRange(items);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<TItem> filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allItems
            : _allItems.Where(item => Matches(item, SearchText));

        Items.Clear();

        foreach (TItem item in filtered)
        {
            Items.Add(item);
        }
    }

    partial void OnSearchTextChanged(string? value)
    {
        ApplyFilter();
    }
}
