namespace FIAP.CloudGames.Bff.Orders.Models
{
    public class VoucherDTO
    {
        public decimal? Percentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public string Code { get; set; }
        public int DiscountType { get; set; }
    }
}