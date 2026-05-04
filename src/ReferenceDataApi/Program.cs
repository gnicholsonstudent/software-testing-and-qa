using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ReferenceDataApi.Repositories;
using ReferenceDataApi.Security;
using ReferenceDataApi.Services;
using ReferenceDataApi.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Repo + service DI
builder.Services.AddSingleton<IReferenceDataRepository, InMemoryReferenceDataRepository>();
builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();

// ApiKey options from config
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKeyAuth"));

// Authentication + authorisation
builder.Services
    .AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

builder.Services.AddAuthorization(options =>
{
    // Optional policies if you prefer using [Authorize(Policy=...)] later
    options.AddPolicy("ReaderOrAdmin", policy => policy.RequireRole("Reader", "Admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiKeyAuth();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reference Data API v1");
});

// Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Health endpoint for post-deploy checks
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        service = "ReferenceDataApi",
        utcNow = DateTime.UtcNow
    });
})
.WithName("Health")
.AllowAnonymous();

app.MapControllers();

app.Run();