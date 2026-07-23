using System;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Domain.Enums;
using BillingISPMikrotik.Infrastructure.Persistence;
using BillingISPMikrotik.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BillingISPMikrotik.Application.Tests;

public class InvoiceServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly InvoiceService _invoiceService;

    public InvoiceServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new AppDbContext(options);
        
        var loggerMock = new Mock<ILogger<InvoiceService>>();
        
        _invoiceService = new InvoiceService(_dbContext, loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_ShouldGenerateForActiveCustomers()
    {
        // Arrange
        var plan = new ServicePlan { Id = Guid.NewGuid(), Name = "Plan A", Price = 100000 };
        var activeCustomer = new Customer { Id = Guid.NewGuid(), Name = "Active", Status = CustomerStatus.Active, ServicePlanId = plan.Id, ServicePlan = plan };
        var suspendedCustomer = new Customer { Id = Guid.NewGuid(), Name = "Suspended", Status = CustomerStatus.Suspended, ServicePlanId = plan.Id, ServicePlan = plan };
        
        _dbContext.ServicePlans.Add(plan);
        _dbContext.Customers.AddRange(activeCustomer, suspendedCustomer);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _invoiceService.GenerateMonthlyInvoicesAsync(5, 2023);

        // Assert
        result.Should().Be(1);
        var invoices = await _dbContext.Invoices.ToListAsync();
        invoices.Should().HaveCount(1);
        invoices.First().CustomerId.Should().Be(activeCustomer.Id);
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_ShouldSetCorrectDueDate()
    {
        // Arrange
        var plan = new ServicePlan { Id = Guid.NewGuid(), Name = "Plan A", Price = 100000 };
        var activeCustomer = new Customer { Id = Guid.NewGuid(), Name = "Active", Status = CustomerStatus.Active, ServicePlanId = plan.Id, ServicePlan = plan };
        
        _dbContext.ServicePlans.Add(plan);
        _dbContext.Customers.Add(activeCustomer);
        await _dbContext.SaveChangesAsync();

        int month = 8;
        int year = 2024;

        // Act
        await _invoiceService.GenerateMonthlyInvoicesAsync(month, year);

        // Assert
        var invoice = await _dbContext.Invoices.FirstAsync();
        invoice.DueDate.Should().Be(new DateTime(year, month, 10));
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_ShouldSkipIfInvoiceAlreadyExists()
    {
        // Arrange
        var plan = new ServicePlan { Id = Guid.NewGuid(), Name = "Plan A", Price = 100000 };
        var activeCustomer = new Customer { Id = Guid.NewGuid(), Name = "Active", Status = CustomerStatus.Active, ServicePlanId = plan.Id, ServicePlan = plan };
        var newCustomer = new Customer { Id = Guid.NewGuid(), Name = "New", Status = CustomerStatus.Active, ServicePlanId = plan.Id, ServicePlan = plan };
        
        var existingInvoice = new Invoice 
        { 
            Id = Guid.NewGuid(), 
            CustomerId = activeCustomer.Id, 
            ServicePlanId = plan.Id, 
            PeriodMonth = 6, 
            PeriodYear = 2025, 
            Status = InvoiceStatus.Unpaid,
            DueDate = new DateTime(2025, 6, 10)
        };

        _dbContext.ServicePlans.Add(plan);
        _dbContext.Customers.AddRange(activeCustomer, newCustomer);
        _dbContext.Invoices.Add(existingInvoice);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _invoiceService.GenerateMonthlyInvoicesAsync(6, 2025);

        // Assert
        result.Should().Be(1); // Only generated for newCustomer
        var invoices = await _dbContext.Invoices.ToListAsync();
        invoices.Should().HaveCount(2); // existing + newly generated
    }
}
