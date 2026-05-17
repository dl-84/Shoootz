using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.ViewModels.Competition;
using Sektionsliga.ViewModels.Info;
using Sektionsliga.ViewModels.Settings;

namespace Sektionsliga.ViewModels;

public record NavItem(string Label, System.Func<ViewModelBase> CreatePage);

public partial class MainWindowViewModel : ViewModelBase
{
    public List<NavItem> CompetitionItems { get; } = [new("Auswerten", () => new EvaluationViewModel())];

    public List<NavItem> SettingsItems { get; } =
    [
        new("Allgemein", () => new GeneralViewModel()),
        new("Datenbank", () => new DatabaseViewModel()),
        new("Gruppen", () => new GroupsViewModel()),
    ];

    public List<NavItem> InfoItems { get; } = [new("Version", () => new VersionViewModel())];

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private NavItem? _selectedCompetitionItem;

    [ObservableProperty]
    private NavItem? _selectedSettingsItem;

    [ObservableProperty]
    private NavItem? _selectedInfoItem;

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
        SelectedInfoItem = null;
        CurrentPage = value.CreatePage();
    }

    partial void OnSelectedSettingsItemChanged(NavItem? value)
    {
        if (value is null)
            return;
        SelectedCompetitionItem = null;
        SelectedInfoItem = null;
        CurrentPage = value.CreatePage();
    }

    partial void OnSelectedInfoItemChanged(NavItem? value)
    {
        if (value is null)
            return;
        SelectedCompetitionItem = null;
        SelectedSettingsItem = null;
        CurrentPage = value.CreatePage();
    }
}
