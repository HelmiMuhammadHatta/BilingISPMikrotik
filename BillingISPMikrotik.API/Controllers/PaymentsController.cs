using System;
using System.Threading.Tasks;
using BillingISPMikrotik.API.DTOs;
using BillingISPMikrotik.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // TODO: Uncomment [Authorize] when authentication is implemented
    // [Authorize(Roles = "Admin")]
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var paymentLog = await _paymentService.ConfirmPaymentAsync(
                request.InvoiceId, 
                request.Method, 
                request.Amount, 
                request.ReferenceNumber);

            return Ok(new { message = "Payment confirmed successfully.", paymentLogId = paymentLog.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
}
