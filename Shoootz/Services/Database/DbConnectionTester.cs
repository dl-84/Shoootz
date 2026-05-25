using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Result;
using Result.Struct;
using Shoootz.Context;
using Shoootz.Models.Settings.Database;
using Shoootz.Services.Localization;

namespace Shoootz.Services.Database;

internal class DbConnectionTester : IDbConnectionTester
{
    public Result<bool, DbConnectionError> Run(DbConnectionModel dbConnection)
    {
        try
        {
            return dbConnection.ProviderType switch
            {
                ProviderType.PostgreSql => TestPostgresConnection(dbConnection.ConnectionString),
                ProviderType.Sqlite => TestSqliteConnection(dbConnection.ConnectionString),
                _ => new Error<DbConnectionError>(
                    new DbConnectionError(LocalizationService.Instance["DbErrorUnknownProvider"])
                ),
            };
        }
        catch (Exception exception)
        {
            return new Error<DbConnectionError>(new DbConnectionError(exception.Message));
        }
    }

    private static Result<bool, DbConnectionError> TestPostgresConnection(string connectionString)
    {
        DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseNpgsql(connectionString);

        using (AppDbContext context = new AppDbContext(builder.Options))
        {
            return context.Database.CanConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    private static Result<bool, DbConnectionError> TestSqliteConnection(string connectionString)
    {
        string dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

        if (!File.Exists(dataSource))
        {
            return new Error<DbConnectionError>(
                new DbConnectionError(string.Format(LocalizationService.Instance["DbErrorFileNotFound"], dataSource))
            );
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
}
