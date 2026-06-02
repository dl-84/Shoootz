using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shoootz.Models.Error;
using Shoootz.Services.Settings;

namespace Shoootz.ViewModels.Error;

internal partial class SettingsErrorViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SettingsErrorViewModel(List<SettingsError> settingsErrors, ISettingsService settingsService)
    {
        _settingsService = settingsService;
        ErrorMessages = settingsErrors.Select(settingsError => settingsError.Message).ToList();
        EditorContent = settingsService.LoadRaw();
    }

    [ObservableProperty]
    public partial string EditorContent { get; set; }

    public IReadOnlyList<string> ErrorMessages { get; }

    [ObservableProperty]
    public partial bool IsSaved { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOriginalErrors))]
    [NotifyPropertyChangedFor(nameof(ShowValidationError))]
    public partial bool IsValid { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOriginalErrors))]
    [NotifyPropertyChangedFor(nameof(ShowValidationError))]
    public partial string? ValidationError { get; set; }

    public bool ShowOriginalErrors => ValidationError == null;

    public bool ShowValidationError => ValidationError != null;

    [RelayCommand]
    private static void Restart()
    {
        Process.Start(new ProcessStartInfo { FileName = Environment.ProcessPath!, UseShellExecute = true });
        Environment.Exit(0);
    }

    partial void OnEditorContentChanged(string value)
    {
        IsValid = false;
        IsSaved = false;
        ValidationError = null;
    }

    [RelayCommand]
    private void Save()
    {
        _settingsService.SaveRaw(EditorContent);
        IsSaved = true;
    }

    [RelayCommand]
    private void Validate()
    {
        ValidationError = _settingsService
            .Validate(EditorContent)
            .Match(_ => null, settingsError => settingsError.Value.Count > 0 ? settingsError.Value[0].Message : null);

        IsValid = ValidationError == null;
    }
}
