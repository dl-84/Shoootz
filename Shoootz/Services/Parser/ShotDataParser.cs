using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using Result;
using Result.Types;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;
using Shoootz.Models.Udp;
using Shoootz.Resources.Lang;
using Shoootz.Services.Localization;

namespace Shoootz.Services.Parser;

internal class ShotDataParser(ILocalizationService localizationService) : IShotDataParser
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public Result<ShotModel?, ShotParseError> Run(byte[] data)
    {
        try
        {
            string rawData = Encoding.UTF8.GetString(data);
            UdpShotModel? udpShot = JsonSerializer.Deserialize<UdpShotModel>(rawData, _options);

            if (udpShot is null)
            {
                return new Error<ShotParseError>(
                    new ShotParseError(localizationService[nameof(Messages.ShotParseErrorNull)], rawData)
                );
            }

            if (udpShot.Objects.Count is not 1)
            {
                string errorMessage = string.Format(
                    localizationService[nameof(Messages.ShotParseErrorObjectCount)],
                    udpShot.Objects.Count
                );

                return new Error<ShotParseError>(new ShotParseError(errorMessage, rawData));
            }

            return udpShot.Objects[0].IsHot ? MapToShotModel(udpShot.Objects[0]) : null;
        }
        catch (Exception exception)
        {
            return new Error<ShotParseError>(new ShotParseError(exception.Message, Convert.ToBase64String(data)));
        }
    }

    private static ShooterModel? MapToShooter(UdpShooterModel? udpShooter)
    {
        if (udpShooter is null)
        {
            return null;
        }

        return new ShooterModel
        {
            Birthyear = udpShooter.Birthyear,
            Club = udpShooter.Club,
            Firstname = udpShooter.Firstname,
            Id = udpShooter.Identification,
            Lastname = udpShooter.Lastname,
            Startnumber = udpShooter.Startnumber,
            Team = udpShooter.Team,
        };
    }

    private static ShotInfoModel? MapToShotInfo(UdpMenuItemModel? udpMenuItem)
    {
        return udpMenuItem is null
            ? null
            : new ShotInfoModel
            {
                MenuId = udpMenuItem.MenuID,
                MenuItemName = udpMenuItem.MenuItemName,
                MenuPointName = udpMenuItem.MenuPointName,
            };
    }

    private static ShotModel MapToShotModel(UdpShotDetailModel udpShotDetail)
    {
        return new ShotModel
        {
            Count = udpShotDetail.Count,
            DecValue = udpShotDetail.DecValue,
            DiscType = udpShotDetail.DiscType,
            Distance = udpShotDetail.Distance,
            IsHot = udpShotDetail.IsHot,
            IsValid = udpShotDetail.IsValid,
            ShotDateTime = DateTime.Now,
            ShotInfo = MapToShotInfo(udpShotDetail.MenuItem),
            Shooter = MapToShooter(udpShotDetail.Shooter),
        };
    }
}
