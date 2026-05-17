using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektionsliga.ViewModels;

public record NavItem(string Label, System.Func<ViewModelBase> CreatePage);

public partial class MainWindowViewModel : ViewModelBase
{
    public List<NavItem> CompetitionItems { get; } = [new("Auswerten", () => new EvaluationViewModel())];

    public List<NavItem> SettingsItems { get; } =
    [new("Datenbank", () => new DatabaseViewModel()), new("Gruppen", () => new GroupsViewModel())];

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private NavItem? _selectedCompetitionItem;

    [ObservableProperty]
    private NavItem? _selectedSettingsItem;

    public MainWindowViewModel()
    {
        _currentPage = new EvaluationViewModel();
        _selectedCompetitionItem = CompetitionItems[0];
    }

    partial void OnSelectedCompetitionItemChanged(NavItem? value)
    {
        if (value is null)
            return;
        SelectedSettingsItem = null;
        CurrentPage = value.CreatePage();
    }

    partial void OnSelectedSettingsItemChanged(NavItem? value)
    {
        if (value is null)
            return;
        SelectedCompetitionItem = null;
        CurrentPage = value.CreatePage();
    }
}
