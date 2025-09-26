using FIAP.CloudGames.Catalog.API.Models;
using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;

namespace FIAP.CloudGames.Catalog.API.Services
{
    public class CatalogIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public CatalogIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
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
            _bus.SubscribeAsync<OrderAuthorizedIntegrationEvent>("OrderAuthorized", async request =>
                await DeductStock(request));
        }

        private async Task DeductStock(OrderAuthorizedIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var productsWithStock = new List<Product>();
                var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

                var idsProducts = string.Join(",", message.Items.Select(c => c.Key));
                var products = await productRepository.GetProductsById(idsProducts);

                if (products.Count != message.Items.Count)
                {
                    CancelOrderForInsufficientStock(message);
                    return;
                }

                foreach (var product in products)
                {
                    var productQuantity = message.Items.FirstOrDefault(p => p.Key == product.Id).Value;

                    if (product.IsAvailable(productQuantity))
                    {
                        product.DecrementStock(productQuantity);
                        productsWithStock.Add(product);
                    }
                }

                if (productsWithStock.Count != message.Items.Count)
                {
                    CancelOrderForInsufficientStock(message);
                    return;
                }

                foreach (var produto in productsWithStock)
                {
                    productRepository.Update(produto);
                }

                if (!await productRepository.UnitOfWork.Commit())
                {
                    throw new DomainException($"Problemas ao atualizar estoque do pedido {message.OrderId}");
                }

                var orderDeducted = new OrderStockDeductedIntegrationEvent(message.CustomerId, message.OrderId);
                await _bus.PublishAsync(orderDeducted);
            }
        }

        public async void CancelOrderForInsufficientStock(OrderAuthorizedIntegrationEvent message)
        {
            var orderCanceled = new OrderStockDeductedIntegrationEvent(message.CustomerId, message.OrderId);
            await _bus.PublishAsync(orderCanceled);
        }
    }
}