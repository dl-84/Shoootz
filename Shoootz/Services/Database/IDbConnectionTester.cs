using Result;
using Shoootz.Models.Settings.Database;

namespace Shoootz.Services.Database;

internal interface IDbConnectionTester
{
    Result<bool, DbConnectionError> Run(DbConnectionModel dbConnection);
}
