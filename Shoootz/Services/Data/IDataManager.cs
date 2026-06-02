using System.Threading.Channels;

namespace Shoootz.Services.Data;

internal interface IDataManager
{
    ChannelWriter<byte[]> UdpChannel { get; }
}
