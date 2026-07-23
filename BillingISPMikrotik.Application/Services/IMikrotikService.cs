using System.Collections.Generic;
using System.Threading.Tasks;

namespace BillingISPMikrotik.Application.Services;

public interface IMikrotikService
{
    Task<bool> ConnectAsync();
    Task<bool> SetPppProfileAsync(string pppUsername, string profileName);
    Task<bool> DisconnectActiveSessionAsync(string pppUsername);
    Task<IEnumerable<string>> GetActivePppUsersAsync();
    Task<bool> TestConnectionAsync();
}
