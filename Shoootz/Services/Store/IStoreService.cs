using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Club;
using Shoootz.Models.Database;
using Shoootz.Models.Error;
using Shoootz.Models.Shooter;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Store;

internal interface IStoreService
{
    event EventHandler? ConnectionChanged;

    string DbVersion { get; }

    bool IsConnected { get; }

    void Connect();

    void Disconnect();

    Task<Result<Unit, StoreSaveError>> DeleteClubAsync(int id);

    Task<Result<Unit, StoreSaveError>> DeleteDatabaseAsync();

    Task<Result<Unit, StoreSaveError>> DeleteShooterAsync(string id, bool deleteShots);

    Task<string> GenerateShooterIdAsync();

    Task<Result<List<ClubModel>, StoreReadError>> GetClubsAsync();

    Task<DbStatus> GetDbStatusAsync();

    Task<Result<List<ShooterModel>, StoreReadError>> GetShootersAsync();

    Task<Result<List<ShotModel>, StoreReadError>> GetShotsAsync();

    Task InitializeAsync();

    Task<Result<Unit, StoreSaveError>> SaveClubAsync(ClubModel club);

    Task<Result<Unit, StoreSaveError>> SaveShooterAsync(ShooterModel shooter);

    Task<Result<Unit, StoreSaveError>> SaveShotAsync(ShotModel shot);
}
