using FIAP.CloudGames.Payment.API.Models;

namespace FIAP.CloudGames.Payment.API.Facade
{
    public interface IPaymentFacade
    {
        Task<Transaction> AuthorizePayment(Models.Payment payment);

        Task<Transaction> CapturePayment(Transaction transaction);

        Task<Transaction> CancelAuthorization(Transaction transaction);
    }
}