using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIAP.CloudGames.Cart.API.Model
{
    public class CartCustomer
    {
        internal const int MAX_QUANTITY_ITEM = 5;

        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal TotalValue { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public ValidationResult ValidationResult { get; set; }

        public bool UsedVoucher { get; set; }
        public decimal Discount { get; set; }

        public Voucher Voucher { get; set; }

        public CartCustomer(Guid customerId)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
        }

        public CartCustomer()
        {
        }

        public void ApplyVoucher(Voucher voucher)
        {
            Voucher = voucher;
            UsedVoucher = true;
            CalculateCartValue();
        }

        private void CalculateTotalValueDiscount()
        {
            if (!UsedVoucher) return;

            decimal discount = 0;
            var value = TotalValue;

            if (Voucher.VoucherDiscountType == VoucherDiscountType.Percentage)
            {
                if (Voucher.Percentage.HasValue)
                {
                    discount = value * Voucher.Percentage.Value / 100;
                    value -= discount;
                }
            }
            else
            {
                if (Voucher.DiscountValue.HasValue)
                {
                    discount = Voucher.DiscountValue.Value;
                    value -= discount;
                }
            }

            TotalValue = value < 0 ? 0 : value;
            Discount = discount;
        }


        internal void CalculateCartValue()
        {
            TotalValue = Items.Sum(p => p.CalculateValue());
            CalculateTotalValueDiscount();
        }

        internal bool CartExistingItem(CartItem item)
        {
            return Items.Any(p => p.ProductId == item.ProductId);
        }

        internal CartItem GetProductById(Guid productId)
        {
            return Items.FirstOrDefault(p => p.ProductId == productId);
        }

        internal void AddItem(CartItem item)
        {
            item.AssociateCart(Id);

            if (CartExistingItem(item))
            {
                var existingItem = GetProductById(item.ProductId);
                existingItem.AddUnits(item.Quantity);

                item = existingItem;
                Items.Remove(existingItem);
            }

            Items.Add(item);
            CalculateCartValue();
        }

        internal void UpdateItem(CartItem item)
        {
            item.AssociateCart(Id);

            var existingItem = GetProductById(item.ProductId);

            Items.Remove(existingItem);
            Items.Add(item);

            CalculateCartValue();
        }

        internal void UpdateUnits(CartItem item, int units)
        {
            item.UpdateUnits(units);
            UpdateItem(item);
        }

        internal void RemoveItem(CartItem item)
        {
            Items.Remove(GetProductById(item.ProductId));
            CalculateCartValue();
        }

        internal bool IsValid()
        {
            var errors = Items.SelectMany(i => new CartItem.CartItemValidation().Validate(i).Errors).ToList();
            errors.AddRange(new CartItemValidation().Validate(this).Errors);
            ValidationResult = new ValidationResult(errors);

            return ValidationResult.IsValid;
        }

        public class CartItemValidation : AbstractValidator<CartCustomer>
        {
            public CartItemValidation()
            {
                RuleFor(c => c.CustomerId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("Unrecognized customer");

                RuleFor(c => c.Items.Count)
                    .GreaterThan(0)
                    .WithMessage("Cart has no items");

                RuleFor(c => c.TotalValue)
                    .GreaterThan(0)
                    .WithMessage("The total value of the cart must be greater than 0");
            }
        }
    }
}