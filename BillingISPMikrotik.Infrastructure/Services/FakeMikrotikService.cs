using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using Microsoft.Extensions.Logging;

namespace BillingISPMikrotik.Infrastructure.Services;

public class FakeMikrotikService : IMikrotikService
{
    private readonly ILogger<FakeMikrotikService> _logger;
    private readonly Random _random = new Random();

    public FakeMikrotikService(ILogger<FakeMikrotikService> logger)
    {
        _logger = logger;
    }

    private async Task<bool> SimulateDelayAndRandomFailure(string operation)
    {
        await Task.Delay(200);
        
        // 5% chance to fail
        if (_random.Next(0, 100) < 5)
        {
            _logger.LogWarning($"[FAKE] {operation} failed due to random simulated error.");
            return false;
        }

        return true;
    }

    public async Task<bool> ConnectAsync()
    {
        _logger.LogInformation("[FAKE] ConnectAsync called.");
        return await SimulateDelayAndRandomFailure("ConnectAsync");
    }

    public async Task<bool> SetPppProfileAsync(string pppUsername, string profileName)
    {
        _logger.LogInformation($"[FAKE] SetPppProfile called for user '{pppUsername}' to profile '{profileName}'.");
        return await SimulateDelayAndRandomFailure("SetPppProfileAsync");
    }

    public async Task<bool> DisconnectActiveSessionAsync(string pppUsername)
    {
        _logger.LogInformation($"[FAKE] DisconnectActiveSession called for user '{pppUsername}'.");
        return await SimulateDelayAndRandomFailure("DisconnectActiveSessionAsync");
    }

    public async Task<IEnumerable<string>> GetActivePppUsersAsync()
    {
        _logger.LogInformation("[FAKE] GetActivePppUsers called.");
        await Task.Delay(200);
        return new List<string> { "budi", "siti" }; // Dummy data
    }

    public async Task<bool> TestConnectionAsync()
    {
        _logger.LogInformation("[FAKE] TestConnection called.");
        return await SimulateDelayAndRandomFailure("TestConnectionAsync");
    }
}
