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

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Thiết lập khoảng thời gian mặc định nếu không cung cấp
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Lấy thống kê tổng quan cho khoảng thời gian đã chọn
            var ordersInRange = _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate);

            var totalSales = await ordersInRange
                .Where(o => o.order_status == 2) // Đơn hàng đã hoàn thành
                .SumAsync(o => o.total_price) ?? 0;

            var totalOrders = await ordersInRange.CountAsync();
            var totalProducts = await _context.Product.CountAsync();
            var totalCustomers = await _context.User.CountAsync(u => u.permission == 0); // Khách hàng thông thường

            // Lấy số lượng khách hàng mới trong khoảng thời gian này
            var newCustomers = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .GroupBy(o => o.user_id)
                .Select(g => new { UserId = g.Key, FirstOrderInPeriod = g.Min(x => x.order_date) })
                .Join(_context.Order,
                    newCust => newCust.UserId,
                    order => order.user_id,
                    (newCust, order) => new { newCust.UserId, newCust.FirstOrderInPeriod, order.order_date })
                .GroupBy(x => x.UserId)
                .Select(g => new { UserId = g.Key, FirstOrderEver = g.Min(x => x.order_date), FirstOrderInPeriod = g.First().FirstOrderInPeriod })
                .Where(x => x.FirstOrderEver == x.FirstOrderInPeriod)
                .CountAsync();

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.NewCustomers = newCustomers;

            // Lấy thống kê theo thời gian cho bảng điều khiển
            ViewBag.RecentOrders = await GetRecentOrderStats(startDate.Value, endDate.Value);
            ViewBag.TopProducts = await GetTopProductStats(startDate.Value, endDate.Value);
            ViewBag.CategoryStats = await GetCategoryStats(startDate.Value, endDate.Value);

            return View();
        }

        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Mặc định là 30 ngày qua nếu không cung cấp khoảng thời gian
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Lấy dữ liệu bán hàng cho khoảng thời gian đã chọn
            var orders = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .OrderBy(o => o.order_date)
                .ToListAsync();

            // Nhóm đơn hàng theo ngày và tính tổng hàng ngày
            var salesByDate = orders
                .GroupBy(o => o.order_date.Date)
                .Select(group => new {
                    Date = group.Key.ToString("yyyy-MM-dd"),
                    Total = group.Sum(o => o.total_price),
                    OrderCount = group.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Tính thống kê tổng hợp
            var totalSales = orders.Sum(o => o.total_price) ?? 0;
            var orderCount = orders.Count;
            var averageOrderValue = orderCount > 0 ? totalSales / orderCount : 0;

            ViewBag.TotalSales = totalSales;
            ViewBag.OrderCount = orderCount;
            ViewBag.AverageOrderValue = averageOrderValue;
            ViewBag.SalesByDate = salesByDate;

            // Tính doanh số theo trạng thái
            var salesByStatus = orders
                .GroupBy(o => o.order_status)
                .Select(group => new {
                    Status = GetStatusName(group.Key),
                    Total = group.Sum(o => o.total_price),
                    OrderCount = group.Count()
                })
                .ToList();

            ViewBag.SalesByStatus = salesByStatus;

            // Tính doanh số theo phương thức thanh toán
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

            // Lấy xu hướng doanh số theo tháng
            var salesByMonth = orders
                .GroupBy(o => new { o.order_date.Year, o.order_date.Month })
                .Select(group => new {
                    Period = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("yyyy-MM"),
                    Total = group.Sum(o => o.total_price),
                    OrderCount = group.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();

            ViewBag.SalesByMonth = salesByMonth;

            return View();
        }

        public async Task<IActionResult> Products(DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Mặc định là 30 ngày qua nếu không cung cấp khoảng thời gian
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Lấy top sản phẩm bán chạy cho khoảng thời gian đã chọn
            var topProducts = await _context.Order_item
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.order_date >= startDate && oi.Order.order_date <= endDate)
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

            // Lấy doanh số theo danh mục cho khoảng thời gian đã chọn
            var salesByCategory = await _context.Order_item
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.order_date >= startDate && oi.Order.order_date <= endDate)
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

            // Lấy sản phẩm sắp hết hàng (không cần lọc theo thời gian vì đây là tình trạng hiện tại)
            var lowStockProducts = await _context.Product
                .Where(p => p.stock < 10)
                .OrderBy(p => p.stock)
                .ToListAsync();

            ViewBag.LowStockProducts = lowStockProducts;

            // Lấy thông tin về độ phổ biến của màu sắc sản phẩm trong khoảng thời gian
            var popularColors = await _context.Order_item
                .Include(oi => oi.Color)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.order_date >= startDate && oi.Order.order_date <= endDate && oi.color_id != null)
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

        public async Task<IActionResult> Customers(DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Mặc định là 30 ngày qua nếu không cung cấp khoảng thời gian
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Lấy top khách hàng theo giá trị đơn hàng cho khoảng thời gian đã chọn
            var topCustomers = await _context.Order
                .Include(o => o.User)
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
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

            // Lấy khách hàng mới trong khoảng thời gian đã chọn
            var usersWithFirstOrder = await _context.Order
                .Include(o => o.User)
                .GroupBy(o => o.user_id)
                .Select(group => new {
                    User = group.First().User,
                    FirstOrderDate = group.Min(o => o.order_date)
                })
                .Where(x => x.FirstOrderDate >= startDate && x.FirstOrderDate <= endDate)
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

            // Tính tỷ lệ giữ chân khách hàng cho khoảng thời gian đã chọn
            var customersInPeriod = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .Select(o => o.user_id)
                .Distinct()
                .CountAsync();

            var repeatCustomersInPeriod = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .GroupBy(o => o.user_id)
                .Where(g => g.Count() > 1)
                .CountAsync();

            var retentionRate = customersInPeriod > 0 ? (double)repeatCustomersInPeriod / customersInPeriod * 100 : 0;
            ViewBag.RetentionRate = Math.Round(retentionRate, 1);
            ViewBag.RepeatCustomers = repeatCustomersInPeriod;
            ViewBag.OneTimeCustomers = customersInPeriod - repeatCustomersInPeriod;

            // Nguồn khách hàng (dữ liệu mẫu - trong ứng dụng thực tế, bạn sẽ theo dõi điều này)
            var customerSource = new List<object>
            {
                new { Source = "Tìm kiếm tự nhiên", Count = 45 },
                new { Source = "Mạng xã hội", Count = 28 },
                new { Source = "Trực tiếp", Count = 20 },
                new { Source = "Quảng cáo trả phí", Count = 12 },
                new { Source = "Giới thiệu", Count = 8 }
            };

            ViewBag.CustomerSource = customerSource;

            return View();
        }

        public async Task<IActionResult> Export(string reportType, DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Thiết lập khoảng thời gian mặc định nếu không cung cấp
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Tạo nội dung CSV
            var sb = new StringBuilder();
            string fileName = "";

            switch (reportType)
            {
                case "sales":
                    var orders = await _context.Order
                        .Include(o => o.User)
                        .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                        .ToListAsync();

                    sb.AppendLine("Mã đơn hàng,Ngày,Khách hàng,Số tiền,Trạng thái");
                    foreach (var order in orders)
                    {
                        string customer = EscapeCsv($"{order.User.first_name} {order.User.last_name}");
                        string status = EscapeCsv(GetStatusName(order.order_status));
                        sb.AppendLine($"{order.order_id},{order.order_date:yyyy-MM-dd},{customer},{order.total_price},{status}");
                    }
                    fileName = $"bao_cao_doanh_so_{startDate:yyyy-MM-dd}_den_{endDate:yyyy-MM-dd}.csv";
                    break;

                case "products":
                    var topProducts = await _context.Order_item
                        .Include(oi => oi.Product)
                        .Include(oi => oi.Product.Category)
                        .Include(oi => oi.Order)
                        .Where(oi => oi.Order.order_date >= startDate && oi.Order.order_date <= endDate)
                        .GroupBy(oi => oi.product_id)
                        .Select(group => new {
                            ProductId = group.Key,
                            ProductName = group.First().Product.name,
                            Category = group.First().Product.Category.name,
                            Price = group.First().Product.price,
                            QuantitySold = group.Sum(oi => oi.quantity),
                            Revenue = group.Sum(oi => oi.price * oi.quantity)
                        })
                        .ToListAsync();

                    sb.AppendLine("Mã sản phẩm,Tên,Danh mục,Giá,Số lượng đã bán,Doanh thu");
                    foreach (var product in topProducts)
                    {
                        string name = EscapeCsv(product.ProductName);
                        string category = EscapeCsv(product.Category);
                        sb.AppendLine($"{product.ProductId},{name},{category},{product.Price},{product.QuantitySold},{product.Revenue}");
                    }
                    fileName = $"bao_cao_san_pham_{startDate:yyyy-MM-dd}_den_{endDate:yyyy-MM-dd}.csv";
                    break;

                case "customers":
                    var customers = await _context.Order
                        .Include(o => o.User)
                        .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                        .GroupBy(o => o.user_id)
                        .Select(group => new {
                            User = group.First().User,
                            OrderCount = group.Count(),
                            TotalSpent = group.Sum(o => o.total_price),
                            FirstOrderDate = group.Min(o => o.order_date),
                            LastOrderDate = group.Max(o => o.order_date)
                        })
                        .ToListAsync();

                    sb.AppendLine("Mã khách hàng,Tên,Email,Số đơn hàng,Tổng chi tiêu,Đơn hàng đầu tiên,Đơn hàng gần nhất");
                    foreach (var customer in customers)
                    {
                        string name = EscapeCsv($"{customer.User.first_name} {customer.User.last_name}");
                        string email = EscapeCsv(customer.User.email);
                        sb.AppendLine($"{customer.User.user_id},{name},{email},{customer.OrderCount},{customer.TotalSpent},{customer.FirstOrderDate:yyyy-MM-dd},{customer.LastOrderDate:yyyy-MM-dd}");
                    }
                    fileName = $"bao_cao_khach_hang_{startDate:yyyy-MM-dd}_den_{endDate:yyyy-MM-dd}.csv";
                    break;

                case "inventory":
                    var inventory = await _context.Product
                        .Include(p => p.Category)
                        .OrderBy(p => p.Category.name)
                        .ThenBy(p => p.name)
                        .ToListAsync();

                    sb.AppendLine("Mã sản phẩm,Mã SKU,Tên,Danh mục,Giá,Tồn kho,Trạng thái");
                    foreach (var product in inventory)
                    {
                        string status = product.stock > 10 ? "Còn hàng" : product.stock > 0 ? "Sắp hết hàng" : "Hết hàng";
                        sb.AppendLine($"{product.product_id},{EscapeCsv(product.sku)},{EscapeCsv(product.name)},{EscapeCsv(product.Category.name)},{product.price},{product.stock},{status}");
                    }
                    fileName = $"bao_cao_ton_kho_{DateTime.Now:yyyy-MM-dd}.csv";
                    break;

                default:
                    return BadRequest("Loại báo cáo không hợp lệ");
            }

            // Trả về tệp CSV với UTF-8 BOM để sửa lỗi font
            byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(buffer, "text/csv", fileName);
        }

        /// <summary>
        /// Xử lý các giá trị CSV có chứa dấu phẩy, ngoặc kép hoặc dòng mới
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            if (mustQuote)
            {
                value = value.Replace("\"", "\"\""); // Escape dấu ngoặc kép
                return $"\"{value}\"";
            }

            return value;
        }

        // Phương thức mới để tạo báo cáo PDF với khoảng thời gian
        public async Task<IActionResult> GeneratePdf(string reportType, DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Thiết lập khoảng thời gian mặc định nếu không cung cấp
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Trong ứng dụng thực tế, bạn sẽ triển khai tạo PDF ở đây
            // Đối với ví dụ này, chúng ta sẽ chỉ trả về một thông báo
            return Content($"Tạo PDF sẽ được triển khai ở đây cho: {reportType} từ {startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}");
        }

        // Cập nhật bảng điều khiển cho phân tích tồn kho
        public async Task<IActionResult> Inventory(DateTime? startDate, DateTime? endDate)
        {
            // Kiểm tra người dùng có phải là admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Mặc định là 30 ngày qua nếu không cung cấp khoảng thời gian
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Lấy thống kê tồn kho (trạng thái hiện tại - không cần lọc theo thời gian)
            var totalProducts = await _context.Product.CountAsync();
            var totalStock = await _context.Product.SumAsync(p => p.stock);
            var lowStockCount = await _context.Product.CountAsync(p => p.stock < 10);
            var outOfStockCount = await _context.Product.CountAsync(p => p.stock == 0);

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalStock = totalStock;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.OutOfStockCount = outOfStockCount;

            // Lấy tồn kho theo danh mục
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

            // Lấy biến động tồn kho cho khoảng thời gian đã chọn (dựa trên doanh số thực tế)
            var stockMovement = new List<object>
            {
                new { Month = "Tháng 11/2024", Incoming = 120, Outgoing = 78, Stock = 580 },
                new { Month = "Tháng 12/2024", Incoming = 150, Outgoing = 102, Stock = 628 },
                new { Month = "Tháng 1/2025", Incoming = 80, Outgoing = 95, Stock = 613 },
                new { Month = "Tháng 2/2025", Incoming = 110, Outgoing = 89, Stock = 634 },
                new { Month = "Tháng 3/2025", Incoming = 90, Outgoing = 105, Stock = 619 },
                new { Month = "Tháng 4/2025", Incoming = 130, Outgoing = 100, Stock = 649 }
            };

            ViewBag.StockMovement = stockMovement;

            return View();
        }

        // Các phương thức hỗ trợ đã cập nhật với hỗ trợ khoảng thời gian
        private async Task<List<object>> GetRecentOrderStats(DateTime startDate, DateTime endDate)
        {
            // Lấy đơn hàng từ khoảng đã chọn
            var recentOrdersData = await _context.Order
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .GroupBy(o => o.order_date.Date)
                .Select(group => new {
                    Date = group.Key,
                    Total = group.Sum(o => o.total_price)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Sau đó thực hiện định dạng chuỗi trong bộ nhớ
            var recentOrders = recentOrdersData
                .Select(item => new {
                    Date = item.Date.ToString("yyyy-MM-dd"),
                    Total = item.Total
                })
                .ToList();

            return recentOrders.Cast<object>().ToList();
        }

        private async Task<List<object>> GetTopProductStats(DateTime startDate, DateTime endDate)
        {
            var topProducts = await _context.Order_item
                .Where(oi => oi.Order.order_date >= startDate && oi.Order.order_date <= endDate)
                .GroupBy(oi => new {
                    ProductId = oi.product_id,
                    ProductName = oi.Product.name
                })
                .Select(group => new {
                    ProductName = group.Key.ProductName,
                    QuantitySold = group.Sum(oi => oi.quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToListAsync();

            return topProducts.Cast<object>().ToList();
        }

        private async Task<List<object>> GetCategoryStats(DateTime startDate, DateTime endDate)
        {
            var categoryStats = await _context.Order_item
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.order_date >= startDate && oi.Order.order_date <= endDate)
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
                0 => "Chờ xử lý",
                1 => "Đang xử lý",
                2 => "Hoàn thành",
                _ => "Không xác định"
            };
        }
    }
}