using Marketplace.Infrastructure.Extensions;
using Marketplace.API.Extensions;
using MarketplaceBackend.Utils.JsonConverters;
using MarketplaceBackend.Middleware;

namespace MarketplaceBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================= Services Configuration =================
            
            // Database
            builder.Services.AddDatabaseServices(builder.Configuration);

            // Authentication & Authorization
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Swagger Documentation
            builder.Services.AddSwaggerDocumentation();

            // CORS
            builder.Services.AddCorsPolicy();

            // Infrastructure Services
            builder.Services.AddInfrastructureServices();

            builder.Services.AddControllers()
           .AddJsonOptions(o =>
           {
               o.JsonSerializerOptions.ReferenceHandler =
                   System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
           });
            builder.Services.AddEndpointsApiExplorer();

            // ================= Build Application =================
            var app = builder.Build();

            // ================= Middleware Pipeline =================
            
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Marketplace API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();


            app.Run();
        }
    }
}
