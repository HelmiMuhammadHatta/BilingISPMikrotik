using System;
using System.Linq;
using System.Threading.Tasks;
using BillingISPMikrotik.Domain.Entities;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingISPMikrotik.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CustomersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _dbContext.Customers
            .Include(c => c.ServicePlan)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        return Ok(customers);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
    {
        customer.Id = Guid.NewGuid();
        customer.CreatedAt = DateTime.UtcNow;
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCustomers), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Customer customer)
    {
        if (id != customer.Id)
        {
            return BadRequest();
        }

        _dbContext.Entry(customer).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
