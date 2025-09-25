using System;

namespace FIAP.CloudGames.Bff.Orders.Models
{
    public class ItemCartDTO
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
    }
}