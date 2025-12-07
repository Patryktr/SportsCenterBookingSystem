using SportsCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SportsCenter.Infrastructure.Persistence;

    public class SportsCenterDbContext : DbContext
    {
        public SportsCenterDbContext(DbContextOptions<SportsCenterDbContext> options)
            : base(options)
        {
        }

        public DbSet<Facility> Facilities => Set<Facility>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unikalna nazwa obiektu
            modelBuilder.Entity<Facility>()
                .HasIndex(f => f.Name)
                .IsUnique();

            // RowVersion do współbieżności (jeśli masz takie pole w Booking)
            modelBuilder.Entity<Booking>()
                .Property(b => b.RowVersion)
                .IsRowVersion();

            base.OnModelCreating(modelBuilder);
        }
    }

