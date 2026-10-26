namespace Shoootz.Common.ViewModels;

public interface IHasUnsavedChanges
{
    bool IsSaveEnabled { get; }
}
