namespace FIAP.CloudGames.Core.Messages.Integration
{
    public class OrderFinishedIntegrationEvent : IntegrationEvent
    {
        public Guid CustomerId { get; private set; }

        public OrderFinishedIntegrationEvent(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}