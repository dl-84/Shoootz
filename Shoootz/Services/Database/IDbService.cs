using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Database;

internal interface IDbService
{
    Task InitializeAsync();

    Task<Result<Unit, DbSaveError>> SaveShotAsync(ShotModel shot);
}
