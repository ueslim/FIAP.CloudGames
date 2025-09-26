using NetDevPack.Specification;
using System.Linq.Expressions;

namespace FIAP.CloudGames.Order.Domain.Voucher.Specs
{
    public class VoucherDateSpecification : Specification<Voucher>
    {
        public override Expression<Func<Voucher, bool>> ToExpression()
        {
            return voucher => voucher.ExpirationDate >= DateTime.Now;
        }
    }

    public class VoucherQuantitySpecification : Specification<Voucher>
    {
        public override Expression<Func<Voucher, bool>> ToExpression()
        {
            return voucher => voucher.Quantity > 0;
        }
    }

    public class VoucherActiveSpecification : Specification<Voucher>
    {
        public override Expression<Func<Voucher, bool>> ToExpression()
        {
            return voucher => voucher.Active && !voucher.Used;
        }
    }
}