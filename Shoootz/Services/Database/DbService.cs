using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Result;
using Result.Types;
using Shoootz.Context;
using Shoootz.Context.Entities;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Database;

internal class DbService(IDbContextFactory<AppDbContext> contextFactory) : IDbService
{
    public async Task InitializeAsync()
    {
        await using (AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false))
        {
            await context.Database.MigrateAsync().ConfigureAwait(false);
        }
    }

    public async Task<Result<Unit, DbSaveError>> SaveShotAsync(ShotModel shot)
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
            return new Error<DbSaveError>(new DbSaveError(exception.Message));
        }
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

    private static async Task<ShooterEntity?> FindOrCreateShooterAsync(AppDbContext context, ShooterModel? shooter)
    {
        if (shooter?.Id is null)
        {
            return null;
        }

        ShooterEntity? entity = await context.Shooters.FindAsync(shooter.Id).ConfigureAwait(false);

        if (entity is null)
        {
            entity = new ShooterEntity
            {
                Birthyear = shooter.Birthyear,
                Club = shooter.Club,
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
