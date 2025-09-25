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
                var produtosComEstoque = new List<Product>();
                var produtoRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();

                var idsProdutos = string.Join(",", message.Items.Select(c => c.Key));
                var produtos = await produtoRepository.GetProductsById(idsProdutos);

                if (produtos.Count != message.Items.Count)
                {
                    CancelOrderForInsufficientStock(message);
                    return;
                }

                foreach (var product in produtos)
                {
                    var quantidadeProduto = message.Items.FirstOrDefault(p => p.Key == product.Id).Value;

                    if (product.IsAvailable(quantidadeProduto))
                    {
                        product.DecrementStock(quantidadeProduto);
                        produtosComEstoque.Add(product);
                    }
                }

                if (produtosComEstoque.Count != message.Items.Count)
                {
                    CancelOrderForInsufficientStock(message);
                    return;
                }

                foreach (var produto in produtosComEstoque)
                {
                    produtoRepository.Update(produto);
                }

                if (!await produtoRepository.UnitOfWork.Commit())
                {
                    throw new DomainException($"Problemas ao atualizar estoque do pedido {message.OrderId}");
                }

                var orderDeducted = new OrderStockDeductedIntegrationEvent(message.CustomerId, message.OrderId);
                await _bus.PublishAsync(orderDeducted);
            }
        }

        public async void CancelOrderForInsufficientStock(OrderAuthorizedIntegrationEvent message)
        {
            var pedidoCancelado = new OrderStockDeductedIntegrationEvent(message.CustomerId, message.OrderId);
            await _bus.PublishAsync(pedidoCancelado);
        }
    }
}