using FIAP.CloudGames.Core.Mediator;
using FIAP.CloudGames.Customer.API.Application.Commands;
using FIAP.CloudGames.Customer.API.Models;
using FIAP.CloudGames.WebAPI.Core.Controllers;
using FIAP.CloudGames.WebAPI.Core.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Customer.API.Controllers
{
    [Authorize]
    public class CustomerController : MainController
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMediatorHandler _mediator;
        private readonly IAspNetUser _user;

        public CustomerController(ICustomerRepository customerRepository, IMediatorHandler mediator, IAspNetUser user)
        {
            _customerRepository = customerRepository;
            _mediator = mediator;
            _user = user;
        }

        [HttpGet("customer/address")]
        public async Task<IActionResult> GetAddress()
        {
            var address = await _customerRepository.GetAddressById(_user.GetUserId());

            return address == null ? NotFound() : CustomResponse(address);
        }

        [HttpPost("customer/address")]
        public async Task<IActionResult> AddAddress(AddAddressCommand address)
        {
            address.CustomerId = _user.GetUserId();
            return CustomResponse(await _mediator.SendCommand(address));
        }
    }
}