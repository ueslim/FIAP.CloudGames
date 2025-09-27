using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FIAP.CloudGames.Bff.Orders.Extensions;
using FIAP.CloudGames.Bff.Orders.Models;
using FIAP.CloudGames.Bff.Orders.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FIAP.CloudGames.Bff.Orders.Tests.Services
{
    public class CustomerServiceTests
    {
        private static IOptions<AppServicesSettings> Settings(string baseUrl) =>
            Options.Create(new AppServicesSettings { CustomerUrl = baseUrl });

        private static CustomerService Make(StubHttpMessageHandler handler, string baseUrl = "https://customer.local")
            => new CustomerService(new HttpClient(handler) { BaseAddress = new Uri(baseUrl) }, Settings(baseUrl));

        [Fact]
        public async Task GetAddress_Should_Return_Null_On_404()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var svc = Make(handler);

            var result = await svc.GetAddress();

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAddress_Should_Get_And_Deserialize()
        {
            var addr = new AddressDTO { Street = "Main", Number = "123" };
            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, addr));
            var svc = Make(handler);

            var result = await svc.GetAddress();

            handler.LastRequest!.RequestUri!.ToString().Should().Be("https://customer.local/customer/address/");
            result.Street.Should().Be("Main");
            result.Number.Should().Be("123");
        }
    }
}
