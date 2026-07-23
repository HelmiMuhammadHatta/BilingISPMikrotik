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
        var plan1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var plan2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var plan3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var plan1 = new ServicePlan
        {
            Id = plan1Id,
            Name = "Basic 10Mbps",
            SpeedUp = 10,
            SpeedDown = 10,
            Price = 150000,
            MikrotikProfileName = "profile_10m"
        };
        var plan2 = new ServicePlan
        {
            Id = plan2Id,
            Name = "Standard 20Mbps",
            SpeedUp = 20,
            SpeedDown = 20,
            Price = 250000,
            MikrotikProfileName = "profile_20m"
        };
        var plan3 = new ServicePlan
        {
            Id = plan3Id,
            Name = "Premium 50Mbps",
            SpeedUp = 50,
            SpeedDown = 50,
            Price = 400000,
            MikrotikProfileName = "profile_50m"
        };

        modelBuilder.Entity<ServicePlan>().HasData(plan1, plan2, plan3);

        var baseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Data for Customers
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Budi Santoso",
                Address = "Jl. Merdeka No 1",
                Phone = "081234567890",
                PppUsername = "budi",
                PppPassword = "passwordbudi",
                Status = CustomerStatus.Active,
                CreatedAt = baseDate.AddMonths(-1),
                ServicePlanId = plan1Id
            },
            new Customer
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Siti Aminah",
                Address = "Jl. Sudirman No 2",
                Phone = "082345678901",
                PppUsername = "siti",
                PppPassword = "passwordsiti",
                Status = CustomerStatus.Active,
                CreatedAt = baseDate.AddMonths(-2),
                ServicePlanId = plan2Id
            },
            new Customer
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Agus Pratama",
                Address = "Jl. Thamrin No 3",
                Phone = "083456789012",
                PppUsername = "agus",
                PppPassword = "passwordagus",
                Status = CustomerStatus.Isolir,
                CreatedAt = baseDate.AddMonths(-3),
                ServicePlanId = plan1Id
            },
            new Customer
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Dewi Lestari",
                Address = "Jl. Gatot Subroto No 4",
                Phone = "084567890123",
                PppUsername = "dewi",
                PppPassword = "passworddewi",
                Status = CustomerStatus.Active,
                CreatedAt = baseDate.AddMonths(-4),
                ServicePlanId = plan3Id
            },
            new Customer
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Joko Widodo",
                Address = "Jl. Ahmad Yani No 5",
                Phone = "085678901234",
                PppUsername = "joko",
                PppPassword = "passwordjoko",
                Status = CustomerStatus.Suspended,
                CreatedAt = baseDate.AddMonths(-5),
                ServicePlanId = plan2Id
            }
        );
    }
}
