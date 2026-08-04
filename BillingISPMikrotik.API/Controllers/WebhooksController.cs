using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BillingISPMikrotik.API.DTOs;
using BillingISPMikrotik.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentService paymentService,
        IConfiguration configuration,
        ILogger<WebhooksController> logger)
    {
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("midtrans-notification")]
    public async Task<IActionResult> PaymentGatewayWebhook([FromBody] MidtransWebhookPayload payload)
    {
        // 1. Validasi Signature
        var serverKey = _configuration["PaymentGateway:ServerKey"];
        if (string.IsNullOrEmpty(serverKey))
        {
            _logger.LogError("PaymentGateway:ServerKey is not configured.");
            return StatusCode(500, "Server configuration error.");
        }

        var rawString = $"{payload.OrderId}{payload.StatusCode}{payload.GrossAmount}{serverKey}";
        
        using var sha512 = SHA512.Create();
        var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(rawString));
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        if (hashString != payload.SignatureKey)
        {
            _logger.LogWarning($"Invalid signature for Order ID {payload.OrderId}. Expected: {hashString}, Got: {payload.SignatureKey}");
            return StatusCode(403, "Invalid signature");
        }

        // 2. Cek status pembayaran
        if (payload.TransactionStatus == "settlement" || payload.TransactionStatus == "capture")
        {
            var invoiceIdStr = payload.OrderId.Length >= 36 ? payload.OrderId.Substring(0, 36) : payload.OrderId;
            if (!Guid.TryParse(invoiceIdStr, out var invoiceId))
            {
                _logger.LogWarning($"Invalid OrderId format: {payload.OrderId}");
                return BadRequest("Invalid Order ID format");
            }

            if (!decimal.TryParse(payload.GrossAmount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                _logger.LogWarning($"Invalid GrossAmount format: {payload.GrossAmount}");
                return BadRequest("Invalid Amount format");
            }

            // 3. Eksekusi ConfirmPaymentAsync
            try
            {
                await _paymentService.ConfirmPaymentAsync(
                    invoiceId, 
                    payload.PaymentType, 
                    amount, 
                    payload.TransactionId);
                
                _logger.LogInformation($"Webhook processed successfully for Order ID: {payload.OrderId}");
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                // 4. Idempotency (kalau sudah Paid)
                _logger.LogInformation($"Webhook idempotency: {ex.Message}");
                return Ok(); // Tetap return 200 agar gateway berhenti retry
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Webhook error: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error processing webhook: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        else if (payload.TransactionStatus == "expire" || payload.TransactionStatus == "cancel" || payload.TransactionStatus == "deny")
        {
            var invoiceIdStr = payload.OrderId.Length >= 36 ? payload.OrderId.Substring(0, 36) : payload.OrderId;
            if (Guid.TryParse(invoiceIdStr, out var invoiceId))
            {
                await _paymentService.CancelPaymentAsync(invoiceId);
                _logger.LogInformation($"Payment cancelled/expired for Order ID: {payload.OrderId}");
            }
        }

        // Return 200 untuk status transaksi lainnya (pending, deny, cancel, expire)
        return Ok();
    }
}
