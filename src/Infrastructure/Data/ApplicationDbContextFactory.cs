using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Tamelo.Api.Infrastructure.Data;

/// <summary>
/// Used exclusively by the EF Core CLI (dotnet ef migrations add / database update).
/// Reads the "Tamelo.Migrations" connection string from appsettings.json so that
/// migrations run against a different endpoint than the runtime app.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Tamelo.Direct")
            ?? throw new InvalidOperationException(
                "Connection string 'Tamelo.Direct' not found in appsettings.json. " +
                "This string is required for EF Core CLI operations (migrations/database update).");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
