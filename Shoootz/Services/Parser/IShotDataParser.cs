using Result;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Parser;

internal interface IShotDataParser
{
    Result<Shot, ShotParseError> Run(byte[] data);
}
