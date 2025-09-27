using FIAP.CloudGames.Cart.API.Model;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Cart.Tests.Domain
{
    public class CartCustomerTests
    {
        private static CartItem MakeItem(Guid? productId = null, string name = "Game A", int qty = 1, decimal value = 10m)
        {
            return new CartItem
            {
                ProductId = productId ?? Guid.NewGuid(),
                Name = name,
                Quantity = qty,
                Value = value,
                Image = "img.png"
            };
        }

        [Fact]
        public void CalculateCartValue_Should_Sum_Without_Voucher()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            var i1 = MakeItem(qty: 2, value: 10m); // 20
            var i2 = MakeItem(qty: 1, value: 15m); // 15
            cart.AddItem(i1);
            cart.AddItem(i2);

            // Act
            // already calculated in AddItem, but call again via noop change
            cart.CalculateCartValue();

            // Assert
            cart.UsedVoucher.Should().BeFalse();
            cart.TotalValue.Should().Be(35m);
            cart.Discount.Should().Be(0m);
        }

        [Fact]
        public void ApplyVoucher_Percentage_Should_Discount_Total_And_Set_UsedVoucher()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            cart.AddItem(MakeItem(qty: 2, value: 50m)); // total = 100

            var voucher = new Voucher
            {
                VoucherDiscountType = VoucherDiscountType.Percentage,
                Percentage = 10m
            };

            // Act
            cart.ApplyVoucher(voucher);

            // Assert
            cart.UsedVoucher.Should().BeTrue();
            cart.Discount.Should().Be(10m);
            cart.TotalValue.Should().Be(90m);
        }

        [Fact]
        public void ApplyVoucher_Percentage_With_Null_Percentage_Should_Not_Change_Total_But_Mark_Used()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            cart.AddItem(MakeItem(qty: 1, value: 80m)); // total = 80

            var voucher = new Voucher
            {
                VoucherDiscountType = VoucherDiscountType.Percentage,
                Percentage = null
            };

            // Act
            cart.ApplyVoucher(voucher);

            // Assert
            cart.UsedVoucher.Should().BeTrue();
            cart.Discount.Should().Be(0m);
            cart.TotalValue.Should().Be(80m);
        }

        [Fact]
        public void ApplyVoucher_Value_Should_Subtract_Fixed_Amount()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            cart.AddItem(MakeItem(qty: 3, value: 20m)); // total = 60

            var voucher = new Voucher
            {
                VoucherDiscountType = VoucherDiscountType.Value,
                DiscountValue = 15m
            };

            // Act
            cart.ApplyVoucher(voucher);

            // Assert
            cart.Discount.Should().Be(15m);
            cart.TotalValue.Should().Be(45m);
        }

        [Fact]
        public void ApplyVoucher_Value_With_Null_Discount_Should_Not_Change_Total_But_Mark_Used()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            cart.AddItem(MakeItem(qty: 2, value: 30m)); // total = 60

            var voucher = new Voucher
            {
                VoucherDiscountType = VoucherDiscountType.Value,
                DiscountValue = null
            };

            // Act
            cart.ApplyVoucher(voucher);

            // Assert
            cart.UsedVoucher.Should().BeTrue();
            cart.Discount.Should().Be(0m);
            cart.TotalValue.Should().Be(60m);
        }

        [Fact]
        public void ApplyVoucher_Should_Clamp_Total_To_Zero_When_Discount_Exceeds_Total()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            cart.AddItem(MakeItem(qty: 1, value: 40m)); // total = 40

            var voucher = new Voucher
            {
                VoucherDiscountType = VoucherDiscountType.Value,
                DiscountValue = 100m
            };

            // Act
            cart.ApplyVoucher(voucher);

            // Assert
            cart.Discount.Should().Be(100m);
            cart.TotalValue.Should().Be(0m);
        }

        [Fact]
        public void AddItem_Should_Add_New_Item_And_Associate_Cart()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            var item = MakeItem(qty: 2, value: 25m);

            // Act
            cart.AddItem(item);

            // Assert
            cart.Items.Should().ContainSingle(i => i.ProductId == item.ProductId);
            item.CartId.Should().Be(cart.Id);
            cart.TotalValue.Should().Be(50m);
        }

        [Fact]
        public void AddItem_Should_Merge_Quantity_When_Product_Already_Exists()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            var pid = Guid.NewGuid();

            var first = MakeItem(productId: pid, qty: 2, value: 10m); // 20
            var second = MakeItem(productId: pid, qty: 3, value: 10m); // merges → qty 5

            // Act
            cart.AddItem(first);
            cart.AddItem(second);

            // Assert
            cart.Items.Should().ContainSingle(); // merged
            var merged = cart.Items.Single();
            merged.Quantity.Should().Be(5);
            cart.TotalValue.Should().Be(50m);
        }

        [Fact]
        public void UpdateItem_Should_Replace_Existing_Item_And_Recalculate_Total()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            var pid = Guid.NewGuid();
            var oldItem = MakeItem(productId: pid, qty: 1, value: 30m); // 30
            cart.AddItem(oldItem);

            var newItem = MakeItem(productId: pid, qty: 2, value: 40m); // 80

            // Act
            cart.UpdateItem(newItem);

            // Assert
            cart.Items.Should().ContainSingle(i => i.ProductId == pid && i.Quantity == 2 && i.Value == 40m);
            newItem.CartId.Should().Be(cart.Id);
            cart.TotalValue.Should().Be(80m);
        }

        [Fact]
        public void UpdateUnits_Should_Set_Quantity_And_Recalculate()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            var item = MakeItem(qty: 1, value: 50m);
            cart.AddItem(item);

            // Act
            cart.UpdateUnits(item, 4);

            // Assert
            cart.Items.Single().Quantity.Should().Be(4);
            cart.TotalValue.Should().Be(200m);
        }

        [Fact]
        public void RemoveItem_Should_Remove_And_Recalculate()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            var a = MakeItem(qty: 1, value: 20m); // 20
            var b = MakeItem(qty: 2, value: 15m); // 30
            cart.AddItem(a);
            cart.AddItem(b);
            cart.TotalValue.Should().Be(50m);

            // Act
            cart.RemoveItem(b);

            // Assert
            cart.Items.Should().ContainSingle(i => i.ProductId == a.ProductId);
            cart.TotalValue.Should().Be(20m);
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Cart_Rules_Fail()
        {
            // Arrange: CustomerId empty + no items + TotalValue 0
            var cart = new CartCustomer(Guid.Empty);

            // Act
            var valid = cart.IsValid();

            // Assert
            valid.Should().BeFalse();
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == "Unrecognized customer");
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == "Cart has no items");
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == "The total value of the cart must be greater than 0");
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Item_Rules_Fail()
        {
            // Arrange: make an invalid item (qty 0, price 0, empty name, empty productId)
            var cart = new CartCustomer(Guid.NewGuid());
            var bad = new CartItem
            {
                ProductId = Guid.Empty,
                Name = "",
                Quantity = 0,
                Value = 0m
            };
            cart.AddItem(bad); // TotalValue = 0 triggers cart rule, so set something else to satisfy cart rules
            var good = MakeItem(qty: 1, value: 100m);
            cart.AddItem(good); // now TotalValue > 0 and has items

            // Act
            var valid = cart.IsValid();

            // Assert
            valid.Should().BeFalse();
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == "Invalid product id");
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == "The product name was not informed");
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == $"The minimum quantity for  is 1");
            cart.ValidationResult.Errors.Should().Contain(e => e.ErrorMessage == $"The value of  must be greater than 0");
        }

        [Fact]
        public void IsValid_Should_Return_True_When_Cart_And_Items_Are_Valid()
        {
            // Arrange
            var cart = new CartCustomer(Guid.NewGuid());
            cart.AddItem(MakeItem(qty: 2, value: 25m)); // 50
            cart.AddItem(MakeItem(qty: 1, value: 10m)); // 10

            // Act
            var valid = cart.IsValid();

            // Assert
            valid.Should().BeTrue();
            cart.ValidationResult.IsValid.Should().BeTrue();
            cart.ValidationResult.Errors.Should().BeEmpty();
        }

        [Fact]
        public void CartItemValidation_Messages_Should_Be_As_Expected()
        {
            // Arrange
            var validator = new CartCustomer.CartItemValidation();
            var cart = new CartCustomer(Guid.Empty); // invalid id
            // no items, total 0

            // Act
            var result = validator.Validate(cart);

            // Assert
            result.Errors.Select(e => e.ErrorMessage).Should().Contain(new[]
            {
                "Unrecognized customer",
                "Cart has no items",
                "The total value of the cart must be greater than 0"
            });
        }
    }
}