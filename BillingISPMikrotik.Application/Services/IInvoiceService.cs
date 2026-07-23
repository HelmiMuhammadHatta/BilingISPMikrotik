using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Domain.Enums;

namespace BillingISPMikrotik.Application.Services;

public interface IInvoiceService
{
    Task<int> GenerateMonthlyInvoicesAsync(int month, int year);
    Task<IEnumerable<Invoice>> GetInvoicesAsync(InvoiceStatus? status, Guid? customerId, int? month, int? year);
}
