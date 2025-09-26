using FIAP.CloudGames.Core.Observability;

namespace FIAP.CloudGames.Order.API.Configuration
{
    public static class ObservabilityConfigExtensions
    {
        public static void AddObservabilityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddObservability(configuration, "order-api");
        }
    }
}
