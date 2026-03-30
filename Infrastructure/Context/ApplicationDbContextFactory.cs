using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManageAccountWebAPI.Infrastructure.Context
{
    /// <summary>
    /// Design-time factory used exclusively by EF Core CLI tooling
    /// (e.g. <c>dotnet ef migrations add</c>, <c>dotnet ef database update</c>).
    ///
    /// At design-time the DI container is not available, so we manually build
    /// an <see cref="IConfiguration"/> from <c>appsettings.json</c> and wire up
    /// the Oracle provider – exactly once, in a single place.
    /// </summary>
    public sealed class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Resolve the project root so the factory works regardless of the
            // working directory the CLI runs from.
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true,  reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("OracleConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'OracleConnection' is not configured in appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseOracle(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
