using Shoootz.Models.Settings;

namespace Shoootz.Models.Error;

internal record SettingsError(SettingsPropertyType PropertyType, string Message, string? Value = null);
