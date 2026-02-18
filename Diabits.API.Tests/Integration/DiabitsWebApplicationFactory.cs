using System.Data.Common;

using Diabits.API.Data;
using Diabits.API.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class DiabitsWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
              d => d.ServiceType ==
                  typeof(IDbContextOptionsConfiguration<DiabitsDbContext>));

            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            services.AddSingleton<DbConnection>(_ =>
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();
                return _connection;
            });

            services.AddDbContext<DiabitsDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DiabitsDbContext>();

        db.Database.EnsureCreated();

        SeedUser(scope.ServiceProvider).GetAwaiter().GetResult();

        return host;
    }

    private static async Task SeedUser(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<DiabitsUser>>();
        var user = new DiabitsUser { UserName = "user", Email = "user@example.com" };

        if (await userManager.FindByNameAsync(user.UserName) is null)
        {
            var result = await userManager.CreateAsync(user, "Password1!");
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection?.Dispose();
    }
}
