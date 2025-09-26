using FIAP.CloudGames.Core.DomainObjects;
using FIAP.CloudGames.Core.Messages.Integration;
using FIAP.CloudGames.Payment.API.Facade;
using FIAP.CloudGames.Payment.API.Models;
using FluentValidation.Results;

namespace FIAP.CloudGames.Payment.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentFacade _paymentFacade;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentFacade paymentFacade,
                                IPaymentRepository paymentRepository)
        {
            _paymentFacade = paymentFacade;
            _paymentRepository = paymentRepository;
        }

        public async Task<ResponseMessage> AuthorizePayment(Models.Payment payment)
        {
            var transaction = await _paymentFacade.AuthorizePayment(payment);
            var validationResult = new ValidationResult();

            if (transaction.Status != TransactionStatus.Authorized)
            {
                validationResult.Errors.Add(new ValidationFailure("Pagamento", "Pagamento recusado, entre em contato com a sua operadora de cartão"));

                return new ResponseMessage(validationResult);
            }

            payment.AddTransaction(transaction);

            _paymentRepository.AddPayment(payment);

            if (!await _paymentRepository.UnitOfWork.Commit())
            {
                validationResult.Errors.Add(new ValidationFailure("Pagamento", "Houve um erro ao realizar o pagamento."));

                // Cancelar pagamento no gateway
                await CancelPayment(payment.OrderId);

                return new ResponseMessage(validationResult);
            }

            return new ResponseMessage(validationResult);
        }

        public async Task<ResponseMessage> CapturePayment(Guid orderId)
        {
            var transactions = await _paymentRepository.GetTransactionsByOrderId(orderId);
            var authorizedTransaction = transactions?.FirstOrDefault(t => t.Status == TransactionStatus.Authorized);
            var validationResult = new ValidationResult();

            if (authorizedTransaction == null) throw new DomainException($"Transação não encontrada para o pedido {orderId}");

            var transaction = await _paymentFacade.CapturePayment(authorizedTransaction);

            if (transaction.Status != TransactionStatus.Paid)
            {
                validationResult.Errors.Add(new ValidationFailure("Pagamento", $"Não foi possível capturar o pagamento do pedido {orderId}"));

                return new ResponseMessage(validationResult);
            }

            transaction.PaymentId = authorizedTransaction.PaymentId;
            _paymentRepository.AddTransaction(transaction);

            if (!await _paymentRepository.UnitOfWork.Commit())
            {
                validationResult.Errors.Add(new ValidationFailure("Pagamento", $"Não foi possível persistir a captura do pagamento do pedido {orderId}"));

                return new ResponseMessage(validationResult);
            }

            return new ResponseMessage(validationResult);
        }

        public async Task<ResponseMessage> CancelPayment(Guid orderId)
        {
            var transactions = await _paymentRepository.GetTransactionsByOrderId(orderId);
            var authorizedTransaction = transactions?.FirstOrDefault(t => t.Status == TransactionStatus.Authorized);
            var validationResult = new ValidationResult();

            if (authorizedTransaction == null) throw new DomainException($"Transação não encontrada para o pedido {orderId}");

            var transaction = await _paymentFacade.CancelAuthorization(authorizedTransaction);

            if (transaction.Status != TransactionStatus.Canceled)
            {
                validationResult.Errors.Add(new ValidationFailure("Pagamento", $"Não foi possível cancelar o pagamento do pedido {orderId}"));

                return new ResponseMessage(validationResult);
            }

            transaction.PaymentId = authorizedTransaction.PaymentId;
            _paymentRepository.AddTransaction(transaction);

            if (!await _paymentRepository.UnitOfWork.Commit())
            {
                validationResult.Errors.Add(new ValidationFailure("Pagamento", $"Não foi possível persistir o cancelamento do pagamento do pedido {orderId}"));

                return new ResponseMessage(validationResult);
            }

            return new ResponseMessage(validationResult);
        }
    }
}