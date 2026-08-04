using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Domain.Enums;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BillingISPMikrotik.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(AppDbContext dbContext, ILogger<InvoiceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> GenerateMonthlyInvoicesAsync(int month, int year)
    {
        _logger.LogInformation($"Starting invoice generation for {month}/{year}");

        // 1. Get all customers with their service plans
        var activeCustomers = await _dbContext.Customers
            .Include(c => c.ServicePlan)
            .Where(c => c.ServicePlanId != null)
            .ToListAsync();

        if (!activeCustomers.Any())
        {
            _logger.LogInformation("No active customers found.");
            return 0;
        }

        // 2. Prevent duplicates by fetching existing invoices for the given period
        var existingInvoices = await _dbContext.Invoices
            .Where(i => i.PeriodMonth == month && i.PeriodYear == year)
            .Select(i => i.CustomerId)
            .ToListAsync();

        int generatedCount = 0;
        var newInvoices = new List<Invoice>();

        var dueDate = new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc);

        foreach (var customer in activeCustomers)
        {
            if (existingInvoices.Contains(customer.Id))
            {
                _logger.LogInformation($"Invoice for customer '{customer.Name}' already exists for period {month}/{year}. Skipping.");
                continue;
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                ServicePlanId = customer.ServicePlanId!.Value,
                PeriodMonth = month,
                PeriodYear = year,
                Amount = customer.ServicePlan!.Price,
                Status = InvoiceStatus.Unpaid,
                DueDate = dueDate
            };

            newInvoices.Add(invoice);
            generatedCount++;
        }

        if (newInvoices.Any())
        {
            _dbContext.Invoices.AddRange(newInvoices);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Successfully generated {generatedCount} invoices.");
        }
        else
        {
            _logger.LogInformation("No new invoices generated. All active customers already have invoices for this period.");
        }

        return generatedCount;
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesAsync(InvoiceStatus? status, Guid? customerId, int? month, int? year)
    {
        var query = _dbContext.Invoices
            .Include(i => i.Customer)
            .Include(i => i.ServicePlan)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(i => i.PeriodMonth == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(i => i.PeriodYear == year.Value);
        }

        return await query.ToListAsync();
    }
}
