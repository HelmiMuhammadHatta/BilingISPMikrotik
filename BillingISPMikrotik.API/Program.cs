using BillingISPMikrotik.Application.Services;
using BillingISPMikrotik.Infrastructure.Persistence;
using BillingISPMikrotik.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Register MikrotikService
builder.Services.AddScoped<IMikrotikService, FakeMikrotikService>();

// Register InvoiceService
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Register AutoIsolirService
builder.Services.AddScoped<IAutoIsolirService, AutoIsolirService>();

// Register PaymentService
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Use Hangfire Dashboard
app.UseHangfireDashboard();

// Configure recurring job for Invoice Generation
// Tanggal 1 jam 00:05 tiap bulan
RecurringJob.AddOrUpdate<IInvoiceService>(
    "GenerateMonthlyInvoices",
    service => service.GenerateMonthlyInvoicesAsync(DateTime.UtcNow.Month, DateTime.UtcNow.Year),
    "5 0 1 * *");

// Configure recurring job for Auto Isolir
// Tiap jam 00:05 setiap hari
RecurringJob.AddOrUpdate<IAutoIsolirService>(
    "AutoIsolirJob",
    service => service.ProcessOverdueInvoicesAsync(),
    "5 0 * * *");

app.Run();
