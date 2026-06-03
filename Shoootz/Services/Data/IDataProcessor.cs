namespace Shoootz.Services.Data;

internal interface IDataProcessor
{
    int ErrorParsedShotCounter { get; }

    int ErrorOnSaveShotToDbCounter { get; }

    int SuccessParsedShotCounter { get; }

    int SuccessSavedShotToDbCounter { get; }

    int UdpRawShotCounter { get; }

    int WarmupShotCounter { get; }
}
