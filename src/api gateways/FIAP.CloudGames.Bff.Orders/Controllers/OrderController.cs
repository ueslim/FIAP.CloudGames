using FIAP.CloudGames.Bff.Orders.Models;
using FIAP.CloudGames.Bff.Orders.Services;
using FIAP.CloudGames.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FIAP.CloudGames.Bff.Orders.Controllers
{
    [Authorize]
    public class OrderController : MainController
    {
        private readonly ICatalogService _catalogService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;

        public OrderController(
            ICatalogService catalogService,
            ICartService cartService,
            IOrderService orderService,
            ICustomerService customerService)
        {
            _catalogService = catalogService;
            _cartService = cartService;
            _orderService = orderService;
            _customerService = customerService;
        }

        [HttpPost]
        [Route("shopping/order")]
        public async Task<IActionResult> AdicionarPedido(OrderDTO order)
        {
            var cart = await _cartService.GetCart();
            var products = await _catalogService.GetItems(cart.Items.Select(p => p.ProductId));
            var address = await _customerService.GetAddress();

            if (!await ValidateCartProducts(cart, products)) return CustomResponse();

            PopulateOrderData(cart, address, order);

            return CustomResponse(await _orderService.FinishOrder(order));
        }

        [HttpGet("shopping/order/last")]
        public async Task<IActionResult> LastOrder()
        {
            var order = await _orderService.GetLastOrder();

            if (order is null)
            {
                AddErrorMessage("Pedido não encontrado!");
                return CustomResponse();
            }

            return CustomResponse(order);
        }

        [HttpGet("shopping/order/list-customer")]
        public async Task<IActionResult> ListByCustomer()
        {
            var pedidos = await _orderService.GetListByCustomerId();

            return pedidos == null ? NotFound() : CustomResponse(pedidos);
        }

        private async Task<bool> ValidateCartProducts(CartDTO cart, IEnumerable<ItemProductDTO> products)
        {
            if (cart.Items.Count != products.Count())
            {
                var unavailableItems = cart.Items.Select(c => c.ProductId).Except(products.Select(p => p.Id)).ToList();

                foreach (var itemId in unavailableItems)
                {
                    var cartItem = cart.Items.FirstOrDefault(c => c.ProductId == itemId);
                    AddErrorMessage($"O item {cartItem.Name} não está mais disponível no catálogo, o remova do carrinho para prosseguir com a compra");
                }

                return false;
            }

            foreach (var cartItem in cart.Items)
            {
                var catalogProduct = products.FirstOrDefault(p => p.Id == cartItem.ProductId);

                if (catalogProduct.Value != cartItem.Value)
                {
                    var msgErro = $"O produto {cartItem.Name} mudou de valor (de: " +
                                  $"{string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:C}", cartItem.Value)} para: " +
                                  $"{string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:C}", catalogProduct.Value)}) desde que foi adicionado ao carrinho.";

                    AddErrorMessage(msgErro);

                    var removalResponse = await _cartService.RemoveCartItem(cartItem.ProductId);

                    if (ResponseHasErrors(removalResponse))
                    {
                        AddErrorMessage($"Não foi possível remover automaticamente o produto {cartItem.Name} do seu carrinho, _" +
                                                   "remova e adicione novamente caso ainda deseje comprar este item");
                        return false;
                    }

                    cartItem.Value = catalogProduct.Value;
                    var addResponse = await _cartService.AddCartItem(cartItem);

                    if (ResponseHasErrors(addResponse))
                    {
                        AddErrorMessage($"Não foi possível atualizar automaticamente o produto {cartItem.Name} do seu carrinho, _" +
                                                   "adicione novamente caso ainda deseje comprar este item");
                        return false;
                    }

                    ClearErrorMessages();
                    AddErrorMessage(msgErro + " Atualizamos o valor em seu carrinho, realize a conferência do pedido e se preferir remova o produto");

                    return false;
                }
            }

            return true;
        }

        private void PopulateOrderData(CartDTO cart, AddressDTO address, OrderDTO order)
        {
            order.VoucherCode = cart.Voucher?.Code;
            order.VoucherUsed = cart.VoucherUsed;
            order.TotalValue = cart.TotalValue;
            order.Discount = cart.Discount;
            order.CartItems = cart.Items;
            order.Address = address;
        }
    }
}