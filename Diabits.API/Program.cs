using Diabits.API.Configuration;
using Diabits.API.Data;
using Diabits.API.Data.Mapping;
using Diabits.API.Helpers.Mapping;
using Diabits.API.Interfaces;
using Diabits.API.Models;
using Diabits.API.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

//TODO Refactor
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasm", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7214",
                "https://192.168.1.43:7214"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<NumericMapper>();
builder.Services.AddSingleton<WorkoutMapper>();
builder.Services.AddSingleton<ManualInputMapper>();
builder.Services.AddSingleton<MapperFactory>();


// Register EF Core DbContext (custom DbContext for domain models) and configure SQL Server connection.
builder.Services.AddDbContext<DiabitsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DiabitsDb")));

// Configure ASP.NET Core Identity with the project's custom user type
builder.Services.AddIdentity<DiabitsUser, IdentityRole>()
                .AddEntityFrameworkStores<DiabitsDbContext>()
                .AddDefaultTokenProviders();

// Register custom service extensions 
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithAuth();

builder.Services.AddScoped<IHealthDataService, HealthDataService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "diabits API V1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

//TODO Refactor
//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("BlazorWasm");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending EF Core migrations and seed Identity roles/admin at startup.
if (!app.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<DiabitsDbContext>();

        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
            await dbContext.Database.MigrateAsync();

        await IdentitySeeder.SeedRolesAndAdminAsync(services);
    }
}

app.Run();
