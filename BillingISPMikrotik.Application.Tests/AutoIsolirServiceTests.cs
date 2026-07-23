using System;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
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

public class AutoIsolirServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMikrotikService> _mikrotikServiceMock;
    private readonly AutoIsolirService _autoIsolirService;

    public AutoIsolirServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new AppDbContext(options);
        
        _mikrotikServiceMock = new Mock<IMikrotikService>();
        var loggerMock = new Mock<ILogger<AutoIsolirService>>();
        
        _autoIsolirService = new AutoIsolirService(_dbContext, _mikrotikServiceMock.Object, loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ProcessOverdueInvoicesAsync_ShouldIsolateOverdueCustomers()
    {
        // Arrange
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Test", PppUsername = "testuser", Status = CustomerStatus.Active };
        var invoice = new Invoice 
        { 
            Id = Guid.NewGuid(), 
            CustomerId = customer.Id, 
            Customer = customer,
            Status = InvoiceStatus.Unpaid,
            DueDate = DateTime.UtcNow.Date.AddDays(-1) // Overdue
        };

        _dbContext.Customers.Add(customer);
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        _mikrotikServiceMock.Setup(m => m.SetPppProfileAsync("testuser", "isolir")).ReturnsAsync(true);
        _mikrotikServiceMock.Setup(m => m.DisconnectActiveSessionAsync("testuser")).ReturnsAsync(true);

        // Act
        var result = await _autoIsolirService.ProcessOverdueInvoicesAsync();

        // Assert
        result.Should().Be(1);

        var updatedInvoice = await _dbContext.Invoices.FirstAsync();
        updatedInvoice.Status.Should().Be(InvoiceStatus.Overdue);

        var updatedCustomer = await _dbContext.Customers.FirstAsync();
        updatedCustomer.Status.Should().Be(CustomerStatus.Isolir);

        var actionLog = await _dbContext.MikrotikActionLogs.FirstAsync();
        actionLog.CustomerId.Should().Be(customer.Id);
        actionLog.Action.Should().Be(MikrotikAction.Isolir);
        actionLog.Status.Should().Be("Success");
        
        _mikrotikServiceMock.Verify(m => m.SetPppProfileAsync("testuser", "isolir"), Times.Once);
        _mikrotikServiceMock.Verify(m => m.DisconnectActiveSessionAsync("testuser"), Times.Once);
    }

    [Fact]
    public async Task ProcessOverdueInvoicesAsync_ShouldHandleMikrotikFailureGracefully()
    {
        // Arrange
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Test", PppUsername = "testuser", Status = CustomerStatus.Active };
        var invoice = new Invoice 
        { 
            Id = Guid.NewGuid(), 
            CustomerId = customer.Id, 
            Customer = customer,
            Status = InvoiceStatus.Unpaid,
            DueDate = DateTime.UtcNow.Date.AddDays(-1) // Overdue
        };

        _dbContext.Customers.Add(customer);
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        _mikrotikServiceMock
            .Setup(m => m.SetPppProfileAsync("testuser", "isolir"))
            .ThrowsAsync(new Exception("Mikrotik connection failed"));

        // Act
        var result = await _autoIsolirService.ProcessOverdueInvoicesAsync();

        // Assert
        result.Should().Be(0); // 0 completely successful isolations

        var updatedInvoice = await _dbContext.Invoices.FirstAsync();
        updatedInvoice.Status.Should().Be(InvoiceStatus.Overdue); // Invoice status should still update

        var updatedCustomer = await _dbContext.Customers.FirstAsync();
        updatedCustomer.Status.Should().Be(CustomerStatus.Active); // Customer status stays active because isolation failed

        var actionLog = await _dbContext.MikrotikActionLogs.FirstAsync();
        actionLog.Status.Should().Be("Failed");
        actionLog.ErrorMessage.Should().Contain("Mikrotik connection failed");
    }

    [Fact]
    public async Task ProcessOverdueInvoicesAsync_ShouldSkipNotOverdueInvoices()
    {
        // Arrange
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Test", PppUsername = "testuser", Status = CustomerStatus.Active };
        var invoice = new Invoice 
        { 
            Id = Guid.NewGuid(), 
            CustomerId = customer.Id, 
            Customer = customer,
            Status = InvoiceStatus.Unpaid,
            DueDate = DateTime.UtcNow.Date.AddDays(1) // Not Overdue
        };

        _dbContext.Customers.Add(customer);
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _autoIsolirService.ProcessOverdueInvoicesAsync();

        // Assert
        result.Should().Be(0);
        
        var existingInvoice = await _dbContext.Invoices.FirstAsync();
        existingInvoice.Status.Should().Be(InvoiceStatus.Unpaid);

        _mikrotikServiceMock.Verify(m => m.SetPppProfileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
