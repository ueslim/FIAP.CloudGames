namespace FIAP.CloudGames.Core.Messages.Integration
{
    public class OrderPlacedIntegrationEvent : IntegrationEvent
    {
        public Guid CustomerId { get; private set; }

        public OrderPlacedIntegrationEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}