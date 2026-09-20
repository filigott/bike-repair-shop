using BikeRepairShop.Api.Configuration;
using BikeRepairShop.Database;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BikeRepairShop")
    ?? throw new InvalidOperationException("Connection string 'BikeRepairShop' is not configured.");

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddDatabase(connectionString);
builder.Services.AddShopClock(builder.Configuration);

var app = builder.Build();

await app.RunMigrationsIfEnabledAsync();

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

public partial class Program;
