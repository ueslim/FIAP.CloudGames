using System;
using FIAP.CloudGames.Catalog.API.Models;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Catalog.Tests.Domain
{
    public class ProductTests
    {
        private static Product MakeProduct(bool active = true, int stock = 10, decimal value = 100m)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = "Game A",
                Description = "Desc",
                Active = active,
                Value = value,
                DateRegister = DateTime.UtcNow,
                Image = "img.png",
                StockQuantity = stock
            };
        }

        [Fact]
        public void DecrementStock_Should_Decrease_When_Sufficient()
        {
            var p = MakeProduct(stock: 5);
            p.DecrementStock(3);
            p.StockQuantity.Should().Be(2);
        }

        [Fact]
        public void DecrementStock_Should_Not_Change_When_Insufficient()
        {
            var p = MakeProduct(stock: 2);
            p.DecrementStock(3);
            p.StockQuantity.Should().Be(2);
        }

        [Theory]
        [InlineData(true, 5, 5, true)]
        [InlineData(true, 5, 6, false)]
        [InlineData(false, 5, 1, false)]
        public void IsAvailable_Should_Respect_Active_And_Stock(bool active, int stock, int ask, bool expected)
        {
            var p = MakeProduct(active: active, stock: stock);
            p.IsAvailable(ask).Should().Be(expected);
        }
    }
}
