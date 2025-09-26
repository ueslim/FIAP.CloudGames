using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FIAP.CloudGames.Catalog.API.Data
{
    public class CatalogContextFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var cs = config.GetConnectionString("DefaultConnection")
                     ?? "Server=(localdb)\\mssqllocaldb;Database=CloudGames_Cart;Trusted_Connection=True;TrustServerCertificate=True";

            var options = new DbContextOptionsBuilder<CatalogContext>()
                .UseSqlServer(cs, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Catalog", "dbo"))
                .Options;

            return new CatalogContext(options);
        }
    }
}