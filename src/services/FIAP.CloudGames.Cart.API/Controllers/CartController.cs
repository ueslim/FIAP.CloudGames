using FIAP.CloudGames.Cart.API.Data;
using FIAP.CloudGames.Cart.API.Model;
using FIAP.CloudGames.WebAPI.Core.Controllers;
using FIAP.CloudGames.WebAPI.Core.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Cart.API.Controllers
{
    [Authorize]
    public class CartController : MainController
    {
        private readonly IAspNetUser _user;
        private readonly CartContext _context;

        public CartController(IAspNetUser user, CartContext context)
        {
            _user = user;
            _context = context;
        }

        [HttpGet("cart")]
        public async Task<CartCustomer> GetCart()
        {
            return await GetCustomerCart() ?? new CartCustomer();
        }

        [HttpPost]
        [Route("cart/apply-voucher")]
        public async Task<IActionResult> ApplyVoucher(Voucher voucher)
        {
            var cart = await GetCustomerCart();

            cart.ApplyVoucher(voucher);

            _context.CartCustomer.Update(cart);

            await PersistData();
            return CustomResponse();
        }

        [HttpPost("cart")]
        public async Task<IActionResult> AddCartItem(CartItem item)
        {
            var cart = await GetCustomerCart();

            if (cart == null)
                ManipulateNewCart(item);
            else
                ManipulateExistingCart(cart, item);

            if (!ValidOperation()) return CustomResponse();

            await PersistData();
            return CustomResponse();
        }

        [HttpPut("cart/{productId}")]
        public async Task<IActionResult> UpdateCartItem(Guid productId, CartItem item)
        {
            var cart = await GetCustomerCart();
            var cartItem = await GetCartItemValidated(productId, cart, item);
            if (cartItem == null) return CustomResponse();

            cart.UpdateUnits(cartItem, item.Quantity);

            ValidateCart(cart);
            if (!ValidOperation()) return CustomResponse();

            _context.CartItems.Update(cartItem);
            _context.CartCustomer.Update(cart);

            await PersistData();
            return CustomResponse();
        }

        [HttpDelete("cart/{productId}")]
        public async Task<IActionResult> RemoveCartItem(Guid productId)
        {
            var cart = await GetCustomerCart();

            var cartItem = await GetCartItemValidated(productId, cart);
            if (cartItem == null) return CustomResponse();

            ValidateCart(cart);
            if (!ValidOperation()) return CustomResponse();

            cart.RemoveItem(cartItem);

            _context.CartItems.Remove(cartItem);
            _context.CartCustomer.Update(cart);

            await PersistData();
            return CustomResponse();
        }

        private async Task<CartCustomer> GetCustomerCart()
        {
            return await _context.CartCustomer
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == _user.GetUserId());
        }

        private void ManipulateNewCart(CartItem item)
        {
            var cart = new CartCustomer(_user.GetUserId());
            cart.AddItem(item);

            ValidateCart(cart);
            _context.CartCustomer.Add(cart);
        }

        private void ManipulateExistingCart(CartCustomer cart, CartItem item)
        {
            var existingProductItem = cart.CartExistingItem(item);

            cart.AddItem(item);
            ValidateCart(cart);

            if (existingProductItem)
            {
                _context.CartItems.Update(cart.GetProductById(item.ProductId));
            }
            else
            {
                _context.CartItems.Add(item);
            }

            _context.CartCustomer.Update(cart);
        }

        private async Task<CartItem> GetCartItemValidated(Guid productId, CartCustomer cart, CartItem item = null)
        {
            if (item != null && productId != item.ProductId)
            {
                AddErrorMessage("The item does not match the information");
                return null;
            }

            if (cart == null)
            {
                AddErrorMessage("Cart not found");
                return null;
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == productId);

            if (cartItem == null || !cart.CartExistingItem(cartItem))
            {
                AddErrorMessage("Item is not in cart");
                return null;
            }

            return cartItem;
        }

        private async Task PersistData()
        {
            var result = await _context.SaveChangesAsync();
            if (result <= 0) AddErrorMessage("Unable to persist data in database");
        }

        private bool ValidateCart(CartCustomer cart)
        {
            if (cart.IsValid()) return true;

            cart.ValidationResult.Errors.ToList().ForEach(e => AddErrorMessage(e.ErrorMessage));
            return false;
        }
    }
}