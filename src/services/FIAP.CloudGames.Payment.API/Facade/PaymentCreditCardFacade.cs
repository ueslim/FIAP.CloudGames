using FIAP.CloudGames.FakePayment;
using Microsoft.Extensions.Options;

namespace FIAP.CloudGames.Payment.API.Facade
{
    public class PaymentCreditCardFacade : IPaymentFacade
    {
        private readonly PaymentConfig _paymentConfig;

        public PaymentCreditCardFacade(IOptions<PaymentConfig> paymentConfig)
        {
            _paymentConfig = paymentConfig.Value;
        }

        public async Task<Models.Transaction> AuthorizePayment(Models.Payment payment)
        {
            var fakePaymentService = new FakePaymentService(_paymentConfig.DefaultApiKey,
                _paymentConfig.DefaultEncryptionKey);

            var cardHashGen = new CardHash(fakePaymentService)
            {
                CardNumber = payment.CreditCard.CardNumber,
                CardHolderName = payment.CreditCard.CardName,
                CardExpirationDate = payment.CreditCard.ExpirationDate,
                CardCvv = payment.CreditCard.CVV
            };
            var cardHash = cardHashGen.Generate();

            var transaction = new FakeTransaction(fakePaymentService)
            {
                CardHash = cardHash,
                CardNumber = payment.CreditCard.CardNumber,
                CardHolderName = payment.CreditCard.CardName,
                CardExpirationDate = payment.CreditCard.ExpirationDate,
                CardCvv = payment.CreditCard.CVV,
                PaymentMethod = PaymentMethod.CreditCard,
                Amount = payment.Value
            };

            return MapToTransaction(await transaction.AuthorizeCardTransaction());
        }

        public async Task<Models.Transaction> CapturePayment(Models.Transaction transaction)
        {
            var fakePaymentService = new FakePaymentService(_paymentConfig.DefaultApiKey, _paymentConfig.DefaultEncryptionKey);

            var transactionFacade = MapToTransaction(transaction, fakePaymentService);

            return MapToTransaction(await transactionFacade.CaptureCardTransaction());
        }

        public async Task<Models.Transaction> CancelAuthorization(Models.Transaction transacao)
        {
            var fakePaymentService = new FakePaymentService(_paymentConfig.DefaultApiKey, _paymentConfig.DefaultEncryptionKey);

            var transaction = MapToTransaction(transacao, fakePaymentService);

            return MapToTransaction(await transaction.CancelAuthorization());
        }

        public static Models.Transaction MapToTransaction(FakeTransaction transaction)
        {
            return new Models.Transaction
            {
                Id = Guid.NewGuid(),
                Status = (Models.TransactionStatus)transaction.Status,
                TotalValue = transaction.Amount,
                CardBrand = transaction.CardBrand,
                AuthorizationCode = transaction.AuthorizationCode,
                TransactionCost = transaction.Cost,
                TransactionDate = transaction.TransactionDate,
                NSU = transaction.Nsu,
                TID = transaction.Tid
            };
        }

        public static FakeTransaction MapToTransaction(Models.Transaction transacao, FakePaymentService fakePayment)
        {
            return new FakeTransaction(fakePayment)
            {
                Status = (TransactionStatus)transacao.Status,
                Amount = transacao.TotalValue,
                CardBrand = transacao.CardBrand,
                AuthorizationCode = transacao.AuthorizationCode,
                Cost = transacao.TransactionCost,
                Nsu = transacao.NSU,
                Tid = transacao.TID
            };
        }
    }
}