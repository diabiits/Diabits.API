using Diabits.API.Configuration;
using Diabits.API.Data;
using Diabits.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register EF Core DbContext (custom DbContext for domain models) and configure SQL Server connection.
builder.Services.AddDbContext<DiabitsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DiabitsDb")));

// Configure ASP.NET Core Identity with the project's custom user type
builder.Services.AddIdentity<DiabitsUser, IdentityRole>()
                .AddEntityFrameworkStores<DiabitsDbContext>()
                .AddDefaultTokenProviders();

// Register custom service extensions 
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithAuth();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply pending EF Core migrations and seed Identity roles/admin at startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DiabitsDbContext>();

    var pendingMigrations = dbContext.Database.GetPendingMigrations();
    if (pendingMigrations.Any())
        await dbContext.Database.MigrateAsync();

    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}

app.Run();
