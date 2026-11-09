using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Localization;
using Shoootz.DisagOriginUpdMessage;
using Shoootz.Models.Shooter;
using Shoootz.Models.Shot;
using Shoootz.Views.Extensions;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ManualShotEntryView : UserControl
{
    private const int MaxShotValue = 10;

    private const int MaxSeriesTotal = ShotsPerSeries * MaxShotValue;

    private const int ShotsPerSeries = 10;

    public ManualShotEntryView()
    {
        InitializeComponent();

        DiscTypeComboBox.ItemsSource = new[]
        {
            DisagDiscType.LG,
            DisagDiscType.LP,
            DisagDiscType.LGA,
            DisagDiscType.LPA,
        };
        DiscTypeComboBox.SelectedItem = DisagDiscType.LG;
    }

    internal List<ShotModel> BuildShots()
    {
        ShooterModel? shooter = ShooterComboBox.SelectedItem as ShooterModel;
        string? discType = (DiscTypeComboBox.SelectedItem as DisagDiscType?)?.ToString();

        if (SingleShotRadioButton.IsChecked == true)
        {
            return
            [
                new ShotModel
                {
                    Count = 1,
                    DecValue = ParseDecimal(SingleDecValueField.Text),
                    DiscType = discType,
                    Distance = ParseDecimal(SingleDistanceField.Text),
                    IsHot = false,
                    IsValid = true,
                    ShotDateTime = DateTime.Now,
                    Shooter = shooter,
                },
            ];
        }

        int.TryParse(SeriesTotalField.Text, out int seriesTotal);

        return DistributeSeriesTotal(seriesTotal)
            .Select(shotValue => new ShotModel
            {
                Count = 1,
                DecValue = shotValue,
                DiscType = discType,
                Distance = 0,
                IsHot = false,
                IsValid = true,
                ShotDateTime = DateTime.Now,
                Shooter = shooter,
            })
            .ToList();
    }

    internal void SetShooterOptions(List<ShooterModel> shooters)
    {
        ShooterComboBox.ItemsSource = shooters;
    }

    internal bool Validate()
    {
        bool isShooterValid = ValidateComboBox(
            ShooterComboBox,
            ShooterErrorText,
            ShooterComboBox.SelectedItem is ShooterModel,
            nameof(Messages.ShooterRequiredMessage)
        );

        bool isDiscTypeValid = ValidateComboBox(
            DiscTypeComboBox,
            DiscTypeErrorText,
            DiscTypeComboBox.SelectedItem is DisagDiscType,
            nameof(Messages.WeaponTypeRequiredMessage)
        );

        bool isEntryValid = SingleShotRadioButton.IsChecked == true ? ValidateSingleShot() : ValidateSeriesTotal();

        return isShooterValid && isDiscTypeValid && isEntryValid;
    }

    private static List<int> DistributeSeriesTotal(int total)
    {
        int fullShots = Math.Min(total / MaxShotValue, ShotsPerSeries);
        int remainder = total - (fullShots * MaxShotValue);
        int remainingSlots = ShotsPerSeries - fullShots;

        List<int> values = Enumerable.Repeat(MaxShotValue, fullShots).ToList();

        for (int i = 0; i < remainingSlots; i++)
        {
            values.Add(i == 0 ? remainder : 0);
        }

        return values;
    }

    private static double ParseDecimal(string? text)
    {
        TryParseDecimal(text, out double value);
        return value;
    }

    private static bool TryParseDecimal(string? text, out double value)
    {
        return double.TryParse(text?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool ValidateComboBox(ComboBox comboBox, TextBlock errorText, bool isValid, string messageKey)
    {
        comboBox.Classes.Set("error", !isValid);
        errorText.Text = isValid ? null : LocalizationService.Instance[messageKey];
        errorText.IsVisible = !isValid;

        return isValid;
    }

    private void OnEntryModeChanged(object? sender, RoutedEventArgs e)
    {
        bool isSingleShot = SingleShotRadioButton.IsChecked == true;

        SeriesTotalField.IsVisible = !isSingleShot;
        SingleShotPanel.IsVisible = isSingleShot;
    }

    private bool ValidateSeriesTotal()
    {
        bool isInRange = int.TryParse(SeriesTotalField.Text, out int total) && total >= 0 && total <= MaxSeriesTotal;

        return SeriesTotalField.SetError(isInRange, nameof(Messages.SeriesTotalInvalidMessage));
    }

    private bool ValidateSingleShot()
    {
        bool isDecValueValid = SingleDecValueField.SetError(
            TryParseDecimal(SingleDecValueField.Text, out _),
            nameof(Messages.ShotValueInvalidMessage)
        );

        bool isDistanceProvided = !string.IsNullOrWhiteSpace(SingleDistanceField.Text);

        bool isDistanceValid = SingleDistanceField.SetError(
            !isDistanceProvided || TryParseDecimal(SingleDistanceField.Text, out _),
            nameof(Messages.DistanceInvalidMessage)
        );

        return isDecValueValid && isDistanceValid;
    }
}
