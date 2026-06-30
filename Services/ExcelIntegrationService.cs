using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PlatinumPOS.Data;
using PlatinumPOS.Models;

namespace PlatinumPOS.Services
{
    public class ExcelIntegrationService
    {
        private readonly IDbContextFactory<PosDbContext> _dbFactory;

        public ExcelIntegrationService(IDbContextFactory<PosDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<byte[]> ExportProductsAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var categories = await db.Categories.ToListAsync();
            var products = await db.Products.Include(p => p.Category).ToListAsync();

            using var workbook = new XLWorkbook();
            
            // --- Categories Sheet ---
            var catSheet = workbook.Worksheets.Add("Categories");
            catSheet.Cell(1, 1).Value = "Id";
            catSheet.Cell(1, 2).Value = "Name";
            catSheet.Cell(1, 3).Value = "Icon";
            catSheet.Cell(1, 4).Value = "Color";

            // Make headers bold
            catSheet.Range("A1:D1").Style.Font.Bold = true;

            for (int i = 0; i < categories.Count; i++)
            {
                var row = i + 2;
                catSheet.Cell(row, 1).Value = categories[i].Id;
                catSheet.Cell(row, 2).Value = categories[i].Name;
                catSheet.Cell(row, 3).Value = categories[i].Icon;
                catSheet.Cell(row, 4).Value = categories[i].Color;
            }
            catSheet.Columns().AdjustToContents();

            // --- Products Sheet ---
            var prodSheet = workbook.Worksheets.Add("Products");
            prodSheet.Cell(1, 1).Value = "Id";
            prodSheet.Cell(1, 2).Value = "CategoryId";
            prodSheet.Cell(1, 3).Value = "CategoryName";
            prodSheet.Cell(1, 4).Value = "Barcode";
            prodSheet.Cell(1, 5).Value = "Name";
            prodSheet.Cell(1, 6).Value = "Price";
            prodSheet.Cell(1, 7).Value = "Cost";
            prodSheet.Cell(1, 8).Value = "StockQuantity";
            prodSheet.Cell(1, 9).Value = "Color";
            prodSheet.Cell(1, 10).Value = "IsActive";

            prodSheet.Range("A1:J1").Style.Font.Bold = true;

            for (int i = 0; i < products.Count; i++)
            {
                var row = i + 2;
                prodSheet.Cell(row, 1).Value = products[i].Id;
                prodSheet.Cell(row, 2).Value = products[i].CategoryId;
                prodSheet.Cell(row, 3).Value = products[i].Category?.Name ?? "";
                prodSheet.Cell(row, 4).Value = products[i].Barcode;
                prodSheet.Cell(row, 5).Value = products[i].Name;
                prodSheet.Cell(row, 6).Value = products[i].Price;
                prodSheet.Cell(row, 7).Value = products[i].Cost;
                prodSheet.Cell(row, 8).Value = products[i].StockQuantity;
                prodSheet.Cell(row, 9).Value = products[i].Color;
                prodSheet.Cell(row, 10).Value = products[i].IsActive;
            }
            prodSheet.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task ImportProductsAsync(Stream fileStream)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            using var workbook = new XLWorkbook(fileStream);

            // 1. Process Categories
            if (workbook.TryGetWorksheet("Categories", out var catSheet))
            {
                var catRows = catSheet.RowsUsed().Skip(1); // Skip header
                foreach (var row in catRows)
                {
                    var idStr = row.Cell(1).GetString().Trim();
                    int id = 0;
                    if (!string.IsNullOrEmpty(idStr))
                        int.TryParse(idStr, out id);
                    
                    var name = row.Cell(2).GetString().Trim();
                    var icon = row.Cell(3).GetString().Trim();
                    var color = row.Cell(4).GetString().Trim();

                    if (string.IsNullOrEmpty(name)) continue;

                    Category? category = null;
                    if (id > 0)
                    {
                        category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
                    }
                    if (category == null)
                    {
                        category = await db.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
                    }

                    if (category == null)
                    {
                        category = new Category();
                        db.Categories.Add(category);
                    }

                    category.Name = name;
                    category.Icon = string.IsNullOrEmpty(icon) ? "Icons.Material.Filled.Category" : icon;
                    category.Color = string.IsNullOrEmpty(color) ? "#000000" : color;
                }
                await db.SaveChangesAsync();
            }

            // 2. Process Products
            if (workbook.TryGetWorksheet("Products", out var prodSheet))
            {
                var prodRows = prodSheet.RowsUsed().Skip(1);
                foreach (var row in prodRows)
                {
                    var barcode = row.Cell(4).GetString().Trim();
                    if (string.IsNullOrEmpty(barcode)) continue;

                    var product = await db.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);
                    if (product == null)
                    {
                        product = new Product();
                        product.Barcode = barcode;
                        db.Products.Add(product);
                    }

                    var name = row.Cell(5).GetString().Trim();
                    if (!string.IsNullOrEmpty(name))
                        product.Name = name;

                    if (decimal.TryParse(row.Cell(6).GetString(), out decimal price))
                        product.Price = price;
                        
                    if (decimal.TryParse(row.Cell(7).GetString(), out decimal cost))
                        product.Cost = cost;
                        
                    if (int.TryParse(row.Cell(8).GetString(), out int stock))
                        product.StockQuantity = stock;
                        
                    var color = row.Cell(9).GetString().Trim();
                    if (!string.IsNullOrEmpty(color))
                        product.Color = color;
                        
                    if (bool.TryParse(row.Cell(10).GetString(), out bool isActive))
                        product.IsActive = isActive;
                    else if (product.Id == 0) // Only default to true if it's a new product
                        product.IsActive = true;

                    // Category matching
                    if (int.TryParse(row.Cell(2).GetString(), out int catId) && catId > 0)
                    {
                        var cat = await db.Categories.FindAsync(catId);
                        if (cat != null)
                            product.CategoryId = catId;
                    }
                    else
                    {
                        var catName = row.Cell(3).GetString().Trim();
                        if (!string.IsNullOrEmpty(catName))
                        {
                            var cat = await db.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == catName.ToLower());
                            if (cat != null)
                            {
                                product.CategoryId = cat.Id;
                            }
                            else
                            {
                                var newCat = new Category { Name = catName, Color = "#000000", Icon = "Icons.Material.Filled.Category" };
                                db.Categories.Add(newCat);
                                await db.SaveChangesAsync();
                                product.CategoryId = newCat.Id;
                            }
                        }
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}
