using FIAP.CloudGames.Core.Data;

namespace FIAP.CloudGames.Catalog.API.Models
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAll();

        Task<Product> GetById(Guid id);

        Task<List<Product>> GetProductsById(string ids);

        void Add(Product product);

        void Update(Product product);
    }
}