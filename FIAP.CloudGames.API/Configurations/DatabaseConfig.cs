using FIAP.CloudGames.Infra;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.API.Configurations
{
    public static class DatabaseConfig
    {
        public static WebApplicationBuilder AddDatabaseConfiguration(this WebApplicationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            return builder;
        }

        public static WebApplication UseDbSeed(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            DbMigrationHelpers.EnsureSeedData(app).Wait();

            return app;
        }
    }
}