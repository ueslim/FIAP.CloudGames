using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using MediatR;

namespace FIAP.CloudGames.Order.API.Application.Events
{
    public class OrderEventHandler : INotificationHandler<OrderPlacedEvent>
    {
        private readonly IMessageBus _bus;

        public OrderEventHandler(IMessageBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(OrderPlacedEvent message, CancellationToken cancellationToken)
        {
            await _bus.PublishAsync(new OrderPlacedIntegrationEvent(message.CustomerId));
        }
    }
}