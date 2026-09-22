using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Result;
using Result.Types;
using Shoootz.Models.Club;
using Shoootz.Models.Database;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;
using Shoootz.Store;
using Shoootz.Store.Entities;

namespace Shoootz.Services.Store;

internal class StoreService(IDbContextFactory<AppDbContext> contextFactory) : IStoreService
{
    public string DbVersion { get; private set; } = "N/A";

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
            return new Error<StoreSaveError>(new StoreSaveError(exception.Message));
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
            return new Error<StoreReadError>(new StoreReadError(exception.Message));
        }
    }

    public async Task<DbStatus> GetDbStatusAsync()
    {
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

                DbVersion = applied.LastOrDefault() switch
                {
                    "20260604150038_InitialSchema" => "1.0.0",
                    "20260604150029_InitialSchema" => "1.0.0",
                    _ => "N/A",
                };

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
            return new Error<StoreReadError>(new StoreReadError(exception.Message));
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
            return new Error<StoreSaveError>(new StoreSaveError(exception.Message));
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
            return new Error<StoreSaveError>(new StoreSaveError(exception.Message));
        }
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

        if (entity is null)
        {
            ClubEntity? club = await FindOrCreateClubAsync(context, shooter.Club).ConfigureAwait(false);

            entity = new ShooterEntity
            {
                Birthyear = shooter.Birthyear,
                Club = club,
                Firstname = shooter.Firstname,
                Id = shooter.Id,
                Lastname = shooter.Lastname,
                Startnumber = shooter.Startnumber,
                Team = shooter.Team,
            };

            context.Shooters.Add(entity);
        }

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
}
