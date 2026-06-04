using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Result;
using Result.Types;

namespace Shoootz.Store.Services;

public class ConnectionTester : IConnectionTester
{
    public Result<bool, DbConnectionError> PostgreSql(string connectionString)
    {
        try
        {
            DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseNpgsql(connectionString);

            using (AppDbContext context = new AppDbContext(builder.Options))
            {
                return context.Database.CanConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (Exception exception)
        {
            return new Error<DbConnectionError>(new DbConnectionError(exception.Message));
        }
    }

    public Result<bool, DbConnectionError> Sqlite(string connectionString)
    {
        try
        {
            string dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

            if (!File.Exists(dataSource))
            {
                return new Error<DbConnectionError>(new DbConnectionError($"Database file not found: {dataSource}"));
            }

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();
                return true;
            }
        }
        catch (Exception exception)
        {
            return new Error<DbConnectionError>(new DbConnectionError(exception.Message));
        }
    }
}
