using System;
using FIAP.CloudGames.FakePayment;
using FIAP.CloudGames.Payment.API.Facade;
using FIAP.CloudGames.Payment.API.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FIAP.CloudGames.Payments.Tests.Services
{
    public class PaymentCreditCardFacadeTests
    {
        private static Payment.API.Models.Payment BuildPayment(decimal value = 150m) => new()
        {
            OrderId = Guid.NewGuid(),
            PaymentType = PaymentType.CreditCard,
            Value = value,
            CreditCard = new CreditCard("John Doe", "4111111111111111", "12/29", "123")
        };

        private static IOptions<PaymentConfig> Cfg() =>
            Options.Create(new PaymentConfig
            {
                DefaultApiKey = "0123456789ABCDEF0123456789ABCDEF",
                DefaultEncryptionKey = "1234567890123456"
            });

        [Fact]
        public async void AuthorizePayment_Should_Return_Authorized_Transaction()
        {
            var facade = new PaymentCreditCardFacade(Cfg());

            var t = await facade.AuthorizePayment(BuildPayment());

            t.Status.Should().Be(Payment.API.Models.TransactionStatus.Authorized);
            t.TotalValue.Should().Be(150m);
            t.AuthorizationCode.Should().NotBeNullOrWhiteSpace();
            t.TID.Should().NotBeNullOrWhiteSpace();
            t.NSU.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async void Capture_And_Cancel_Should_Return_Mapped_Transactions()
        {
            var facade = new PaymentCreditCardFacade(Cfg());

            var auth = await facade.AuthorizePayment(BuildPayment(200m));

            var captured = await facade.CapturePayment(auth);
            captured.Status.Should().Be(Payment.API.Models.TransactionStatus.Paid);
            captured.TotalValue.Should().Be(auth.TotalValue);
            captured.TID.Should().Be(auth.TID);
            captured.NSU.Should().Be(auth.NSU);

            var canceled = await facade.CancelAuthorization(auth);
            canceled.Status.Should().Be(Payment.API.Models.TransactionStatus.Canceled);
            canceled.TotalValue.Should().Be(auth.TotalValue);
        }

        [Fact]
        public void Map_Converters_Should_Be_Invertible_For_Core_Fields()
        {
            var cfg = new FakePaymentService("0123456789ABCDEF0123456789ABCDEF", "1234567890123456");
            var fakeTx = new FakeTransaction(cfg)
            {
                Amount = 99m,
                CardBrand = "MasterCard",
                AuthorizationCode = "ABC",
                Cost = 1.23m,
                Nsu = "NSU",
                Tid = "TID",
                Status = FakePayment.TransactionStatus.Authorized
            };

            var modelTx = PaymentCreditCardFacade.MapToTransaction(fakeTx);
            var back = PaymentCreditCardFacade.MapToTransaction(modelTx, cfg);

            back.Amount.Should().Be(fakeTx.Amount);
            back.CardBrand.Should().Be(fakeTx.CardBrand);
            back.AuthorizationCode.Should().Be(fakeTx.AuthorizationCode);
            back.Cost.Should().Be(fakeTx.Cost);
            back.Nsu.Should().Be(fakeTx.Nsu);
            back.Tid.Should().Be(fakeTx.Tid);
            back.Status.Should().Be(fakeTx.Status);
        }
    }
}
