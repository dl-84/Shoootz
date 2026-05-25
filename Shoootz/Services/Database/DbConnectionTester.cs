using System;
using Microsoft.EntityFrameworkCore;
using Result;
using Result.Struct;
using Shoootz.Context;
using Shoootz.Models.Settings.Database;

namespace Shoootz.Services.Database;

internal class DbConnectionTester : IDbConnectionTester
{
    public Result<bool, DbConnectionError> Run(DbConnectionModel dbConnection)
    {
        try
        {
            DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();

            switch (dbConnection.ProviderType)
            {
                case ProviderType.PostgreSql:
                    builder.UseNpgsql(dbConnection.ConnectionString);
                    break;

                case ProviderType.Sqlite:
                    builder.UseSqlite(dbConnection.ConnectionString);
                    break;

                default:
                    return new Error<DbConnectionError>(new DbConnectionError("Unknown database provider"));
            }

            using AppDbContext context = new AppDbContext(builder.Options);
            return context.Database.CanConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            return new Error<DbConnectionError>(new DbConnectionError(exception.Message));
        }
    }
}
