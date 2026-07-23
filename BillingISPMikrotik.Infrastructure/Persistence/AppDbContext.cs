using System;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BillingISPMikrotik.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ServicePlan> ServicePlans => Set<ServicePlan>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PaymentLog> PaymentLogs => Set<PaymentLog>();
    public DbSet<MikrotikActionLog> MikrotikActionLogs => Set<MikrotikActionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Data for Service Plans
        var plan1 = new ServicePlan
        {
            Id = Guid.NewGuid(),
            Name = "Basic 10Mbps",
            SpeedUp = 10,
            SpeedDown = 10,
            Price = 150000,
            MikrotikProfileName = "profile_10m"
        };
        var plan2 = new ServicePlan
        {
            Id = Guid.NewGuid(),
            Name = "Standard 20Mbps",
            SpeedUp = 20,
            SpeedDown = 20,
            Price = 250000,
            MikrotikProfileName = "profile_20m"
        };
        var plan3 = new ServicePlan
        {
            Id = Guid.NewGuid(),
            Name = "Premium 50Mbps",
            SpeedUp = 50,
            SpeedDown = 50,
            Price = 400000,
            MikrotikProfileName = "profile_50m"
        };

        modelBuilder.Entity<ServicePlan>().HasData(plan1, plan2, plan3);

        // Seed Data for Customers
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Budi Santoso",
                Address = "Jl. Merdeka No 1",
                Phone = "081234567890",
                PppUsername = "budi",
                PppPassword = "passwordbudi",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                ServicePlanId = plan1.Id
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Siti Aminah",
                Address = "Jl. Sudirman No 2",
                Phone = "082345678901",
                PppUsername = "siti",
                PppPassword = "passwordsiti",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                ServicePlanId = plan2.Id
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Agus Pratama",
                Address = "Jl. Thamrin No 3",
                Phone = "083456789012",
                PppUsername = "agus",
                PppPassword = "passwordagus",
                Status = CustomerStatus.Isolir,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                ServicePlanId = plan1.Id
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Dewi Lestari",
                Address = "Jl. Gatot Subroto No 4",
                Phone = "084567890123",
                PppUsername = "dewi",
                PppPassword = "passworddewi",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                ServicePlanId = plan3.Id
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Joko Widodo",
                Address = "Jl. Ahmad Yani No 5",
                Phone = "085678901234",
                PppUsername = "joko",
                PppPassword = "passwordjoko",
                Status = CustomerStatus.Suspended,
                CreatedAt = DateTime.UtcNow.AddMonths(-5),
                ServicePlanId = plan2.Id
            }
        );
    }
}
