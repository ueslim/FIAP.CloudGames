using FIAP.CloudGames.Core.Observability;

namespace FIAP.CloudGames.Bff.Orders.Configuration
{
    public static class ObservabilityConfigExtensions
    {
        public static void AddObservabilityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddObservability(configuration, "shopping-bff-api");
        }
    }
}
