using Result;
using Shoootz.Models.Error;
using Shoootz.Models.Shot;

namespace Shoootz.Services.Parser;

internal interface IShotDataParser
{
    Result<ShotModel?, ShotParseError> Run(byte[] data);
}
