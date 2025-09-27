using System;
using System.Linq;
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
    public class CatalogServiceTests
    {
        private static IOptions<AppServicesSettings> Settings(string baseUrl) =>
            Options.Create(new AppServicesSettings { CatalogUrl = baseUrl });

        private static CatalogService Make(StubHttpMessageHandler handler, string baseUrl = "https://catalog.local")
            => new CatalogService(new HttpClient(handler) { BaseAddress = new Uri(baseUrl) }, Settings(baseUrl));

        [Fact]
        public async Task GetById_Should_Get_And_Deserialize()
        {
            var id = Guid.NewGuid();
            var product = new ItemProductDTO { Id = id, Name = "Game", Value = 99.9m, StockQuantity = 5 };

            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, product));
            var svc = Make(handler);

            var result = await svc.GetById(id);

            handler.LastRequest!.RequestUri!.ToString().Should().Be($"https://catalog.local/catalog/products/{id}");
            result.Id.Should().Be(id);
            result.Name.Should().Be("Game");
        }

        [Fact]
        public async Task GetItems_Should_Format_Ids_And_Deserialize_List()
        {
            var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var payload = new[]
            {
                new ItemProductDTO { Id = ids[0], Name="A", Value=1 },
                new ItemProductDTO { Id = ids[1], Name="B", Value=2 }
            };

            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, payload));
            var svc = Make(handler);

            var result = (await svc.GetItems(ids)).ToList();

            handler.LastRequest!.RequestUri!.ToString()
                .Should().Be($"https://catalog.local/catalog/products/list/{ids[0]},{ids[1]}/");
            result.Should().HaveCount(2);
        }
    }
}
