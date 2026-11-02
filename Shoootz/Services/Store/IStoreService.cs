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
using Shoootz.Services.ShotIntegrity;

namespace Shoootz.Services.Store;

internal interface IStoreService
{
    event EventHandler? ConnectionChanged;

    string DbVersion { get; }

    bool IsConnected { get; }

    Task<Result<bool, StoreReadErrorModel>> ClubHasShootersAsync(int id);

    void Connect();

    void Disconnect();

    Task<Result<Unit, StoreSaveErrorModel>> DeleteClubAsync(int id);

    Task<Result<Unit, StoreSaveErrorModel>> DeleteDatabaseAsync();

    Task<Result<Unit, StoreSaveErrorModel>> DeleteShooterAsync(string id, bool deleteShots);

    Task<Result<Unit, StoreSaveErrorModel>> DeleteShotsAsync(IReadOnlyCollection<int> ids);

    Task<Result<Unit, StoreSaveErrorModel>> EditShotAsync(ShotModel shot);

    Task<string> GenerateShooterIdAsync();

    Task<Result<List<ClubModel>, StoreReadErrorModel>> GetClubsAsync();

    Task<DbStatusType> GetDbStatusAsync();

    Task<Result<List<ShooterModel>, StoreReadErrorModel>> GetShootersAsync();

    Task<Result<List<ShotModel>, StoreReadErrorModel>> GetShotsAsync();

    Task InitializeAsync();

    Task<Result<Unit, StoreSaveErrorModel>> SaveClubAsync(ClubModel club);

    Task<Result<Unit, StoreSaveErrorModel>> SaveManualShotsAsync(IReadOnlyCollection<ShotModel> shots);

    Task<Result<Unit, StoreSaveErrorModel>> SaveShooterAsync(ShooterModel shooter, bool isNew);

    Task<Result<Unit, StoreSaveErrorModel>> SaveShotAsync(ShotModel shot);

    Task<List<ShotIntegrityError>> VerifyShotIntegrityAsync();
}
