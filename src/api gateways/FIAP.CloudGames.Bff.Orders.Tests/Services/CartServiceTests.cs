using FIAP.CloudGames.Bff.Orders.Extensions;
using FIAP.CloudGames.Bff.Orders.Models;
using FIAP.CloudGames.Bff.Orders.Services;
using FIAP.CloudGames.Core.Communication;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Net;
using Xunit;

namespace FIAP.CloudGames.Bff.Orders.Tests.Services
{
    public class CartServiceTests
    {
        private static IOptions<AppServicesSettings> Settings(string baseUrl) =>
            Options.Create(new AppServicesSettings { CartUrl = baseUrl });

        private static CartService Make(StubHttpMessageHandler handler, string baseUrl = "https://cart.local")
            => new CartService(new HttpClient(handler) { BaseAddress = new Uri(baseUrl) }, Settings(baseUrl));

        [Fact]
        public async Task GetCart_Should_Get_And_Deserialize()
        {
            var cart = new CartDTO { TotalValue = 123.45m, VoucherUsed = false };
            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, cart));

            var svc = Make(handler);
            var result = await svc.GetCart();

            handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
            handler.LastRequest.RequestUri!.ToString().Should().Be("https://cart.local/cart/");
            result.TotalValue.Should().Be(123.45m);
        }

        [Fact]
        public async Task AddCartItem_Should_Return_Ok_On_2xx()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
            var svc = Make(handler);

            var resp = await svc.AddCartItem(new ItemCartDTO { ProductId = Guid.NewGuid(), Name = "P1", Quantity = 1, Value = 10 });

            handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
            handler.LastRequest.RequestUri!.ToString().Should().Be("https://cart.local/cart/");
            resp.Should().BeOfType<ResponseResult>();
            resp.Errors?.Messages.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task AddCartItem_Should_Return_ResponseResult_On_400()
        {
            var error = new ResponseResult();
            error.Errors = new ResponseErrorMessages { Messages = new List<string> { "invalid" } };

            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, error));
            var svc = Make(handler);

            var resp = await svc.AddCartItem(new ItemCartDTO());

            resp.Errors!.Messages.Should().ContainSingle("invalid");
        }

        [Fact]
        public async Task UpdateCartItem_Should_Put_To_ProductId_And_Return_Ok_On_2xx()
        {
            var productId = Guid.NewGuid();
            var item = new ItemCartDTO { ProductId = productId, Name = "X", Quantity = 2, Value = 20 };
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var svc = Make(handler);

            var resp = await svc.UpdateCartItem(productId, item);

            handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
            handler.LastRequest.RequestUri!.ToString().Should().Be($"https://cart.local/cart/{productId}");
            resp.Errors?.Messages.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task RemoveCartItem_Should_Delete_And_Return_Ok_On_2xx()
        {
            var id = Guid.NewGuid();
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
            var svc = Make(handler);

            var resp = await svc.RemoveCartItem(id);

            handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
            handler.LastRequest.RequestUri!.ToString().Should().Be($"https://cart.local/cart/{id}");
            resp.Errors?.Messages.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task ApplyCartVoucher_Should_Post_And_Return_Ok_On_2xx()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var svc = Make(handler);

            var resp = await svc.ApplyCartVoucher(new VoucherDTO { Code = "OFF10", DiscountType = 0, Percentage = 10 });

            handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
            handler.LastRequest.RequestUri!.ToString().Should().Be("https://cart.local/cart/apply-voucher/");
            resp.Errors?.Messages.Should().BeNullOrEmpty();
        }
    }
}