using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FIAP.CloudGames.Bff.Orders.Extensions;
using FIAP.CloudGames.Bff.Orders.Models;
using Microsoft.Extensions.Options;

namespace FIAP.CloudGames.Bff.Orders.Services
{
    public interface ICustomerService
    {
        Task<AddressDTO> GetAddress();
    }

    public class CustomerService : Service, ICustomerService
    {
        private readonly HttpClient _httpClient;

        public CustomerService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.CustomerUrl);
        }

        public async Task<AddressDTO> GetAddress()
        {
            var response = await _httpClient.GetAsync("/customer/address/");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            HandleErrorResponse(response);

            return await DeserializeResponseObject<AddressDTO>(response);
        }
    }
}