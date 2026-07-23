using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MikrotikController : ControllerBase
{
    private readonly IMikrotikService _mikrotikService;

    public MikrotikController(IMikrotikService mikrotikService)
    {
        _mikrotikService = mikrotikService;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        var result = await _mikrotikService.TestConnectionAsync();
        
        if (result)
        {
            return Ok(new { message = "Connection successful (simulated)." });
        }

        return StatusCode(500, new { message = "Connection failed (simulated random error)." });
    }
}
