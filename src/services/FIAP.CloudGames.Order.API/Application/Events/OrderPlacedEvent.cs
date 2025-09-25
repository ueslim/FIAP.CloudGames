using FIAP.CloudGames.Core.Messages;

namespace FIAP.CloudGames.Order.API.Application.Events
{
    public class OrderPlacedEvent : Event
    {
        public Guid OrderId { get; private set; }
        public Guid CustomerId { get; private set; }

        public OrderPlacedEvent(Guid orderId, Guid customerId)
        {
            OrderId = orderId;
            CustomerId = customerId;
        }
    }
}