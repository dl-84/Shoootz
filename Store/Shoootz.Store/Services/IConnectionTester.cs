using System.Threading.Tasks;
using Result;
using Shoootz.Store.Types;

namespace Shoootz.Store.Services;

public interface IConnectionTester
{
    Task<Result<bool, DbConnectionError>> Run(ProviderType provider, string connectionString);
}
