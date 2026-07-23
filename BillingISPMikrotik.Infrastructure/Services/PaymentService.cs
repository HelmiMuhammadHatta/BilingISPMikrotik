using System;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Domain.Enums;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BillingISPMikrotik.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _dbContext;
    private readonly IMikrotikService _mikrotikService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext dbContext,
        IMikrotikService mikrotikService,
        ILogger<PaymentService> logger)
    {
        _dbContext = dbContext;
        _mikrotikService = mikrotikService;
        _logger = logger;
    }

    public async Task<PaymentLog> ConfirmPaymentAsync(Guid invoiceId, string method, decimal amount, string referenceNumber)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.Customer)
            .ThenInclude(c => c.ServicePlan)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            throw new ArgumentException($"Invoice with ID {invoiceId} not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException($"Invoice with ID {invoiceId} is already paid.");
        }

        // Insert PaymentLog
        var paymentLog = new PaymentLog
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Method = method,
            Amount = amount,
            PaidAt = DateTime.UtcNow,
            ReferenceNumber = referenceNumber
        };
        _dbContext.PaymentLogs.Add(paymentLog);

        // Update Invoice
        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;

        var customer = invoice.Customer;

        // Auto-Restore Logic
        if (customer.Status == CustomerStatus.Isolir)
        {
            _logger.LogInformation($"Customer {customer.Name} is currently isolated. Attempting auto-restore.");

            if (customer.ServicePlan == null)
            {
                _logger.LogWarning($"Customer {customer.Name} does not have a ServicePlan. Cannot restore Mikrotik profile.");
            }
            else
            {
                bool mikrotikSuccess = false;
                string errorMessage = string.Empty;

                try
                {
                    bool setProfileSuccess = await _mikrotikService.SetPppProfileAsync(customer.PppUsername, customer.ServicePlan.MikrotikProfileName);
                    bool disconnectSuccess = await _mikrotikService.DisconnectActiveSessionAsync(customer.PppUsername);

                    if (!setProfileSuccess || !disconnectSuccess)
                    {
                        errorMessage = "Mikrotik API returned false during restore.";
                        _logger.LogWarning(errorMessage);
                    }
                    else
                    {
                        mikrotikSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    _logger.LogError($"Error calling Mikrotik API for restore: {ex.Message}");
                }

                // Log Mikrotik Action
                var actionLog = new MikrotikActionLog
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Action = MikrotikAction.Restore,
                    Status = mikrotikSuccess ? "Success" : "Failed",
                    ExecutedAt = DateTime.UtcNow,
                    ErrorMessage = errorMessage
                };
                _dbContext.MikrotikActionLogs.Add(actionLog);

                // If Mikrotik success, update customer status
                // We'll also update customer status if we want to assume it's paid anyway, 
                // but usually we update status if mikrotik successfully applied it.
                // For this, we'll update to Active if mikrotikSuccess is true.
                if (mikrotikSuccess)
                {
                    customer.Status = CustomerStatus.Active;
                    _logger.LogInformation($"Successfully restored customer {customer.Name} to normal profile.");
                }
            }
        }

        await _dbContext.SaveChangesAsync();

        return paymentLog;
    }
}
