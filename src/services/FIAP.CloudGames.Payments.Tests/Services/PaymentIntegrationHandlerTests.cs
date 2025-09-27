using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.MessageBus;
using FIAP.CloudGames.Payment.API.Models;
using FIAP.CloudGames.Payment.API.Services;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Payments.Tests.Services
{
    public class PaymentIntegrationHandlerTests
    {
        private static ServiceProvider ProviderWith(IPaymentService svc)
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => svc);
            return services.BuildServiceProvider(true);
        }

        [Fact]
        public async Task ExecuteAsync_Should_Set_Responder_And_Subscribers()
        {
            var svc = new Mock<IPaymentService>(MockBehavior.Strict);
            var provider = ProviderWith(svc.Object);

            // Use Loose ONLY for bus (implementation details vary)
            var bus = new Mock<IMessageBus>(MockBehavior.Loose);

            // opcional: só para confirmar que são registrados
            bus.Setup(b => b.RespondAsync<OrderStartedIntegrationEvent, ResponseMessage>(
                It.IsAny<Func<OrderStartedIntegrationEvent, Task<ResponseMessage>>>())).Verifiable();
            bus.Setup(b => b.SubscribeAsync<OrderCanceledIntegrationEvent>(
                It.IsAny<string>(), It.IsAny<Func<OrderCanceledIntegrationEvent, Task>>())).Verifiable();
            bus.Setup(b => b.SubscribeAsync<OrderStockDeductedIntegrationEvent>(
                It.IsAny<string>(), It.IsAny<Func<OrderStockDeductedIntegrationEvent, Task>>())).Verifiable();

            var sut = new PaymentIntegrationHandler(provider, bus.Object);

            await sut.StartAsync(CancellationToken.None);
            await sut.StopAsync(CancellationToken.None);

            bus.VerifyAll();
        }

        [Fact]
        public async Task AuthorizePayment_Responder_Should_Call_Service()
        {
            var svc = new Mock<IPaymentService>(MockBehavior.Strict);
            svc.Setup(s => s.AuthorizePayment(It.IsAny<FIAP.CloudGames.Payment.API.Models.Payment>()))
               .ReturnsAsync(new ResponseMessage(new ValidationResult()));

            var provider = ProviderWith(svc.Object);

            var bus = new Mock<IMessageBus>(MockBehavior.Loose);

            Func<OrderStartedIntegrationEvent, Task<ResponseMessage>> responder = null!;
            bus.Setup(b => b.RespondAsync<OrderStartedIntegrationEvent, ResponseMessage>(
                    It.IsAny<Func<OrderStartedIntegrationEvent, Task<ResponseMessage>>>()))
               .Callback<Func<OrderStartedIntegrationEvent, Task<ResponseMessage>>>(f => responder = f);

            var sut = new PaymentIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None);

            var evt = new OrderStartedIntegrationEvent
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Value = 123m,
                PaymentType = (int)PaymentType.CreditCard,
                CardName = "John",
                CardNumber = "4111111111111111",
                CardExpirationDate = "12/29",
                CvvCard = "123"
            };

            var resp = await responder(evt);

            resp.Should().NotBeNull();
            svc.Verify(s => s.AuthorizePayment(It.Is<FIAP.CloudGames.Payment.API.Models.Payment>(p =>
                p.OrderId == evt.OrderId &&
                p.Value == evt.Value &&
                p.PaymentType == PaymentType.CreditCard)), Times.Once);
        }

        [Fact]
        public async Task CapturePayment_Should_Publish_OrderPaid_On_Success()
        {
            var svc = new Mock<IPaymentService>(MockBehavior.Strict);
            svc.Setup(s => s.CapturePayment(It.IsAny<Guid>()))
               .ReturnsAsync(new ResponseMessage(new ValidationResult()));

            var provider = ProviderWith(svc.Object);

            var bus = new Mock<IMessageBus>(MockBehavior.Loose);

            Func<OrderStockDeductedIntegrationEvent, Task> captured = null!;
            bus.Setup(b => b.SubscribeAsync<OrderStockDeductedIntegrationEvent>(
                    It.IsAny<string>(), It.IsAny<Func<OrderStockDeductedIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderStockDeductedIntegrationEvent, Task>>((_, h) => captured = h);

            bus.Setup(b => b.PublishAsync(It.IsAny<OrderPaidIntegrationEvent>()))
               .Returns(Task.CompletedTask)
               .Verifiable();

            var sut = new PaymentIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None);

            var clientId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            await captured(new OrderStockDeductedIntegrationEvent(clientId, orderId));

            bus.Verify(b => b.PublishAsync(It.Is<OrderPaidIntegrationEvent>(e =>
                e.ClientId == clientId && e.OrderId == orderId)), Times.Once);
        }

        [Fact]
        public async Task CancelPayment_Should_Throw_When_Service_Returns_Error()
        {
            var vr = new ValidationResult();
            vr.Errors.Add(new FluentValidation.Results.ValidationFailure("Pagamento", "Falha"));

            var svc = new Mock<IPaymentService>(MockBehavior.Strict);
            svc.Setup(s => s.CancelPayment(It.IsAny<Guid>()))
               .ReturnsAsync(new ResponseMessage(vr));

            var provider = ProviderWith(svc.Object);

            var bus = new Mock<IMessageBus>(MockBehavior.Loose);

            Func<OrderCanceledIntegrationEvent, Task> canceled = null!;
            bus.Setup(b => b.SubscribeAsync<OrderCanceledIntegrationEvent>(
                    It.IsAny<string>(), It.IsAny<Func<OrderCanceledIntegrationEvent, Task>>()))
               .Callback<string, Func<OrderCanceledIntegrationEvent, Task>>((_, h) => canceled = h);

            var sut = new PaymentIntegrationHandler(provider, bus.Object);
            await sut.StartAsync(CancellationToken.None);

            var act = async () => await canceled(new OrderCanceledIntegrationEvent(Guid.NewGuid(), Guid.NewGuid()));
            await act.Should().ThrowAsync<FIAP.CloudGames.Core.DomainObjects.DomainException>();
        }
    }
}
