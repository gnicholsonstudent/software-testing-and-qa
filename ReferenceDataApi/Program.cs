using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using ReferenceDataApi.Repositories;
using ReferenceDataApi.Security;
using ReferenceDataApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Options for API key auth
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKeyAuth"));

// AuthN/AuthZ
builder.Services
    .AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ReaderOrAdmin", policy => policy.RequireRole("Reader", "Admin"));
});

// DI for repository + service
builder.Services.AddSingleton<IReferenceDataRepository, InMemoryReferenceDataRepository>();
builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();

// Swagger + API Key header
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reference Data API",
        Version = "v1",
        Description = "CRUD API for internal reference data (type-based routing) secured with API keys."
    });

    var headerName = builder.Configuration["ApiKeyAuth:HeaderName"] ?? "X-API-KEY";
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = $"API Key authentication via header `{headerName}`",
        Name = headerName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reference Data API v1");
});

// Standard pipeline
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();