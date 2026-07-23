using System;
using BillingISPMikrotik.Domain.Enums;

namespace BillingISPMikrotik.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PppUsername { get; set; } = string.Empty;
    public string PppPassword { get; set; } = string.Empty;
    public CustomerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Guid? ServicePlanId { get; set; }
    public ServicePlan? ServicePlan { get; set; }
}
