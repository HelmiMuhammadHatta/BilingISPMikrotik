using System;
using BillingISPMikrotik.Domain.Enums;

namespace BillingISPMikrotik.Domain.Entities;

public class MikrotikActionLog
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public MikrotikAction Action { get; set; }
    public string Status { get; set; } = string.Empty; // e.g. Success, Failed
    public DateTime ExecutedAt { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
