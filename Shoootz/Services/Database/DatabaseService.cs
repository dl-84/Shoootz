using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shoootz.Context;

namespace Shoootz.Services.Database;

internal class DatabaseService(IDbContextFactory<AppDbContext> contextFactory) : IDatabaseService
{
    public async Task InitializeAsync()
    {
        await using AppDbContext context = await contextFactory.CreateDbContextAsync().ConfigureAwait(false);
        await context.Database.MigrateAsync().ConfigureAwait(false);
    }
}
