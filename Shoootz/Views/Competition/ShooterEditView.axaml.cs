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
    private string? _shooterId;

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
            Gender = GetSelectedGender(),
            Id = _shooterId,
            Lastname = LastnameField.Text,
        };
    }

    internal void LoadShooter(ShooterModel shooter)
    {
        _shooterId = shooter.Id;
        LastnameField.Text = shooter.Lastname;
        FirstnameField.Text = shooter.Firstname;
        BirthyearField.Text = shooter.Birthyear == 0 ? null : shooter.Birthyear.ToString();
        ClubComboBox.SelectedItem = shooter.Club;

        GenderType gender = shooter.Gender ?? GenderType.Male;
        MaleRadioButton.IsChecked = gender == GenderType.Male;
        FemaleRadioButton.IsChecked = gender == GenderType.Female;
        DiverseRadioButton.IsChecked = gender == GenderType.Diverse;
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

    private GenderType GetSelectedGender()
    {
        if (FemaleRadioButton.IsChecked == true)
        {
            return GenderType.Female;
        }

        if (DiverseRadioButton.IsChecked == true)
        {
            return GenderType.Diverse;
        }

        return GenderType.Male;
    }
}
