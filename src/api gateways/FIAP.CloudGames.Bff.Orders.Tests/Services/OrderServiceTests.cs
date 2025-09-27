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
    public class OrderServiceTests
    {
        private static IOptions<AppServicesSettings> Settings(string baseUrl) =>
            Options.Create(new AppServicesSettings { OrderUrl = baseUrl });

        private static OrderService Make(StubHttpMessageHandler handler, string baseUrl = "https://order.local")
            => new OrderService(new HttpClient(handler) { BaseAddress = new Uri(baseUrl) }, Settings(baseUrl));

        [Fact]
        public async Task FinishOrder_Should_Return_Ok_On_2xx()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Created));
            var svc = Make(handler);

            var resp = await svc.FinishOrder(new OrderDTO { CardName = "J", CardNumber = "4111111111111111", CardExpirationDate = "12/29", CvvCard = "123" });

            handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
            handler.LastRequest.RequestUri!.ToString().Should().Be("https://order.local/order/");
            resp.Errors?.Messages.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task FinishOrder_Should_Return_ResponseResult_On_400()
        {
            var rr = new ResponseResult
            {
                Errors = new ResponseErrorMessages
                {
                    Messages = new List<string> { "err" }
                }
            };
            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.BadRequest, rr));
            var svc = Make(handler);

            var resp = await svc.FinishOrder(new OrderDTO { CardName = "J", CardNumber = "4111111111111111", CardExpirationDate = "12/29", CvvCard = "123" });

            resp.Errors!.Messages.Should().Contain("err");
        }

        [Fact]
        public async Task GetLastOrder_Should_Return_Null_On_404()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var svc = Make(handler);

            var result = await svc.GetLastOrder();

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLastOrder_Should_Get_And_Deserialize()
        {
            var order = new OrderDTO { Code = 42, TotalValue = 50 };
            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, order));
            var svc = Make(handler);

            var result = await svc.GetLastOrder();

            handler.LastRequest!.RequestUri!.ToString().Should().Be("https://order.local/order/last/");
            result.Code.Should().Be(42);
        }

        [Fact]
        public async Task GetListByCustomerId_Should_Return_Null_On_404()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var svc = Make(handler);

            var result = await svc.GetListByCustomerId();

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetListByCustomerId_Should_Get_And_Deserialize()
        {
            var list = new[] { new OrderDTO { Code = 1 }, new OrderDTO { Code = 2 } };
            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, list));
            var svc = Make(handler);

            var result = await svc.GetListByCustomerId();

            handler.LastRequest!.RequestUri!.ToString().Should().Be("https://order.local/order/customer-list/");
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetVoucherByCode_Should_Return_Null_On_404()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var svc = Make(handler);

            var result = await svc.GetVoucherByCode("OFF10");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetVoucherByCode_Should_Get_And_Deserialize()
        {
            var voucher = new VoucherDTO { Code = "OFF10", Percentage = 10, DiscountType = 0 };
            var handler = new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, voucher));
            var svc = Make(handler);

            var result = await svc.GetVoucherByCode("OFF10");

            handler.LastRequest!.RequestUri!.ToString().Should().Be("https://order.local/voucher/OFF10/");
            result!.Code.Should().Be("OFF10");
        }
    }
}