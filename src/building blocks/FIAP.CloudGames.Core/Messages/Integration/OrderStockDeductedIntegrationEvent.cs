namespace FIAP.CloudGames.Core.Messages.Integration
{
    public class OrderStockDeductedIntegrationEvent : IntegrationEvent
    {
        public Guid ClientId { get; private set; }
        public Guid OrderId { get; private set; }

        public OrderStockDeductedIntegrationEvent(Guid clienteId, Guid pedidoId)
        {
            ClientId = clienteId;
            OrderId = pedidoId;
        }
    }
}