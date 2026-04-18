using Marketplace.Domain.Entities;
using MarketplaceBackend.Interfaces;
using MarketplaceBackend.Services;
using MarketplaceBackend.Utils.JsonConverters;
using MarketplaceBackend.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace MarketplaceBackend
{
    public class Program
    {
        public static void Main(string[] args)  
        {
            var builder = WebApplication.CreateBuilder(args);



            // ================= DB =================
            builder.Services.AddDbContext<MarketplaceDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ================= Controllers =================
            builder.Services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new NullToEmptyEnumerableConverterFactory());
                });

            builder.Services.AddEndpointsApiExplorer();

            // ================= JWT =================
            // Use a fallback key in development; in production supply a secure key via configuration
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "VerySecretKey12345888888888";
            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddAuthorization();

            // ================= Swagger =================
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer <token>'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            // ================= CORS =================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            // ================= Services =================
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IFlagService, FlagService>();

            var app = builder.Build();
           

            // Global exception handling middleware - ensure this runs early
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // ================= Middleware =================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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