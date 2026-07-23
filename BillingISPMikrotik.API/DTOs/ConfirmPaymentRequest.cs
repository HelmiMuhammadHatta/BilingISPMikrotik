using System;

namespace BillingISPMikrotik.API.DTOs;

public class ConfirmPaymentRequest
{
    public Guid InvoiceId { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
