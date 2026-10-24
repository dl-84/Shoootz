using CommunityToolkit.Mvvm.ComponentModel;
using Shoootz.Models.Shot;

namespace Shoootz.ViewModels.Competition;

internal partial class ShotRowViewModel(ShotModel shot) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public ShotModel Shot { get; } = shot;
}
