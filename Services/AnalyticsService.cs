using Microsoft.EntityFrameworkCore;
using PlatinumPOS.Data;
using PlatinumPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatinumPOS.Services
{
    // ── Result DTOs ──────────────────────────────────────────────────────

    public class SalesForecastResult
    {
        public decimal PredictedRevenue7 { get; set; }
        public decimal PredictedRevenue14 { get; set; }
        public decimal PredictedRevenue30 { get; set; }
        public decimal DailyAverage { get; set; }
        public decimal GrowthTrendPercent { get; set; }
        public List<DailyRevenue> HistoricalDaily { get; set; } = new();
        public List<DailyRevenue> PredictedDaily { get; set; } = new();
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProductRanking
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProductRankingsResult
    {
        public List<ProductRanking> TopSellers { get; set; } = new();
        public List<ProductRanking> WorstSellers { get; set; } = new();
        public List<ProductRanking> AllRankedByRevenue { get; set; } = new();
        public Dictionary<string, double> RevenueByCategory { get; set; } = new();
        public Dictionary<string, double> UnitsByCategory { get; set; } = new();
    }

    public class StockRecommendation
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public decimal AvgDailySales { get; set; }
        public int RecommendedStock { get; set; }
        public string Status { get; set; } = "OK"; // OK, Overstocked, Understocked, Critical
        public decimal StockValue { get; set; }     // CurrentStock × Cost
    }

    public class StockRecommendationsResult
    {
        public List<StockRecommendation> Items { get; set; } = new();
        public int OverstockedCount { get; set; }
        public int UnderstockedCount { get; set; }
        public int CriticalCount { get; set; }
        public decimal TotalExcessValue { get; set; }
        public Dictionary<string, double> StockValueByCategory { get; set; } = new();
    }

    public class DayOfWeekResult
    {
        public Dictionary<string, decimal> RevenueByDay { get; set; } = new();
        public Dictionary<string, int> TransactionsByDay { get; set; } = new();
        public string BestDay { get; set; } = string.Empty;
        public string WorstDay { get; set; } = string.Empty;
        public decimal BestDayRevenue { get; set; }
        public decimal WorstDayRevenue { get; set; }
    }

    public class WeekMonthResult
    {
        public Dictionary<string, decimal> RevenueByWeek { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
        public string BestWeek { get; set; } = string.Empty;
        public string WorstWeek { get; set; } = string.Empty;
        public decimal BestWeekRevenue { get; set; }
        public decimal WorstWeekRevenue { get; set; }
        public string BestMonth { get; set; } = string.Empty;
        public string WorstMonth { get; set; } = string.Empty;
        public decimal BestMonthRevenue { get; set; }
        public decimal WorstMonthRevenue { get; set; }
    }

    public class StockConsolidationItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal StockCostValue { get; set; }    // Qty × Cost
        public decimal StockRetailValue { get; set; }  // Qty × Price
        public decimal PotentialProfit { get; set; }    // Retail - Cost value
        public bool IsActive { get; set; }
    }

    public class StockConsolidationResult
    {
        public List<StockConsolidationItem> Items { get; set; } = new();
        public int TotalItemCount { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalCostValue { get; set; }
        public decimal TotalRetailValue { get; set; }
        public decimal TotalPotentialProfit { get; set; }
        public decimal GrossMarginPercent { get; set; }
        public Dictionary<string, double> CostValueByCategory { get; set; } = new();
        public Dictionary<string, double> MarginByCategory { get; set; } = new();
    }

    // ── Service ──────────────────────────────────────────────────────────

    public class AnalyticsService
    {
        private readonly IDbContextFactory<PosDbContext> _dbFactory;
        private const int DefaultLeadTimeDays = 7;
        private const double SafetyMultiplier = 1.5;

        public AnalyticsService(IDbContextFactory<PosDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // ── Tab 1: Sales Forecast ────────────────────────────────────────

        public async Task<SalesForecastResult> GetSalesForecastAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var completed = await db.Transactions
                .Where(t => t.OrderStatus == "Completed")
                .OrderBy(t => t.Timestamp)
                .Select(t => new { t.Timestamp, t.TotalAmount })
                .ToListAsync();

            var result = new SalesForecastResult();

            if (!completed.Any())
                return result;

            // Group by date (local time)
            var dailyGroups = completed
                .GroupBy(t => t.Timestamp.ToLocalTime().Date)
                .Select(g => new DailyRevenue { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
                .OrderBy(d => d.Date)
                .ToList();

            result.HistoricalDaily = dailyGroups;
            result.DailyAverage = dailyGroups.Any() ? dailyGroups.Average(d => d.Revenue) : 0;

            if (dailyGroups.Count < 2)
            {
                // Not enough data for regression — use flat average
                result.PredictedRevenue7 = result.DailyAverage * 7;
                result.PredictedRevenue14 = result.DailyAverage * 14;
                result.PredictedRevenue30 = result.DailyAverage * 30;
                return result;
            }

            // Linear regression: y = slope * x + intercept
            var baseDate = dailyGroups.First().Date;
            var xs = dailyGroups.Select(d => (double)(d.Date - baseDate).TotalDays).ToArray();
            var ys = dailyGroups.Select(d => (double)d.Revenue).ToArray();
            var (slope, intercept) = LinearRegression(xs, ys);

            var lastDayIndex = (double)(dailyGroups.Last().Date - baseDate).TotalDays;

            // Generate predictions
            decimal Predict(double dayIndex) => (decimal)Math.Max(0, slope * dayIndex + intercept);

            result.PredictedRevenue7 = 0;
            result.PredictedRevenue14 = 0;
            result.PredictedRevenue30 = 0;

            for (int i = 1; i <= 30; i++)
            {
                var futureDay = lastDayIndex + i;
                var predicted = Predict(futureDay);

                if (i <= 7) result.PredictedRevenue7 += predicted;
                if (i <= 14) result.PredictedRevenue14 += predicted;
                result.PredictedRevenue30 += predicted;

                if (i <= 14)
                {
                    result.PredictedDaily.Add(new DailyRevenue
                    {
                        Date = dailyGroups.Last().Date.AddDays(i),
                        Revenue = predicted
                    });
                }
            }

            // Growth trend: compare first-half average to second-half average
            var halfIndex = dailyGroups.Count / 2;
            var firstHalf = dailyGroups.Take(halfIndex).Average(d => d.Revenue);
            var secondHalf = dailyGroups.Skip(halfIndex).Average(d => d.Revenue);
            result.GrowthTrendPercent = firstHalf > 0
                ? Math.Round(((secondHalf - firstHalf) / firstHalf) * 100, 1)
                : 0;

            return result;
        }

        // ── Tab 2: Best & Worst Sellers ──────────────────────────────────

        public async Task<ProductRankingsResult> GetProductRankingsAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var completedTransactionIds = await db.Transactions
                .Where(t => t.OrderStatus == "Completed")
                .Select(t => t.Id)
                .ToListAsync();

            var items = await db.TransactionItems
                .Where(ti => completedTransactionIds.Contains(ti.TransactionId))
                .ToListAsync();

            var products = await db.Products.Include(p => p.Category).ToListAsync();
            var productMap = products.ToDictionary(p => p.Id);

            var rankings = items
                .Where(ti => ti.ProductId.HasValue && productMap.ContainsKey(ti.ProductId.Value))
                .GroupBy(ti => ti.ProductId!.Value)
                .Select(g =>
                {
                    var prod = productMap[g.Key];
                    return new ProductRanking
                    {
                        ProductId = g.Key,
                        ProductName = prod.Name,
                        CategoryName = prod.Category?.Name ?? "Uncategorized",
                        TotalUnitsSold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity - x.DiscountAmount)
                    };
                })
                .OrderByDescending(r => r.TotalRevenue)
                .ToList();

            // Also include products that have never been sold
            var soldProductIds = rankings.Select(r => r.ProductId).ToHashSet();
            var unsold = products
                .Where(p => !soldProductIds.Contains(p.Id) && p.IsActive)
                .Select(p => new ProductRanking
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category?.Name ?? "Uncategorized",
                    TotalUnitsSold = 0,
                    TotalRevenue = 0
                })
                .ToList();

            var allRanked = rankings.Concat(unsold).ToList();

            var result = new ProductRankingsResult
            {
                TopSellers = allRanked.Take(5).ToList(),
                WorstSellers = allRanked.OrderBy(r => r.TotalRevenue).Take(5).ToList(),
                AllRankedByRevenue = allRanked,
                RevenueByCategory = allRanked
                    .GroupBy(r => r.CategoryName)
                    .ToDictionary(g => g.Key, g => (double)g.Sum(x => x.TotalRevenue)),
                UnitsByCategory = allRanked
                    .GroupBy(r => r.CategoryName)
                    .ToDictionary(g => g.Key, g => (double)g.Sum(x => x.TotalUnitsSold))
            };

            return result;
        }

        // ── Tab 3: Stock Recommendations ─────────────────────────────────

        public async Task<StockRecommendationsResult> GetStockRecommendationsAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();

            var products = await db.Products.Include(p => p.Category).Where(p => p.IsActive).ToListAsync();

            var completedTransactionIds = await db.Transactions
                .Where(t => t.OrderStatus == "Completed")
                .Select(t => t.Id)
                .ToListAsync();

            var allItems = await db.TransactionItems
                .Where(ti => completedTransactionIds.Contains(ti.TransactionId))
                .ToListAsync();

            // Calculate date range of sales data
            var firstTransaction = await db.Transactions
                .Where(t => t.OrderStatus == "Completed")
                .OrderBy(t => t.Timestamp)
                .Select(t => t.Timestamp)
                .FirstOrDefaultAsync();

            var daysSinceFirst = firstTransaction == default
                ? 1
                : Math.Max(1, (int)(DateTime.UtcNow - firstTransaction).TotalDays);

            var recommendations = new List<StockRecommendation>();

            foreach (var prod in products)
            {
                var soldQty = allItems
                    .Where(ti => ti.ProductId == prod.Id)
                    .Sum(ti => ti.Quantity);

                var avgDaily = (decimal)soldQty / daysSinceFirst;
                var recommended = (int)Math.Ceiling((double)avgDaily * DefaultLeadTimeDays * SafetyMultiplier);

                // Minimum recommended stock of 1 if the product has ever sold
                if (soldQty > 0 && recommended < 1) recommended = 1;

                string status;
                if (prod.StockQuantity <= 0 && soldQty > 0)
                    status = "Critical";
                else if (recommended > 0 && prod.StockQuantity < recommended)
                    status = "Understocked";
                else if (recommended > 0 && prod.StockQuantity > recommended * 3)
                    status = "Overstocked";
                else
                    status = "OK";

                recommendations.Add(new StockRecommendation
                {
                    ProductId = prod.Id,
                    ProductName = prod.Name,
                    CategoryName = prod.Category?.Name ?? "Uncategorized",
                    CurrentStock = prod.StockQuantity,
                    AvgDailySales = Math.Round(avgDaily, 2),
                    RecommendedStock = recommended,
                    Status = status,
                    StockValue = prod.StockQuantity * prod.Cost
                });
            }

            var result = new StockRecommendationsResult
            {
                Items = recommendations.OrderByDescending(r => r.Status == "Critical")
                    .ThenByDescending(r => r.Status == "Overstocked")
                    .ThenByDescending(r => r.Status == "Understocked")
                    .ThenBy(r => r.ProductName)
                    .ToList(),
                OverstockedCount = recommendations.Count(r => r.Status == "Overstocked"),
                UnderstockedCount = recommendations.Count(r => r.Status == "Understocked"),
                CriticalCount = recommendations.Count(r => r.Status == "Critical"),
                TotalExcessValue = recommendations
                    .Where(r => r.Status == "Overstocked")
                    .Sum(r => (r.CurrentStock - r.RecommendedStock) * (r.StockValue / Math.Max(1, r.CurrentStock))),
                StockValueByCategory = recommendations
                    .GroupBy(r => r.CategoryName)
                    .ToDictionary(g => g.Key, g => (double)g.Sum(x => x.StockValue))
            };

            return result;
        }

        // ── Tab 4: Day of Week Analysis ──────────────────────────────────

        public async Task<DayOfWeekResult> GetDayOfWeekAnalysisAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var completed = await db.Transactions
                .Where(t => t.OrderStatus == "Completed")
                .Select(t => new { t.Timestamp, t.TotalAmount })
                .ToListAsync();

            var result = new DayOfWeekResult();

            var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            // Initialize all days
            foreach (var day in dayNames)
            {
                result.RevenueByDay[day] = 0;
                result.TransactionsByDay[day] = 0;
            }

            foreach (var t in completed)
            {
                var day = t.Timestamp.ToLocalTime().DayOfWeek;
                var dayName = day.ToString();
                result.RevenueByDay[dayName] += t.TotalAmount;
                result.TransactionsByDay[dayName]++;
            }

            if (result.RevenueByDay.Values.Any(v => v > 0))
            {
                var best = result.RevenueByDay.OrderByDescending(kv => kv.Value).First();
                var worst = result.RevenueByDay.Where(kv => kv.Value > 0).OrderBy(kv => kv.Value).First();

                result.BestDay = best.Key;
                result.BestDayRevenue = best.Value;
                result.WorstDay = worst.Key;
                result.WorstDayRevenue = worst.Value;
            }

            return result;
        }

        // ── Tab 5: Week & Month Analysis ─────────────────────────────────

        public async Task<WeekMonthResult> GetWeekMonthAnalysisAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var completed = await db.Transactions
                .Where(t => t.OrderStatus == "Completed")
                .Select(t => new { t.Timestamp, t.TotalAmount })
                .ToListAsync();

            var result = new WeekMonthResult();

            // Revenue by week of month (1-5)
            var weekLabels = new[] { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5" };
            foreach (var w in weekLabels) result.RevenueByWeek[w] = 0;

            // Revenue by month of year
            var monthLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            foreach (var m in monthLabels) result.RevenueByMonth[m] = 0;

            foreach (var t in completed)
            {
                var local = t.Timestamp.ToLocalTime();
                var weekOfMonth = Math.Min(5, ((local.Day - 1) / 7) + 1);
                var weekLabel = $"Week {weekOfMonth}";
                result.RevenueByWeek[weekLabel] += t.TotalAmount;

                var monthLabel = monthLabels[local.Month - 1];
                result.RevenueByMonth[monthLabel] += t.TotalAmount;
            }

            // Best/Worst week
            var weeksWithData = result.RevenueByWeek.Where(kv => kv.Value > 0).ToList();
            if (weeksWithData.Any())
            {
                var bestW = weeksWithData.OrderByDescending(kv => kv.Value).First();
                var worstW = weeksWithData.OrderBy(kv => kv.Value).First();
                result.BestWeek = bestW.Key;
                result.BestWeekRevenue = bestW.Value;
                result.WorstWeek = worstW.Key;
                result.WorstWeekRevenue = worstW.Value;
            }

            // Best/Worst month
            var monthsWithData = result.RevenueByMonth.Where(kv => kv.Value > 0).ToList();
            if (monthsWithData.Any())
            {
                var bestM = monthsWithData.OrderByDescending(kv => kv.Value).First();
                var worstM = monthsWithData.OrderBy(kv => kv.Value).First();
                result.BestMonth = bestM.Key;
                result.BestMonthRevenue = bestM.Value;
                result.WorstMonth = worstM.Key;
                result.WorstMonthRevenue = worstM.Value;
            }

            return result;
        }

        // ── Tab 6: Stock Consolidation ───────────────────────────────────

        public async Task<StockConsolidationResult> GetStockConsolidationAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var products = await db.Products.Include(p => p.Category).ToListAsync();

            var items = products.Select(p => new StockConsolidationItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                StockQuantity = p.StockQuantity,
                UnitCost = p.Cost,
                SellingPrice = p.Price,
                StockCostValue = p.StockQuantity * p.Cost,
                StockRetailValue = p.StockQuantity * p.Price,
                PotentialProfit = (p.StockQuantity * p.Price) - (p.StockQuantity * p.Cost),
                IsActive = p.IsActive
            }).OrderBy(i => i.CategoryName).ThenBy(i => i.ProductName).ToList();

            var totalCost = items.Sum(i => i.StockCostValue);
            var totalRetail = items.Sum(i => i.StockRetailValue);

            var result = new StockConsolidationResult
            {
                Items = items,
                TotalItemCount = items.Count,
                TotalUnits = items.Sum(i => i.StockQuantity),
                TotalCostValue = totalCost,
                TotalRetailValue = totalRetail,
                TotalPotentialProfit = totalRetail - totalCost,
                GrossMarginPercent = totalRetail > 0
                    ? Math.Round(((totalRetail - totalCost) / totalRetail) * 100, 1)
                    : 0,
                CostValueByCategory = items
                    .GroupBy(i => i.CategoryName)
                    .ToDictionary(g => g.Key, g => (double)g.Sum(x => x.StockCostValue)),
                MarginByCategory = items
                    .GroupBy(i => i.CategoryName)
                    .ToDictionary(g => g.Key, g => (double)g.Sum(x => x.PotentialProfit))
            };

            return result;
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static (double slope, double intercept) LinearRegression(double[] xs, double[] ys)
        {
            int n = xs.Length;
            double sumX = xs.Sum();
            double sumY = ys.Sum();
            double sumXY = xs.Zip(ys, (x, y) => x * y).Sum();
            double sumX2 = xs.Sum(x => x * x);

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < 1e-10)
                return (0, sumY / n);

            double slope = (n * sumXY - sumX * sumY) / denominator;
            double intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }
    }
}
