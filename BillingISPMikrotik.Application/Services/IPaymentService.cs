using System;
using System.Threading.Tasks;
using BillingISPMikrotik.Domain.Entities;

namespace BillingISPMikrotik.Application.Services;

public interface IPaymentService
{
    Task<PaymentLog> ConfirmPaymentAsync(Guid invoiceId, string method, decimal amount, string referenceNumber);
}
