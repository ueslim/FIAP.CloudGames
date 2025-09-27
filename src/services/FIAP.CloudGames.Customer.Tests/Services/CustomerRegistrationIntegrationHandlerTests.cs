using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using FIAP.CloudGames.Core.Mediator;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.Customer.API.Application.Commands;
using FIAP.CloudGames.Customer.API.Services;
using FIAP.CloudGames.MessageBus;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Customer.Tests.Services
{
    public class CustomerRegistrationIntegrationHandlerTests
    {
        private static ServiceProvider BuildProvider(IMediatorHandler mediator)
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => mediator);
            return services.BuildServiceProvider(validateScopes: true);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Register_Responder_For_UserRegistered()
        {
            var mediatorMock = new Mock<IMediatorHandler>(MockBehavior.Strict);
            var provider = BuildProvider(mediatorMock.Object);

            var busMock = new Mock<IMessageBus>(MockBehavior.Loose); // Loose para evitar mismatch de overload/retorno
            Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>> captured = null!;
            busMock.Setup(b => b.RespondAsync<UserRegisteredIntegrationEvent, ResponseMessage>(
                              It.IsAny<Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>>>()))
                   .Callback<Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>>>(f => captured = f);

            var advBusMock = new Mock<IAdvancedBus>(MockBehavior.Loose);
            advBusMock.SetupAdd(a => a.Connected += It.IsAny<EventHandler<ConnectedEventArgs>>()).Verifiable();
            advBusMock.SetupRemove(a => a.Connected -= It.IsAny<EventHandler<ConnectedEventArgs>>()).Verifiable();
            busMock.SetupGet(b => b.AdvancedBus).Returns(advBusMock.Object);

            var sut = new CustomerRegistrationIntegrationHandler(provider, busMock.Object);

            await sut.StartAsync(CancellationToken.None);
            await sut.StopAsync(CancellationToken.None);

            busMock.Verify(b => b.RespondAsync<UserRegisteredIntegrationEvent, ResponseMessage>(
                               It.IsAny<Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>>>()), Times.AtLeastOnce);
            captured.Should().NotBeNull();
        }

        [Fact]
        public async Task RegisterCustomer_Should_Send_Command_And_Return_ResponseMessage()
        {
            var expectedValidation = new ValidationResult();
            var mediatorMock = new Mock<IMediatorHandler>(MockBehavior.Strict);
            mediatorMock.Setup(m => m.SendCommand(It.IsAny<RegisterCustomerCommand>())).ReturnsAsync(expectedValidation);

            var provider = BuildProvider(mediatorMock.Object);

            var busMock = new Mock<IMessageBus>(MockBehavior.Loose);
            Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>> captured = null!;
            busMock.Setup(b => b.RespondAsync<UserRegisteredIntegrationEvent, ResponseMessage>(
                              It.IsAny<Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>>>()))
                   .Callback<Func<UserRegisteredIntegrationEvent, Task<ResponseMessage>>>(f => captured = f);

            var advBusMock = new Mock<IAdvancedBus>(MockBehavior.Loose);
            advBusMock.SetupAdd(a => a.Connected += It.IsAny<EventHandler<ConnectedEventArgs>>()).Verifiable();
            advBusMock.SetupRemove(a => a.Connected -= It.IsAny<EventHandler<ConnectedEventArgs>>()).Verifiable();
            busMock.SetupGet(b => b.AdvancedBus).Returns(advBusMock.Object);

            var sut = new CustomerRegistrationIntegrationHandler(provider, busMock.Object);
            await sut.StartAsync(CancellationToken.None);

            var user = new UserRegisteredIntegrationEvent(Guid.NewGuid(), "Alice", "alice@mail.com", "52998224725");

            var response = await captured(user);

            mediatorMock.Verify(m => m.SendCommand(It.IsAny<RegisterCustomerCommand>()), Times.Once);
            response.Should().NotBeNull();
            response.ValidationResult.Should().BeSameAs(expectedValidation);
        }
    }
}
