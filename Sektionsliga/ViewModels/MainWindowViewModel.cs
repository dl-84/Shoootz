using System;
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
    public partial ViewModelBase CurrentPage { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedCompetitionItem { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedSettingsItem { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedInfoItem { get; set; }

    public MainWindowViewModel()
    {
        CurrentPage = new EvaluationViewModel();
        SelectedCompetitionItem = CompetitionItems[0];
    }

    partial void OnSelectedCompetitionItemChanged(NavItem? value) =>
        NavigateTo(
            value,
            () =>
            {
                SelectedSettingsItem = null;
                SelectedInfoItem = null;
            }
        );

    partial void OnSelectedSettingsItemChanged(NavItem? value) =>
        NavigateTo(
            value,
            () =>
            {
                SelectedCompetitionItem = null;
                SelectedInfoItem = null;
            }
        );

    partial void OnSelectedInfoItemChanged(NavItem? value) =>
        NavigateTo(
            value,
            () =>
            {
                SelectedCompetitionItem = null;
                SelectedSettingsItem = null;
            }
        );

    private void NavigateTo(NavItem? value, Action clearSiblings)
    {
        if (value is null)
        {
            return;
        }

        clearSiblings();
        CurrentPage = value.CreatePage();
    }
}
