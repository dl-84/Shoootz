namespace Shoootz.Utils;

internal interface IHasUnsavedChanges
{
    bool IsSaveEnabled { get; }
}
