using System;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Domain.Enums;
using BillingISPMikrotik.Infrastructure.Persistence;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public DashboardController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalActiveCustomers = await _dbContext.Customers.CountAsync(c => c.Status == CustomerStatus.Active);
        var totalIsolirCustomers = await _dbContext.Customers.CountAsync(c => c.Status == CustomerStatus.Isolir);

        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;

        var revenueThisMonth = await _dbContext.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue && i.PaidAt.Value.Month == currentMonth && i.PaidAt.Value.Year == currentYear)
            .SumAsync(i => i.Amount);

        var pendingInvoicesCount = await _dbContext.Invoices
            .CountAsync(i => i.Status == InvoiceStatus.Unpaid);

        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
        var revenueChartDataRaw = await _dbContext.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue && i.PaidAt.Value >= twelveMonthsAgo)
            .GroupBy(i => new { i.PaidAt.Value.Year, i.PaidAt.Value.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                revenue = g.Sum(i => i.Amount)
            })
            .ToListAsync();

        var revenueChartData = revenueChartDataRaw
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .Select(x => new
            {
                name = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                revenue = x.revenue
            })
            .ToList();

        return Ok(new
        {
            totalActiveCustomers,
            totalIsolirCustomers,
            revenueThisMonth,
            pendingInvoicesCount,
            revenueChartData
        });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = new List<object>();

        // Get 5 most recent isolated customers
        var recentIsolir = await _dbContext.MikrotikActionLogs
            .Where(l => l.Action == MikrotikAction.Isolir)
            .OrderByDescending(l => l.ExecutedAt)
            .Take(5)
            .Select(l => new {
                type = "Isolir",
                title = "Customer Isolated",
                message = $"Customer {l.CustomerId} was isolated.",
                timestamp = l.ExecutedAt
            })
            .ToListAsync();
        
        // Get 5 most recent overdue invoices
        var recentOverdue = await _dbContext.Invoices
            .Where(i => i.Status == InvoiceStatus.Overdue)
            .OrderByDescending(i => i.DueDate)
            .Take(5)
            .Select(i => new {
                type = "Overdue",
                title = "Invoice Overdue",
                message = $"Invoice for period {i.PeriodMonth}/{i.PeriodYear} is overdue.",
                timestamp = i.DueDate
            })
            .ToListAsync();

        notifications.AddRange(recentIsolir);
        notifications.AddRange(recentOverdue);

        var sortedNotifications = notifications
            .OrderByDescending(n => ((dynamic)n).timestamp)
            .Take(5)
            .ToList();

        return Ok(sortedNotifications);
    }
}
