using FIAP.CloudGames.Catalog.API.Services;
using FIAP.CloudGames.Core.Utils;
using FIAP.CloudGames.MessageBus;

namespace FIAP.CloudGames.Catalog.API.Configuration
{
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"))
                .AddHostedService<CatalogIntegrationHandler>();
        }
    }
}