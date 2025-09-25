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
            const string sql = @"SELECT
                                P.ID AS 'ProdutoId', P.CODIGO, P.VOUCHERUTILIZADO, P.DESCONTO, P.VALORTOTAL,P.PEDIDOSTATUS,
                                P.LOGRADOURO,P.NUMERO, P.BAIRRO, P.CEP, P.COMPLEMENTO, P.CIDADE, P.ESTADO,
                                PIT.ID AS 'ProdutoItemId',PIT.PRODUTONOME, PIT.QUANTIDADE, PIT.PRODUTOIMAGEM, PIT.VALORUNITARIO
                                FROM PEDIDOS P
                                INNER JOIN PEDIDOITEMS PIT ON P.ID = PIT.PEDIDOID
                                WHERE P.CLIENTEID = @clienteId
                                AND P.DATACADASTRO between DATEADD(minute, -3,  GETDATE()) and DATEADD(minute, 0,  GETDATE())
                                AND P.PEDIDOSTATUS = 1
                                ORDER BY P.DATACADASTRO DESC";

            var pedido = await _orderRepository.GetConnection()
                .QueryAsync<dynamic>(sql, new { customerId });

            return MapOrder(pedido);
        }

        public async Task<IEnumerable<OrderDTO>> GetListByCustomerId(Guid clienteId)
        {
            var pedidos = await _orderRepository.GetListByCustomer(clienteId);

            return pedidos.Select(OrderDTO.ToOrderDTO);
        }

        public async Task<OrderDTO> GetAuthorizedOrders()
        {
            // Correção para pegar todos os itens do pedido e ordernar pelo pedido mais antigo
            const string sql = @"SELECT
                                P.ID as 'PedidoId', P.ID, P.CLIENTEID,
                                PI.ID as 'PedidoItemId', PI.ID, PI.PRODUTOID, PI.QUANTIDADE
                                FROM PEDIDOS P
                                INNER JOIN PEDIDOITEMS PI ON P.ID = PI.PEDIDOID
                                WHERE P.PEDIDOSTATUS = 1
                                ORDER BY P.DATACADASTRO";

            // Utilizacao do lookup para manter o estado a cada ciclo de registro retornado
            var lookup = new Dictionary<Guid, OrderDTO>();

            await _orderRepository.GetConnection().QueryAsync<OrderDTO, OrderItemDTO, OrderDTO>(sql,
                (p, pi) =>
                {
                    if (!lookup.TryGetValue(p.Id, out var pedidoDTO))
                        lookup.Add(p.Id, pedidoDTO = p);

                    pedidoDTO.OrdetItems ??= new List<OrderItemDTO>();
                    pedidoDTO.OrdetItems.Add(pi);

                    return pedidoDTO;
                }, splitOn: "PedidoId,PedidoItemId");

            // Obtendo dados o lookup
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