using System;
using FIAP.CloudGames.Payment.API.Models;
using FluentAssertions;
using Xunit;

namespace FIAP.CloudGames.Payments.Tests.Domain
{
    public class PaymentDomainTests
    {
        [Fact]
        public void CreditCard_Ctor_Should_Set_All_Properties()
        {
            var cc = new CreditCard("John Doe", "4111111111111111", "12/29", "123");
            cc.CardName.Should().Be("John Doe");
            cc.CardNumber.Should().Be("4111111111111111");
            cc.ExpirationDate.Should().Be("12/29");
            cc.CVV.Should().Be("123");
        }

        [Fact]
        public void Payment_Should_Add_Transaction()
        {
            var payment = new Payment.API.Models.Payment { OrderId = Guid.NewGuid(), PaymentType = PaymentType.CreditCard, Value = 42.5m };
            var tx = new Transaction { Id = Guid.NewGuid(), Status = TransactionStatus.Authorized, TotalValue = 42.5m };

            payment.AddTransaction(tx);

            payment.Transactions.Should().ContainSingle().Which.Should().Be(tx);
        }
    }
}
