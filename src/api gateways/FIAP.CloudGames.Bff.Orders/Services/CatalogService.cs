using FIAP.CloudGames.Bff.Orders.Extensions;
using FIAP.CloudGames.Bff.Orders.Models;
using Microsoft.Extensions.Options;

namespace FIAP.CloudGames.Bff.Orders.Services
{
    public interface ICatalogService
    {
        Task<ItemProductDTO> GetById(Guid id);

        Task<IEnumerable<ItemProductDTO>> GetItems(IEnumerable<Guid> ids);
    }

    public class CatalogService : Service, ICatalogService
    {
        private readonly HttpClient _httpClient;

        public CatalogService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.CatalogUrl);
        }

        public async Task<ItemProductDTO> GetById(Guid id)
        {
            var response = await _httpClient.GetAsync($"/catalog/products/{id}");

            HandleErrorResponse(response);

            return await DeserializeResponseObject<ItemProductDTO>(response);
        }

        public async Task<IEnumerable<ItemProductDTO>> GetItems(IEnumerable<Guid> ids)
        {
            var idsRequest = string.Join(",", ids);

            var response = await _httpClient.GetAsync($"/catalog/products/list/{idsRequest}/");

            HandleErrorResponse(response);

            return await DeserializeResponseObject<IEnumerable<ItemProductDTO>>(response);
        }
    }
}