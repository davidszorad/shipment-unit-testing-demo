using Microsoft.EntityFrameworkCore;
using ShipmentApi.Abstractions;
using ShipmentApi.Infrastructure;
using ShipmentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ShipmentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ShipmentDb") ?? "Data Source=shipments.db"));

builder.Services.AddScoped<IShipmentRepository, EfShipmentRepository>();
builder.Services.AddScoped<IDeliveryLocationRepository, EfDeliveryLocationRepository>();
builder.Services.AddSingleton<IProductCatalog, InMemoryProductCatalog>();
builder.Services.AddSingleton<INotificationSender, LoggingNotificationSender>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ShipmentService>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposed as a public partial type so Microsoft.AspNetCore.Mvc.Testing's
// WebApplicationFactory<Program> can reference this entry point from the
// integration test project.
public partial class Program;
