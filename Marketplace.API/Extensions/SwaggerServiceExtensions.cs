using Microsoft.OpenApi.Models;

namespace Marketplace.API.Extensions;

/// <summary>
/// Swagger/OpenAPI configuration extensions
/// </summary>
public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Add Swagger documentation with JWT security
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Marketplace API",
                Version = "v1",
                Description = "Two-sided marketplace API for buyers and sellers",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Url = new Uri("https://example.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // JWT Security Scheme
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer [token]'"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            // Include XML documentation
            var xmlFile = Path.Combine(AppContext.BaseDirectory, "Marketplace.API.xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }
        });

        return services;
    }
}
