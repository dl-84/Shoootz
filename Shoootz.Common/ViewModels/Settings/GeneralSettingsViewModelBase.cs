using System;
using System.Threading.Tasks;

namespace Shoootz.Common.ViewModels.Settings;

public abstract class GeneralSettingsViewModelBase : ViewModelBase
{
    public event Action<bool, string?>? DeleteAllCompleted;

    public event Action? DeleteAllStarted;

    public event Action? DeleteSettingsFileRequested;

    public event Action? DeleteSettingsFolderRequested;

    public event Action<string>? ReadSettingsFileFailed;

    public event Action<string>? SettingsContentRequested;

    public abstract bool HasAdditionalDeleteFolderCleanup { get; }

    public abstract void ExecuteDeleteSettingsFile();

    public abstract Task ExecuteDeleteSettingsFolderAsync();

    protected void RaiseDeleteAllCompleted(bool success, string? errorMessage) =>
        DeleteAllCompleted?.Invoke(success, errorMessage);

    protected void RaiseDeleteAllStarted() => DeleteAllStarted?.Invoke();

    protected void RaiseDeleteSettingsFileRequested() => DeleteSettingsFileRequested?.Invoke();

    protected void RaiseDeleteSettingsFolderRequested() => DeleteSettingsFolderRequested?.Invoke();

    protected void RaiseReadSettingsFileFailed(string message) => ReadSettingsFileFailed?.Invoke(message);

    protected void RaiseSettingsContentRequested(string content) => SettingsContentRequested?.Invoke(content);
}
