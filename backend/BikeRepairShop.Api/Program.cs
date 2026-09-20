using System.Text.Json.Serialization;
using BikeRepairShop.Api.Configuration;
using BikeRepairShop.Api.Exceptions;
using BikeRepairShop.Database;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BikeRepairShop")
    ?? throw new InvalidOperationException("Connection string 'BikeRepairShop' is not configured.");

// HTTP surface: controllers, enums as strings on the wire, request validation, ProblemDetails, OpenAPI
builder.Services.AddControllers(options => options.Filters.Add<RequestValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddOpenApi();

// Infrastructure: database, health, the shop clock
builder.Services.AddDatabase(connectionString);
builder.Services.AddHealthChecks().AddDbContextCheck<BikeRepairShopDbContext>();
builder.Services.AddShopClock(builder.Configuration);

var app = builder.Build();

await app.RunMigrationsIfEnabledAsync();

// Pipeline: every failure leaves as ProblemDetails, including bare status codes
app.UseExceptionHandler();
app.UseStatusCodePages();

// Endpoints
app.MapHealthChecks("/health");
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

await app.RunAsync();

public partial class Program;
