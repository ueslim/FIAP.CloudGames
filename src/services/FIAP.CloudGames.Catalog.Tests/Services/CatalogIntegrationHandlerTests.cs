using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FIAP.CloudGames.Catalog.API.Models;
using FIAP.CloudGames.Catalog.API.Services;
using FIAP.CloudGames.Core.Data;
using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Catalog.Tests.Services
{
    public class CatalogIntegrationHandlerTests
    {
        private static ServiceProvider BuildProvider(IProductRepository repo)
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => repo);
            return services.BuildServiceProvider(validateScopes: true);
        }

        private static Product P(Guid id, bool active = true, int stock = 10) =>
            new Product
            {
                Id = id,
                Name = $"P-{id.ToString()[..8]}",
                Description = "desc",
                Active = active,
                Value = 10m,
                DateRegister = DateTime.UtcNow,
                Image = "img.png",
                StockQuantity = stock
            };

        private static OrderAuthorizedIntegrationEvent MakeEvent(Dictionary<Guid, int> items, Guid? orderId = null, Guid? customerId = null)
        {
            var t = typeof(OrderAuthorizedIntegrationEvent);
            var publicCtor = t.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 3);
            if (publicCtor != null)
            {
                try { return (OrderAuthorizedIntegrationEvent)publicCtor.Invoke(new object[] { orderId ?? Guid.NewGuid(), customerId ?? Guid.NewGuid(), items }); }
                catch { }
            }
            var evt = Activator.CreateInstance(t, nonPublic: true)!;
            t.GetProperty("OrderId")?.SetValue(evt, orderId ?? Guid.NewGuid());
            t.GetProperty("CustomerId")?.SetValue(evt, customerId ?? Guid.NewGuid());
            t.GetProperty("Items")?.SetValue(evt, items);
            return (OrderAuthorizedIntegrationEvent)evt;
        }

        [Fact]
        public async Task ExecuteAsync_Should_Subscribe_To_OrderAuthorized_Topic()
        {
            var repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.Dispose());
            var provider = BuildProvider(repoMock.Object);

            var busMock = new Mock<IMessageBus>();
            string topic = null!;
            Func<OrderAuthorizedIntegrationEvent, Task> handler = null!;
            busMock.Setup(b => b.SubscribeAsync<OrderAuthorizedIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderAuthorizedIntegrationEvent, Task>>()))
                   .Callback<string, Func<OrderAuthorizedIntegrationEvent, Task>>((t, h) => { topic = t; handler = h; });

            var sut = new CatalogIntegrationHandler(provider, busMock.Object);

            await sut.StartAsync(CancellationToken.None);
            await sut.StopAsync(CancellationToken.None);

            busMock.Verify(b => b.SubscribeAsync<OrderAuthorizedIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderAuthorizedIntegrationEvent, Task>>()), Times.Once);
            topic.Should().Be("OrderAuthorized");
            handler.Should().NotBeNull();
        }

        [Fact]
        public async Task DeductStock_Should_Update_All_Commit_And_Publish()
        {
            var p1 = Guid.NewGuid();
            var p2 = Guid.NewGuid();
            var items = new Dictionary<Guid, int> { { p1, 2 }, { p2, 3 } };

            var repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.Dispose());
            repoMock.Setup(r => r.GetProductsById($"{p1},{p2}"))
                    .ReturnsAsync(new List<Product> { P(p1, stock: 5), P(p2, stock: 10) });
            repoMock.Setup(r => r.Update(It.IsAny<Product>()));
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Commit()).ReturnsAsync(true);
            repoMock.SetupGet(r => r.UnitOfWork).Returns(uowMock.Object);

            var provider = BuildProvider(repoMock.Object);

            var busMock = new Mock<IMessageBus>(MockBehavior.Strict);
            string topic = null!;
            Func<OrderAuthorizedIntegrationEvent, Task> handler = null!;

            busMock.Setup(b => b.SubscribeAsync<OrderAuthorizedIntegrationEvent>(
                            It.IsAny<string>(),
                            It.IsAny<Func<OrderAuthorizedIntegrationEvent, Task>>()))
                   .Callback<string, Func<OrderAuthorizedIntegrationEvent, Task>>((t, h) => { topic = t; handler = h; });

            busMock.Setup(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()))
                   .Returns(Task.CompletedTask);

            var sut = new CatalogIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            await handler(MakeEvent(items));

            repoMock.Verify(r => r.Update(It.Is<Product>(x => x.Id == p1 && x.StockQuantity == 3)), Times.Once);
            repoMock.Verify(r => r.Update(It.Is<Product>(x => x.Id == p2 && x.StockQuantity == 7)), Times.Once);
            uowMock.Verify(u => u.Commit(), Times.Once);
            busMock.Verify(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()), Times.Once);
        }


        [Fact]
        public async Task DeductStock_Should_Cancel_When_Not_All_Products_Found()
        {
            var p1 = Guid.NewGuid();
            var p2 = Guid.NewGuid();
            var items = new Dictionary<Guid, int> { { p1, 1 }, { p2, 1 } };

            var repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.Dispose());
            repoMock.Setup(r => r.GetProductsById($"{p1},{p2}")).ReturnsAsync(new List<Product> { P(p1) });

            var provider = BuildProvider(repoMock.Object);

            var busMock = new Mock<IMessageBus>(MockBehavior.Strict);
            string topic = null!;
            Func<OrderAuthorizedIntegrationEvent, Task> handler = null!;
            busMock.Setup(b => b.SubscribeAsync<OrderAuthorizedIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderAuthorizedIntegrationEvent, Task>>()))
                   .Callback<string, Func<OrderAuthorizedIntegrationEvent, Task>>((t, h) => { topic = t; handler = h; });
            busMock.Setup(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()));

            var sut = new CatalogIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            await handler(MakeEvent(items));

            busMock.Verify(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()), Times.Once);
            repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            repoMock.VerifyGet(r => r.UnitOfWork, Times.Never);
        }

        [Fact]
        public async Task DeductStock_Should_Cancel_When_Any_Product_Has_Insufficient_Stock()
        {
            var p1 = Guid.NewGuid();
            var p2 = Guid.NewGuid();
            var items = new Dictionary<Guid, int> { { p1, 2 }, { p2, 9 } };

            var repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.Dispose());
            repoMock.Setup(r => r.GetProductsById($"{p1},{p2}")).ReturnsAsync(new List<Product> { P(p1, stock: 5), P(p2, stock: 5) });

            var provider = BuildProvider(repoMock.Object);

            var busMock = new Mock<IMessageBus>(MockBehavior.Strict);
            string topic = null!;
            Func<OrderAuthorizedIntegrationEvent, Task> handler = null!;
            busMock.Setup(b => b.SubscribeAsync<OrderAuthorizedIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderAuthorizedIntegrationEvent, Task>>()))
                   .Callback<string, Func<OrderAuthorizedIntegrationEvent, Task>>((t, h) => { topic = t; handler = h; });
            busMock.Setup(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()));

            var sut = new CatalogIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            await handler(MakeEvent(items));

            busMock.Verify(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()), Times.Once);
            repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            repoMock.VerifyGet(r => r.UnitOfWork, Times.Never);
        }

        [Fact]
        public async Task DeductStock_Should_Throw_When_Commit_Fails_And_Not_Publish()
        {
            var p1 = Guid.NewGuid();
            var items = new Dictionary<Guid, int> { { p1, 1 } };

            var repoMock = new Mock<IProductRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.Dispose());
            repoMock.Setup(r => r.GetProductsById($"{p1}")).ReturnsAsync(new List<Product> { P(p1, stock: 2) });
            repoMock.Setup(r => r.Update(It.IsAny<Product>()));

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Commit()).ReturnsAsync(false);
            repoMock.SetupGet(r => r.UnitOfWork).Returns(uowMock.Object);

            var provider = BuildProvider(repoMock.Object);

            var busMock = new Mock<IMessageBus>(MockBehavior.Strict);
            string topic = null!;
            Func<OrderAuthorizedIntegrationEvent, Task> handler = null!;
            busMock.Setup(b => b.SubscribeAsync<OrderAuthorizedIntegrationEvent>(It.IsAny<string>(), It.IsAny<Func<OrderAuthorizedIntegrationEvent, Task>>()))
                   .Callback<string, Func<OrderAuthorizedIntegrationEvent, Task>>((t, h) => { topic = t; handler = h; });

            var sut = new CatalogIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            var act = async () => await handler(MakeEvent(items));
            await act.Should().ThrowAsync<DomainException>().WithMessage("*Problemas ao atualizar estoque*");
            busMock.Verify(b => b.PublishAsync(It.IsAny<OrderStockDeductedIntegrationEvent>()), Times.Never);
        }
    }
}
