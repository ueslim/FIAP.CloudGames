using System.Collections.Generic;

namespace FIAP.CloudGames.Bff.Orders.Models
{
    public class CartDTO
    {
        public decimal TotalValue { get; set; }
        public VoucherDTO Voucher { get; set; }
        public bool VoucherUsed { get; set; }
        public decimal Discount { get; set; }
        public List<ItemCartDTO> Items { get; set; } = new List<ItemCartDTO>();
    }
}