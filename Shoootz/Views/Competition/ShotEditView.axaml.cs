using System.Globalization;
using Avalonia.Controls;
using Shoootz.Common.Resources.Lang;
using Shoootz.Models.Shot;
using Shoootz.Views.Extensions;

namespace Shoootz.Views.Competition;

/// <inheritdoc/>
public partial class ShotEditView : UserControl
{
    public ShotEditView()
    {
        InitializeComponent();
    }

    internal void ApplyTo(ShotModel shot)
    {
        shot.DecValue = ParseDecimal(DecValueField.Text);
        shot.Distance = ParseDecimal(DistanceField.Text);
    }

    internal void LoadShot(ShotModel shot)
    {
        DecValueField.Text = shot.DecValue.ToString(CultureInfo.InvariantCulture);
        DistanceField.Text = shot.Distance.ToString(CultureInfo.InvariantCulture);
    }

    internal bool Validate()
    {
        bool isDecValueValid = DecValueField.SetError(
            TryParseDecimal(DecValueField.Text, out _),
            nameof(Messages.ShotValueInvalidMessage)
        );

        bool isDistanceValid = DistanceField.SetError(
            TryParseDecimal(DistanceField.Text, out _),
            nameof(Messages.DistanceInvalidMessage)
        );

        return isDecValueValid && isDistanceValid;
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
}
