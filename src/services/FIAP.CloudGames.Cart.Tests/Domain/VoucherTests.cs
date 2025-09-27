using System;
using FIAP.CloudGames.Cart.API.Model;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Cart.Tests.Domain
{
    public class VoucherTests
    {
        [Fact]
        public void Voucher_Should_Allow_Setting_Percentage_Discount()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "PROMO10",
                VoucherDiscountType = VoucherDiscountType.Percentage,
                Percentage = 10m,
                DiscountValue = null
            };

            // Act & Assert
            voucher.Code.Should().Be("PROMO10");
            voucher.VoucherDiscountType.Should().Be(VoucherDiscountType.Percentage);
            voucher.Percentage.Should().Be(10m);
            voucher.DiscountValue.Should().BeNull();
        }

        [Fact]
        public void Voucher_Should_Allow_Setting_Value_Discount()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "OFF50",
                VoucherDiscountType = VoucherDiscountType.Value,
                DiscountValue = 50m,
                Percentage = null
            };

            // Act & Assert
            voucher.Code.Should().Be("OFF50");
            voucher.VoucherDiscountType.Should().Be(VoucherDiscountType.Value);
            voucher.DiscountValue.Should().Be(50m);
            voucher.Percentage.Should().BeNull();
        }

        [Fact]
        public void Voucher_Should_Allow_Null_Percentage_And_Value()
        {
            // Arrange
            var voucher = new Voucher
            {
                Code = "NULLCASE",
                VoucherDiscountType = VoucherDiscountType.Percentage,
                Percentage = null,
                DiscountValue = null
            };

            // Act & Assert
            voucher.Percentage.Should().BeNull();
            voucher.DiscountValue.Should().BeNull();
        }

        [Fact]
        public void VoucherDiscountType_Should_Have_Correct_Enum_Values()
        {
            // Act & Assert
            ((int)VoucherDiscountType.Percentage).Should().Be(0);
            ((int)VoucherDiscountType.Value).Should().Be(1);
        }

        [Fact]
        public void Voucher_Should_Be_Instantiable_With_Default_Values()
        {
            // Act
            var voucher = new Voucher();

            // Assert
            voucher.Code.Should().BeNull();
            voucher.Percentage.Should().BeNull();
            voucher.DiscountValue.Should().BeNull();
            voucher.VoucherDiscountType.Should().BeNull();
        }
    }
}
