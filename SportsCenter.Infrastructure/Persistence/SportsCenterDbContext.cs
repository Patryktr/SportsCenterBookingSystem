using Microsoft.EntityFrameworkCore;
using SportsCenter.Domain.Entities;

namespace SportsCenter.Infrastructure.Persistence;

public class SportsCenterDbContext : DbContext
{
    public SportsCenterDbContext(DbContextOptions<SportsCenterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<OperatingHours> OperatingHours => Set<OperatingHours>();
    public DbSet<TimeBlock> TimeBlocks => Set<TimeBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfiguracja Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(b => b.RowVersion)
                .IsRowVersion();

            entity.HasOne(b => b.Facility)
                .WithMany()
                .HasForeignKey(b => b.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Konfiguracja Facility
        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.PricePerHour)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(f => f.Name)
                .IsUnique();
        });

        // Konfiguracja Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.PublicId)
                .HasDefaultValueSql("NEWID()");

            entity.HasIndex(c => c.Email)
                .IsUnique();

            entity.HasIndex(c => c.PublicId)
                .IsUnique();
        });

        // Konfiguracja OperatingHours
        modelBuilder.Entity<OperatingHours>(entity =>
        {
            entity.HasKey(o => o.Id);

            // Unikalny indeks: jeden wpis na dzień tygodnia dla danego obiektu
            entity.HasIndex(o => new { o.FacilityId, o.DayOfWeek })
                .IsUnique();

            entity.HasOne(o => o.Facility)
                .WithMany()
                .HasForeignKey(o => o.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfiguracja TimeBlock
        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Reason)
                .HasMaxLength(500);

            entity.HasOne(t => t.Facility)
                .WithMany()
                .HasForeignKey(t => t.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indeks dla szybkiego wyszukiwania blokad po czasie
            entity.HasIndex(t => new { t.FacilityId, t.StartTime, t.EndTime });
        });
    }
}