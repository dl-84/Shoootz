using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Shoootz.Models.Shot;
using Shoootz.Services.Parser;

namespace Shoootz.Services.Data;

internal class DataManager : IDataManager, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly IShotDataParser _parser;

    private readonly Channel<byte[]> _udpChannel = Channel.CreateUnbounded<byte[]>();

    public DataManager(IShotDataParser parser)
    {
        _parser = parser;
        _ = ProcessUdpChannelReaderLoopAsync(_cancellationTokenSource.Token);
    }

    public ChannelWriter<byte[]> UdpChannel => _udpChannel.Writer;

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    private async Task ProcessUdpChannelReaderLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (byte[] data in _udpChannel.Reader.ReadAllAsync(cancellationToken))
        {
            _parser
                .Run(data)
                .Match(
                    shot =>
                    {
                        ShotModel? tt = shot;
                    },
                    error =>
                    {
                        var zz = error;
                    }
                );
        }
    }
}
