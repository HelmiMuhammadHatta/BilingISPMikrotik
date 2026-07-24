using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using BillingISPMikrotik.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingISPMikrotik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AuthController(IConfiguration configuration, AppDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var adminUser = _configuration.GetValue<string>("AdminCredentials:Username");
            var adminPass = _configuration.GetValue<string>("AdminCredentials:Password");

            if (request.Username == adminUser && request.Password == adminPass)
            {
                var token = GenerateJwtToken("admin", "Admin");
                return Ok(new { token });
            }

            var customer = _dbContext.Customers
                .FirstOrDefault(c => c.PppUsername == request.Username && c.PppPassword == request.Password);

            if (customer != null)
            {
                var token = GenerateJwtToken(customer.Id.ToString(), "Customer");
                return Ok(new { token });
            }

            return Unauthorized(new { message = "Invalid username or password" });
        }

        private string GenerateJwtToken(string id, string role)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
