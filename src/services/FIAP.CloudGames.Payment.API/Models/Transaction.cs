using FIAP.CloudGames.Core.DomainObjects;

namespace FIAP.CloudGames.Payment.API.Models
{
    public class Transaction : Entity
    {
        public string AuthorizationCode { get; set; }
        public string CardBrand { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TransactionCost { get; set; }
        public TransactionStatus Status { get; set; }
        public string TID { get; set; }
        public string NSU { get; set; }

        public Guid PaymentId { get; set; }

        // EF Relation
        public Payment Payment { get; set; }
    }
}