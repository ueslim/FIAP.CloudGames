using FIAP.CloudGames.Bff.Orders.Extensions;
using FIAP.CloudGames.Bff.Orders.Models;
using FIAP.CloudGames.Core.Communication;
using Microsoft.Extensions.Options;

namespace FIAP.CloudGames.Bff.Orders.Services
{
    public interface ICartService
    {
        Task<CartDTO> GetCart();

        Task<ResponseResult> AddCartItem(ItemCartDTO product);

        Task<ResponseResult> UpdateCartItem(Guid productId, ItemCartDTO cart);

        Task<ResponseResult> RemoveCartItem(Guid productId);

        Task<ResponseResult> ApplyCartVoucher(VoucherDTO voucher);
    }

    public class CartService : Service, ICartService
    {
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.CartUrl);
        }

        public async Task<CartDTO> GetCart()
        {
            var response = await _httpClient.GetAsync("/cart/");

            HandleErrorResponse(response);

            return await DeserializeResponseObject<CartDTO>(response);
        }

        public async Task<ResponseResult> AddCartItem(ItemCartDTO product)
        {
            var itemContent = GetContent(product);

            var response = await _httpClient.PostAsync("/cart/", itemContent);

            if (!HandleErrorResponse(response)) return await DeserializeResponseObject<ResponseResult>(response);

            return OkReturn();
        }

        public async Task<ResponseResult> UpdateCartItem(Guid productId, ItemCartDTO cart)
        {
            var itemContent = GetContent(cart);

            var response = await _httpClient.PutAsync($"/cart/{cart.ProductId}", itemContent);

            if (!HandleErrorResponse(response)) return await DeserializeResponseObject<ResponseResult>(response);

            return OkReturn();
        }

        public async Task<ResponseResult> RemoveCartItem(Guid productId)
        {
            var response = await _httpClient.DeleteAsync($"/cart/{productId}");

            if (!HandleErrorResponse(response)) return await DeserializeResponseObject<ResponseResult>(response);

            return OkReturn();
        }

        public async Task<ResponseResult> ApplyCartVoucher(VoucherDTO voucher)
        {
            var itemContent = GetContent(voucher);

            var response = await _httpClient.PostAsync("/cart/apply-voucher/", itemContent);

            if (!HandleErrorResponse(response)) return await DeserializeResponseObject<ResponseResult>(response);

            return OkReturn();
        }
    }
}