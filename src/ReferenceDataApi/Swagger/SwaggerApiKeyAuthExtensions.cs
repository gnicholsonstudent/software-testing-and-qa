using Microsoft.OpenApi.Models;

namespace ReferenceDataApi.Swagger;

public static class SwaggerApiKeyAuthExtensions
{
    public static void AddApiKeyAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Reference Data API",
                Version = "v1"
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "API Key authentication using the X-API-KEY header",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            };

            c.AddSecurityDefinition("ApiKey", scheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { scheme, Array.Empty<string>() }
            });
        });
    }
}