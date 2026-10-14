
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Traffic.Models;

namespace Traffic.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<TrafficFine> TrafficFines { get; set; }

        public DbSet<ApprovalHistory> ApprovalHistories { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Plaka benzersizliği
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Plate)
                .IsUnique();

            // Araç tipi zorunlu
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.VehicleType)
                .IsRequired();

            // Araç kayıtlarında eş zamanlı güncelleme kontrolü
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.RowVersion)
                .IsRowVersion();

            // Trafik cezası tutarı
            modelBuilder.Entity<TrafficFine>()
                .Property(f => f.FineAmount)
                .HasPrecision(18, 2);

            // Araç - Trafik Cezası
            modelBuilder.Entity<TrafficFine>()
                .HasOne(f => f.Vehicle)
                .WithMany()
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trafik Cezası - Onay Geçmişi
            modelBuilder.Entity<ApprovalHistory>()
                .HasOne(h => h.TrafficFine)
                .WithMany(f => f.ApprovalHistories)
                .HasForeignKey(h => h.TrafficFineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Geçmişi ceza ve işlem tarihine göre sorgulama
            modelBuilder.Entity<ApprovalHistory>()
                .HasIndex(h => new
                {
                    h.TrafficFineId,
                    h.ActionDate
                });

            // Eş zamanlı güncelleme koruması
            modelBuilder.Entity<TrafficFine>()
                .Property(f => f.RowVersion)
                .IsRowVersion();
        }
    }
}
