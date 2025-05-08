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

            modelBuilder.Entity<Incident>().HasData(
                new Incident
                {
                    Id = 1,
                    Title = "Kapotte lantaarnpaal",
                    Description = "Werkt niet sinds gisteren",
                    Latitude = 52.3702M,
                    Longitude = 4.8952M,
                    CreatedAt = new DateTime(2024, 01, 01, 12, 0, 0, DateTimeKind.Utc),
                    Status = Shared.Enums.IncidentStatus.Reported,
                    Priority = Shared.Enums.PriorityLevel.Normal,
                    CreatedByUserId = 2,
                    HandledByUserId = 1,
                    HandledByExternal = false
                },
                new Incident
                {
                    Id = 2,
                    Title = "Gevaarlijke stoeptegel",
                    Description = "Losliggende stoeptegel bij de speeltuin",
                    Latitude = 52.3792M,
                    Longitude = 4.8922M,
                    CreatedAt = new DateTime(2024, 01, 01, 12, 0, 0, DateTimeKind.Utc),
                    Status = Shared.Enums.IncidentStatus.InProgress,
                    Priority = Shared.Enums.PriorityLevel.High,
                    CreatedByUserId = 2,
                    HandledByUserId = 1,
                    HandledByExternal = false
                }
            );
        }
    }
}
