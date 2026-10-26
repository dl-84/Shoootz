using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shoootz.Store;

namespace Shoootz.Store.Adapter.SQLite;

public class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite(
            "Data Source=migrations.db",
            o => o.MigrationsAssembly("Shoootz.Store.Adapter.SQLite")
        );
        return new AppDbContext(optionsBuilder.Options);
    }
}
