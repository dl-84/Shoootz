using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Result;
using Sektionsliga.Models;

namespace Sektionsliga.Services.Settings;

internal class SettingsService : ISettingsService
{
    private const string fileName = "settings.json";

    private static readonly string _directoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppDomain.CurrentDomain.FriendlyName
    );

    private static readonly string _filePath = Path.Combine(_directoryPath, fileName);

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

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

    private static List<SettingsError> CollectErrors(EvaluationResults evaluationResults, JsonNode? jsonNode)
    {
        List<SettingsError> result = [];

        foreach (EvaluationResults detail in evaluationResults.Details ?? [])
        {
            if (detail.IsValid || detail.Errors == null)
            {
                continue;
            }

            SettingsProperty? property = detail.InstanceLocation.ToSettingsProperty();

            if (property is null)
            {
                continue;
            }

            string? value = null;
            if (detail.InstanceLocation.TryEvaluate(jsonNode, out JsonNode? node))
            {
                value = node?.GetValue<string>();
            }

            foreach (KeyValuePair<string, string> error in detail.Errors!)
            {
                result.Add(new SettingsError(property.Value, error.Value, value));
            }
        }

        return result;
    }

    private static JsonSchema LoadSchema()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "Sektionsliga.Resources.JsonSchemas.Settings.schema.json";

        using StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)!);

        return JsonSchema.FromText(reader.ReadToEnd());
    }

    private static Result<SettingsModel, List<SettingsError>> Validate(string content)
    {
        try
        {
            using (JsonDocument document = JsonDocument.Parse(content))
            {
                EvaluationOptions options = new() { OutputFormat = OutputFormat.List };
                EvaluationResults evaluationResults = LoadSchema().Evaluate(document.RootElement, options);

                if (evaluationResults is { IsValid: false })
                {
                    return new Error<List<SettingsError>>(CollectErrors(evaluationResults, JsonNode.Parse(content)));
                }
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
