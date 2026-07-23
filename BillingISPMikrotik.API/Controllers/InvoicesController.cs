using System;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    // TODO: Uncomment [Authorize] when authentication is implemented
    // [Authorize(Roles = "Admin")]
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateInvoices([FromQuery] int? month, [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var count = await _invoiceService.GenerateMonthlyInvoicesAsync(targetMonth, targetYear);
        return Ok(new { message = $"Successfully generated {count} invoices for {targetMonth}/{targetYear}." });
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceStatus? status, [FromQuery] Guid? customerId, [FromQuery] int? month, [FromQuery] int? year)
    {
        var invoices = await _invoiceService.GetInvoicesAsync(status, customerId, month, year);
        return Ok(invoices);
    }
}
