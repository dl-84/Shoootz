using System;
using Result;
using Result.Types;
using Shoootz.Models.Error;

namespace Shoootz.Services.Receiver;

internal interface IReceiverService : IDisposable
{
    event EventHandler<bool> IsListeningChanged;

    event EventHandler<byte[]> ShotRawDataReceived;

    bool IsListening { get; }

    Result<Unit, ReceiverConnectionErrorModel> Connect();

    void Disconnect();

    Result<bool, ReceiverConnectionErrorModel> TestConnection();
}
