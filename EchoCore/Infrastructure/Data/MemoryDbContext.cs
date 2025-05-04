using EchoCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EchoCore.Infrastructure.Data
{
    public class MemoryDbContext : DbContext
    {
        public MemoryDbContext(DbContextOptions<MemoryDbContext> options) : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<MemoryThread> Threads => Set<MemoryThread>();
        public DbSet<MemoryEntry> Entries => Set<MemoryEntry>();
        public DbSet<SemanticMemoryEntry> SemanticMemoryEntries => Set<SemanticMemoryEntry>();
        public DbSet<PromptProfile> PromptProfiles => Set<PromptProfile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Threads)
                .WithOne(t => t.Project!)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MemoryThread>()
                .HasMany(t => t.Entries)
                .WithOne(e => e.Thread!)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<MemoryThread>()
                .HasMany(t => t.SemanticEntries)
                .WithOne(e => e.Thread!)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromptProfile>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.AgentType).IsRequired();
                entity.Property(p => p.Template).IsRequired();
            });
        }
    }
}
