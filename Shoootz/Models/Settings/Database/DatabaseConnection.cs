using System.Text.Json.Serialization;
using Shoootz.Services.App;

namespace Shoootz.Models.Settings.Database;

internal class DatabaseConnection
{
    public string ConnectionString { get; set; } = $"Data Source={AppPath.DbFile}";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProviderType Provider { get; set; } = ProviderType.Sqlite;
}
