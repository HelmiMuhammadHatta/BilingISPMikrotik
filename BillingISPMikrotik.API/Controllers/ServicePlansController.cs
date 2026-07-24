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
    public async Task<IActionResult> GetServicePlans([FromQuery] bool includeInactive = false)
    {
        var query = _dbContext.ServicePlans.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        var plans = await query.ToListAsync();
        return Ok(plans);
    }

    [HttpPost]
    public async Task<IActionResult> CreateServicePlan([FromBody] BillingISPMikrotik.Domain.Entities.ServicePlan plan)
    {
        plan.Id = System.Guid.NewGuid();
        plan.IsActive = true;
        _dbContext.ServicePlans.Add(plan);
        await _dbContext.SaveChangesAsync();
        return Ok(plan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServicePlan(System.Guid id, [FromBody] BillingISPMikrotik.Domain.Entities.ServicePlan plan)
    {
        var existing = await _dbContext.ServicePlans.FindAsync(id);
        if (existing == null) return NotFound(new { message = "Plan not found." });

        existing.Name = plan.Name;
        existing.SpeedUp = plan.SpeedUp;
        existing.SpeedDown = plan.SpeedDown;
        existing.Price = plan.Price;
        existing.MikrotikProfileName = plan.MikrotikProfileName;

        await _dbContext.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteServicePlan(System.Guid id)
    {
        var existing = await _dbContext.ServicePlans.FindAsync(id);
        if (existing == null) return NotFound(new { message = "Plan not found." });

        existing.IsActive = false;
        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "Service plan deactivated." });
    }
}
