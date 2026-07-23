using System;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Domain.Enums;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BillingISPMikrotik.Infrastructure.Services;

public class AutoIsolirService : IAutoIsolirService
{
    private readonly AppDbContext _dbContext;
    private readonly IMikrotikService _mikrotikService;
    private readonly ILogger<AutoIsolirService> _logger;

    public AutoIsolirService(
        AppDbContext dbContext,
        IMikrotikService mikrotikService,
        ILogger<AutoIsolirService> logger)
    {
        _dbContext = dbContext;
        _mikrotikService = mikrotikService;
        _logger = logger;
    }

    public async Task<int> ProcessOverdueInvoicesAsync()
    {
        _logger.LogInformation("Starting auto-isolir job for overdue invoices.");

        var today = DateTime.UtcNow.Date;

        // 1. Query semua invoice dengan status Unpaid dan due_date < hari ini
        var overdueInvoices = await _dbContext.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status == InvoiceStatus.Unpaid && i.DueDate.Date < today)
            .ToListAsync();

        if (!overdueInvoices.Any())
        {
            _logger.LogInformation("No overdue invoices found. Auto-isolir job finished.");
            return 0;
        }

        int processedCount = 0;

        foreach (var invoice in overdueInvoices)
        {
            var customer = invoice.Customer;
            if (customer == null || customer.Status == CustomerStatus.Isolir)
            {
                continue; // Skip if already isolated or customer not found
            }

            _logger.LogInformation($"Processing overdue invoice {invoice.Id} for customer {customer.Name} ({customer.PppUsername}).");

            bool mikrotikSuccess = false;
            string errorMessage = string.Empty;

            try
            {
                // 3. Panggil MikrotikService.SetPppProfileAsync() untuk ganti ke profile "isolir"
                bool setProfileSuccess = await _mikrotikService.SetPppProfileAsync(customer.PppUsername, "isolir");
                
                // 4. Panggil DisconnectActiveSessionAsync() supaya perubahan langsung apply
                bool disconnectSuccess = await _mikrotikService.DisconnectActiveSessionAsync(customer.PppUsername);

                if (!setProfileSuccess || !disconnectSuccess)
                {
                    mikrotikSuccess = false;
                    errorMessage = "Mikrotik API returned false";
                    _logger.LogWarning($"[WARNING] Failed to isolate customer {customer.PppUsername} in Mikrotik (API returned false).");
                }
                else
                {
                    mikrotikSuccess = true;
                }
            }
            catch (Exception ex)
            {
                // 7. Kalau Mikrotik API gagal dipanggil, log error, lanjut ke customer berikutnya
                errorMessage = ex.Message;
                _logger.LogCritical($"[CRITICAL] Failed to isolate customer {customer.PppUsername} in Mikrotik. Error: {ex.Message}");
            }

            // 6. Catat hasil (sukses/gagal) ke MikrotikActionLog
            var actionLog = new MikrotikActionLog
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Action = MikrotikAction.Isolir,
                Status = mikrotikSuccess ? "Success" : "Failed",
                ExecutedAt = DateTime.UtcNow,
                ErrorMessage = errorMessage
            };
            
            _dbContext.MikrotikActionLogs.Add(actionLog);

            // 5. Update invoice.status jadi Overdue
            invoice.Status = InvoiceStatus.Overdue;

            // Jika sukses di Mikrotik, update status customer jadi Isolir
            if (mikrotikSuccess)
            {
                customer.Status = CustomerStatus.Isolir;
                processedCount++;
            }
        }

        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation($"Auto-isolir job finished. Successfully isolated {processedCount} customers.");
        return processedCount;
    }
}
