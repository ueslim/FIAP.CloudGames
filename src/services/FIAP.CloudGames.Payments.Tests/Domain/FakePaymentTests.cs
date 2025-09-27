using System;
using FIAP.CloudGames.FakePayment;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Payments.Tests.Domain
{
    public class FakePaymentTests
    {
        [Fact]
        public void CardHash_Should_Generate_NonEmpty_String()
        {
            var svc = new FakePaymentService(apiKey: "0123456789ABCDEF0123456789ABCDEF", encryptionKey: "1234567890123456");
            var card = new CardHash(svc)
            {
                CardHolderName = "John",
                CardNumber = "4111111111111111",
                CardExpirationDate = "12/29",
                CardCvv = "123"
            };

            var hash = card.Generate();

            hash.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task FakeTransaction_Authorize_Capture_Cancel_Should_Map_Statuses()
        {
            var svc = new FakePaymentService(apiKey: "0123456789ABCDEF0123456789ABCDEF", encryptionKey: "1234567890123456");
            var tx = new FakeTransaction(svc) { Amount = 100m };

            var auth = await tx.AuthorizeCardTransaction();
            auth.Status.Should().Be(TransactionStatus.Authorized);
            auth.Amount.Should().Be(100m);
            auth.AuthorizationCode.Should().NotBeNullOrWhiteSpace();

            var captured = await new FakeTransaction(svc)
            {
                Amount = auth.Amount,
                CardBrand = auth.CardBrand,
                Tid = auth.Tid,
                Nsu = auth.Nsu
            }.CaptureCardTransaction();
            captured.Status.Should().Be(TransactionStatus.Paid);
            captured.Amount.Should().Be(100m);

            var canceled = await new FakeTransaction(svc)
            {
                Amount = auth.Amount,
                CardBrand = auth.CardBrand,
                Tid = auth.Tid,
                Nsu = auth.Nsu
            }.CancelAuthorization();
            canceled.Status.Should().Be(TransactionStatus.Cancelled);
            canceled.Amount.Should().Be(100m);
        }

    }
}
