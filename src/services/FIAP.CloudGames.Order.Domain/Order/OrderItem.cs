using FIAP.CloudGames.Core.DomainObjects;

namespace FIAP.CloudGames.Order.Domain.Order
{
    public class OrderItem : Entity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitValue { get; private set; }
        public string ProductImage { get; set; }

        // EF Rel.
        public Order Order { get; set; }

        public OrderItem(Guid productId, string productName, int quantity, decimal unitValue, string productImage = null)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitValue = unitValue;
            ProductImage = productImage;
        }

        // EF ctor
        protected OrderItem()
        { }

        internal decimal CalculateValue()
        {
            return Quantity * UnitValue;
        }
    }
}