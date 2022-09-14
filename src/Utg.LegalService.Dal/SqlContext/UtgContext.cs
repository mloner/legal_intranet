using Microsoft.EntityFrameworkCore;

namespace Utg.LegalService.Dal.SqlContext
{
    public class UtgContext : DbContext
    {
        public UtgContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
        }
    }
}
