using Microsoft.EntityFrameworkCore;
using Utg.LegalService.Common.Models.Domain;

namespace Utg.LegalService.Dal.SqlContext
{
    public class UtgContext : DbContext
    {
        public UtgContext(DbContextOptions<UtgContext> options)
            : base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<UserProfileAgregate> UserProfileAgregates { get; set; }

         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
        }
    }
}
