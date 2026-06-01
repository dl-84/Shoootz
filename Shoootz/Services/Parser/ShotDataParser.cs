using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Result;
using Result.Struct;
using Shoootz.Models.Shot;
using Shoootz.Services.Udp;

namespace Shoootz.Services.Parser;

internal class ShotDataParser : IShotDataParser
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly IUdpListenerService _udpListenerService;

    public ShotDataParser(IUdpListenerService udpListenerService)
    {
        _udpListenerService = udpListenerService;
        _ = ProcessLoopAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    private static Result<Shot, ShotParseError> Run(byte[] data)
    {
        try
        {
            string json = Encoding.UTF8.GetString(data);
            Shot? message = JsonSerializer.Deserialize<Shot>(json, _options);

            if (message is null)
            {
                return new Error<ShotParseError>(new ShotParseError("Deserialization returned null."));
            }

            return message;
        }
        catch (Exception exception)
        {
            return new Error<ShotParseError>(new ShotParseError(exception.Message));
        }
    }

    private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (byte[] data in _udpListenerService.Reader.ReadAllAsync(cancellationToken))
        {
            _ = Run(data);
        }
    }
}
