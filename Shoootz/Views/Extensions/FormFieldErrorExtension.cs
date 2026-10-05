using Controls.FormField;
using Shoootz.Services.Localization;

namespace Shoootz.Views.Extensions;

internal static class FormFieldErrorExtension
{
    public static bool SetError(this FormFieldControl field, bool isValid, string messageKey)
    {
        field.IsError = !isValid;
        field.ErrorMessage = isValid ? null : LocalizationService.Instance[messageKey];

        return isValid;
    }
}
