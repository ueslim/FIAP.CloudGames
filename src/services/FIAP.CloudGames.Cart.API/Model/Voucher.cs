namespace FIAP.CloudGames.Cart.API.Model
{
    public class Voucher
    {
        public decimal? Percentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public string? Code { get; set; }
        public VoucherDiscountType? VoucherDiscountType { get; set; }
    }

    public enum VoucherDiscountType
    {
        Percentage = 0,
        Value = 1
    }
}