using Microsoft.EntityFrameworkCore;

namespace WikimediaMonitor.StorageKeeper
{
    public class AppDbContext : DbContext
    {
        public DbSet<Wikimedia> Wikimedias { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
