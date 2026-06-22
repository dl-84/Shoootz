using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Result;
using Shoootz.Models.Error;
using Shoootz.Services.Settings;
using Shoootz.Services.Settings.Validation;

namespace Shoootz.ViewModels.Error;

internal partial class SettingsErrorViewModel(ISettingsWriter settingsWriter) : ViewModelBase
{
    public List<string> ErrorMessages { get; set; } = [];

    [ObservableProperty]
    public partial string EditorContent { get; set; } = string.Empty;

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
        settingsWriter.SaveRaw(EditorContent);
        IsSaved = true;
    }

    [RelayCommand]
    private void Validate()
    {
        Result<string, List<SettingsError>> validationResult = SettingsValidation.Run(EditorContent);

        validationResult.Match(
            _ =>
            {
                IsValid = true;
            },
            error =>
            {
                ErrorMessages.Clear();
                ErrorMessages = error.Value.Select(settingsError => settingsError.Message).ToList();
            }
        );
    }
}
