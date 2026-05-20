namespace Shoootz.Models;

internal record SettingsError(SettingsProperty Property, string Message, string? Value = null);
