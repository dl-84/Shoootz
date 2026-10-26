using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Validation;
using Result;
using Result.Types;
using Shoootz.Common.Models.Language.Error;

namespace Shoootz.Common.Services.Settings;

public static class SettingsValidation
{
    public static Result<string, List<SettingsError>> Run(string content, Assembly assembly, string schemaResourceName)
    {
        try
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(content))
            {
                ICollection<ValidationError> errors = LoadSchema(assembly, schemaResourceName).Validate(content);

                if (errors.Count > 0)
                {
                    return new Error<List<SettingsError>>(CollectErrors(errors, jsonDocument));
                }

                return content;
            }
        }
        catch (Exception exception)
        {
            return new Error<List<SettingsError>>([new SettingsError("JsonExceptionOnValidate", exception.Message)]);
        }
    }

    private static List<SettingsError> CollectErrors(ICollection<ValidationError> errors, JsonDocument jsonDocument)
    {
        List<SettingsError> result = [];

        foreach (ValidationError error in errors)
        {
            string? value =
                error.Property is not null
                && jsonDocument.RootElement.TryGetProperty(error.Property, out JsonElement element)
                    ? element.ToString()
                    : null;

            string allowed = error is { Kind: ValidationErrorKind.NotInEnumeration, Schema.Enumeration.Count: > 0 }
                ? $" — Valid: {string.Join(", ", error.Schema.Enumeration)}"
                : string.Empty;

            string message = value is not null
                ? $"{error.Property}: {error.Kind} ('{value}'){allowed}"
                : $"{error.Property}: {error.Kind}{allowed}";

            result.Add(new SettingsError(error.Property, message, value));
        }

        return result;
    }

    private static JsonSchema LoadSchema(Assembly assembly, string schemaResourceName)
    {
        using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(schemaResourceName)!))
        {
            return JsonSchema.FromJsonAsync(reader.ReadToEnd()).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
