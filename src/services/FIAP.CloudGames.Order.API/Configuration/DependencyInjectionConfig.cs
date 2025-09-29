using FIAP.CloudGames.Core.Events;
using FIAP.CloudGames.Core.Mediator;
using FIAP.CloudGames.Order.API.Application.Commands;
using FIAP.CloudGames.Order.API.Application.Events;
using FIAP.CloudGames.Order.API.Application.Queries;
using FIAP.CloudGames.Order.Domain.Order;
using FIAP.CloudGames.Order.Domain.Voucher;
using FIAP.CloudGames.Order.Infra.Data;
using FIAP.CloudGames.Order.Infra.Data.EventSourcing;
using FIAP.CloudGames.Order.Infra.Data.Repository;
using FIAP.CloudGames.Order.Infra.Data.Repository.EventSourcing;
using FIAP.CloudGames.WebAPI.Core.User;
using FluentValidation.Results;
using MediatR;

namespace FIAP.CloudGames.Order.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            // API
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAspNetUser, AspNetUser>();

            services.AddScoped<IMediatorHandler, MediatorHandler>();

            // Commands
            services.AddScoped<IRequestHandler<AddOrderCommand, ValidationResult>, OrderCommandHandler>();

            // Events
            services.AddScoped<INotificationHandler<OrderFinishedEvent>, OrderEventHandler>();

            // Application
            services.AddScoped<IMediatorHandler, MediatorHandler>();
            services.AddScoped<IVoucherQueries, VoucherQueries>();
            services.AddScoped<IOrderQueries, OrderQueries>();

            // Data
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<IEventStore, SqlEventStore>();
            services.AddScoped<IEventStoreRepository, EventStoreSqlRepository>();
        }
    }
}