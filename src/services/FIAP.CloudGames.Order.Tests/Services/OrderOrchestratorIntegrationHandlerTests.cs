using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Application.DTO;
using FIAP.CloudGames.Order.API.Application.Queries;
using FIAP.CloudGames.Order.API.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Services
{
    public class OrderOrchestratorIntegrationHandlerTests
    {
        [Fact]
        public async Task ProcessOrders_Should_Publish_When_Authorized_Order_Exists()
        {
            var order = new OrderDTO
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = Guid.NewGuid(), Quantity = 2 } }
            };

            var queries = new Mock<IOrderQueries>(MockBehavior.Strict);
            queries.Setup(q => q.GetAuthorizedOrders()).ReturnsAsync(order);

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            bus.Setup(b => b.PublishAsync(It.IsAny<OrderAuthorizedIntegrationEvent>())).Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<OrderOrchestratorIntegrationHandler>>();
            var provider = new ServiceCollection()
                .AddScoped(_ => queries.Object)
                .AddScoped(_ => bus.Object)
                .BuildServiceProvider(true);

            var sut = new OrderOrchestratorIntegrationHandler(logger.Object, provider);

            // chama o método privado ProcessOrders por reflexão para evitar esperar o Timer
            var mi = typeof(OrderOrchestratorIntegrationHandler).GetMethod("ProcessOrders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            mi.Invoke(sut, new object?[] { null });

            bus.Verify(b => b.PublishAsync(It.IsAny<OrderAuthorizedIntegrationEvent>()), Times.Once);
        }

        [Fact]
        public void ProcessOrders_Should_Do_Nothing_When_No_Order()
        {
            var queries = new Mock<IOrderQueries>(MockBehavior.Strict);
            queries.Setup(q => q.GetAuthorizedOrders()).ReturnsAsync((OrderDTO)null);

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);

            var logger = new Mock<ILogger<OrderOrchestratorIntegrationHandler>>();
            var provider = new ServiceCollection()
                .AddScoped(_ => queries.Object)
                .AddScoped(_ => bus.Object)
                .BuildServiceProvider(true);

            var sut = new OrderOrchestratorIntegrationHandler(logger.Object, provider);

            var mi = typeof(OrderOrchestratorIntegrationHandler).GetMethod("ProcessOrders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            mi.Invoke(sut, new object?[] { null });

            bus.Verify(b => b.PublishAsync(It.IsAny<OrderAuthorizedIntegrationEvent>()), Times.Never);
        }
    }
}
