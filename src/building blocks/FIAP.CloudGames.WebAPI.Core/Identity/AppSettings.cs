namespace FIAP.CloudGames.WebAPI.Core.Identity
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public int Expiration { get; set; }
        public string Issuer { get; set; }
        public string ValidOn { get; set; }
    }
}