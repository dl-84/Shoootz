using System.Text.Json.Serialization;

namespace Shoootz.Models;

internal record PackageModel(
    [property: JsonPropertyName("PackageName")] string Name,
    [property: JsonPropertyName("PackageVersion")] string Version,
    string LicenseType,
    string? LicenseUrl,
    [property: JsonIgnore] string? LicenseContent,
    string Copyright,
    string? PackageUrl
);
