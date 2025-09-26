using FluentValidation;
using System.Text.Json.Serialization;

namespace FIAP.CloudGames.Cart.API.Model
{
    public class CartItem
    {
        public CartItem()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public string Image { get; set; }

        public Guid CartId { get; set; }

        [JsonIgnore]
        public CartCustomer CartCustomer { get; set; }

        internal void AssociateCart(Guid cartId)
        {
            CartId = cartId;
        }

        internal decimal CalculateValue()
        {
            return Quantity * Value;
        }

        internal void AddUnits(int unit)
        {
            Quantity += unit;
        }

        internal void UpdateUnits(int units)
        {
            Quantity = units;
        }

        internal bool IsValid()
        {
            return new CartItemValidation().Validate(this).IsValid;
        }

        public class CartItemValidation : AbstractValidator<CartItem>
        {
            public CartItemValidation()
            {
                RuleFor(c => c.ProductId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("Invalid product id");

                RuleFor(c => c.Name)
                    .NotEmpty()
                    .WithMessage("The product name was not informed");

                RuleFor(c => c.Quantity)
                    .GreaterThan(0)
                    .WithMessage(item => $"The minimum quantity for {item.Name} is 1");

                RuleFor(c => c.Quantity)
                    .LessThanOrEqualTo(CartCustomer.MAX_QUANTITY_ITEM)
                    .WithMessage(item => $"The maximum quantity of {item.Name} is {CartCustomer.MAX_QUANTITY_ITEM}");

                RuleFor(c => c.Value)
                    .GreaterThan(0)
                    .WithMessage(item => $"The value of {item.Name} must be greater than 0");
            }
        }
    }
}