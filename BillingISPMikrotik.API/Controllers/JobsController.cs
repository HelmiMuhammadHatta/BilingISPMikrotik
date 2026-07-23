using System.Threading.Tasks;
using BillingISPMikrotik.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IAutoIsolirService _autoIsolirService;

    public JobsController(IAutoIsolirService autoIsolirService)
    {
        _autoIsolirService = autoIsolirService;
    }

    // TODO: Uncomment [Authorize] when authentication is implemented
    // [Authorize(Roles = "Admin")]
    [HttpPost("run-auto-isolir")]
    public async Task<IActionResult> RunAutoIsolir()
    {
        var count = await _autoIsolirService.ProcessOverdueInvoicesAsync();
        return Ok(new { message = $"Auto-isolir job completed. Processed {count} customers." });
    }
}
