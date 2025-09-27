using FIAP.CloudGames.Core.Data;
using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Order.API.Services;
using FIAP.CloudGames.Order.Domain.Order;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FIAP.CloudGames.Order.Tests.Services
{
    public class OrderIntegrationHandlerTests
    {
        private static Order.Domain.Order.Order MakeOrder() => new Order.Domain.Order.Order(Guid.NewGuid(), 10m, new() { new OrderItem(Guid.NewGuid(), "A", 1, 10m) });

        [Fact]
        public async Task ExecuteAsync_Should_Subscribe_To_Canceled_And_Paid()
        {
            var repo = new Mock<IOrderRepository>(MockBehavior.Strict);
            repo.Setup(r => r.Dispose());
            var provider = new ServiceCollectionHelper().WithScoped(repo.Object).Provider;

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            string t1 = null!, t2 = null!;
            Func<OrderCanceledIntegrationEvent, Task> h1 = null!;
            Func<OrderPaidIntegrationEvent, Task> h2 = null!;
            bus.Setup(b => b.SubscribeAsync<OrderCanceledIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderCanceledIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderCanceledIntegrationEvent, Task>>((t, h) => { t1 = t; h1 = h; });
            bus.Setup(b => b.SubscribeAsync<OrderPaidIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderPaidIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderPaidIntegrationEvent, Task>>((t, h) => { t2 = t; h2 = h; });

            var sut = new OrderIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None); await sut.StopAsync(CancellationToken.None);

            t1.Should().Be("OrderCanceled"); h1.Should().NotBeNull();
            t2.Should().Be("OrderPaid"); h2.Should().NotBeNull();
        }

        [Fact]
        public async Task CancelOrder_Should_Update_Status_And_Commit()
        {
            var order = MakeOrder();
            var repo = new Mock<IOrderRepository>(MockBehavior.Strict);
            repo.Setup(r => r.Dispose());
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
            repo.Setup(r => r.Update(order));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);
            var provider = new ServiceCollectionHelper().WithScoped(repo.Object).Provider;

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            Func<OrderCanceledIntegrationEvent, Task> handler = null!;
            bus.Setup(b => b.SubscribeAsync<OrderCanceledIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderCanceledIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderCanceledIntegrationEvent, Task>>((_, h) => handler = h);
            bus.Setup(b => b.SubscribeAsync<OrderPaidIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderPaidIntegrationEvent, Task>>()));

            var sut = new OrderIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None);

            await handler(new OrderCanceledIntegrationEvent(Guid.NewGuid(), Guid.NewGuid()));

            repo.Verify(r => r.Update(It.Is<Order.Domain.Order.Order>(o => o.OrderStatus == OrderStatus.Canceled)), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task FinishOrder_Should_Update_Status_And_Commit()
        {
            var order = MakeOrder();
            var repo = new Mock<IOrderRepository>(MockBehavior.Strict);
            repo.Setup(r => r.Dispose());
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
            repo.Setup(r => r.Update(order));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);
            var provider = new ServiceCollectionHelper().WithScoped(repo.Object).Provider;

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            Func<OrderPaidIntegrationEvent, Task> handler = null!;
            bus.Setup(b => b.SubscribeAsync<OrderCanceledIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderCanceledIntegrationEvent, Task>>()));
            bus.Setup(b => b.SubscribeAsync<OrderPaidIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderPaidIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderPaidIntegrationEvent, Task>>((_, h) => handler = h);

            var sut = new OrderIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None);

            await handler(new OrderPaidIntegrationEvent(Guid.NewGuid(), Guid.NewGuid()));

            repo.Verify(r => r.Update(It.Is<Order.Domain.Order.Order>(o => o.OrderStatus == OrderStatus.Paid)), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task CancelOrder_Should_Throw_When_Commit_Fails()
        {
            var order = MakeOrder();
            var repo = new Mock<IOrderRepository>(MockBehavior.Strict);
            repo.Setup(r => r.Dispose());
            repo.Setup(r => r.GetById(It.IsAny<Guid>())).ReturnsAsync(order);
            repo.Setup(r => r.Update(order));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(false);
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);
            var provider = new ServiceCollectionHelper().WithScoped(repo.Object).Provider;

            var bus = new Mock<IMessageBus>(MockBehavior.Strict);
            Func<OrderCanceledIntegrationEvent, Task> handler = null!;
            bus.Setup(b => b.SubscribeAsync<OrderCanceledIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderCanceledIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderCanceledIntegrationEvent, Task>>((_, h) => handler = h);
            bus.Setup(b => b.SubscribeAsync<OrderPaidIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderPaidIntegrationEvent, Task>>()));

            var sut = new OrderIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None);

            var act = async () => await handler(new OrderCanceledIntegrationEvent(Guid.NewGuid(), Guid.NewGuid()));
            await act.Should().ThrowAsync<DomainException>().WithMessage("*cancelar o pedido*");
        }
    }

    internal sealed class ServiceCollectionHelper
    {
        private readonly Microsoft.Extensions.DependencyInjection.ServiceCollection _services = new();
        public ServiceCollectionHelper WithScoped<T>(T instance) where T : class { _services.AddScoped(_ => instance); return this; }
        public IServiceProvider Provider => _services.BuildServiceProvider(true);
    }
}
