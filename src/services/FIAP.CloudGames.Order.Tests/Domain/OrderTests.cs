using System;
using System.Collections.Generic;
using FIAP.CloudGames.Order.Domain.Order;
using FIAP.CloudGames.Order.Domain.Voucher;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Domain
{
    public class OrderTests
    {
        private static Order.Domain.Order.Order MakeOrder(decimal total, IEnumerable<OrderItem> items)
            => new Order.Domain.Order.Order(Guid.NewGuid(), total, new List<OrderItem>(items));

        [Fact]
        public void Status_Transitions_Should_Set_Expected_Values()
        {
            var o = MakeOrder(10m, new[] { new OrderItem(Guid.NewGuid(), "A", 1, 10m) });

            o.AuthorizeOrder();
            o.OrderStatus.Should().Be(OrderStatus.Authorized);

            o.FinishOrder();
            o.OrderStatus.Should().Be(OrderStatus.Paid);

            o.CancelOrder();
            o.OrderStatus.Should().Be(OrderStatus.Canceled);
        }

        [Fact]
        public void AssignAddress_Should_Set_Address()
        {
            var o = MakeOrder(10m, new[] { new OrderItem(Guid.NewGuid(), "A", 1, 10m) });
            var addr = new Address { Street = "Main", Number = "123" };
            o.AssignAddress(addr);
            o.Address.Should().NotBeNull();
            o.Address.Street.Should().Be("Main");
            o.Address.Number.Should().Be("123");
        }

        [Fact]
        public void CalculateOrderValue_Should_Sum_Items()
        {
            var p1 = new OrderItem(Guid.NewGuid(), "A", 2, 5m);
            var p2 = new OrderItem(Guid.NewGuid(), "B", 1, 7m);
            var o = MakeOrder(999m, new[] { p1, p2 });

            o.CalculateOrderValue();

            o.TotalValue.Should().Be(17m);
        }

        [Fact]
        public void CalculateTotalDiscountValue_Should_Apply_Percentage_Discount()
        {
            var p = new OrderItem(Guid.NewGuid(), "A", 2, 50m);
            var o = MakeOrder(100m, new[] { p });

            var voucher = new VoucherBuilder().Percent(10m).Build();
            o.AssignVoucher(voucher);
            o.CalculateOrderValue();

            o.Discount.Should().Be(10m);
            o.TotalValue.Should().Be(90m);
        }

        [Fact]
        public void CalculateTotalDiscountValue_Should_Apply_Value_Discount_And_Clamp_To_Zero()
        {
            var p = new OrderItem(Guid.NewGuid(), "A", 1, 30m);
            var o = MakeOrder(30m, new[] { p });

            var voucher = new VoucherBuilder().Value(50m).Build();
            o.AssignVoucher(voucher);
            o.CalculateOrderValue();

            o.Discount.Should().Be(50m);
            o.TotalValue.Should().Be(0m);
        }

        // simple builder to create vouchers with public properties via reflection since the entity has private setters
        private class VoucherBuilder
        {
            private string _code = "OFF";
            private decimal? _pct;
            private decimal? _val;
            private int _qty = 10;
            private VoucherDiscountType _type = VoucherDiscountType.Percent;
            private DateTime _exp = DateTime.UtcNow.AddDays(1);
            public VoucherBuilder Percent(decimal p) { _pct = p; _val = null; _type = VoucherDiscountType.Percent; return this; }
            public VoucherBuilder Value(decimal v) { _val = v; _pct = null; _type = VoucherDiscountType.Value; return this; }
            public Voucher Build()
            {
                var v = (Voucher)Activator.CreateInstance(typeof(Voucher), nonPublic: true)!;
                typeof(Voucher).GetProperty("Code")!.SetValue(v, _code);
                typeof(Voucher).GetProperty("Percentage")!.SetValue(v, _pct);
                typeof(Voucher).GetProperty("DiscountValue")!.SetValue(v, _val);
                typeof(Voucher).GetProperty("Quantity")!.SetValue(v, _qty);
                typeof(Voucher).GetProperty("DiscountType")!.SetValue(v, _type);
                typeof(Voucher).GetProperty("CreatedAt")!.SetValue(v, DateTime.UtcNow);
                typeof(Voucher).GetProperty("ExpirationDate")!.SetValue(v, _exp);
                typeof(Voucher).GetProperty("Active")!.SetValue(v, true);
                typeof(Voucher).GetProperty("Used")!.SetValue(v, false);
                return v;
            }
        }
    }
}
