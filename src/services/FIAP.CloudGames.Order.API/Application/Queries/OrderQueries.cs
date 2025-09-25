using Dapper;
using FIAP.CloudGames.Order.API.Application.DTO;
using FIAP.CloudGames.Order.Domain.Order;

namespace FIAP.CloudGames.Order.API.Application.Queries
{
    public interface IOrderQueries
    {
        Task<OrderDTO> GetLastOrder(Guid customerId);

        Task<IEnumerable<OrderDTO>> GetListByCustomerId(Guid customerId);

        Task<OrderDTO> GetAuthorizedOrders();
    }

    public class OrderQueries : IOrderQueries
    {
        private readonly IOrderRepository _orderRepository;

        public OrderQueries(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDTO> GetLastOrder(Guid customerId)
        {
            const string sql = @"
                                SELECT
                                    -- Order
                                    O.Id,
                                    O.Code,
                                    O.VoucherUsed,
                                    O.Discount,
                                    O.TotalValue,
                                    O.OrderStatus,
                                    O.RegisterDate   AS [Date],

                                    -- Address
                                    O.Street,
                                    O.Number,
                                    O.Neighborhood,
                                    O.PostalCode,
                                    O.AdditionalInfo,
                                    O.City,
                                    O.State,

                                    -- Items
                                    I.Id             AS OrderItemId,
                                    I.ProductId,
                                    I.ProductName,
                                    I.Quantity,
                                    I.ProductImage,
                                    I.UnitValue
                                FROM [order].[Orders] O
                                INNER JOIN [order].[OrderItems] I ON O.Id = I.OrderId
                                WHERE O.CustomerId = @customerId
                                  AND O.RegisterDate BETWEEN DATEADD(minute, -3, SYSDATETIME()) AND SYSDATETIME()
                                  AND O.OrderStatus = 1
                                ORDER BY O.RegisterDate DESC;";

            var lookup = new Dictionary<Guid, OrderDTO>();

            await _orderRepository.GetConnection()
                .QueryAsync<OrderDTO, OrderItemDTO, OrderDTO>(
                    sql,
                    (o, item) =>
                    {
                        if (!lookup.TryGetValue(o.Id, out var dto))
                        {
                            dto = o;
                            dto.OrdetItems ??= new List<OrderItemDTO>();
                            lookup[o.Id] = dto;
                        }

                        dto.OrdetItems.Add(item);
                        return dto;
                    },
                    splitOn: "OrderItemId");

            return lookup.Values.FirstOrDefault();
        }

        public async Task<IEnumerable<OrderDTO>> GetListByCustomerId(Guid clienteId)
        {
            var pedidos = await _orderRepository.GetListByCustomer(clienteId);

            return pedidos.Select(OrderDTO.ToOrderDTO);
        }

        public async Task<OrderDTO> GetAuthorizedOrders()
        {
            const string sql = @"
                                SELECT
                                    O.Id,
                                    O.CustomerId,
                                    O.Code,
                                    O.RegisterDate   AS [Date],
                                    O.OrderStatus,
                                    O.TotalValue,
                                    O.Discount,
                                    O.VoucherUsed,

                                    I.Id             AS OrderItemId,
                                    I.ProductId,
                                    I.ProductName,
                                    I.Quantity,
                                    I.UnitValue      AS Value,
                                    I.ProductImage   AS Image
                                FROM [order].[Orders] O
                                INNER JOIN [order].[OrderItems] I ON O.Id = I.OrderId
                                WHERE O.OrderStatus = 1
                                ORDER BY O.RegisterDate;";

            var lookup = new Dictionary<Guid, OrderDTO>();

            await _orderRepository.GetConnection()
                .QueryAsync<OrderDTO, OrderItemDTO, OrderDTO>(
                    sql,
                    (o, item) =>
                    {
                        if (!lookup.TryGetValue(o.Id, out var dto))
                        {
                            dto = o;
                            dto.OrdetItems ??= new List<OrderItemDTO>();
                            lookup[o.Id] = dto;
                        }

                        dto.OrdetItems.Add(item);
                        return dto;
                    },
                    splitOn: "OrderItemId");

            // oldest authorized order
            return lookup.Values.OrderBy(p => p.Date).FirstOrDefault();
        }

        private OrderDTO MapOrder(dynamic result)
        {
            var order = new OrderDTO
            {
                Code = result[0].CODE,
                Status = result[0].ORDERSTATUS,
                TotalValue = result[0].TOTALVALUE,
                Discount = result[0].Discount,
                VoucherUsed = result[0].VOUCHERUSED,

                OrdetItems = new List<OrderItemDTO>(),
                Address = new AddressDTO
                {
                    Street = result[0].STREET,
                    Neighborhood = result[0].NEIGHBORHOOD,
                    PostalCode = result[0].POSTALCODE,
                    City = result[0].CITY,
                    AdditionalInfo = result[0].ADDITIONALINFO,
                    State = result[0].STATE,
                    Number = result[0].NUMBER
                }
            };

            foreach (var item in result)
            {
                var orderItem = new OrderItemDTO
                {
                    Name = item.PRODUCTNAME,
                    Value = item.UNITVALUE,
                    Quantity = item.QUANTITY,
                    Image = item.PRODUCTIMAGE
                };

                order.OrdetItems.Add(orderItem);
            }

            return order;
        }
    }
}