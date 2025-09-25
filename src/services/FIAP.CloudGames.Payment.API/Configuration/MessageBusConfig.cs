using FIAP.CloudGames.Core.Utils;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Payment.API.Services;

namespace FIAP.CloudGames.Payment.API.Configuration
{
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"))
                .AddHostedService<PaymentIntegrationHandler>();
        }
    }
}