using System;
using System.Net.Http;
using FIAP.CloudGames.Bff.Orders.Extensions;
using Microsoft.Extensions.Options;

namespace FIAP.CloudGames.Bff.Orders.Services
{
    public interface IPaymentService
    {
    }

    public class PaymentService : Service, IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.PaymentUrl);
        }
    }
}