using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Application.Queries;

namespace FIAP.CloudGames.Order.API.Services
{
    public class OrderOrchestratorIntegrationHandler : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderOrchestratorIntegrationHandler> _logger;
        private Timer _timer;

        public OrderOrchestratorIntegrationHandler(ILogger<OrderOrchestratorIntegrationHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de pedidos iniciado.");

            _timer = new Timer(ProcessOrders, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

            return Task.CompletedTask;
        }

        private async void ProcessOrders(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var orderQueries = scope.ServiceProvider.GetRequiredService<IOrderQueries>();
                var order = await orderQueries.GetAuthorizedOrders();

                if (order == null) return;

                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                var orderAuthorized = new OrderAuthorizedIntegrationEvent(order.CustomerId, order.Id,
                    order.OrderItems.ToDictionary(p => p.ProductId, p => p.Quantity));

                await bus.PublishAsync(orderAuthorized);

                _logger.LogInformation($"Pedido ID: {order.Id} foi encaminhado para baixa no estoque.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de pedidos finalizado.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}