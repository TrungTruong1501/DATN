using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using FashionShop.Models.Entities;

namespace FashionShop.Controllers
{
    public class WishlistController : Controller
    {
        private readonly FashionShopContext _context;

        public WishlistController(FashionShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await _context.Wishlist
                .Include(w => w.Product)
                    .ThenInclude(p => p.Colors)
                .Where(w => w.user_id == userId.Value)
                .ToListAsync();

            return View(wishlist);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var existingItem = await _context.Wishlist
                .FirstOrDefaultAsync(w => w.user_id == userId.Value && w.product_id == productId);

            if (existingItem == null)
            {
                var wishlistItem = new Wishlist
                {
                    user_id = userId.Value,
                    product_id = productId
                };

                _context.Wishlist.Add(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int wishlistId)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItem = await _context.Wishlist
                .FirstOrDefaultAsync(w => w.wishlist_id == wishlistId && w.user_id == userId.Value);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            _context.Wishlist.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MoveToCart(int wishlistId, int colorId)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItem = await _context.Wishlist
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.wishlist_id == wishlistId && w.user_id == userId.Value);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            var existingCartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.user_id == userId.Value && ci.product_id == wishlistItem.product_id && ci.color_id == colorId);

            if (existingCartItem != null)
            {
                existingCartItem.quantity += 1;
                _context.CartItem.Update(existingCartItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    user_id = userId.Value,
                    product_id = wishlistItem.product_id.Value,
                    color_id = colorId,
                    quantity = 1
                };
                _context.CartItem.Add(cartItem);
            }

            _context.Wishlist.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}