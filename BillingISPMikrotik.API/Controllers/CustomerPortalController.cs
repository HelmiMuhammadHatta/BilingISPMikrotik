using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Infrastructure.Persistence;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CustomerPortalController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPaymentService _paymentService;

    public CustomerPortalController(AppDbContext dbContext, IPaymentService paymentService)
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
    }

    [HttpGet("my-data")]
    public async Task<IActionResult> GetMyData()
    {
        var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var customer = await _dbContext.Customers
            .Include(c => c.ServicePlan)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
        {
            return NotFound("Customer not found.");
        }

        var invoices = await _dbContext.Invoices
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.DueDate)
            .Select(i => new {
                i.Id,
                i.Amount,
                i.DueDate,
                i.Status,
                i.PeriodMonth,
                i.PeriodYear,
                i.PaidAt
            })
            .ToListAsync();

        return Ok(new
        {
            customer = new
            {
                customer.Name,
                customer.Phone,
                customer.Address,
                customer.Status,
                ServicePlan = customer.ServicePlan?.Name,
                Speed = $"{customer.ServicePlan?.SpeedDown}Mbps / {customer.ServicePlan?.SpeedUp}Mbps",
                Price = customer.ServicePlan?.Price
            },
            invoices
        });
    }

    [HttpPost("pay/{invoiceId}")]
    public async Task<IActionResult> SimulatePayment(Guid invoiceId)
    {
        var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId && i.CustomerId == customerId);
        if (invoice == null)
        {
            return NotFound("Invoice not found or does not belong to you.");
        }

        if (invoice.Status == BillingISPMikrotik.Domain.Enums.InvoiceStatus.Paid)
        {
            return BadRequest("Invoice is already paid.");
        }

        // Simulate payment
        var amount = invoice.Amount;
        await _paymentService.ConfirmPaymentAsync(invoiceId, "Simulated Customer Payment", amount, $"SIM-REF-{DateTime.UtcNow.Ticks}");

        return Ok(new { message = "Payment simulated successfully." });
    }
}
