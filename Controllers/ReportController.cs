using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class ReportController : Controller
    {
        private readonly FashionShopContext _context;

        public ReportController(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get summary statistics
            var totalSales = await _context.Order
                .Where(o => o.order_status == 2) // Completed orders
                .SumAsync(o => o.total_price) ?? 0;

            var totalOrders = await _context.Order.CountAsync();
            var totalProducts = await _context.Product.CountAsync();
            var totalCustomers = await _context.User.CountAsync(u => u.permission == 0); // Regular customers

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCustomers = totalCustomers;

            // Get recent statistics for the dashboard
            ViewBag.RecentOrders = await GetRecentOrderStats();
            ViewBag.TopProducts = await GetTopProductStats();
            ViewBag.CategoryStats = await GetCategoryStats();

            return View();
        }

        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Default to last 30 days if no date range provided
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Get sales data for the selected period
            var orders = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .OrderBy(o => o.order_date)
                .ToListAsync();

            // Group orders by date and calculate daily totals
            var salesByDate = orders
                .GroupBy(o => o.order_date.Date)
                .Select(group => new {
                    Date = group.Key.ToString("yyyy-MM-dd"),
                    Total = group.Sum(o => o.total_price),
                    OrderCount = group.Count()
                })
                .ToList();

            // Calculate summary statistics
            var totalSales = orders.Sum(o => o.total_price) ?? 0;
            var orderCount = orders.Count;
            var averageOrderValue = orderCount > 0 ? totalSales / orderCount : 0;

            ViewBag.TotalSales = totalSales;
            ViewBag.OrderCount = orderCount;
            ViewBag.AverageOrderValue = averageOrderValue;
            ViewBag.SalesByDate = salesByDate;

            // Calculate sales by status
            var salesByStatus = orders
                .GroupBy(o => o.order_status)
                .Select(group => new {
                    Status = GetStatusName(group.Key),
                    Total = group.Sum(o => o.total_price),
                    OrderCount = group.Count()
                })
                .ToList();

            ViewBag.SalesByStatus = salesByStatus;

            // Calculate sales by payment method (new feature)
            var salesByPayment = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .Include(o => o.Payment)
                .GroupBy(o => o.payment_id)
                .Select(group => new {
                    PaymentMethod = group.First().Payment.name,
                    Total = group.Sum(o => o.total_price),
                    OrderCount = group.Count()
                })
                .ToListAsync();

            ViewBag.SalesByPayment = salesByPayment;

            return View();
        }

        public async Task<IActionResult> Products()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get top selling products
            var topProducts = await _context.Order_item
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.product_id)
                .Select(group => new {
                    ProductId = group.Key,
                    ProductName = group.First().Product.name,
                    QuantitySold = group.Sum(oi => oi.quantity),
                    Revenue = group.Sum(oi => oi.price * oi.quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToListAsync();

            ViewBag.TopProducts = topProducts;

            // Get sales by category
            var salesByCategory = await _context.Order_item
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .GroupBy(oi => oi.Product.category_id)
                .Select(group => new {
                    CategoryId = group.Key,
                    CategoryName = group.First().Product.Category.name,
                    QuantitySold = group.Sum(oi => oi.quantity),
                    Revenue = group.Sum(oi => oi.price * oi.quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            ViewBag.SalesByCategory = salesByCategory;

            // Get low stock products
            var lowStockProducts = await _context.Product
                .Where(p => p.stock < 10)
                .OrderBy(p => p.stock)
                .ToListAsync();

            ViewBag.LowStockProducts = lowStockProducts;

            // Get product popularity by color (new feature)
            var popularColors = await _context.Order_item
                .Include(oi => oi.Color)
                .Where(oi => oi.color_id != null)
                .GroupBy(oi => oi.color_id)
                .Select(group => new {
                    ColorId = group.Key,
                    ColorName = group.First().Color.color_name,
                    ColorHex = group.First().Color.color_hex,
                    QuantitySold = group.Sum(oi => oi.quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            ViewBag.PopularColors = popularColors;

            return View();
        }

        public async Task<IActionResult> Customers()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get top customers by order value
            var topCustomers = await _context.Order
                .Include(o => o.User)
                .GroupBy(o => o.user_id)
                .Select(group => new {
                    UserId = group.Key,
                    UserName = group.First().User.first_name + " " + group.First().User.last_name,
                    OrderCount = group.Count(),
                    TotalSpent = group.Sum(o => o.total_price)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            ViewBag.TopCustomers = topCustomers;

            // Get new customers per month
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var usersWithFirstOrder = await _context.Order
                .Include(o => o.User)
                .GroupBy(o => o.user_id)
                .Select(group => new {
                    User = group.First().User,
                    FirstOrderDate = group.Min(o => o.order_date)
                })
                .Where(x => x.FirstOrderDate >= sixMonthsAgo)
                .ToListAsync();

            var newCustomersByMonth = usersWithFirstOrder
                .GroupBy(x => new { Month = x.FirstOrderDate.Month, Year = x.FirstOrderDate.Year })
                .Select(group => new {
                    Month = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMM yyyy"),
                    Count = group.Count()
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", null))
                .ToList();

            ViewBag.NewCustomersByMonth = newCustomersByMonth;

            // Calculate customer retention rate (new feature)
            // This is a simplified calculation - in a real application you would need more sophisticated logic
            var allCustomers = await _context.Order
                .Select(o => o.user_id)
                .Distinct()
                .CountAsync();

            var repeatCustomers = await _context.Order
                .GroupBy(o => o.user_id)
                .Where(g => g.Count() > 1)
                .CountAsync();

            var retentionRate = allCustomers > 0 ? (double)repeatCustomers / allCustomers * 100 : 0;
            ViewBag.RetentionRate = Math.Round(retentionRate, 1);
            ViewBag.RepeatCustomers = repeatCustomers;
            ViewBag.OneTimeCustomers = allCustomers - repeatCustomers;

            return View();
        }

        public async Task<IActionResult> Export(string reportType)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Generate CSV content
            var sb = new StringBuilder();
            string fileName = "";

            switch (reportType)
            {
                case "sales":
                    var orders = await _context.Order.Include(o => o.User).ToListAsync();
                    sb.AppendLine("OrderID,Date,Customer,Amount,Status");
                    foreach (var order in orders)
                    {
                        string customer = EscapeCsv($"{order.User.first_name} {order.User.last_name}");
                        string status = EscapeCsv(GetStatusName(order.order_status));
                        sb.AppendLine($"{order.order_id},{order.order_date:yyyy-MM-dd},{customer},{order.total_price},{status}");
                    }
                    fileName = "sales_report.csv";
                    break;

                case "products":
                    var products = await _context.Product.Include(p => p.Category).ToListAsync();
                    sb.AppendLine("ProductID,Name,Category,Price,Stock");
                    foreach (var product in products)
                    {
                        string name = EscapeCsv(product.name);
                        string category = EscapeCsv(product.Category.name);
                        sb.AppendLine($"{product.product_id},{name},{category},{product.price},{product.stock}");
                    }
                    fileName = "products_report.csv";
                    break;

                case "customers":
                    var customers = await _context.User.Where(u => u.permission == 0).ToListAsync();
                    sb.AppendLine("UserID,Name,Email,Phone,Address");
                    foreach (var customer in customers)
                    {
                        string name = EscapeCsv($"{customer.first_name} {customer.last_name}");
                        string email = EscapeCsv(customer.email);
                        string phone = EscapeCsv(customer.phone_number);
                        string address = EscapeCsv(customer.address ?? "");
                        sb.AppendLine($"{customer.user_id},{name},{email},{phone},{address}");
                    }
                    fileName = "customers_report.csv";
                    break;

                case "inventory":
                    var inventory = await _context.Product
                        .Include(p => p.Category)
                        .OrderBy(p => p.Category.name)
                        .ThenBy(p => p.name)
                        .ToListAsync();

                    sb.AppendLine("ProductID,SKU,Name,Category,Price,Stock,Status");
                    foreach (var product in inventory)
                    {
                        string status = product.stock > 10 ? "In Stock" : product.stock > 0 ? "Low Stock" : "Out of Stock";
                        sb.AppendLine($"{product.product_id},{EscapeCsv(product.sku)},{EscapeCsv(product.name)},{EscapeCsv(product.Category.name)},{product.price},{product.stock},{status}");
                    }
                    fileName = "inventory_report.csv";
                    break;

                default:
                    return BadRequest("Invalid report type");
            }

            // Return CSV file with UTF-8 BOM to fix font issues
            byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(buffer, "text/csv", fileName);
        }

        /// <summary>
        /// Escapes CSV values with commas, quotes, or newlines
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            if (mustQuote)
            {
                value = value.Replace("\"", "\"\""); // Escape double quotes
                return $"\"{value}\"";
            }

            return value;
        }

        // New action for generating PDF reports
        public async Task<IActionResult> GeneratePdf(string reportType)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // In a real application, you would implement PDF generation here
            // For this example, we'll just return a message
            return Content("PDF generation would be implemented here for: " + reportType);
        }

        // New dashboard for inventory analysis
        public async Task<IActionResult> Inventory()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get inventory statistics
            var totalProducts = await _context.Product.CountAsync();
            var totalStock = await _context.Product.SumAsync(p => p.stock);
            var lowStockCount = await _context.Product.CountAsync(p => p.stock < 10);
            var outOfStockCount = await _context.Product.CountAsync(p => p.stock == 0);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalStock = totalStock;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.OutOfStockCount = outOfStockCount;

            // Get inventory by category
            var inventoryByCategory = await _context.Product
                .Include(p => p.Category)
                .GroupBy(p => p.category_id)
                .Select(group => new {
                    CategoryId = group.Key,
                    CategoryName = group.First().Category.name,
                    ProductCount = group.Count(),
                    TotalStock = group.Sum(p => p.stock),
                    AveragePrice = group.Average(p => p.price)
                })
                .OrderByDescending(x => x.TotalStock)
                .ToListAsync();

            ViewBag.InventoryByCategory = inventoryByCategory;

            // Get stock movement (mock data - in a real application, you would track inventory changes)
            // This would require a new table to track inventory changes over time
            var stockMovement = new List<object>
            {
                new { Month = "Nov 2024", Incoming = 120, Outgoing = 78, Stock = 580 },
                new { Month = "Dec 2024", Incoming = 150, Outgoing = 102, Stock = 628 },
                new { Month = "Jan 2025", Incoming = 80, Outgoing = 95, Stock = 613 },
                new { Month = "Feb 2025", Incoming = 110, Outgoing = 89, Stock = 634 },
                new { Month = "Mar 2025", Incoming = 90, Outgoing = 105, Stock = 619 },
                new { Month = "Apr 2025", Incoming = 130, Outgoing = 100, Stock = 649 }
            };

            ViewBag.StockMovement = stockMovement;

            return View();
        }
        private async Task<List<object>> GetRecentOrderStats()
        {
            // Get orders from the last 30 days
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            // First get the data from the database without doing the string formatting
            var recentOrdersData = await _context.Order
                .Where(o => o.order_date >= thirtyDaysAgo)
                .GroupBy(o => o.order_date.Date)
                .Select(group => new {
                    Date = group.Key,
                    Total = group.Sum(o => o.total_price)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Then do the string formatting in memory
            var recentOrders = recentOrdersData
                .Select(item => new {
                    Date = item.Date.ToString("yyyy-MM-dd"),
                    Total = item.Total
                })
                .ToList();

            return recentOrders.Cast<object>().ToList();
        }

        private async Task<List<object>> GetTopProductStats()
        {
            var topProducts = await _context.Order_item
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.product_id)
                .Select(group => new {
                    ProductName = group.First().Product.name,
                    QuantitySold = group.Sum(oi => oi.quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            return topProducts.Cast<object>().ToList();
        }

        private async Task<List<object>> GetCategoryStats()
        {
            var categoryStats = await _context.Order_item
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .GroupBy(oi => oi.Product.category_id)
                .Select(group => new {
                    CategoryName = group.First().Product.Category.name,
                    Revenue = group.Sum(oi => oi.price * oi.quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return categoryStats.Cast<object>().ToList();
        }

        private string GetStatusName(int? status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Processing",
                2 => "Completed",
                _ => "Unknown"
            };
        }
    }
}