namespace Marketplace.API.Extensions;

/// <summary>
/// CORS policy configuration extensions
/// </summary>
public static class CorsServiceExtensions
{
    /// <summary>
    /// Add CORS policy for frontend applications
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy
                    .WithOrigins("http://localhost:3000", "http://localhost:3001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Total-Count", "X-Page-Number"));
        });

        return services;
    }
}
