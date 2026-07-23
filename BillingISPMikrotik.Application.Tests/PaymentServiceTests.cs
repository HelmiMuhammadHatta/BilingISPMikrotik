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

public class PaymentServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMikrotikService> _mikrotikServiceMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new AppDbContext(options);
        
        _mikrotikServiceMock = new Mock<IMikrotikService>();
        var loggerMock = new Mock<ILogger<PaymentService>>();
        
        _paymentService = new PaymentService(_dbContext, _mikrotikServiceMock.Object, loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldThrowWhenInvoiceAlreadyPaid()
    {
        // Arrange
        var invoice = new Invoice 
        { 
            Id = Guid.NewGuid(), 
            Status = InvoiceStatus.Paid,
            Customer = new Customer { Id = Guid.NewGuid(), Name = "Test" }
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        Func<Task> act = async () => await _paymentService.ConfirmPaymentAsync(invoice.Id, "Cash", 100000, "REF123");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invoice with ID {invoice.Id} is already paid.");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldRestoreIsolirCustomerAndMarkPaid()
    {
        // Arrange
        var plan = new ServicePlan { Id = Guid.NewGuid(), Name = "Plan A", MikrotikProfileName = "profile_normal" };
        var customer = new Customer 
        { 
            Id = Guid.NewGuid(), 
            Name = "Test Isolir", 
            PppUsername = "test_user", 
            Status = CustomerStatus.Isolir,
            ServicePlanId = plan.Id,
            ServicePlan = plan
        };
        var invoice = new Invoice 
        { 
            Id = Guid.NewGuid(), 
            Status = InvoiceStatus.Unpaid,
            CustomerId = customer.Id,
            Customer = customer
        };

        _dbContext.ServicePlans.Add(plan);
        _dbContext.Customers.Add(customer);
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        _mikrotikServiceMock.Setup(m => m.SetPppProfileAsync("test_user", "profile_normal")).ReturnsAsync(true);
        _mikrotikServiceMock.Setup(m => m.DisconnectActiveSessionAsync("test_user")).ReturnsAsync(true);

        // Act
        var paymentLog = await _paymentService.ConfirmPaymentAsync(invoice.Id, "Transfer", 150000, "TRF-999");

        // Assert
        paymentLog.Should().NotBeNull();
        paymentLog.InvoiceId.Should().Be(invoice.Id);
        paymentLog.Amount.Should().Be(150000);
        paymentLog.ReferenceNumber.Should().Be("TRF-999");

        var updatedInvoice = await _dbContext.Invoices.FirstAsync();
        updatedInvoice.Status.Should().Be(InvoiceStatus.Paid);

        var updatedCustomer = await _dbContext.Customers.FirstAsync();
        updatedCustomer.Status.Should().Be(CustomerStatus.Active);

        var actionLog = await _dbContext.MikrotikActionLogs.FirstAsync();
        actionLog.Action.Should().Be(MikrotikAction.Restore);
        actionLog.Status.Should().Be("Success");

        _mikrotikServiceMock.Verify(m => m.SetPppProfileAsync("test_user", "profile_normal"), Times.Once);
        _mikrotikServiceMock.Verify(m => m.DisconnectActiveSessionAsync("test_user"), Times.Once);
    }
}
