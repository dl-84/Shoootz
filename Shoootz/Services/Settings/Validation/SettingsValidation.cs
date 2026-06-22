using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Validation;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;

namespace Shoootz.Services.Settings.Validation;

internal static class SettingsValidation
{
    internal static Result<string, List<SettingsError>> Run(string content)
    {
        try
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(content))
            {
                ICollection<ValidationError> errors = LoadSchema().Validate(content);

                if (errors.Count > 0)
                {
                    return new Error<List<SettingsError>>(CollectErrors(errors, jsonDocument));
                }

                return content;
            }
        }
        catch (Exception exception)
        {
            return new Error<List<SettingsError>>([
                new SettingsError(SettingsPropertyType.JsonExceptionOnValidate, exception.Message),
            ]);
        }
    }

    private static List<SettingsError> CollectErrors(ICollection<ValidationError> errors, JsonDocument jsonDocument)
    {
        List<SettingsError> result = [];

        foreach (ValidationError error in errors)
        {
            if (error.Property.ToSettingsProperty() is not { } property)
            {
                continue;
            }

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

            result.Add(new SettingsError(property, message, value));
        }

        return result;
    }

    private static JsonSchema LoadSchema()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "Shoootz.Resources.JsonSchemas.Settings.schema.json";

        using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)!))
        {
            return JsonSchema.FromJsonAsync(reader.ReadToEnd()).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
