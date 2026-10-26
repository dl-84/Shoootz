using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Result;
using Shoootz.Common.Models.Language.Error;
using Shoootz.Common.Services.Settings;

namespace Shoootz.Common.ViewModels.Error;

/// <inheritdoc/>
public partial class SettingsErrorViewModel<TSettings>(
    ISettingsWriter<TSettings> settingsWriter,
    Assembly assembly,
    string schemaResourceName
) : ViewModelBase
{
    [ObservableProperty]
    public partial string EditorContent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial List<string> ErrorMessages { get; set; } = [];

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
        Result<string, List<SettingsError>> validationResult = SettingsValidation.Run(
            EditorContent,
            assembly,
            schemaResourceName
        );

        validationResult.Match(
            _ =>
            {
                IsValid = true;
            },
            error =>
            {
                ErrorMessages = error.Value.Select(settingsError => settingsError.Message).ToList();
            }
        );
    }
}
