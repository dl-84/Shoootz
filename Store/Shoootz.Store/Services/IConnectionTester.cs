using Result;
using Shoootz.Store.Types;

namespace Shoootz.Store.Services;

public interface IConnectionTester
{
    Result<bool, DbConnectionError> Run(ProviderType provider, string connectionString);
}
