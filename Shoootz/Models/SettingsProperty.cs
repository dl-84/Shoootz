using Json.Pointer;

namespace Shoootz.Models;

internal enum SettingsProperty
{
    CurrentLanguageCode,
    JsonExceptionOnValidate,
    ExceptionOnReadContent,
}

internal static class JsonPointerExtensions
{
    extension(JsonPointer pointer)
    {
        public SettingsProperty? ToSettingsProperty() =>
            pointer.ToString() switch
            {
                "/CurrentLanguageCode" => SettingsProperty.CurrentLanguageCode,
                _ => null,
            };
    }
}
