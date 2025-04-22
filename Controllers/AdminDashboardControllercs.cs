using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FashionShop.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly FashionShopContext _context;

        public AdminDashboardController(FashionShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get counts for dashboard
            ViewBag.ProductCount = _context.Product.Count();
            ViewBag.OrderCount = _context.Order.Count();
            ViewBag.UserCount = _context.User.Count();
            ViewBag.CategoryCount = _context.Category.Count();

            // Get recent orders
            var recentOrders = _context.Order
                .Include(o => o.User)
                .OrderByDescending(o => o.order_date)
                .Take(5)
                .ToList();

            return View(recentOrders);
        }
    }
}