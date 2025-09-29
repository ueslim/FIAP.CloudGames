using FIAP.CloudGames.Core.Messages;

namespace FIAP.CloudGames.Order.API.Application.Events
{
    public class OrderFinishedEvent : Event
    {
        public Guid OrderId { get; private set; }
        public Guid CustomerId { get; private set; }

        public OrderFinishedEvent(Guid orderId, Guid customerId)
        {
            OrderId = orderId;
            CustomerId = customerId;
        }
    }
}