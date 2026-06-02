using Result;
using Shoootz.Models.Error;
using Shoootz.Models.Udp;

namespace Shoootz.Services.Parser;

internal interface IShotDataParser
{
    Result<UdpShot, ShotParseError> Run(byte[] data);
}
