using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlatinumPOS.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string Icon { get; set; } = string.Empty; // MudBlazor Icon name or identifier
        public string Color { get; set; } = string.Empty; // Custom hex or palette color name
        
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public int Id { get; set; }
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int StockQuantity { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty; // Hex color for product tile
        public bool IsActive { get; set; } = true;
    }

    public class Cashier
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string PinHash { get; set; } = string.Empty; // BCrypt hash of 4-digit pin
        
        [Required]
        public string Role { get; set; } = "Cashier"; // Manager or Cashier
        
        public bool IsActive { get; set; } = true;
    }

    public class Transaction
    {
        public int Id { get; set; }
        
        public int CashierId { get; set; }
        public Cashier? Cashier { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Split
        public decimal CashReceived { get; set; }
        public decimal CardAmount { get; set; }
        public decimal ChangeGiven { get; set; }
        
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Completed"; // Completed, Refunded, Voided
        
        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
    }

    public class TransactionItem
    {
        public int Id { get; set; }
        
        public int TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        
        public int? ProductId { get; set; }
        
        [Required]
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class SystemSetting
    {
        [Key]
        [Required]
        public string Key { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CashierName { get; set; } = "System";
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;
        
        public string Details { get; set; } = string.Empty;
    }
}
