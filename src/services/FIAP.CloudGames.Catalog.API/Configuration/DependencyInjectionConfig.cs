using FIAP.CloudGames.Catalog.API.Data;
using FIAP.CloudGames.Catalog.API.Data.Repository;
using FIAP.CloudGames.Catalog.API.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Catalog.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<CatalogContext>();
        }
    }
}
