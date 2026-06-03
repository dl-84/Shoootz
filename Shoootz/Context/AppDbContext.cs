using Microsoft.EntityFrameworkCore;
using Shoootz.Context.Entities;

namespace Shoootz.Context;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShooterEntity> Shooters { get; set; }

    public DbSet<ShotEntity> Shots { get; set; }

    public DbSet<ShotInfoEntity> ShotInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShooterEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Shots).WithOne(e => e.Shooter).HasForeignKey(e => e.ShooterId);
        });

        modelBuilder.Entity<ShotInfoEntity>(entity =>
        {
            entity.HasKey(e => e.MenuId);
            entity.HasMany(e => e.Shots).WithOne(e => e.ShotInfo).HasForeignKey(e => e.ShotInfoMenuId);
        });

        modelBuilder.Entity<ShotEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
