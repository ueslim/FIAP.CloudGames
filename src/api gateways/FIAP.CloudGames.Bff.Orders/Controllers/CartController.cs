using FIAP.CloudGames.Bff.Orders.Models;
using FIAP.CloudGames.Bff.Orders.Services;
using FIAP.CloudGames.WebAPI.Core.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Bff.Orders.Controllers
{
    [Authorize]
    public class CartController : MainController
    {
        private readonly ICartService _cartService;
        private readonly ICatalogService _catalogService;
        private readonly IOrderService _orderService;

        public CartController(
            ICartService cartService,
            ICatalogService catalogService,
            IOrderService orderService)
        {
            _cartService = cartService;
            _catalogService = catalogService;
            _orderService = orderService;
        }

        [HttpGet]
        [Route("shopping/cart")]
        public async Task<IActionResult> Index()
        {
            return CustomResponse(await _cartService.GetCart());
        }

        [HttpGet]
        [Route("shopping/cart-quantity")]
        public async Task<int> ObterQuantidadeCarrinho()
        {
            var quantity = await _cartService.GetCart();
            return quantity?.Items.Sum(i => i.Quantity) ?? 0;
        }

        [HttpPost]
        [Route("shopping/cart/items")]
        public async Task<IActionResult> AddCartItem(ItemCartDTO itemCart)
        {
            var product = await _catalogService.GetById(itemCart.ProductId);

            await ValidateItemCart(product, itemCart.Quantity, true);
            if (!ValidOperation()) return CustomResponse();

            itemCart.Name = product.Name;
            itemCart.Value = product.Value;
            itemCart.Image = product.Image;

            var response = await _cartService.AddCartItem(itemCart);

            return CustomResponse(response);
        }

        [HttpPut]
        [Route("shopping/cart/items/{productId}")]
        public async Task<IActionResult> AtualizarItemCarrinho(Guid productId, ItemCartDTO itemCart)
        {
            var product = await _catalogService.GetById(productId);

            await ValidateItemCart(product, itemCart.Quantity);
            if (!ValidOperation()) return CustomResponse();

            var response = await _cartService.UpdateCartItem(productId, itemCart);

            return CustomResponse(response);
        }

        [HttpDelete]
        [Route("shopping/cart/items/{productId}")]
        public async Task<IActionResult> RemoverItemCarrinho(Guid productId)
        {
            var product = await _catalogService.GetById(productId);

            if (product == null)
            {
                AddErrorMessage("Produto inexistente!");
                return CustomResponse();
            }

            var response = await _cartService.RemoveCartItem(productId);

            return CustomResponse(response);
        }

        [HttpPost]
        [Route("compras/carrinho/aplicar-voucher")]
        public async Task<IActionResult> AplicarVoucher([FromBody] string voucherCode)
        {
            var voucher = await _orderService.GetVoucherByCode(voucherCode);

            if (voucher is null)
            {
                AddErrorMessage("Voucher inválido ou não encontrado!");
                return CustomResponse();
            }

            var response = await _cartService.ApplyCartVoucher(voucher);

            return CustomResponse(response);
        }

        private async Task ValidateItemCart(ItemProductDTO product, int quantity, bool addProduct = false)
        {
            if (product == null) AddErrorMessage("Produto inexistente!");
            if (quantity < 1) AddErrorMessage($"Escolha ao menos uma unidade do produto {product.Name}");

            var cart = await _cartService.GetCart();
            var itemCarrinho = cart.Items.FirstOrDefault(p => p.ProductId == product.Id);

            if (itemCarrinho != null && addProduct && itemCarrinho.Quantity + quantity > product.StockQuantity)
            {
                AddErrorMessage($"O produto {product.Name} possui {product.StockQuantity} unidades em estoque, você selecionou {quantity}");
                return;
            }

            if (quantity > product.StockQuantity) AddErrorMessage($"O produto {product.Name} possui {product.StockQuantity} unidades em estoque, você selecionou {quantity}");
        }
    }
}