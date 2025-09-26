using FIAP.CloudGames.Payment.API.Data;
using FIAP.CloudGames.Payment.API.Data.Repository;
using FIAP.CloudGames.Payment.API.Facade;
using FIAP.CloudGames.Payment.API.Models;
using FIAP.CloudGames.Payment.API.Services;
using FIAP.CloudGames.WebAPI.Core.User;

namespace FIAP.CloudGames.Payment.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAspNetUser, AspNetUser>();

            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentFacade, PaymentCreditCardFacade>();

            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<PaymentContext>();
        }
    }
}