using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using MediatR;

namespace FIAP.CloudGames.Order.API.Application.Events
{
    public class OrderEventHandler : INotificationHandler<OrderFinishedEvent>
    {
        private readonly IMessageBus _bus;

        public OrderEventHandler(IMessageBus bus)
        {
            _bus = bus;
        }

        //  API Order manda para si mesmo um evento e depois um integration event
        public async Task Handle(OrderFinishedEvent message, CancellationToken cancellationToken)
        {
            await _bus.PublishAsync(new OrderFinishedIntegrationEvent(message.CustomerId));
        }
    }
}