using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using FashionShop.Helpers;

namespace FashionShop.Controllers
{
    public class OrderAdminController : Controller
    {
        private readonly FashionShopContext _context;

        public OrderAdminController(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;
            var orders = await _context.Order
                .Include(o => o.User)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.order_date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(_context.Order.Count() / (double)pageSize);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Order
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.Order_item)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Order_item)
                    .ThenInclude(oi => oi.Color)
                .FirstOrDefaultAsync(o => o.order_id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, int status)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Order.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.order_status = status;
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}