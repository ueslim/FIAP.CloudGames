using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FIAP.CloudGames.Core.Data;
using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.Payment.API.Facade;
using FIAP.CloudGames.Payment.API.Models;
using FIAP.CloudGames.Payment.API.Services;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Xunit;

namespace FIAP.CloudGames.Payments.Tests.Services
{
    public class PaymentServiceTests
    {
        private static Payment.API.Models.Payment BuildPayment(decimal value = 100m) => new()
        {
            OrderId = Guid.NewGuid(),
            PaymentType = PaymentType.CreditCard,
            Value = value,
            CreditCard = new CreditCard("John Doe", "4111111111111111", "12/29", "123")
        };

        private static Transaction Tx(TransactionStatus s, decimal total) => new()
        {
            Id = Guid.NewGuid(),
            Status = s,
            TotalValue = total,
            CardBrand = "MC",
            AuthorizationCode = "AUTH",
            TransactionCost = 1.23m,
            NSU = "NSU",
            TID = "TID"
        };

        [Fact]
        public async Task AuthorizePayment_Should_Persist_When_Authorized_And_Commit()
        {
            var payment = BuildPayment(120m);

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            facade.Setup(f => f.AuthorizePayment(payment)).ReturnsAsync(Tx(TransactionStatus.Authorized, 120m));

            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repo.Setup(r => r.AddPayment(payment));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);

            var sut = new PaymentService(facade.Object, repo.Object);

            var resp = await sut.AuthorizePayment(payment);

            resp.ValidationResult.IsValid.Should().BeTrue();
            repo.Verify(r => r.AddPayment(It.IsAny<Payment.API.Models.Payment>()), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task AuthorizePayment_Should_Return_Error_When_Gateway_Refuses()
        {
            var payment = BuildPayment();

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            facade.Setup(f => f.AuthorizePayment(payment)).ReturnsAsync(Tx(TransactionStatus.Denied, payment.Value));

            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);

            var sut = new PaymentService(facade.Object, repo.Object);

            var resp = await sut.AuthorizePayment(payment);

            resp.ValidationResult.IsValid.Should().BeFalse();
            repo.Verify(r => r.AddPayment(It.IsAny<Payment.API.Models.Payment>()), Times.Never);
        }

        [Fact]
        public async Task AuthorizePayment_Should_Cancel_On_Commit_Failure()
        {
            var payment = BuildPayment();

            // Transação que o gateway vai retornar como AUTORIZADA
            var authorizedTx = Tx(TransactionStatus.Authorized, payment.Value);
            authorizedTx.PaymentId = Guid.NewGuid();

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            facade.Setup(f => f.AuthorizePayment(payment)).ReturnsAsync(authorizedTx);

            // Quando o serviço chamar CancelPayment(orderId), o repositório deve retornar a transação autorizada
            facade.Setup(f => f.CancelAuthorization(authorizedTx))
                  .ReturnsAsync(Tx(TransactionStatus.Canceled, payment.Value));

            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repo.Setup(r => r.AddPayment(payment));

            // CancelPayment() vai consultar por transações do pedido
            repo.Setup(r => r.GetTransactionsByOrderId(payment.OrderId))
                .ReturnsAsync(new List<Transaction> { authorizedTx });

            // CancelPayment() vai adicionar a transação "Canceled"
            repo.Setup(r => r.AddTransaction(It.Is<Transaction>(t => t.Status == TransactionStatus.Canceled)));

            // Primeira chamada de Commit() (após AddPayment) falha; segunda (após AddTransaction) deve passar
            var uow = new Mock<IUnitOfWork>();
            uow.SetupSequence(u => u.Commit())
               .ReturnsAsync(false)  // falha para disparar o cancelamento
               .ReturnsAsync(true);  // sucesso ao persistir "Canceled"
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);

            var sut = new PaymentService(facade.Object, repo.Object);

            var resp = await sut.AuthorizePayment(payment);

            resp.ValidationResult.IsValid.Should().BeFalse();
            facade.Verify(f => f.CancelAuthorization(It.Is<Transaction>(t => t.Status == TransactionStatus.Authorized)), Times.Once);
            repo.Verify(r => r.AddTransaction(It.Is<Transaction>(t => t.Status == TransactionStatus.Canceled)), Times.Once);
            uow.Verify(u => u.Commit(), Times.Exactly(2));
        }


        [Fact]
        public async Task CapturePayment_Should_Add_Transaction_When_Paid_And_Commit()
        {
            var orderId = Guid.NewGuid();
            var authorized = Tx(TransactionStatus.Authorized, 100m);
            authorized.PaymentId = Guid.NewGuid();

            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetTransactionsByOrderId(orderId)).ReturnsAsync(new List<Transaction> { authorized });
            repo.Setup(r => r.AddTransaction(It.Is<Transaction>(t => t.Status == TransactionStatus.Paid)));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            facade.Setup(f => f.CapturePayment(authorized)).ReturnsAsync(Tx(TransactionStatus.Paid, 100m));

            var sut = new PaymentService(facade.Object, repo.Object);

            var resp = await sut.CapturePayment(orderId);

            resp.ValidationResult.IsValid.Should().BeTrue();
            repo.Verify(r => r.AddTransaction(It.Is<Transaction>(t => t.Status == TransactionStatus.Paid)), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task CapturePayment_Should_Return_Error_When_Gateway_Not_Paid()
        {
            var orderId = Guid.NewGuid();
            var authorized = Tx(TransactionStatus.Authorized, 50m);
            authorized.PaymentId = Guid.NewGuid();

            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetTransactionsByOrderId(orderId)).ReturnsAsync(new List<Transaction> { authorized });

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            facade.Setup(f => f.CapturePayment(authorized)).ReturnsAsync(Tx(TransactionStatus.Denied, 100m));

            var sut = new PaymentService(facade.Object, repo.Object);

            var resp = await sut.CapturePayment(orderId);

            resp.ValidationResult.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task CapturePayment_Should_Throw_When_Authorized_Not_Found()
        {
            var orderId = Guid.NewGuid();
            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetTransactionsByOrderId(orderId)).ReturnsAsync(new List<Transaction>());

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            var sut = new PaymentService(facade.Object, repo.Object);

            var act = async () => await sut.CapturePayment(orderId);
            await act.Should().ThrowAsync<DomainException>().WithMessage($"*{orderId}*");
        }

        [Fact]
        public async Task CancelPayment_Should_Add_Transaction_When_Canceled_And_Commit()
        {
            var orderId = Guid.NewGuid();
            var authorized = Tx(TransactionStatus.Authorized, 50m);
            authorized.PaymentId = Guid.NewGuid();

            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetTransactionsByOrderId(orderId)).ReturnsAsync(new List<Transaction> { authorized });
            repo.Setup(r => r.AddTransaction(It.Is<Transaction>(t => t.Status == TransactionStatus.Canceled)));
            var uow = new Mock<IUnitOfWork>(); uow.Setup(u => u.Commit()).ReturnsAsync(true);
            repo.SetupGet(r => r.UnitOfWork).Returns(uow.Object);

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            facade.Setup(f => f.CancelAuthorization(authorized)).ReturnsAsync(Tx(TransactionStatus.Canceled, 50m));

            var sut = new PaymentService(facade.Object, repo.Object);

            var resp = await sut.CancelPayment(orderId);

            resp.ValidationResult.IsValid.Should().BeTrue();
            repo.Verify(r => r.AddTransaction(It.Is<Transaction>(t => t.Status == TransactionStatus.Canceled)), Times.Once);
            uow.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public async Task CancelPayment_Should_Throw_When_Authorized_Not_Found()
        {
            var repo = new Mock<IPaymentRepository>(MockBehavior.Strict);
            var orderId = Guid.NewGuid();
            repo.Setup(r => r.GetTransactionsByOrderId(orderId)).ReturnsAsync(Enumerable.Empty<Transaction>());

            var facade = new Mock<IPaymentFacade>(MockBehavior.Strict);
            var sut = new PaymentService(facade.Object, repo.Object);

            var act = async () => await sut.CancelPayment(orderId);
            await act.Should().ThrowAsync<DomainException>().WithMessage($"*{orderId}*");
        }
    }
}
