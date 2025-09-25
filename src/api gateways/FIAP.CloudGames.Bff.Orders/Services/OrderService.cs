using FIAP.CloudGames.Bff.Orders.Extensions;
using FIAP.CloudGames.Bff.Orders.Models;
using FIAP.CloudGames.Core.Communication;
using Microsoft.Extensions.Options;
using System.Net;

namespace FIAP.CloudGames.Bff.Orders.Services
{
    public interface IOrderService
    {
        Task<ResponseResult> FinishOrder(OrderDTO order);

        Task<OrderDTO> GetLastOrder();

        Task<IEnumerable<OrderDTO>> GetListByCustomerId();

        Task<VoucherDTO> GetVoucherByCode(string code);
    }

    public class OrderService : Service, IOrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.OrderUrl);
        }

        public async Task<ResponseResult> FinishOrder(OrderDTO order)
        {
            var orderContent = GetContent(order);

            var response = await _httpClient.PostAsync("/order/", orderContent);

            if (!HandleErrorResponse(response)) return await DeserializeResponseObject<ResponseResult>(response);

            return OkReturn();
        }

        public async Task<OrderDTO> GetLastOrder()
        {
            var response = await _httpClient.GetAsync("/order/last/");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            HandleErrorResponse(response);

            return await DeserializeResponseObject<OrderDTO>(response);
        }

        public async Task<IEnumerable<OrderDTO>> GetListByCustomerId()
        {
            var response = await _httpClient.GetAsync("/order/list-customer/");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            HandleErrorResponse(response);

            return await DeserializeResponseObject<IEnumerable<OrderDTO>>(response);
        }

        public async Task<VoucherDTO> GetVoucherByCode(string code)
        {
            var response = await _httpClient.GetAsync($"/voucher/{code}/");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            HandleErrorResponse(response);

            return await DeserializeResponseObject<VoucherDTO>(response);
        }
    }
}