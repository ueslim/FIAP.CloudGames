using FIAP.CloudGames.Core.Mediator;
using FIAP.CloudGames.Order.API.Application.Commands;
using FIAP.CloudGames.Order.API.Application.Queries;
using FIAP.CloudGames.WebAPI.Core.Controllers;
using FIAP.CloudGames.WebAPI.Core.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Order.API.Controllers
{
    [Authorize]
    public class OrderController : MainController
    {
        private readonly IMediatorHandler _mediator;
        private readonly IAspNetUser _user;
        private readonly IOrderQueries _orderQueries;

        public OrderController(IMediatorHandler mediator, IAspNetUser user, IOrderQueries orderQueries)
        {
            _mediator = mediator;
            _user = user;
            _orderQueries = orderQueries;
        }

        [HttpPost("order")]
        public async Task<IActionResult> AddOrder(AddOrderCommand order)
        {
            order.CustomerId = _user.GetUserId();
            return CustomResponse(await _mediator.SendCommand(order));
        }

        [HttpGet("order/last")]
        public async Task<IActionResult> LastOrder()
        {
            var order = await _orderQueries.GetLastOrder(_user.GetUserId());

            return order == null ? NotFound() : CustomResponse(order);
        }

        [HttpGet("order/customer-list")]
        public async Task<IActionResult> ListByCustomer()
        {
            var orders = await _orderQueries.GetListByCustomerId(_user.GetUserId());

            return orders == null ? NotFound() : CustomResponse(orders);
        }
    }
}