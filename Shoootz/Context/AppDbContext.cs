using Microsoft.EntityFrameworkCore;

namespace Shoootz.Context;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) { }
