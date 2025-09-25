namespace FIAP.CloudGames.FakePayment
{
    public class FakePaymentService
    {
        public readonly string ApiKey;
        public readonly string EncryptionKey;

        public FakePaymentService(string apiKey, string encryptionKey)
        {
            ApiKey = apiKey;
            EncryptionKey = encryptionKey;
        }
    }
}