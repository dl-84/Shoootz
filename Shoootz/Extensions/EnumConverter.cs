using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Shoootz.Models.Shooter;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;

namespace Shoootz.Extensions;

internal class EnumConverter : IValueConverter
{
    public static string ToDisplayText(GenderType? gender)
    {
        return gender switch
        {
            GenderType.Female => LocalizationService.Instance[nameof(Messages.GenderFemale)],
            GenderType.Diverse => LocalizationService.Instance[nameof(Messages.GenderDiverse)],
            _ => LocalizationService.Instance[nameof(Messages.GenderMale)],
        };
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ToDisplayText(value as GenderType?);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
