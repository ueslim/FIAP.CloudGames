using System;
using System.Linq;
using FIAP.CloudGames.Cart.API.Model;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Cart.Tests.Domain
{
    public class CartItemTests
    {
        private static CartItem MakeItem(
            Guid? productId = null,
            string name = "Game A",
            int qty = 1,
            decimal value = 10m)
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
        public void AssociateCart_Should_Set_CartId()
        {
            // Arrange
            var item = MakeItem();
            var cartId = Guid.NewGuid();

            // Act
            item.AssociateCart(cartId);

            // Assert
            item.CartId.Should().Be(cartId);
        }

        [Fact]
        public void CalculateValue_Should_Return_Quantity_Times_Value()
        {
            // Arrange
            var item = MakeItem(qty: 3, value: 19.90m);

            // Act
            var total = item.CalculateValue();

            // Assert
            total.Should().Be(59.70m);
        }

        [Fact]
        public void AddUnits_Should_Increment_Quantity()
        {
            // Arrange
            var item = MakeItem(qty: 2);

            // Act
            item.AddUnits(3);

            // Assert
            item.Quantity.Should().Be(5);
        }

        [Fact]
        public void UpdateUnits_Should_Set_Quantity()
        {
            // Arrange
            var item = MakeItem(qty: 2);

            // Act
            item.UpdateUnits(7);

            // Assert
            item.Quantity.Should().Be(7);
        }

        [Fact]
        public void IsValid_Should_Return_True_For_Valid_Item()
        {
            // Arrange
            var item = MakeItem(qty: 2, value: 50m);

            // Act
            var result = item.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_Should_Return_False_When_ProductId_Is_Empty()
        {
            // Arrange
            var item = MakeItem(productId: Guid.Empty);

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid product id");
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Name_Is_Empty()
        {
            // Arrange
            var item = MakeItem(name: "");

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "The product name was not informed");
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Quantity_Is_Less_Than_One_With_Message_Containing_Name()
        {
            // Arrange
            var item = MakeItem(name: "Game X", qty: 0);

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == $"The minimum quantity for {item.Name} is 1");
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Quantity_Exceeds_Max_With_Message_Containing_Name_And_Limit()
        {
            // Arrange
            var item = MakeItem(name: "Game Y",
                                qty: CartCustomer.MAX_QUANTITY_ITEM + 1);

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage == $"The maximum quantity of {item.Name} is {CartCustomer.MAX_QUANTITY_ITEM}");
        }

        [Fact]
        public void IsValid_Should_Pass_When_Quantity_Equals_Max()
        {
            // Arrange
            var item = MakeItem(qty: CartCustomer.MAX_QUANTITY_ITEM);

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Value_Is_Not_Greater_Than_Zero_With_Message_Containing_Name()
        {
            // Arrange
            var item = MakeItem(name: "Game Z", value: 0m);

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage == $"The value of {item.Name} must be greater than 0");
        }

        [Fact]
        public void Validator_Should_Return_All_Relevant_Errors_When_Multiple_Rules_Fail()
        {
            // Arrange: empty name, qty 0, value 0, productId empty
            var item = new CartItem
            {
                ProductId = Guid.Empty,
                Name = "",
                Quantity = 0,
                Value = 0m
            };

            // Act
            var result = new CartItem.CartItemValidation().Validate(item);

            // Assert
            result.IsValid.Should().BeFalse();

            var messages = result.Errors.Select(e => e.ErrorMessage).ToArray();
            messages.Should().Contain("Invalid product id");
            messages.Should().Contain("The product name was not informed");
            messages.Should().Contain("The minimum quantity for  is 1");
            messages.Should().Contain("The value of  must be greater than 0");
        }
    }
}
