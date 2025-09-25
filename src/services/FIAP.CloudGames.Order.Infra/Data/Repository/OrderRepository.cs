using FIAP.CloudGames.Core.Data;
using FIAP.CloudGames.Order.Domain.Order;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace FIAP.CloudGames.Order.Infra.Data.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _context;

        public OrderRepository(OrderContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => _context;

        public DbConnection GetConnection() => _context.Database.GetDbConnection();

        public async Task<Domain.Order.Order> GetById(Guid id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task<IEnumerable<Domain.Order.Order>> GetListByCustomer(Guid customerId)
        {
            return await _context.Orders
                .Include(p => p.OrderItems)
                .AsNoTracking()
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
        }

        public void Add(Domain.Order.Order order)
        {
            _context.Orders.Add(order);
        }

        public void Update(Domain.Order.Order order)
        {
            _context.Orders.Update(order);
        }

        public async Task<OrderItem> GetItemById(Guid id)
        {
            return await _context.OrderItems.FindAsync(id);
        }

        public async Task<OrderItem> GetItemByOrder(Guid orderId, Guid productId)
        {
            return await _context.OrderItems
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.OrderId == orderId);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}