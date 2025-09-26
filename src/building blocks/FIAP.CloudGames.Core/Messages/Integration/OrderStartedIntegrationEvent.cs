namespace FIAP.CloudGames.Core.Messages.Integration
{
    public class OrderStartedIntegrationEvent : IntegrationEvent
    {
        public Guid CustomerId { get; set; }
        public Guid OrderId { get; set; }
        public int PaymentType { get; set; }
        public decimal Value { get; set; }

        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string CardExpirationDate { get; set; }
        public string CvvCard { get; set; }
    }
}