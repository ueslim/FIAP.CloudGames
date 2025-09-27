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
                                ;WITH LastOrder AS (
                                    SELECT TOP (1) Id
                                    FROM [order].[Orders]
                                    WHERE CustomerId = @customerId
                                      AND OrderStatus = 1
                                    ORDER BY RegisterDate DESC
                                )
                                SELECT
                                    -- Order
                                    O.Id,
                                    O.CustomerId,
                                    O.Code,
                                    O.OrderStatus    AS Status,
                                    O.RegisterDate   AS [Date],
                                    O.TotalValue,
                                    O.Discount,
                                    O.VoucherUsed,
                                    V.Code           AS VoucherCode,

                                    -- Address
                                    O.Street         AS Street,
                                    O.Number         AS [Number],
                                    O.Neighborhood   AS Neighborhood,
                                    O.PostalCode     AS PostalCode,
                                    O.AdditionalInfo AS AdditionalInfo,
                                    O.City           AS City,
                                    O.State          AS [State],

                                    -- Items
                                    I.Id             AS OrderItemId,
                                    I.ProductId,
                                    I.ProductName    AS [Name],
                                    I.Quantity,
                                    I.ProductImage   AS [Image],
                                    I.UnitValue      AS [Value]
                                FROM [order].[Orders] O
                                INNER JOIN LastOrder L ON L.Id = O.Id
                                LEFT JOIN [order].[Vouchers] V ON V.Id = O.VoucherId
                                INNER JOIN [order].[OrderItems] I ON O.Id = I.OrderId
                                ORDER BY O.RegisterDate DESC;";

            var lookup = new Dictionary<Guid, OrderDTO>();

            await _orderRepository.GetConnection()
                .QueryAsync<OrderDTO, AddressDTO, OrderItemDTO, OrderDTO>(
                    sql,
                    (o, addr, item) =>
                    {
                        if (!lookup.TryGetValue(o.Id, out var dto))
                        {
                            dto = o;
                            dto.Address = addr;
                            dto.OrderItems ??= new List<OrderItemDTO>();
                            lookup[o.Id] = dto;
                        }
                        dto.OrderItems.Add(item);
                        return dto;
                    },
                    new { customerId },
                    splitOn: "Street,OrderItemId");

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
                                    -- Order
                                    O.Id,
                                    O.CustomerId,
                                    O.Code,
                                    O.OrderStatus    AS Status,
                                    O.RegisterDate   AS [Date],
                                    O.TotalValue,
                                    O.Discount,
                                    O.VoucherUsed,
                                    V.Code           AS VoucherCode,   -- <- aqui

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
                                    I.ProductName    AS [Name],
                                    I.Quantity,
                                    I.UnitValue      AS [Value],
                                    I.ProductImage   AS [Image]
                                FROM [order].[Orders] O
                                LEFT JOIN [order].[Vouchers] V ON V.Id = O.VoucherId
                                INNER JOIN [order].[OrderItems] I ON O.Id = I.OrderId
                                WHERE O.OrderStatus = 1
        ORDER BY O.RegisterDate;";

            var lookup = new Dictionary<Guid, OrderDTO>();

            await _orderRepository.GetConnection()
                .QueryAsync<OrderDTO, AddressDTO, OrderItemDTO, OrderDTO>(
                    sql,
                    (o, addr, item) =>
                    {
                        if (!lookup.TryGetValue(o.Id, out var dto))
                        {
                            dto = o;
                            dto.Address = addr;
                            dto.OrderItems ??= new List<OrderItemDTO>();
                            lookup[o.Id] = dto;
                        }
                        dto.OrderItems.Add(item);
                        return dto;
                    },
                    splitOn: "Street,OrderItemId");

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

                OrderItems = new List<OrderItemDTO>(),
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

                order.OrderItems.Add(orderItem);
            }

            return order;
        }
    }
}