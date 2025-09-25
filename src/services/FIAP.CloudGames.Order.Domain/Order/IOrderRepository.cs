using FIAP.CloudGames.Core.Data;
using System.Data.Common;

namespace FIAP.CloudGames.Order.Domain.Order
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order> GetById(Guid id);

        Task<IEnumerable<Order>> GetListByCustomer(Guid customerId);

        void Add(Order order);

        void Update(Order order);

        DbConnection GetConnection();

        /* Pedido Item */

        Task<OrderItem> GetItemById(Guid id);

        Task<OrderItem> GetItemByOrder(Guid orderId, Guid productId);
    }
}