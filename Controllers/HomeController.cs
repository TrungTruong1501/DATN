using FashionShop.Models.Entities;
using FashionShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly FashionShopContext _context;

        public HomeController(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Category.ToListAsync();
            var featuredProducts = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Colors)
                .OrderByDescending(p => p.product_id)
                .Take(8)
                .ToListAsync();

            var newArrivals = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Colors)
                .OrderByDescending(p => p.product_id)
                .Take(4)
                .ToListAsync();

            var recentBlogs = await _context.Blog
                .OrderByDescending(b => b.id)
                .Take(3)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts,
                NewArrivals = newArrivals,
                RecentBlogs = recentBlogs
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}