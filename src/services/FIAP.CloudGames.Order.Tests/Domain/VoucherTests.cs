using System;
using FIAP.CloudGames.Order.Domain.Voucher;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Domain
{
    public class VoucherTests
    {
        private static Voucher NewVoucher(int qty = 5, bool active = true, bool used = false, DateTime? exp = null)
        {
            var v = (Voucher)Activator.CreateInstance(typeof(Voucher), nonPublic: true)!;
            typeof(Voucher).GetProperty("Code")!.SetValue(v, "CODE");
            typeof(Voucher).GetProperty("Quantity")!.SetValue(v, qty);
            typeof(Voucher).GetProperty("Active")!.SetValue(v, active);
            typeof(Voucher).GetProperty("Used")!.SetValue(v, used);
            typeof(Voucher).GetProperty("CreatedAt")!.SetValue(v, DateTime.UtcNow);
            typeof(Voucher).GetProperty("ExpirationDate")!.SetValue(v, exp ?? DateTime.UtcNow.AddDays(1));
            typeof(Voucher).GetProperty("DiscountType")!.SetValue(v, VoucherDiscountType.Value);
            typeof(Voucher).GetProperty("DiscountValue")!.SetValue(v, 10m);
            return v;
        }

        [Fact]
        public void IsValidForUse_Should_Respect_Specifications()
        {
            NewVoucher().IsValidForUse().Should().BeTrue();
            NewVoucher(qty: 0).IsValidForUse().Should().BeFalse();
            NewVoucher(active: false).IsValidForUse().Should().BeFalse();
            NewVoucher(used: true).IsValidForUse().Should().BeFalse();
            NewVoucher(exp: DateTime.UtcNow.AddHours(-1)).IsValidForUse().Should().BeFalse();
        }

        [Fact]
        public void MarkAsUsed_Should_Set_Flags_And_Zero_Quantity()
        {
            var v = NewVoucher();
            v.MarkAsUsed();
            v.Active.Should().BeFalse();
            v.Used.Should().BeTrue();
            v.Quantity.Should().Be(0);
            v.UsedAt.Should().NotBeNull();
        }

        [Fact]
        public void DebitQuantity_Should_Decrease_And_Mark_When_Reaches_Zero()
        {
            var v = NewVoucher(qty: 1);
            v.DebitQuantity();
            v.Quantity.Should().Be(0);
            v.Used.Should().BeTrue();
            v.Active.Should().BeFalse();
        }
    }
}
