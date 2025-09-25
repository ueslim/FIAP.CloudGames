using FIAP.CloudGames.Core.Messages.Integration;

namespace FIAP.CloudGames.Payment.API.Services
{
    public interface IPaymentService
    {
        Task<ResponseMessage> AuthorizePayment(Models.Payment payment);

        Task<ResponseMessage> CapturePayment(Guid orderId);

        Task<ResponseMessage> CancelPayment(Guid orderId);
    }
}