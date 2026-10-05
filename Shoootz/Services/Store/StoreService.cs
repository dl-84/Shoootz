using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Result;
using Result.Types;
using Shoootz.Models.Club;
using Shoootz.Models.Database;
using Shoootz.Models.Error;
using Shoootz.Models.Settings;
using Shoootz.Models.Settings.Database;
using Shoootz.Models.Shooter;
using Shoootz.Models.Shot;
using Shoootz.Store;
using Shoootz.Store.Entities;
using Shoootz.Store.Types;

namespace Shoootz.Services.Store;

internal class StoreService(IDbContextFactory<AppDbContext> contextFactory, IOptionsMonitor<SettingsModel> settings)
    : IStoreService
{
    public event EventHandler? ConnectionChanged;

    public string DbVersion { get; private set; } = "N/A";

    public bool IsConnected { get; private set; } = true;

    public void Connect()
    {
        IsConnected = true;
        ConnectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Disconnect()
    {
        IsConnected = false;
        ConnectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<Result<Unit, StoreSaveError>> DeleteClubAsync(int id)
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                ClubEntity? entity = await context.Clubs.FindAsync(id).ConfigureAwait(false);

                if (entity is not null)
                {
                    context.Clubs.Remove(entity);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }

                return Unit.Value;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreSaveError>(new StoreSaveError(GetFullMessage(exception)));
        }
    }

    public async Task<Result<Unit, StoreSaveError>> DeleteDatabaseAsync()
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                bool deleted = await context.Database.EnsureDeletedAsync().ConfigureAwait(false);

                if (!deleted)
                {
                    return new Error<StoreSaveError>(new StoreSaveError("Database not found."));
                }

                return Unit.Value;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreSaveError>(new StoreSaveError(GetFullMessage(exception)));
        }
    }

    public async Task<Result<Unit, StoreSaveError>> DeleteShooterAsync(string id, bool deleteShots)
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                ShooterEntity? entity = await context.Shooters.FindAsync(id).ConfigureAwait(false);

                if (entity is not null)
                {
                    List<ShotEntity> shots = await context
                        .Shots.Where(shot => shot.ShooterId == id)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    if (deleteShots)
                    {
                        context.Shots.RemoveRange(shots);
                    }
                    else
                    {
                        foreach (ShotEntity shot in shots)
                        {
                            shot.ShooterId = null;
                        }
                    }

                    context.Shooters.Remove(entity);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }

                return Unit.Value;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreSaveError>(new StoreSaveError(GetFullMessage(exception)));
        }
    }

    public async Task<string> GenerateShooterIdAsync()
    {
        await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
        {
            List<string> existingIds = await context
                .Shooters.Where(shooter => shooter.Id.StartsWith("M"))
                .Select(shooter => shooter.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            int nextNumber =
                existingIds
                    .Select(id => int.TryParse(id.AsSpan(1), out int number) ? number : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

            return $"M{nextNumber}";
        }
    }

    public async Task<Result<List<ClubModel>, StoreReadError>> GetClubsAsync()
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                List<ClubModel> clubs = await context
                    .Clubs.OrderBy(club => club.Id)
                    .Select(club => new ClubModel
                    {
                        Id = club.Id,
                        Name = club.Name,
                        ShooterCount = club.Shooters.Count,
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);

                return clubs;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreReadError>(new StoreReadError(GetFullMessage(exception)));
        }
    }

    public async Task<DbStatus> GetDbStatusAsync()
    {
        DatabaseConnection databaseConnection = settings.CurrentValue.DatabaseConnection;

        if (databaseConnection is { Provider: ProviderType.Sqlite })
        {
            string dataSource = new SqliteConnectionStringBuilder(databaseConnection.ConnectionString).DataSource;

            if (!File.Exists(dataSource))
            {
                return DbStatus.NotInitialized;
            }
        }

        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                bool canConnect = await context.Database.CanConnectAsync().ConfigureAwait(false);
                if (!canConnect)
                {
                    return DbStatus.NotInitialized;
                }

                List<string> applied = (
                    await context.Database.GetAppliedMigrationsAsync().ConfigureAwait(false)
                ).ToList();

                if (applied.Count == 0)
                {
                    return DbStatus.NotInitialized;
                }

                DbVersion = GetSchemaDate(applied.LastOrDefault());

                List<string> pending = (
                    await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false)
                ).ToList();

                return pending.Count > 0 ? DbStatus.UpdateAvailable : DbStatus.UpToDate;
            }
        }
        catch
        {
            return DbStatus.NotInitialized;
        }
    }

    public async Task<Result<List<ShooterModel>, StoreReadError>> GetShootersAsync()
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                List<ShooterModel> shooters = await context
                    .Shooters.Include(shooter => shooter.Club)
                    .OrderBy(shooter => shooter.Lastname)
                    .ThenBy(shooter => shooter.Firstname)
                    .Select(shooter => new ShooterModel
                    {
                        Birthyear = shooter.Birthyear,
                        Club = shooter.Club!.Name,
                        Firstname = shooter.Firstname,
                        Gender = MapToGenderModel(shooter.Gender),
                        Id = shooter.Id,
                        Lastname = shooter.Lastname,
                        Startnumber = shooter.Startnumber,
                        Team = shooter.Team,
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);

                return shooters;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreReadError>(new StoreReadError(GetFullMessage(exception)));
        }
    }

    public async Task<Result<List<ShotModel>, StoreReadError>> GetShotsAsync()
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                List<ShotEntity> entities = await context
                    .Shots.Include(s => s.Shooter)
                    .Include(s => s.ShotInfo)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return entities.Select(MapToShotModel).ToList();
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreReadError>(new StoreReadError(GetFullMessage(exception)));
        }
    }

    public async Task InitializeAsync()
    {
        await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
        {
            await context.Database.MigrateAsync().ConfigureAwait(false);
        }
    }

    public async Task<Result<Unit, StoreSaveError>> SaveClubAsync(ClubModel club)
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                if (club.Id == 0)
                {
                    context.Clubs.Add(new ClubEntity { Name = club.Name });
                }
                else
                {
                    ClubEntity? entity = await context.Clubs.FindAsync(club.Id).ConfigureAwait(false);

                    if (entity is null)
                    {
                        return new Error<StoreSaveError>(new StoreSaveError($"Club {club.Id} not found."));
                    }

                    entity.Name = club.Name;
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
                return Unit.Value;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreSaveError>(new StoreSaveError(GetFullMessage(exception)));
        }
    }

    public async Task<Result<Unit, StoreSaveError>> SaveShooterAsync(ShooterModel shooter)
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                ClubEntity? club = await FindOrCreateClubAsync(context, shooter.Club).ConfigureAwait(false);
                ShooterEntity? entity = await context.Shooters.FindAsync(shooter.Id).ConfigureAwait(false);

                if (entity is null)
                {
                    entity = new ShooterEntity { Id = shooter.Id! };
                    context.Shooters.Add(entity);
                }

                entity.Birthyear = shooter.Birthyear;
                entity.Club = club;
                entity.Firstname = shooter.Firstname;
                entity.Gender = MapToGenderStore(shooter.Gender);
                entity.Lastname = shooter.Lastname;

                await context.SaveChangesAsync().ConfigureAwait(false);
                return Unit.Value;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreSaveError>(new StoreSaveError(GetFullMessage(exception)));
        }
    }

    public async Task<Result<Unit, StoreSaveError>> SaveShotAsync(ShotModel shot)
    {
        try
        {
            await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
            {
                ShooterEntity? shooterEntity = await FindOrCreateShooterAsync(context, shot.Shooter)
                    .ConfigureAwait(false);

                ShotInfoEntity? shotInfoEntity = await FindOrCreateShotInfoAsync(context, shot.ShotInfo)
                    .ConfigureAwait(false);

                context.Shots.Add(MapToShotEntity(shot, shooterEntity, shotInfoEntity));
                await context.SaveChangesAsync().ConfigureAwait(false);

                return Unit.Value;
            }
        }
        catch (Exception exception)
        {
            return new Error<StoreSaveError>(new StoreSaveError(GetFullMessage(exception)));
        }
    }

    private static async Task<ClubEntity?> FindOrCreateClubAsync(AppDbContext context, string? clubName)
    {
        if (string.IsNullOrWhiteSpace(clubName))
        {
            return null;
        }

        ClubEntity? entity = await context
            .Clubs.FirstOrDefaultAsync(club => club.Name == clubName)
            .ConfigureAwait(false);

        if (entity is null)
        {
            entity = new ClubEntity { Name = clubName };
            context.Clubs.Add(entity);
        }

        return entity;
    }

    private static async Task<ShooterEntity?> FindOrCreateShooterAsync(AppDbContext context, ShooterModel? shooter)
    {
        if (shooter?.Id is null)
        {
            return null;
        }

        ShooterEntity? entity = await context.Shooters.FindAsync(shooter.Id).ConfigureAwait(false);
        ClubEntity? club = await FindOrCreateClubAsync(context, shooter.Club).ConfigureAwait(false);

        if (entity is null)
        {
            entity = new ShooterEntity { Gender = MapToGenderStore(shooter.Gender), Id = shooter.Id };
            context.Shooters.Add(entity);
        }

        entity.Birthyear = shooter.Birthyear;
        entity.Club = club;
        entity.Firstname = shooter.Firstname;
        entity.Lastname = shooter.Lastname;
        entity.Startnumber = shooter.Startnumber;
        entity.Team = shooter.Team;

        return entity;
    }

    private static async Task<ShotInfoEntity?> FindOrCreateShotInfoAsync(AppDbContext context, ShotInfoModel? shotInfo)
    {
        if (shotInfo?.MenuId is null)
        {
            return null;
        }

        ShotInfoEntity? entity = await context.ShotInfos.FindAsync(shotInfo.MenuId).ConfigureAwait(false);

        if (entity is null)
        {
            entity = new ShotInfoEntity
            {
                MenuId = shotInfo.MenuId,
                MenuItemName = shotInfo.MenuItemName,
                MenuPointName = shotInfo.MenuPointName,
            };

            context.ShotInfos.Add(entity);
        }

        return entity;
    }

    private static string GetFullMessage(Exception exception)
    {
        List<string> messages = [];
        Exception? current = exception;

        while (current is not null)
        {
            messages.Add(current.Message);
            current = current.InnerException;
        }

        return string.Join(" -> ", messages);
    }

    private static string GetSchemaDate(string? migrationName)
    {
        if (migrationName is null || migrationName.Length < 14)
        {
            return "N/A";
        }

        return DateTime.TryParseExact(
            migrationName[..14],
            "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime timestamp
        )
            ? timestamp.ToString("yyMMdd", CultureInfo.InvariantCulture)
            : "N/A";
    }

    private static Models.Shooter.GenderType MapToGenderModel(Shoootz.Store.Types.GenderType? gender)
    {
        return gender switch
        {
            Shoootz.Store.Types.GenderType.Diverse => Models.Shooter.GenderType.Diverse,
            Shoootz.Store.Types.GenderType.Female => Models.Shooter.GenderType.Female,
            _ => Models.Shooter.GenderType.Male,
        };
    }

    private static Shoootz.Store.Types.GenderType MapToGenderStore(Models.Shooter.GenderType? gender)
    {
        return gender switch
        {
            Models.Shooter.GenderType.Diverse => Shoootz.Store.Types.GenderType.Diverse,
            Models.Shooter.GenderType.Female => Shoootz.Store.Types.GenderType.Female,
            _ => Shoootz.Store.Types.GenderType.Male,
        };
    }

    private static ShotEntity MapToShotEntity(ShotModel shot, ShooterEntity? shooter, ShotInfoEntity? shotInfo)
    {
        return new ShotEntity
        {
            Count = shot.Count,
            DecValue = shot.DecValue,
            DiscType = shot.DiscType,
            Distance = shot.Distance,
            IsHot = shot.IsHot,
            IsValid = shot.IsValid,
            ShotDateTime = shot.ShotDateTime,
            ShooterId = shooter?.Id,
            ShotInfoMenuId = shotInfo?.MenuId,
        };
    }

    private static ShotModel MapToShotModel(ShotEntity entity)
    {
        return new ShotModel
        {
            Count = entity.Count,
            DecValue = entity.DecValue,
            DiscType = entity.DiscType,
            Distance = entity.Distance,
            IsHot = entity.IsHot,
            IsValid = entity.IsValid,
            ShotDateTime = entity.ShotDateTime,
            Shooter = entity.Shooter is null
                ? null
                : new ShooterModel
                {
                    Birthyear = entity.Shooter.Birthyear,
                    Club = entity.Shooter.Club?.Name,
                    Firstname = entity.Shooter.Firstname,
                    Id = entity.Shooter.Id,
                    Lastname = entity.Shooter.Lastname,
                    Startnumber = entity.Shooter.Startnumber,
                    Team = entity.Shooter.Team,
                },
            ShotInfo = entity.ShotInfo is null
                ? null
                : new ShotInfoModel
                {
                    MenuId = entity.ShotInfo.MenuId,
                    MenuItemName = entity.ShotInfo.MenuItemName,
                    MenuPointName = entity.ShotInfo.MenuPointName,
                },
        };
    }
}
