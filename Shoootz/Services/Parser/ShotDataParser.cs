using System;
using System.Text.Json;
using Result;
using Result.Types;
using Shoootz.DisagOriginUpdMessage;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Parser;

internal class ShotDataParser : IShotDataParser
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public Result<ShotModel, ShotCategoryType> Run(string rawData)
    {
        try
        {
            DisagUdpMessageDto? message = JsonSerializer.Deserialize<DisagUdpMessageDto>(rawData, _options);

            if (message is null)
            {
                return new Error<ShotCategoryType>(ShotCategoryType.MessageIsNull);
            }

            if (message.Objects?.Count is not 1)
            {
                return new Error<ShotCategoryType>(ShotCategoryType.IncorrectNumberOfObjects);
            }

            if (message.Objects[0] is not DisagShotDto disagShot)
            {
                return new Error<ShotCategoryType>(ShotCategoryType.IsNotTypeOfShot);
            }

            if (string.IsNullOrEmpty(disagShot.Shooter?.Firstname) || string.IsNullOrEmpty(disagShot.Shooter?.Lastname))
            {
                return new Error<ShotCategoryType>(ShotCategoryType.HasMissingShooterData);
            }

            if (disagShot.IsWarmup)
            {
                return new Error<ShotCategoryType>(ShotCategoryType.IsWarmup);
            }

            return MapToShotModel(disagShot);
        }
        catch (Exception)
        {
            return new Error<ShotCategoryType>(ShotCategoryType.Exception);
        }
    }

    private static ShooterModel? MapToShooter(DisagShooterDto? disagShooter)
    {
        if (disagShooter is null)
        {
            return null;
        }

        return new ShooterModel
        {
            Birthyear = disagShooter.Birthyear,
            Club = disagShooter.Club?.Name,
            Firstname = disagShooter.Firstname,
            Id = disagShooter.Identification,
            Lastname = disagShooter.Lastname,
            Startnumber = disagShooter.Startnumber,
            Team = disagShooter.Team?.Name,
        };
    }

    private static ShotInfoModel? MapToShotInfo(DisagMenuItemDto? disagMenuItem)
    {
        return disagMenuItem is null
            ? null
            : new ShotInfoModel
            {
                MenuId = disagMenuItem.MenuID,
                MenuItemName = disagMenuItem.MenuItemName,
                MenuPointName = disagMenuItem.MenuPointName,
            };
    }

    private static ShotModel MapToShotModel(DisagShotDto disagShot)
    {
        return new ShotModel
        {
            Count = disagShot.Count,
            DecValue = (double)disagShot.DecValue,
            DiscType = disagShot.DiscType.ToString(),
            Distance = (double)disagShot.Distance,
            IsHot = disagShot.IsHot,
            IsValid = disagShot.IsValid,
            ShotDateTime = DateTime.Now,
            ShotInfo = MapToShotInfo(disagShot.MenuItem),
            Shooter = MapToShooter(disagShot.Shooter),
        };
    }
}
