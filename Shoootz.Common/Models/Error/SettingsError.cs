namespace Shoootz.Common.Models.Language.Error;

public record SettingsError(string? PropertyName, string Message, string? Value = null);
