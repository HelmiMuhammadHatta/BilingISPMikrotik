using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;

namespace BillingISPMikrotik.Infrastructure.Services;

public class MikrotikService : IMikrotikService
{
    public Task<bool> ConnectAsync()
    {
        // TODO: Implement with tik4net
        throw new NotImplementedException();
    }

    public Task<bool> SetPppProfileAsync(string pppUsername, string profileName)
    {
        // TODO: Implement with tik4net
        throw new NotImplementedException();
    }

    public Task<bool> DisconnectActiveSessionAsync(string pppUsername)
    {
        // TODO: Implement with tik4net
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetActivePppUsersAsync()
    {
        // TODO: Implement with tik4net
        throw new NotImplementedException();
    }

    public Task<bool> TestConnectionAsync()
    {
        // TODO: Implement with tik4net
        throw new NotImplementedException();
    }
}
