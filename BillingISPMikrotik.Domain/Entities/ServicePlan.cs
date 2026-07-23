using System;

namespace BillingISPMikrotik.Domain.Entities;

public class ServicePlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SpeedUp { get; set; } // in Mbps or kbps, can be adjusted
    public int SpeedDown { get; set; } // in Mbps or kbps
    public decimal Price { get; set; }
    public string MikrotikProfileName { get; set; } = string.Empty;
}
