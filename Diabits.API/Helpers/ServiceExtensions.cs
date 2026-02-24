using Diabits.API.Data;
using Diabits.API.Helpers.Mapping;
using Diabits.API.Interfaces;
using Diabits.API.Models;
using Diabits.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Diabits.API.Configuration;

/// <summary>
/// Centralized extension methods to register common services used by the Diabits API.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication and authorization services using configuration values.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Configure token validation parameters to ensure tokens are valid and signed with the configured key
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Configures Swagger generation and adds a security definition so the Swagger UI can accept a JWT bearer token.
    /// </summary>
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Diabits API", Version = "v1" });

            // Define the Bearer authentication scheme for Swagger/OpenAPI.
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            // Require the Bearer scheme for operations that need authentication.
            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", doc)] = []
            });
        });

        return services;
    }

    /// <summary>
    /// Configures CORS policy for the Blazor WebAssembly client.
    /// Allows specified origins to make authenticated requests to the API.
    /// </summary>
    public static IServiceCollection AddDiabitsCors(this IServiceCollection services, IConfiguration config)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("BlazorWasm", policy =>
            {
                var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? ["https://localhost:7214"];

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers the database context and configures ASP.NET Core Identity.
    /// </summary>
    public static IServiceCollection AddDiabitsDataAccess(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DiabitsDbContext>(options => 
            options.UseSqlServer(config.GetConnectionString("DiabitsDb")));

        services
            .AddIdentity<DiabitsUser, IdentityRole>()
            .AddEntityFrameworkStores<DiabitsDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Registers all application services and mappers used by the API.
    /// </summary>
    public static IServiceCollection AddDiabitsServices(this IServiceCollection services)
    {
        // Mappers (singleton for performance - stateless)
        services.AddSingleton<NumericMapper>();
        services.AddSingleton<WorkoutMapper>();
        services.AddSingleton<ManualInputMapper>();
        services.AddSingleton<MapperFactory>();

        // Application services (scoped to request lifetime)
        services.AddScoped<IHealthDataService, HealthDataService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITimelineDashboardService, TimelineDashboardService>();
        services.AddScoped<IGlucoseDashboardService, GlucoseDashboardService>();

        return services;
    }

    /// <summary>
    /// Applies pending database migrations and seeds initial data.
    /// Should only be called in non-test environments.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var dbContext = scopedServices.GetRequiredService<DiabitsDbContext>();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }

        await IdentitySeeder.SeedRolesAndAdminAsync(scopedServices);
    }
}