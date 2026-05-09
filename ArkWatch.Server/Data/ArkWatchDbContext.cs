using ArkWatch.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ArkWatch.Server.Data
{
    public class ArkWatchDbContext : DbContext
    {
        public ArkWatchDbContext(DbContextOptions<ArkWatchDbContext> options) : base(options)
        {
        }
        public DbSet<Alert> StoredAlerts => Set<Alert>();

    }
}
