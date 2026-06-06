using System.Collections.Generic;
using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Database;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Store;

internal interface IStoreService
{
    Task<DbStatus> GetDbStatusAsync();

    Task<string> GetDbVersionAsync();

    Task<Result<List<ShotModel>, StoreReadError>> GetShotsAsync();

    Task InitializeAsync();

    Task<Result<Unit, StoreSaveError>> SaveShotAsync(ShotModel shot);
}
