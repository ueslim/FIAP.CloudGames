using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Order.Domain.Voucher.Specs;

namespace FIAP.CloudGames.Order.Domain.Voucher
{
    public class Voucher : Entity, IAggregateRoot
    {
        public string Code { get; private set; }
        public decimal? Percentage { get; private set; }
        public decimal? DiscountValue { get; private set; }
        public int Quantity { get; private set; }
        public VoucherDiscountType DiscountType { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public bool Active { get; private set; }
        public bool Used { get; private set; }

        public bool IsValidForUse()
        {
            return new VoucherActiveSpecification()
                .And(new VoucherDateSpecification())
                .And(new VoucherQuantitySpecification())
                .IsSatisfiedBy(this);
        }

        public void MarkAsUsed()
        {
            Active = false;
            Used = true;
            Quantity = 0;
            UsedAt = DateTime.Now;
        }

        public void DebitQuantity()
        {
            Quantity -= 1;
            if (Quantity >= 1) return;

            MarkAsUsed();
        }
    }
}