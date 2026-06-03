namespace Shoootz.Services.Data;

internal interface IDataProcessor
{
    int ErrorParsedShotCounter { get; }

    int SavedToDbShotCounter { get; }

    int SuccessParsedShotCounter { get; }

    int UdpRawShotCounter { get; }

    int WarmupShotCounter { get; }
}
