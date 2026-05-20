namespace Shoootz.Models;

internal enum SettingsProperty
{
    CurrentLanguageCode,
    JsonExceptionOnValidate,
    ExceptionOnReadContent,
}

internal static class StringExtensions
{
    extension(string? value)
    {
        public SettingsProperty? ToSettingsProperty() =>
            value switch
            {
                "CurrentLanguageCode" => SettingsProperty.CurrentLanguageCode,
                _ => null,
            };
    }
}
