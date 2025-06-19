using Microsoft.EntityFrameworkCore;
using PingIt.Api.Models;

namespace PingIt.Api.Data
{
    public class PingItDbContext : DbContext
    {
        public PingItDbContext(DbContextOptions<PingItDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentPhoto> IncidentPhotos { get; set; }
        public DbSet<IncidentStatusHistory> IncidentStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Incident>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Incident>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(i => i.HandledByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<IncidentStatusHistory>()
                .HasOne<Incident>()
                .WithMany()
                .HasForeignKey(h => h.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IncidentStatusHistory>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
