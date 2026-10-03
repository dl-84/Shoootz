namespace Shoootz.Utils.Interfaces;

internal interface IHasUnsavedChanges
{
    bool IsSaveEnabled { get; }
}
