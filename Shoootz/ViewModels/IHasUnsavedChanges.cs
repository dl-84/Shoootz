namespace Shoootz.ViewModels;

internal interface IHasUnsavedChanges
{
    bool IsSaveEnabled { get; }
}
