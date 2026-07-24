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

        var existingCustomer = await _dbContext.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            return NotFound();
        }

        // Update fields, but preserve CreatedAt
        existingCustomer.Name = customer.Name;
        existingCustomer.PppUsername = customer.PppUsername;
        existingCustomer.PppPassword = customer.PppPassword;
        existingCustomer.ServicePlanId = customer.ServicePlanId;
        existingCustomer.Phone = customer.Phone;
        existingCustomer.Address = customer.Address;
        existingCustomer.Status = customer.Status;
        
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
