using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Order.Domain.Voucher;

namespace FIAP.CloudGames.Order.Domain.Order
{
    public class Order : Entity, IAggregateRoot
    {
        public Order(Guid customerId, decimal totalValue, List<OrderItem> orderItems, bool voucherUsed = false, decimal discount = 0, Guid? voucherId = null)
        {
            CustomerId = customerId;
            TotalValue = totalValue;
            _orderItems = orderItems;
            Discount = discount;
            VoucherUsed = voucherUsed;
            VoucherId = voucherId;
        }

        // EF ctor
        protected Order()
        { }

        public int Code { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid? VoucherId { get; private set; }
        public bool VoucherUsed { get; private set; }
        public decimal Discount { get; private set; }
        public decimal TotalValue { get; private set; }
        public DateTime RegisterDate { get; private set; }
        public OrderStatus OrderStatus { get; private set; }

        private readonly List<OrderItem> _orderItems;
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        public Address Address { get; private set; }

        // EF Rel.
        public Voucher.Voucher Voucher { get; private set; }

        public void AuthorizeOrder()
        {
            OrderStatus = OrderStatus.Authorized;
        }

        public void CancelOrder()
        {
            OrderStatus = OrderStatus.Canceled;
        }

        public void FinishOrder()
        {
            OrderStatus = OrderStatus.Paid;
        }

        public void AssignVoucher(Voucher.Voucher voucher)
        {
            VoucherUsed = true;
            VoucherId = voucher.Id;
            Voucher = voucher;
        }

        public void AssignAddress(Address address)
        {
            Address = address;
        }

        public void CalculateOrderValue()
        {
            TotalValue = OrderItems.Sum(p => p.CalculateValue());
            CalculateTotalDiscountValue();
        }

        public void CalculateTotalDiscountValue()
        {
            if (!VoucherUsed) return;

            decimal discount = 0;
            var value = TotalValue;

            if (Voucher.DiscountType == VoucherDiscountType.Percent)
            {
                if (Voucher.Percentage.HasValue)
                {
                    discount = value * Voucher.Percentage.Value / 100;
                    value -= discount;
                }
            }
            else
            {
                if (Voucher.DiscountValue.HasValue)
                {
                    discount = Voucher.DiscountValue.Value;
                    value -= discount;
                }
            }

            TotalValue = value < 0 ? 0 : value;
            Discount = discount;
        }
    }
}