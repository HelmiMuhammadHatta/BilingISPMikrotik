using System;
using BillingISPMikrotik.Domain.Enums;

namespace BillingISPMikrotik.Domain.Entities;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid ServicePlanId { get; set; }
    public ServicePlan ServicePlan { get; set; } = null!;
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? MidtransOrderId { get; set; }
    public string? SnapToken { get; set; }
}
