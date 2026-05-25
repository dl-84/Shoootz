using System.Threading.Tasks;

namespace Shoootz.Services.Database;

internal interface IDbService
{
    Task InitializeAsync();
}
