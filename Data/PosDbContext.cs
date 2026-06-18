using Microsoft.EntityFrameworkCore;
using PlatinumPOS.Models;
using PlatinumPOS.Services;
using System;

namespace PlatinumPOS.Data
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Cashier> Cashiers { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionItem> TransactionItems { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Transaction relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Cashier)
                .WithMany()
                .HasForeignKey(t => t.CashierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionItem>()
                .HasOne(ti => ti.Transaction)
                .WithMany(t => t.Items)
                .HasForeignKey(ti => ti.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // No demo catalogue is seeded — the store adds its own categories/products in Back Office.

            // Seed Cashiers
            modelBuilder.Entity<Cashier>().HasData(
                new Cashier { Id = 1, Name = "System Administrator", PinHash = SecurityHelper.HashPin("2510"), Role = "Manager", IsActive = true },
                new Cashier { Id = 2, Name = "Devon", PinHash = SecurityHelper.HashPin("1111"), Role = "Manager", IsActive = true },
                new Cashier { Id = 3, Name = "Stanton", PinHash = SecurityHelper.HashPin("2222"), Role = "Cashier", IsActive = true },
                new Cashier { Id = 4, Name = "General Cashier", PinHash = SecurityHelper.HashPin("1234"), Role = "Cashier", IsActive = true }
            );

            // Seed Settings
            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting { Key = "StoreName", Value = "PlatinumPOS" },
                new SystemSetting { Key = "CurrencySymbol", Value = "R" },
                new SystemSetting { Key = "TaxRate", Value = "15" } // 15% VAT standard in SA
            );
        }
    }
}
