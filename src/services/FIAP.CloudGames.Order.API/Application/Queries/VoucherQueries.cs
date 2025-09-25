using FIAP.CloudGames.Order.API.Application.DTO;
using FIAP.CloudGames.Order.Domain.Voucher;

namespace FIAP.CloudGames.Order.API.Application.Queries
{
    public interface IVoucherQueries
    {
        Task<VoucherDTO> GetVoucherByCode(string code);
    }

    public class VoucherQueries : IVoucherQueries
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherQueries(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<VoucherDTO> GetVoucherByCode(string code)
        {
            var voucher = await _voucherRepository.GetVoucherByCode(code);

            if (voucher == null) return null;

            if (!voucher.IsValidForUse()) return null;

            return new VoucherDTO
            {
                Code = voucher.Code,
                DiscountType = (int)voucher.DiscountType,
                Percentage = voucher.Percentage,
                DiscountValue = voucher.DiscountValue
            };
        }
    }
}