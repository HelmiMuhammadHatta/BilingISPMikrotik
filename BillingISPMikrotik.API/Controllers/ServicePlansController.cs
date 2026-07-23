using System.Threading.Tasks;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicePlansController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ServicePlansController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetServicePlans()
    {
        var plans = await _dbContext.ServicePlans.ToListAsync();
        return Ok(plans);
    }
}
