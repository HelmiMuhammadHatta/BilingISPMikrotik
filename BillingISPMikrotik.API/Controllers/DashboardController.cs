using System;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Domain.Enums;
using BillingISPMikrotik.Infrastructure.Persistence;
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

        var revenueChartData = await _dbContext.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt.HasValue)
            .GroupBy(i => new { i.PaidAt.Value.Year, i.PaidAt.Value.Month })
            .Select(g => new
            {
                name = $"{g.Key.Month}/{g.Key.Year}",
                revenue = g.Sum(i => i.Amount)
            })
            .OrderBy(x => x.name)
            .ToListAsync();

        return Ok(new
        {
            totalActiveCustomers,
            totalIsolirCustomers,
            revenueThisMonth,
            pendingInvoicesCount,
            revenueChartData
        });
    }
}
