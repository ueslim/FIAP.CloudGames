namespace FIAP.CloudGames.Order.API.Application.DTO
{
    public class AddressDTO
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string AdditionalInfo { get; set; }
        public string Neighborhood { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}