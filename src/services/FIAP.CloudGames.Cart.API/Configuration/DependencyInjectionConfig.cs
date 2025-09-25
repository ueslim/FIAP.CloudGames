using FIAP.CloudGames.Cart.API.Data;
using FIAP.CloudGames.WebAPI.Core.User;

namespace FIAP.CloudGames.Cart.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAspNetUser, AspNetUser>();
            services.AddScoped<CartContext>();
        }
    }
}