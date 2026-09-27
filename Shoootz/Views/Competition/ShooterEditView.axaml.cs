using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Controls.FormField;
using Shoootz.Models.Shooter;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ShooterEditView : UserControl
{
    public ShooterEditView()
    {
        InitializeComponent();
    }

    internal ShooterModel BuildShooter()
    {
        int.TryParse(BirthyearField.Text, out int birthyear);

        return new ShooterModel
        {
            Birthyear = birthyear,
            Club = ClubComboBox.SelectedItem as string,
            Firstname = FirstnameField.Text,
            Id = IdField.Text,
            Lastname = LastnameField.Text,
        };
    }

    internal void LoadShooter(ShooterModel shooter)
    {
        IdField.Text = shooter.Id;
        IdField.IsInputEnabled = false;
        LastnameField.Text = shooter.Lastname;
        FirstnameField.Text = shooter.Firstname;
        BirthyearField.Text = shooter.Birthyear == 0 ? null : shooter.Birthyear.ToString();
        ClubComboBox.SelectedItem = shooter.Club;
    }

    internal void SetClubOptions(IEnumerable<string> clubNames)
    {
        ClubComboBox.ItemsSource = clubNames;
    }

    internal bool Validate()
    {
        int currentYear = DateTime.Now.Year;

        bool isBirthyearValid =
            int.TryParse(BirthyearField.Text, out int birthyear)
            && birthyear >= currentYear - 100
            && birthyear <= currentYear;

        bool isFirstnameValid = ValidateField(
            FirstnameField,
            !string.IsNullOrWhiteSpace(FirstnameField.Text),
            nameof(Messages.FirstnameRequiredMessage)
        );

        bool isLastnameValid = ValidateField(
            LastnameField,
            !string.IsNullOrWhiteSpace(LastnameField.Text),
            nameof(Messages.LastnameRequiredMessage)
        );

        isBirthyearValid = ValidateField(BirthyearField, isBirthyearValid, nameof(Messages.BirthyearInvalidMessage));

        bool isClubValid = ValidateComboBox(
            ClubComboBox,
            ClubErrorText,
            ClubComboBox.SelectedItem is string,
            nameof(Messages.ClubRequiredMessage)
        );

        return isFirstnameValid && isLastnameValid && isBirthyearValid && isClubValid;
    }

    private static bool ValidateComboBox(ComboBox comboBox, TextBlock errorText, bool isValid, string messageKey)
    {
        comboBox.Classes.Set("error", !isValid);
        errorText.Text = isValid ? null : LocalizationService.Instance[messageKey];
        errorText.IsVisible = !isValid;

        return isValid;
    }

    private static bool ValidateField(FormFieldControl field, bool isValid, string messageKey)
    {
        field.IsError = !isValid;
        field.ErrorMessage = isValid ? null : LocalizationService.Instance[messageKey];

        return isValid;
    }
}
