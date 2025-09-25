using FIAP.CloudGames.Core.Utils;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Services;

namespace FIAP.CloudGames.Order.API.Configuration
{
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"))
                .AddHostedService<OrderOrchestratorIntegrationHandler>()
                .AddHostedService<OrderIntegrationHandler>();
        }
    }
}