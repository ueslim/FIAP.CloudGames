using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Bff.Orders.Tests.Services
{
    internal sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public HttpRequestMessage? LastRequest { get; private set; }

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responder(request));
        }

        public static HttpResponseMessage Json(HttpStatusCode code, object payload) =>
            new HttpResponseMessage(code)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)),
                                            System.Text.Encoding.UTF8,
                                            "application/json")
            };
    }
}
