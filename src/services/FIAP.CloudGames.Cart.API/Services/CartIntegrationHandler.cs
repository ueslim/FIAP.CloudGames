using FIAP.CloudGames.Cart.API.Data;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Cart.API.Services
{
    public class CartIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public CartIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetSubscribers();
            return Task.CompletedTask;
        }

        private void SetSubscribers()
        {
            _bus.SubscribeAsync<OrderPlacedIntegrationEvent>("OrderPlaced", async request => await DeleteCart(request));
        }

        private async Task DeleteCart(OrderPlacedIntegrationEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CartContext>();

            var cart = await context.CartCustomer.FirstOrDefaultAsync(c => c.CustomerId == message.CustomerId);

            if (cart != null)
            {
                context.CartCustomer.Remove(cart);
                await context.SaveChangesAsync();
            }
        }
    }
}