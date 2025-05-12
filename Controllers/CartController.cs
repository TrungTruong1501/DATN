using FashionShop.Models;
using FashionShop.Models.Entities;
using FashionShop.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class CartController : Controller
    {
        private readonly FashionShopContext _context;

        public CartController(FashionShopContext context)
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

            var cartItems = await _context.CartItem
                .Include(ci => ci.Product)
                .Include(ci => ci.Color)
                .Where(ci => ci.user_id == userId.Value)
                .ToListAsync();

            var totalPrice = cartItems.Sum(ci => ci.Product.price * ci.quantity);

            var viewModel = new CartViewModel
            {
                CartItems = cartItems,
                TotalPrice = totalPrice,
                ItemCount = cartItems.Count
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int colorId, int sizeId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if product exists
            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Check if color exists
            var color = await _context.Color.FindAsync(colorId);
            if (color == null)
            {
                return NotFound();
            }

            // Check if the item is already in the cart
            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(c => c.user_id == userId && c.product_id == productId && c.color_id == colorId && c.size == sizeId);

            if (cartItem != null)
            {
                // Update quantity if item already in cart
                cartItem.quantity += quantity;
            }
            else
            {
                // Add new item to cart
                cartItem = new CartItem
                {
                    user_id = userId.Value,
                    product_id = productId,
                    color_id = colorId,
                    size = sizeId,
                    quantity = quantity
                };
                _context.CartItem.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.cart_item_id == cartItemId && ci.user_id == userId.Value);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItem.Remove(cartItem);
            }
            else
            {
                cartItem.quantity = quantity;
                _context.CartItem.Update(cartItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.cart_item_id == cartItemId && ci.user_id == userId.Value);

            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItem.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.User.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            var cartItems = await _context.CartItem
                .Include(ci => ci.Product)
                .Include(ci => ci.Color)
                .Where(ci => ci.user_id == userId.Value)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var totalPrice = cartItems.Sum(ci => ci.Product.price * ci.quantity);
            var paymentMethods = await _context.Payment.ToListAsync();

            var viewModel = new CheckoutViewModel
            {
                User = user,
                CartItems = cartItems,
                PaymentMethods = paymentMethods,
                TotalPrice = totalPrice,
                ShippingAddress = user.address,
                PhoneNumber = user.phone_number
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sử dụng transaction để đảm bảo tính toàn vẹn
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cartItems = await _context.CartItem
                        .Include(ci => ci.Product)
                        .Include(ci => ci.Color)
                        .Where(ci => ci.user_id == userId.Value)
                        .ToListAsync();

                    if (cartItems.Count == 0)
                    {
                        return RedirectToAction("Index");
                    }

                    var totalPrice = cartItems.Sum(ci => ci.Product.price * ci.quantity);

                    // Kiểm tra số lượng tồn kho
                    foreach (var item in cartItems)
                    {
                        var product = await _context.Product.FindAsync(item.product_id);
                        if (product.stock < item.quantity)
                        {
                            ModelState.AddModelError("", $"Sản phẩm {product.name} không đủ số lượng trong kho.");
                            return RedirectToAction("Checkout");
                        }
                    }

                    var order = new Order
                    {
                        user_id = userId.Value,
                        order_date = System.DateTime.Now,
                        total_price = (int)totalPrice,
                        address = model.ShippingAddress,
                        phone = model.PhoneNumber,
                        payment_id = model.SelectedPaymentId,
                        order_status = 0 // Pending
                    };

                    _context.Order.Add(order);
                    await _context.SaveChangesAsync();

                    // Chuyển đổi CartItems thành danh sách OrderItems
                    var orderItems = cartItems.Select(item => new Order_item
                    {
                        order_id = order.order_id,
                        product_id = item.product_id,
                        color_id = item.color_id,
                        quantity = item.quantity,
                        price = item.Product.price
                    }).ToList();

                    // Thêm tất cả OrderItems cùng một lúc
                    _context.Order_item.AddRange(orderItems);

                    // Giảm số lượng sản phẩm trong kho
                    foreach (var item in cartItems)
                    {
                        var product = await _context.Product.FindAsync(item.product_id);
                        product.stock -= item.quantity;
                        _context.Product.Update(product);
                    }

                    // Xóa tất cả CartItems
                    _context.CartItem.RemoveRange(cartItems);

                    // Lưu thay đổi
                    await _context.SaveChangesAsync();

                    // Commit transaction khi mọi thứ thành công
                    await transaction.CommitAsync();

                    return RedirectToAction("OrderConfirmation", new { orderId = order.order_id });
                }
                catch (DbUpdateException ex)
                {
                    // Xử lý lỗi cụ thể liên quan đến cơ sở dữ liệu
                    await transaction.RollbackAsync();

                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu đơn hàng. Vui lòng thử lại sau.");
                    return RedirectToAction("Checkout");
                }
                catch (Exception ex)
                {
                    // Xử lý các lỗi khác
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại sau.");
                    return RedirectToAction("Checkout");
                }
            }
        }
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Order
                .Include(o => o.Order_item)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Order_item)
                    .ThenInclude(oi => oi.Color)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.order_id == orderId && o.user_id == userId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}