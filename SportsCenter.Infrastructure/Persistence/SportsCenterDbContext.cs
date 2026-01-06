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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfiguracja Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);

            // Precyzja dla TotalPrice: 18 cyfr, 2 miejsca po przecinku
            entity.Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(b => b.RowVersion)
                .IsRowVersion();

            // Relacje
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

            // Precyzja dla PricePerHour: 18 cyfr, 2 miejsca po przecinku
            entity.Property(f => f.PricePerHour)
                .HasColumnType("decimal(18,2)");

            // Unikalny indeks na Name
            entity.HasIndex(f => f.Name)
                .IsUnique();
        });

        // Konfiguracja Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.PublicId)
                .HasDefaultValueSql("NEWID()");

            // Unikalny email
            entity.HasIndex(c => c.Email)
                .IsUnique();

            entity.HasIndex(c => c.PublicId)
                .IsUnique();
        });
    }
}