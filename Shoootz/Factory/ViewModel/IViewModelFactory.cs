using Shoootz.ViewModels;

namespace Shoootz.Factory.ViewModel;

internal interface IViewModelFactory
{
    ViewModelBase? CreateView(int index);
}
