using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FashionShop.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly FashionShopContext _context;

        public CartSummaryViewComponent(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return View(0);
            }

            var itemCount = await _context.CartItem
                .Where(ci => ci.user_id == userId.Value)
                .CountAsync();

            return View(itemCount);
        }
    }
}