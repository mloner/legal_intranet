using Microsoft.EntityFrameworkCore;
using Utg.LegalService.Common.Models.Domain;

namespace Utg.LegalService.Dal.SqlContext
{
    public class UtgContext : DbContext
    {
        public UtgContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
        }
    }
}
