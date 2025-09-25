namespace FIAP.CloudGames.Order.API.Application.DTO
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public int Code { get; set; }

        public Guid CustomerId { get; set; }
        public int Status { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalValue { get; set; }

        public decimal Discount { get; set; }
        public string VoucherCode { get; set; }
        public bool VoucherUsed { get; set; }

        public List<OrderItemDTO> OrdetItems { get; set; }
        public AddressDTO Address { get; set; }

        public static OrderDTO ToOrderDTO(Domain.Order.Order order)
        {
            var orderDTO = new OrderDTO
            {
                Id = order.Id,
                Code = order.Code,
                Status = (int)order.OrderStatus,
                Date = order.RegisterDate,
                TotalValue = order.TotalValue,
                Discount = order.Discount,
                VoucherUsed = order.VoucherUsed,
                OrdetItems = new List<OrderItemDTO>(),
                Address = new AddressDTO()
            };

            foreach (var item in order.OrderItems)
            {
                orderDTO.OrdetItems.Add(new OrderItemDTO
                {
                    Name = item.ProductName,
                    Image = item.ProductImage,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    Value = item.UnitValue,
                    OrderId = item.OrderId
                });
            }

            orderDTO.Address = new AddressDTO
            {
                Street = order.Address.Street,
                Number = order.Address.Number,
                AdditionalInfo = order.Address.AdditionalInfo,
                Neighborhood = order.Address.Neighborhood,
                PostalCode = order.Address.PostalCode,
                City = order.Address.City,
                State = order.Address.State,
            };

            return orderDTO;
        }
    }
}