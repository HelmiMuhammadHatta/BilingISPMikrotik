using System.Threading.Tasks;

namespace BillingISPMikrotik.Application.Services;

public interface IAutoIsolirService
{
    Task<int> ProcessOverdueInvoicesAsync();
}
