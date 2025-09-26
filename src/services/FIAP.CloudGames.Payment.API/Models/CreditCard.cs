namespace FIAP.CloudGames.Payment.API.Models
{
    public class CreditCard
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }

        protected CreditCard()
        { }

        public CreditCard(string cardName, string cardNumber, string expirationDate, string cvv)
        {
            CardName = cardName;
            CardNumber = cardNumber;
            ExpirationDate = expirationDate;
            CVV = cvv;
        }
    }
}