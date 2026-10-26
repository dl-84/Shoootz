using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Result;
using Result.Types;
using Shoootz.Store.Types;

namespace Shoootz.Store.Services;

public class ConnectionTester : IConnectionTester
{
    public Task<Result<bool, DbConnectionError>> Run(ProviderType provider, string connectionString)
    {
        return provider switch
        {
            ProviderType.PostgreSql => PostgreSql(connectionString),
            _ => Sqlite(connectionString),
        };
    }

    private static async Task<Result<bool, DbConnectionError>> PostgreSql(string connectionString)
    {
        try
        {
            DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseNpgsql(connectionString);

            await using (AppDbContext context = new AppDbContext(builder.Options))
            {
                bool canConnect = await context.Database.CanConnectAsync().ConfigureAwait(false);

                return canConnect ? true : new Error<DbConnectionError>(new DbConnectionError("Connection failed."));
            }
        }
        catch (Exception exception)
        {
            return new Error<DbConnectionError>(new DbConnectionError(exception.Message));
        }
    }

    private static Task<Result<bool, DbConnectionError>> Sqlite(string connectionString)
    {
        try
        {
            string dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

            if (!File.Exists(dataSource))
            {
                return Task.FromResult<Result<bool, DbConnectionError>>(
                    new Error<DbConnectionError>(new DbConnectionError($"Database file not found: {dataSource}"))
                );
            }

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();

                // Language=SQLite
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();
                return Task.FromResult<Result<bool, DbConnectionError>>(true);
            }
        }
        catch (Exception exception)
        {
            return Task.FromResult<Result<bool, DbConnectionError>>(
                new Error<DbConnectionError>(new DbConnectionError(exception.Message))
            );
        }
    }
}
