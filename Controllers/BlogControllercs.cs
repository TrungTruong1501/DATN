using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class BlogController : Controller
    {
        private readonly FashionShopContext _context;

        public BlogController(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 6;
            var query = _context.Blog.AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

            var blogs = await query
                .OrderByDescending(b => b.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(blogs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blog
                .FirstOrDefaultAsync(b => b.id == id);

            if (blog == null)
            {
                return NotFound();
            }

            var recentBlogs = await _context.Blog
                .Where(b => b.id != id)
                .OrderByDescending(b => b.id)
                .Take(3)
                .ToListAsync();

            ViewBag.RecentBlogs = recentBlogs;

            return View(blog);
        }
    }
}