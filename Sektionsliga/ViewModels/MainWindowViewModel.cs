using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Sektionsliga.Services.Settings;
using Sektionsliga.ViewModels.Competition;
using Sektionsliga.ViewModels.Info;
using Sektionsliga.ViewModels.Settings;

namespace Sektionsliga.ViewModels;

public record NavItem(string Label, Func<ViewModelBase> CreatePage);

public partial class MainWindowViewModel : ViewModelBase
{
    public List<NavItem> CompetitionItems { get; }

    public List<NavItem> SettingsItems { get; }

    public List<NavItem> InfoItems { get; }

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedCompetitionItem { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedSettingsItem { get; set; }

    [ObservableProperty]
    public partial NavItem? SelectedInfoItem { get; set; }

    public MainWindowViewModel(ISettingsService settingsService)
    {
        CompetitionItems = [new NavItem("Auswerten", () => new EvaluationViewModel())];
        SettingsItems =
        [
            new NavItem("Allgemein", () => new GeneralViewModel(settingsService)),
            new NavItem("Datenbank", () => new DatabaseViewModel()),
            new NavItem("Gruppen", () => new GroupsViewModel()),
        ];
        InfoItems = [new NavItem("Version", () => new VersionViewModel())];

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
