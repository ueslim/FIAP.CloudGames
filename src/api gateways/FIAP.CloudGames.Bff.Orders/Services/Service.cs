using FIAP.CloudGames.Core.Communication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FIAP.CloudGames.Bff.Orders.Services
{
    public abstract class Service
    {
        private static readonly JsonSerializerOptions _jsonOptions =
    new(JsonSerializerDefaults.Web);

        protected StringContent GetContent(object data)
        {
            return new StringContent(
                JsonSerializer.Serialize(data, _jsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        protected async Task<T> DeserializeResponseObject<T>(HttpResponseMessage responseMessage)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(await responseMessage.Content.ReadAsStringAsync(), options);
        }

        protected bool HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest) return false;

            response.EnsureSuccessStatusCode();
            return true;
        }

        protected ResponseResult OkReturn()
        {
            return new ResponseResult();
        }
    }
}