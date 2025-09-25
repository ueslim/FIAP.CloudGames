using FIAP.CloudGames.Core.DomainObjects;

namespace FIAP.CloudGames.Payment.API.Models
{
    public class Payment : Entity, IAggregateRoot
    {
        public Payment()
        {
            Transactions = new List<Transaction>();
        }

        public Guid OrderId { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal Value { get; set; }

        public CreditCard CreditCard { get; set; }

        // EF Relation
        public ICollection<Transaction> Transactions { get; set; }

        public void AddTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);
        }
    }
}