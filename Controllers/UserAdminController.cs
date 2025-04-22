using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class UserAdminController : Controller
    {
        private readonly FashionShopContext _context;

        public UserAdminController(FashionShopContext context)
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
            var users = await _context.User
                .OrderBy(u => u.username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(_context.User.Count() / (double)pageSize);

            return View(users);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's orders
            var userOrders = await _context.Order
                .Where(o => o.user_id == id)
                .OrderByDescending(o => o.order_date)
                .ToListAsync();

            ViewBag.UserOrders = userOrders;

            return View(user);
        }
    }
}