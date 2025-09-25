namespace FIAP.CloudGames.Payment.API.Models
{
    public enum TransactionStatus
    {
        Authorized = 1,
        Paid,
        Denied,
        Refunded,
        Canceled
    }
}