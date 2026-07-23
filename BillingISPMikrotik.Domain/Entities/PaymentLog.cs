using System;

namespace BillingISPMikrotik.Domain.Entities;

public class PaymentLog
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
