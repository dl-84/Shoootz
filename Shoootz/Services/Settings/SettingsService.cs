using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Validation;
using Result;
using Shoootz.Models;

namespace Shoootz.Services.Settings;

internal class SettingsService : ISettingsService
{
    private const string fileName = "settings.json";

    private static readonly string _directoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppDomain.CurrentDomain.FriendlyName
    );

    private static readonly string _filePath = Path.Combine(_directoryPath, fileName);

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    private static readonly JsonSchema _schema = LoadSchema();

    public string FolderPath => _directoryPath;

    public void Delete()
    {
        Directory.Delete(_directoryPath, true);
    }

    public Result<SettingsModel, List<SettingsError>> Load()
    {
        if (!File.Exists(_filePath))
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

        Directory.CreateDirectory(_directoryPath);
        File.WriteAllText(_filePath, JsonSerializer.Serialize(settings, _jsonSerializerOptions));
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

            result.Add(new SettingsError(property, error.Kind.ToString(), value));
        }

        return result;
    }

    private static JsonSchema LoadSchema()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "Shoootz.Resources.JsonSchemas.Settings.schema.json";

        using StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)!);
        return JsonSchema.FromJsonAsync(reader.ReadToEnd()).GetAwaiter().GetResult();
    }

    private static Result<SettingsModel, List<SettingsError>> Validate(string content)
    {
        try
        {
            ICollection<ValidationError> errors = _schema.Validate(content);

            if (errors.Count > 0)
            {
                using JsonDocument jsonDocument = JsonDocument.Parse(content);
                return new Error<List<SettingsError>>(CollectErrors(errors, jsonDocument));
            }

            SettingsModel result = JsonSerializer.Deserialize<SettingsModel>(content, _jsonSerializerOptions)!;
            return result;
        }
        catch (JsonException exception)
        {
            return new Error<List<SettingsError>>([
                new SettingsError(SettingsProperty.JsonExceptionOnValidate, exception.Message),
            ]);
        }
    }

    private static Result<string, List<SettingsError>> ReadContent()
    {
        try
        {
            return File.ReadAllText(_filePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new Error<List<SettingsError>>([
                new SettingsError(SettingsProperty.ExceptionOnReadContent, exception.Message),
            ]);
        }
    }
}
