using Result;

namespace Shoootz.Store.Services;

public interface IConnectionTester
{
    Result<bool, DbConnectionError> PostgreSql(string connectionString);

    Result<bool, DbConnectionError> Sqlite(string connectionString);
}
