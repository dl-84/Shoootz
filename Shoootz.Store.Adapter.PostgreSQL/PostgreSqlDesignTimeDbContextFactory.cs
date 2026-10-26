using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shoootz.Store;

namespace Shoootz.Store.Adapter.PostgreSQL;

public class PostgreSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=migrations",
            o => o.MigrationsAssembly("Shoootz.Store.Adapter.PostgreSQL")
        );
        return new AppDbContext(optionsBuilder.Options);
    }
}
