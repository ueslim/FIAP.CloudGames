using FIAP.CloudGames.Core.DomainObjects;

namespace FIAP.CloudGames.Customer.API.Models
{
    public class Address : Entity
    {
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string AdditionalInfo { get; private set; }
        public string Neighborhood { get; private set; }
        public string PostalCode { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public Guid CustomerId { get; private set; }

        // EF Relation
        public Customer Customer { get; protected set; }

        public Address(string street, string number, string additionalInfo, string neighborhood, string postalCode, string city, string state, Guid customerId)
        {
            Street = street;
            Number = number;
            AdditionalInfo = additionalInfo;
            Neighborhood = neighborhood;
            PostalCode = postalCode;
            City = city;
            State = state;
            CustomerId = customerId;
        }

        // EF Constructor
        protected Address()
        { }
    }
}