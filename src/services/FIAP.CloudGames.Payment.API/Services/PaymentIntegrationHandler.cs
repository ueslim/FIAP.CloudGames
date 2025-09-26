using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Payment.API.Models;

namespace FIAP.CloudGames.Payment.API.Services
{
    public class PaymentIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public PaymentIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        private void SetResponder()
        {
            _bus.RespondAsync<OrderStartedIntegrationEvent, ResponseMessage>(async request => await AuthorizePayment(request));
        }

        private void SetSubscribers()
        {
            _bus.SubscribeAsync<OrderCanceledIntegrationEvent>("OrderCanceled", async request => await CancelPayment(request));

            _bus.SubscribeAsync<OrderStockDeductedIntegrationEvent>("OrderStockDeductedIntegrationEvent", async request => await CapturePayment(request));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetResponder();
            SetSubscribers();
            return Task.CompletedTask;
        }

        private async Task<ResponseMessage> AuthorizePayment(OrderStartedIntegrationEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var payment = new Models.Payment
            {
                OrderId = message.OrderId,
                PaymentType = (PaymentType)message.PaymentType,
                Value = message.Value,
                CreditCard = new CreditCard(message.CardName, message.CardNumber, message.CardExpirationDate, message.CvvCard)
            };

            var response = await paymentService.AuthorizePayment(payment);

            return response;
        }

        private async Task CancelPayment(OrderCanceledIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                var response = await paymentService.CancelPayment(message.OrderId);

                if (!response.ValidationResult.IsValid)
                    throw new DomainException($"Falha ao cancelar pagamento do pedido {message.OrderId}");
            }
        }

        private async Task CapturePayment(OrderStockDeductedIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                var response = await paymentService.CapturePayment(message.OrderId);

                if (!response.ValidationResult.IsValid)
                    throw new DomainException($"Falha ao capturar pagamento do pedido {message.OrderId}");

                await _bus.PublishAsync(new OrderPaidIntegrationEvent(message.ClientId, message.OrderId));
            }
        }
    }
}