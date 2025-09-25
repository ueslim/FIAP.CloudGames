namespace FIAP.CloudGames.Order.API.Application.DTO
{
    public class VoucherDTO
    {
        public string Code { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public int DiscountType { get; set; }
    }
}