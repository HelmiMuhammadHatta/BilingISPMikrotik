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

public class BusinessFlowIntegrationTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMikrotikService> _mikrotikServiceMock;
    
    // Services
    private readonly InvoiceService _invoiceService;
    private readonly AutoIsolirService _autoIsolirService;
    private readonly PaymentService _paymentService;

    public BusinessFlowIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new AppDbContext(options);
        
        _mikrotikServiceMock = new Mock<IMikrotikService>();
        
        var invoiceLogger = new Mock<ILogger<InvoiceService>>();
        var autoIsolirLogger = new Mock<ILogger<AutoIsolirService>>();
        var paymentLogger = new Mock<ILogger<PaymentService>>();
        
        _invoiceService = new InvoiceService(_dbContext, invoiceLogger.Object);
        _autoIsolirService = new AutoIsolirService(_dbContext, _mikrotikServiceMock.Object, autoIsolirLogger.Object);
        _paymentService = new PaymentService(_dbContext, _mikrotikServiceMock.Object, paymentLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CompleteBusinessFlow_ShouldWorkEndToEnd()
    {
        // ---------------------------------------------------------
        // SETUP: Base Data
        // ---------------------------------------------------------
        var plan = new ServicePlan { Id = Guid.NewGuid(), Name = "Plan A", Price = 100000, MikrotikProfileName = "profile_10m" };
        _dbContext.ServicePlans.Add(plan);

        // 2 Active customers, 1 already Isolir
        var activeCust1 = new Customer { Id = Guid.NewGuid(), Name = "Cust 1", PppUsername = "cust1", Status = CustomerStatus.Active, ServicePlanId = plan.Id, ServicePlan = plan };
        var activeCust2 = new Customer { Id = Guid.NewGuid(), Name = "Cust 2", PppUsername = "cust2", Status = CustomerStatus.Active, ServicePlanId = plan.Id, ServicePlan = plan };
        var isolirCust3 = new Customer { Id = Guid.NewGuid(), Name = "Cust 3", PppUsername = "cust3", Status = CustomerStatus.Isolir, ServicePlanId = plan.Id, ServicePlan = plan };
        
        _dbContext.Customers.AddRange(activeCust1, activeCust2, isolirCust3);
        await _dbContext.SaveChangesAsync();

        int targetMonth = 8;
        int targetYear = 2026;

        // ---------------------------------------------------------
        // STAGE 1: INVOICE GENERATION
        // ---------------------------------------------------------
        
        // 1.1 Generate invoice
        var generatedCount = await _invoiceService.GenerateMonthlyInvoicesAsync(targetMonth, targetYear);
        generatedCount.Should().Be(2, "Hanya 2 pelanggan aktif yang harus dibuatkan invoice");

        var invoices = await _dbContext.Invoices.ToListAsync();
        invoices.Should().HaveCount(2);
        
        var expectedDueDate = new DateTime(targetYear, targetMonth, 10).ToUniversalTime();
        invoices.All(i => i.DueDate == expectedDueDate || i.DueDate == new DateTime(targetYear, targetMonth, 10)).Should().BeTrue("Due date harus tanggal 10");

        // 1.2 Generate invoice 2x, ensure no duplicates
        var duplicateCount = await _invoiceService.GenerateMonthlyInvoicesAsync(targetMonth, targetYear);
        duplicateCount.Should().Be(0, "Invoice sudah ter-generate sebelumnya, tidak boleh ada duplikat");
        (await _dbContext.Invoices.CountAsync()).Should().Be(2);

        // ---------------------------------------------------------
        // STAGE 2: AUTO ISOLIR
        // ---------------------------------------------------------
        
        // Force the invoices to be overdue to simulate time passing
        var invoice1 = invoices.First(i => i.CustomerId == activeCust1.Id);
        var invoice2 = invoices.First(i => i.CustomerId == activeCust2.Id);
        
        invoice1.DueDate = DateTime.UtcNow.AddDays(-2); // Overdue
        invoice2.DueDate = DateTime.UtcNow.AddDays(-2); // Overdue
        await _dbContext.SaveChangesAsync();

        // 2.1 Set up Mikrotik Mock: Success for Cust1, Random Failure for Cust2
        _mikrotikServiceMock.Setup(m => m.SetPppProfileAsync("cust1", "isolir")).ReturnsAsync(true);
        _mikrotikServiceMock.Setup(m => m.DisconnectActiveSessionAsync("cust1")).ReturnsAsync(true);

        _mikrotikServiceMock.Setup(m => m.SetPppProfileAsync("cust2", "isolir")).ThrowsAsync(new Exception("Mikrotik Timeout Fake Error"));

        // Run auto isolir
        var isolirCount = await _autoIsolirService.ProcessOverdueInvoicesAsync();
        isolirCount.Should().Be(1, "Hanya Cust1 yang sukses isolir karena Cust2 disimulasikan gagal");

        // Verify Cust1
        var dbCust1 = await _dbContext.Customers.FindAsync(activeCust1.Id);
        dbCust1.Status.Should().Be(CustomerStatus.Isolir);
        
        var dbInvoice1 = await _dbContext.Invoices.FindAsync(invoice1.Id);
        dbInvoice1.Status.Should().Be(InvoiceStatus.Overdue);

        var logCust1 = await _dbContext.MikrotikActionLogs.FirstOrDefaultAsync(l => l.CustomerId == activeCust1.Id);
        logCust1.Should().NotBeNull();
        logCust1.Action.Should().Be(MikrotikAction.Isolir);
        logCust1.Status.Should().Be("Success");

        // Verify Cust2 (Failed Mikrotik)
        var dbCust2 = await _dbContext.Customers.FindAsync(activeCust2.Id);
        dbCust2.Status.Should().Be(CustomerStatus.Active, "Status tidak boleh berubah jika interaksi Mikrotik gagal");
        
        var dbInvoice2 = await _dbContext.Invoices.FindAsync(invoice2.Id);
        dbInvoice2.Status.Should().Be(InvoiceStatus.Overdue, "Invoice tetap menjadi overdue walau Mikrotik gagal");

        var logCust2 = await _dbContext.MikrotikActionLogs.FirstOrDefaultAsync(l => l.CustomerId == activeCust2.Id);
        logCust2.Should().NotBeNull();
        logCust2.Status.Should().Be("Failed");
        logCust2.ErrorMessage.Should().Contain("Fake Error", "Error harus tercatat di database");

        // ---------------------------------------------------------
        // STAGE 3: PAYMENT CONFIRMATION & AUTO RESTORE
        // ---------------------------------------------------------
        
        _mikrotikServiceMock.Setup(m => m.SetPppProfileAsync("cust1", "profile_10m")).ReturnsAsync(true);
        _mikrotikServiceMock.Setup(m => m.DisconnectActiveSessionAsync("cust1")).ReturnsAsync(true);

        // 3.1 Pay Invoice 1 (Cust1 who is currently Isolir)
        await _paymentService.ConfirmPaymentAsync(invoice1.Id, "VA_BCA", 100000, "TRX12345");

        // Verify Payment
        var paidInvoice = await _dbContext.Invoices.FindAsync(invoice1.Id);
        paidInvoice.Status.Should().Be(InvoiceStatus.Paid);
        paidInvoice.PaidAt.Should().NotBeNull();

        var paymentLog = await _dbContext.PaymentLogs.FirstOrDefaultAsync(p => p.InvoiceId == invoice1.Id);
        paymentLog.Should().NotBeNull();
        paymentLog.ReferenceNumber.Should().Be("TRX12345");

        var restoredCust1 = await _dbContext.Customers.FindAsync(activeCust1.Id);
        restoredCust1.Status.Should().Be(CustomerStatus.Active, "Setelah bayar, pelanggan harus kembali Active");

        var restoreLog = await _dbContext.MikrotikActionLogs.OrderByDescending(l => l.ExecutedAt).FirstOrDefaultAsync(l => l.CustomerId == activeCust1.Id);
        restoreLog.Action.Should().Be(MikrotikAction.Restore);
        restoreLog.Status.Should().Be("Success");

        // 3.2 Try paying the already paid invoice
        Func<Task> act = async () => await _paymentService.ConfirmPaymentAsync(invoice1.Id, "QRIS", 100000, "TRX999");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invoice with ID*");

        // ---------------------------------------------------------
        // STAGE 4: DASHBOARD VERIFICATION
        // ---------------------------------------------------------
        
        // Simulating the exact dashboard logic
        var totalActive = await _dbContext.Customers.CountAsync(c => c.Status == CustomerStatus.Active);
        totalActive.Should().Be(2, "Cust1 sudah bayar dan aktif lagi, Cust2 tidak jadi terisolir (masih aktif), Cust3 dari awal Isolir");

        var totalIsolir = await _dbContext.Customers.CountAsync(c => c.Status == CustomerStatus.Isolir);
        totalIsolir.Should().Be(1, "Hanya Cust3 yang berstatus isolir");

        var totalRevenue = await _dbContext.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue && i.PaidAt.Value.Month == DateTime.UtcNow.Month)
            .SumAsync(i => i.Amount);
        totalRevenue.Should().Be(100000, "Hanya ada 1 pembayaran sebesar 100k");

        var pendingInvoicesCount = await _dbContext.Invoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.Overdue);
        pendingInvoicesCount.Should().Be(1, "Invoice2 berstatus overdue dan belum dibayar");

        var dashboardData = await _dbContext.Customers
            .Where(c => c.Status == CustomerStatus.Isolir)
            .Select(c => new
            {
                c.Name,
                c.Phone,
                PlanName = c.ServicePlan.Name
            })
            .ToListAsync();
        
        dashboardData.Should().HaveCount(1);
        dashboardData[0].Name.Should().Be("Cust 3"); // Cust3 was created as isolir. Cust1 was restored.
    }
}
