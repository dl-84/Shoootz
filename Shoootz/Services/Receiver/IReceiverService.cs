using System;
using System.Threading.Tasks;
using Result;
using Result.Types;
using Shoootz.Models.Error;

namespace Shoootz.Services.Receiver;

internal interface IReceiverService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    event EventHandler<byte[]> ShotRawDataReceived;

    bool IsListening { get; }

    Task<Result<Unit, ReceiverConnectionErrorModel>> ConnectAsync();

    void Disconnect();

    Result<bool, ReceiverConnectionErrorModel> TestConnection();
}
