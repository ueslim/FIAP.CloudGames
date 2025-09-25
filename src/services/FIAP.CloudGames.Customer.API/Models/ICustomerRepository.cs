using FIAP.CloudGames.Core.Data;

namespace FIAP.CloudGames.Customer.API.Models
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        void Add(Customer customer);

        Task<IEnumerable<Customer>> GetAll();

        Task<Customer> GetByCpf(string cpf);

        void AddAddress(Address address);

        Task<Address> GetAddressById(Guid id);
    }
}