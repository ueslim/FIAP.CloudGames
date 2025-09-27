using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Application.Events;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Application
{
    public class OrderEventHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Publish_OrderPlacedIntegrationEvent()
        {
            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            bus.Setup(b => b.PublishAsync(It.IsAny<OrderPlacedIntegrationEvent>())).Returns(Task.CompletedTask);

            var handler = new OrderEventHandler(bus.Object);

            await handler.Handle(new OrderPlacedEvent(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            bus.Verify(b => b.PublishAsync(It.IsAny<OrderPlacedIntegrationEvent>()), Times.Once);
        }
    }
}
