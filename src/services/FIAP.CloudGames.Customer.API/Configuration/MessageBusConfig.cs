using FIAP.CloudGames.Core.Utils;
using FIAP.CloudGames.Customer.API.Services;
using FIAP.CloudGames.MessageBus;

namespace FIAP.CloudGames.Customer.API.Configuration
{
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"))
                .AddHostedService<CustomerRegistrationIntegrationHandler>();
        }
    }
}