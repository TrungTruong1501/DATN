using FashionShop.Models.Entities;
using FashionShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
namespace FashionShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly FashionShopContext _context;
        public ProductController(FashionShopContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? categoryId, string search, int page = 1)
        {
            const int pageSize = 12;
            var query = _context.Product
                .Include(p => p.Category)
                .Include(p => p.Colors)
                .AsQueryable();
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.category_id == categoryId);
                ViewBag.CategoryName = await _context.Category
                    .Where(c => c.category_id == categoryId)
                    .Select(c => c.name)
                    .FirstOrDefaultAsync();
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.name.Contains(search) || p.description.Contains(search));
                ViewBag.SearchTerm = search;
            }
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var products = await query
                .OrderByDescending(p => p.product_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CategoryId = categoryId;
            return View(products);
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Product
                .Include(p => p.Category)
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.product_id == id);
            if (product == null)
            {
                return NotFound();
            }
            var relatedProducts = await _context.Product
                .Include(p => p.Colors)
                .Where(p => p.category_id == product.category_id && p.product_id != id)
                .Take(4)
                .ToListAsync();

            // Create a list of available sizes based on the size attribute values
            var sizeNames = new Dictionary<int, string>
            {
                { 0, "S" },
                { 1, "M" },
                { 2, "L" },
                { 3, "XL" },
                { 4, "XXL" }
            };

            // Default size is 0 (S) if not specified
            var defaultSize = 0;

            var viewModel = new ProductViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                AvailableColors = product.Colors.ToList(),
                Category = product.Category,
                SizeOptions = sizeNames,
                SelectedSize = defaultSize
            };

            return View(viewModel);
        }
    }
}