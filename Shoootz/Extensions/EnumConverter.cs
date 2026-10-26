using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Shoootz.Common.Resources.Lang;
using Shoootz.Common.Services.Localization;
using Shoootz.Models.Shooter;

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

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ToDisplayText(value as GenderType?);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
