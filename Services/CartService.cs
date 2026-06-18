using PlatinumPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatinumPOS.Services
{
    public class CartItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Notes { get; set; } = string.Empty;

        public decimal LineTotal => (UnitPrice * Quantity) - DiscountAmount;
    }

    public class CartService
    {
        public List<CartItem> Items { get; private set; } = new();
        public decimal CartDiscountPercent { get; set; } = 0; // overall cart discount %
        
        public event Action? OnChange;

        public void AddItem(Product product)
        {
            var existing = Items.FirstOrDefault(i => i.Product.Id == product.Id && string.IsNullOrEmpty(i.Notes));
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                Items.Add(new CartItem
                {
                    Product = product,
                    UnitPrice = product.Price,
                    Quantity = 1
                });
            }
            NotifyStateChanged();
        }

        public void RemoveItem(CartItem item)
        {
            Items.Remove(item);
            NotifyStateChanged();
        }

        public void UpdateQuantity(CartItem item, int quantity)
        {
            if (quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            NotifyStateChanged();
        }

        public void ApplyItemDiscount(CartItem item, decimal amount)
        {
            item.DiscountAmount = Math.Max(0, amount);
            NotifyStateChanged();
        }

        public void SetItemNotes(CartItem item, string notes)
        {
            item.Notes = notes;
            NotifyStateChanged();
        }

        public void ClearCart()
        {
            Items.Clear();
            CartDiscountPercent = 0;
            NotifyStateChanged();
        }

        public decimal GetRawSubtotal()
        {
            return Items.Sum(i => i.LineTotal);
        }

        public decimal GetTotalDiscount(decimal subtotal)
        {
            return (subtotal * (CartDiscountPercent / 100m));
        }

        public decimal GetFinalTotal()
        {
            var sub = GetRawSubtotal();
            var disc = GetTotalDiscount(sub);
            return Math.Max(0, sub - disc);
        }

        public decimal GetTaxAmount(decimal total, decimal taxRate)
        {
            // Tax inclusive price calculation: Tax = Total * (TaxRate / (100 + TaxRate))
            var factor = taxRate / (100m + taxRate);
            return Math.Round(total * factor, 2);
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
