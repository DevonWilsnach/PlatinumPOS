using System;
using System.Collections.Generic;
using System.Linq;
using PlatinumPOS.Models;

namespace PlatinumPOS.Data
{
    public static class GadgetShopSeeder
    {
        public static void Seed(PosDbContext context)
        {
            if (context.Categories.Any())
            {
                context.Transactions.RemoveRange(context.Transactions);
                context.Products.RemoveRange(context.Products);
                context.Categories.RemoveRange(context.Categories);
                context.SaveChanges();
            }

            // 1. Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Laptops", Icon = "Icons.Material.Filled.Laptop", Color = "#1E88E5" },
                new Category { Name = "Screens", Icon = "Icons.Material.Filled.Monitor", Color = "#E53935" },
                new Category { Name = "Cables", Icon = "Icons.Material.Filled.Cable", Color = "#43A047" },
                new Category { Name = "Peripherals", Icon = "Icons.Material.Filled.Mouse", Color = "#FB8C00" },
                new Category { Name = "Audio", Icon = "Icons.Material.Filled.Headphones", Color = "#8E24AA" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            // 2. Seed Products
            var products = new List<Product>
            {
                // Laptops
                new Product { CategoryId = categories[0].Id, Name = "MacBook Pro 16\"", Price = 45000m, Cost = 38000m, StockQuantity = 10, Barcode = "MAC16-001", Color = "#1E88E5", IsActive = true },
                new Product { CategoryId = categories[0].Id, Name = "Dell XPS 13", Price = 25000m, Cost = 20000m, StockQuantity = 15, Barcode = "DELL13-002", Color = "#1E88E5", IsActive = true },
                new Product { CategoryId = categories[0].Id, Name = "Lenovo ThinkPad X1", Price = 32000m, Cost = 26000m, StockQuantity = 8, Barcode = "LENX1-003", Color = "#1E88E5", IsActive = true },
                
                // Screens
                new Product { CategoryId = categories[1].Id, Name = "LG 27\" 4K Monitor", Price = 8000m, Cost = 6000m, StockQuantity = 20, Barcode = "LG27-001", Color = "#E53935", IsActive = true },
                new Product { CategoryId = categories[1].Id, Name = "Samsung 34\" Ultrawide", Price = 15000m, Cost = 11000m, StockQuantity = 12, Barcode = "SAM34-002", Color = "#E53935", IsActive = true },
                new Product { CategoryId = categories[1].Id, Name = "Dell 24\" 1080p Monitor", Price = 3500m, Cost = 2500m, StockQuantity = 35, Barcode = "DELL24-003", Color = "#E53935", IsActive = true },

                // Cables
                new Product { CategoryId = categories[2].Id, Name = "USB-C to USB-C 1m", Price = 250m, Cost = 100m, StockQuantity = 150, Barcode = "USBC-1M", Color = "#43A047", IsActive = true },
                new Product { CategoryId = categories[2].Id, Name = "HDMI Cable 2m", Price = 180m, Cost = 60m, StockQuantity = 200, Barcode = "HDMI-2M", Color = "#43A047", IsActive = true },
                new Product { CategoryId = categories[2].Id, Name = "DisplayPort Cable 1.5m", Price = 300m, Cost = 120m, StockQuantity = 80, Barcode = "DP-15M", Color = "#43A047", IsActive = true },
                new Product { CategoryId = categories[2].Id, Name = "Cat6 Ethernet Cable 5m", Price = 150m, Cost = 50m, StockQuantity = 120, Barcode = "CAT6-5M", Color = "#43A047", IsActive = true },

                // Peripherals
                new Product { CategoryId = categories[3].Id, Name = "Logitech MX Master 3S", Price = 2200m, Cost = 1500m, StockQuantity = 40, Barcode = "LOGIMX-3S", Color = "#FB8C00", IsActive = true },
                new Product { CategoryId = categories[3].Id, Name = "Keychron K2 Keyboard", Price = 1800m, Cost = 1200m, StockQuantity = 25, Barcode = "KEYK2-001", Color = "#FB8C00", IsActive = true },
                new Product { CategoryId = categories[3].Id, Name = "Logitech C920 Webcam", Price = 1500m, Cost = 900m, StockQuantity = 30, Barcode = "LOGIC920", Color = "#FB8C00", IsActive = true },
                new Product { CategoryId = categories[3].Id, Name = "Anker USB-C Hub", Price = 800m, Cost = 400m, StockQuantity = 60, Barcode = "ANKHUB-01", Color = "#FB8C00", IsActive = true },

                // Audio
                new Product { CategoryId = categories[4].Id, Name = "Sony WH-1000XM5", Price = 8500m, Cost = 6500m, StockQuantity = 15, Barcode = "SONYWH5", Color = "#8E24AA", IsActive = true },
                new Product { CategoryId = categories[4].Id, Name = "Apple AirPods Pro", Price = 5500m, Cost = 4200m, StockQuantity = 25, Barcode = "AIRPRO-2", Color = "#8E24AA", IsActive = true },
                new Product { CategoryId = categories[4].Id, Name = "JBL Flip 6 Speaker", Price = 2500m, Cost = 1600m, StockQuantity = 45, Barcode = "JBLFLIP6", Color = "#8E24AA", IsActive = true },
                new Product { CategoryId = categories[4].Id, Name = "Audio-Technica ATH-M50x", Price = 3500m, Cost = 2400m, StockQuantity = 20, Barcode = "ATHM50X", Color = "#8E24AA", IsActive = true }
            };

            context.Products.AddRange(products);
            context.SaveChanges();

            // 3. Seed Transactions (over the last 180 days - 6 months)
            var rand = new Random(123);
            var transactions = new List<Transaction>();
            
            // Just use any existing cashier or fallback to ID 1
            var cashierId = context.Cashiers.FirstOrDefault()?.Id ?? 1;
            
            const decimal taxRate = 0.15m;
            var today = DateTime.UtcNow.Date;
            
            for (int i = 0; i < 180; i++)
            {
                var currentDay = today.AddDays(-179 + i);
                
                int numTransactions = (currentDay.DayOfWeek == DayOfWeek.Saturday || currentDay.DayOfWeek == DayOfWeek.Sunday) 
                    ? rand.Next(10, 20) 
                    : rand.Next(5, 12);

                for (int t = 0; t < numTransactions; t++)
                {
                    var timeOfDay = TimeSpan.FromHours(rand.Next(9, 18)).Add(TimeSpan.FromMinutes(rand.Next(0, 60)));
                    var transactionTime = currentDay.Add(timeOfDay);

                    var transaction = new Transaction
                    {
                        CashierId = cashierId,
                        Timestamp = transactionTime,
                        PaymentMethod = rand.NextDouble() > 0.3 ? "Card" : "Cash",
                        OrderStatus = "Completed",
                        Items = new List<TransactionItem>()
                    };

                    int numItems = rand.Next(1, 5);
                    for (int j = 0; j < numItems; j++)
                    {
                        var product = products[rand.Next(products.Count)];
                        int quantity = (product.Name.Contains("Cable")) ? rand.Next(1, 4) : 1;

                        var item = new TransactionItem
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = quantity,
                            UnitPrice = product.Price,
                            DiscountAmount = 0
                        };
                        
                        transaction.Items.Add(item);
                    }
                    
                    transaction.Subtotal = transaction.Items.Sum(x => x.UnitPrice * x.Quantity);
                    transaction.TaxAmount = transaction.Subtotal * taxRate;
                    transaction.TotalAmount = transaction.Subtotal + transaction.TaxAmount;
                    
                    if (transaction.PaymentMethod == "Cash")
                    {
                        transaction.CashReceived = Math.Ceiling(transaction.TotalAmount / 100) * 100;
                        transaction.ChangeGiven = transaction.CashReceived - transaction.TotalAmount;
                    }
                    else
                    {
                        transaction.CardAmount = transaction.TotalAmount;
                    }

                    transactions.Add(transaction);
                }
            }

            context.Transactions.AddRange(transactions);
            context.SaveChanges();
        }
    }
}
