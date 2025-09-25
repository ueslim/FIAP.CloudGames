using FIAP.CloudGames.Order.Domain.Order;

namespace FIAP.CloudGames.Order.API.Application.DTO
{
    public class OrderItemDTO
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }

        public static OrderItem ToOrderItem(OrderItemDTO orderItemDTO)
        {
            return new OrderItem(orderItemDTO.ProductId, orderItemDTO.Name, orderItemDTO.Quantity,
                orderItemDTO.Value, orderItemDTO.Image);
        }
    }
}