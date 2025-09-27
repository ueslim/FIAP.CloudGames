using System;
using FIAP.CloudGames.Order.Domain.Order;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Domain
{
    public class OrderItemTests
    {
        [Fact]
        public void CalculateValue_Should_Multiply_Quantity_By_UnitValue()
        {
            var item = new OrderItem(Guid.NewGuid(), "Game", 3, 19.90m, "img");
            item.GetType().GetMethod("CalculateValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(item, null).Should().Be(59.70m);
        }
    }
}
