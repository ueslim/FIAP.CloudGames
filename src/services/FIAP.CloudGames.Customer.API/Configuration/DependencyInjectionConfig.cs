using FIAP.CloudGames.Core.Events;
using FIAP.CloudGames.Core.Mediator;
using FIAP.CloudGames.Customer.API.Application.Commands;
using FIAP.CloudGames.Customer.API.Application.Events;
using FIAP.CloudGames.Customer.API.Data;
using FIAP.CloudGames.Customer.API.Data.Repository;
using FIAP.CloudGames.Customer.API.Models;
using FIAP.CloudGames.WebAPI.Core.User;
using FluentValidation.Results;
using MediatR;

namespace FIAP.CloudGames.Customer.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAspNetUser, AspNetUser>();

            services.AddScoped<IMediatorHandler, MediatorHandler>();

            services.AddScoped<IRequestHandler<RegisterCustomerCommand, ValidationResult>, CustomerCommandHandler>();
            services.AddScoped<IRequestHandler<AddAddressCommand, ValidationResult>, CustomerCommandHandler>();

            services.AddScoped<INotificationHandler<CustomerRegisteredEvent>, CustomerEventHandler>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<CustomerContext>();
            services.AddSingleton<IEventStore, NoOpEventStore>();
        }
    }
}