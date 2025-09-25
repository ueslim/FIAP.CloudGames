using FIAP.CloudGames.Core.Data;

namespace FIAP.CloudGames.Order.Domain.Voucher
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<Voucher> GetVoucherByCode(string code);

        void Update(Voucher voucher);
    }
}