using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FashionShop.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly FashionShopContext _context;

        public CategoryMenuViewComponent(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Category.ToListAsync();
            return View(categories);
        }
    }
}