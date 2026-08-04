using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CustomerPortalController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public CustomerPortalController(AppDbContext dbContext, IPaymentService paymentService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _configuration = configuration;
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

    [HttpPost("invoices/{invoiceId}/create-payment")]
    public async Task<IActionResult> CreatePayment(Guid invoiceId)
    {
        var customerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return Unauthorized();
        }

        var invoice = await _dbContext.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CustomerId == customerId);
            
        if (invoice == null)
        {
            return NotFound("Invoice not found or does not belong to you.");
        }

        if (invoice.Status == BillingISPMikrotik.Domain.Enums.InvoiceStatus.Paid)
        {
            return BadRequest("Invoice is already paid.");
        }
        
        if (!string.IsNullOrEmpty(invoice.SnapToken))
        {
            return Ok(new { token = invoice.SnapToken });
        }

        var serverKey = _configuration["PaymentGateway:ServerKey"];
        if (string.IsNullOrEmpty(serverKey))
        {
            return StatusCode(500, "Server configuration error.");
        }

        var orderId = $"{invoiceId.ToString("N")}-{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";

        var payload = new
        {
            transaction_details = new
            {
                order_id = orderId,
                gross_amount = (int)invoice.Amount
            },
            customer_details = new
            {
                first_name = invoice.Customer.Name,
                phone = invoice.Customer.Phone,
                billing_address = new
                {
                    address = invoice.Customer.Address
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{serverKey.Trim()}:"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var baseUrl = "https://app.sandbox.midtrans.com";
        var response = await client.PostAsync($"{baseUrl}/snap/v1/transactions", content);
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            return StatusCode(500, $"Failed to create transaction with Midtrans: {errorResponse}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;
        
        if (root.TryGetProperty("token", out var tokenElement))
        {
            var snapToken = tokenElement.GetString();
            invoice.SnapToken = snapToken;
            invoice.MidtransOrderId = orderId;
            await _dbContext.SaveChangesAsync();

            return Ok(new { token = snapToken });
        }

        return StatusCode(500, "Invalid response from Midtrans.");
    }
}
