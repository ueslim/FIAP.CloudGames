using FIAP.CloudGames.Cart.API.Data;
using FIAP.CloudGames.Cart.API.Model;
using FIAP.CloudGames.Cart.API.Services;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Cart.Tests.Services
{
    public class CartIntegrationHandlerTests
    {
        private static ServiceProvider BuildServiceProvider(string dbName)
        {
            var services = new ServiceCollection();

            services.AddDbContext<CartContext>(opt => opt.UseInMemoryDatabase(dbName));

            return services.BuildServiceProvider(validateScopes: true);
        }

        private static CartCustomer NewCart(Guid customerId)
        {
            var cart = new CartCustomer(customerId);

            var item = new CartItem
            {
                ProductId = Guid.NewGuid(),
                Name = "Game A",
                Quantity = 1,
                Value = 10m,
                Image = "img.png"
            };

            // IMPORTANT: set CartId (and optionally the navigation)
            item.AssociateCart(cart.Id);
            item.CartCustomer = cart;

            cart.Items.Add(item);
            cart.CalculateCartValue(); // total = 10

            return cart;
        }

        [Fact]
        public async Task ExecuteAsync_Should_Subscribe_To_OrderPlaced_Topic()
        {
            // Arrange
            var dbName = $"cartdb-{Guid.NewGuid()}";
            var provider = BuildServiceProvider(dbName);

            string capturedTopic = null;
            Func<OrderFinishedIntegrationEvent, Task> capturedHandler = null;

            var busMock = new Mock<IMessageBus>();
            busMock
                .Setup(b => b.SubscribeAsync<OrderFinishedIntegrationEvent>(
                    It.IsAny<string>(),
                    It.IsAny<Func<OrderFinishedIntegrationEvent, Task>>()))
                .Callback<string, Func<OrderFinishedIntegrationEvent, Task>>((topic, handler) =>
                {
                    capturedTopic = topic;
                    capturedHandler = handler;
                }); // <-- sem .Returns(...)

            var sut = new CartIntegrationHandler(provider, busMock.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);
            await sut.StopAsync(CancellationToken.None);

            // Assert
            busMock.Verify(b => b.SubscribeAsync<OrderFinishedIntegrationEvent>(
                    It.IsAny<string>(), It.IsAny<Func<OrderFinishedIntegrationEvent, Task>>()),
                Times.Once);

            capturedTopic.Should().Be("OrderPlaced");
            capturedHandler.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteCart_Handler_Should_Remove_Cart_When_It_Exists()
        {
            // Arrange
            var dbName = $"cartdb-{Guid.NewGuid()}";
            var provider = BuildServiceProvider(dbName);

            // seed
            Guid existingCustomerId;
            using (var scope = provider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<CartContext>();
                existingCustomerId = Guid.NewGuid();
                ctx.CartCustomer.Add(NewCart(existingCustomerId));
                await ctx.SaveChangesAsync();
            }

            Func<OrderFinishedIntegrationEvent, Task> capturedHandler = null;
            var busMock = new Mock<IMessageBus>();
            busMock
                .Setup(b => b.SubscribeAsync<OrderFinishedIntegrationEvent>(
                    It.IsAny<string>(),
                    It.IsAny<Func<OrderFinishedIntegrationEvent, Task>>()))
                .Callback<string, Func<OrderFinishedIntegrationEvent, Task>>((_, handler) =>
                {
                    capturedHandler = handler;
                }); // <-- sem .Returns(...)

            var sut = new CartIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            // Sanity check pre
            using (var pre = provider.CreateScope())
            {
                var ctx = pre.ServiceProvider.GetRequiredService<CartContext>();
                (await ctx.CartCustomer.CountAsync()).Should().Be(1);
            }

            // Act: simula mensagem do bus
            await capturedHandler!(new OrderFinishedIntegrationEvent(existingCustomerId));

            // Assert: cart removido
            using (var post = provider.CreateScope())
            {
                var ctx = post.ServiceProvider.GetRequiredService<CartContext>();
                (await ctx.CartCustomer.CountAsync()).Should().Be(0);
            }
        }

        [Fact]
        public async Task DeleteCart_Handler_Should_Do_Nothing_When_Cart_Not_Found()
        {
            // Arrange
            var dbName = $"cartdb-{Guid.NewGuid()}";
            var provider = BuildServiceProvider(dbName);

            Guid existingCustomerId;
            using (var scope = provider.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<CartContext>();
                existingCustomerId = Guid.NewGuid();
                ctx.CartCustomer.Add(NewCart(existingCustomerId));
                await ctx.SaveChangesAsync();
            }

            Func<OrderFinishedIntegrationEvent, Task> capturedHandler = null;
            var busMock = new Mock<IMessageBus>();
            busMock
                .Setup(b => b.SubscribeAsync<OrderFinishedIntegrationEvent>(
                    It.IsAny<string>(),
                    It.IsAny<Func<OrderFinishedIntegrationEvent, Task>>()))
                .Callback<string, Func<OrderFinishedIntegrationEvent, Task>>((_, handler) =>
                {
                    capturedHandler = handler;
                }); // <-- sem .Returns(...)

            var sut = new CartIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            // Act: evento para cliente inexistente
            await capturedHandler!(new OrderFinishedIntegrationEvent(Guid.NewGuid()));

            // Assert: cart original permanece
            using (var scope2 = provider.CreateScope())
            {
                var ctx = scope2.ServiceProvider.GetRequiredService<CartContext>();
                (await ctx.CartCustomer.CountAsync()).Should().Be(1);
                var remaining = await ctx.CartCustomer.FirstOrDefaultAsync();
                remaining!.CustomerId.Should().Be(existingCustomerId);
            }
        }
    }
}