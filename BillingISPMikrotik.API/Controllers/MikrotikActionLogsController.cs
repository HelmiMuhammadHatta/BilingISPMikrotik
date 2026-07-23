using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/logs/[controller]")]
public class MikrotikActionLogsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MikrotikActionLogsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _dbContext.MikrotikActionLogs
            .Include(l => l.Customer)
            .OrderByDescending(l => l.ExecutedAt)
            .Take(100)
            .ToListAsync();
            
        return Ok(logs);
    }
}
