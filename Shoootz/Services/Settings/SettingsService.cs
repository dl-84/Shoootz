using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Validation;
using Result;
using Shoootz.Models.Settings;
using Shoootz.Services.App;

namespace Shoootz.Services.Settings;

internal class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    private static readonly JsonSchema _schema = LoadSchema();

    public void DeleteSettingsFile()
    {
        if (File.Exists(AppPath.SettingsFile))
        {
            File.Delete(AppPath.SettingsFile);
        }
    }

    public void DeleteSettingsFolder()
    {
        if (Directory.Exists(AppPath.AppDataBase))
        {
            Directory.Delete(AppPath.AppDataBase, true);
        }
    }

    public string LoadRaw()
    {
        return File.Exists(AppPath.SettingsFile) ? File.ReadAllText(AppPath.SettingsFile) : string.Empty;
    }

    public Result<SettingsModel, List<SettingsError>> Load()
    {
        if (!File.Exists(AppPath.SettingsFile))
        {
            SettingsModel settings = new();
            Save(settings);
            return settings;
        }

        return ReadContent().AndThen(Validate);
    }

    public void Save(SettingsModel? settings)
    {
        if (settings is null)
        {
            return;
        }

        SaveRaw(JsonSerializer.Serialize(settings, _jsonSerializerOptions));
    }

    public void SaveRaw(string content)
    {
        if (!Directory.Exists(AppPath.AppDataBase))
        {
            Directory.CreateDirectory(AppPath.AppDataBase);
        }

        File.WriteAllText(AppPath.SettingsFile, content);
    }

    public Result<SettingsModel, List<SettingsError>> Validate(string content)
    {
        try
        {
            using JsonDocument jsonDocument = JsonDocument.Parse(content);

            ICollection<ValidationError> errors = _schema.Validate(content);

            if (errors.Count > 0)
            {
                return new Error<List<SettingsError>>(CollectErrors(errors, jsonDocument));
            }

            SettingsModel result = JsonSerializer.Deserialize<SettingsModel>(content, _jsonSerializerOptions)!;
            return result;
        }
        catch (JsonException exception)
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

            string? value = jsonDocument.RootElement.TryGetProperty(error.Property!, out JsonElement element)
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

        using StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)!);
        return JsonSchema.FromJsonAsync(reader.ReadToEnd()).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private static Result<string, List<SettingsError>> ReadContent()
    {
        try
        {
            return File.ReadAllText(AppPath.SettingsFile);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new Error<List<SettingsError>>([
                new SettingsError(SettingsPropertyType.ExceptionOnReadContent, exception.Message),
            ]);
        }
    }
}
