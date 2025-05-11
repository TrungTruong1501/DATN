// UserAdminController.cs - Complete Updated Version
using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

namespace FashionShop.Controllers
{
    public class UserAdminController : Controller
    {
        private readonly FashionShopContext _context;

        public UserAdminController(FashionShopContext context)
        {
            _context = context;
        }

        // Helper method to hash password using MD5
        private string HashPasswordMD5(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // GET: UserAdmin
        public async Task<IActionResult> Index(int page = 1)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;
            var skip = (page - 1) * pageSize;

            var users = await _context.User
                .OrderBy(u => u.username)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalUsers = await _context.User.CountAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            ViewBag.TotalUsers = totalUsers;

            return View(users);
        }

        // GET: UserAdmin/Details/5
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
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Get user's orders
            var userOrders = await _context.Order
                .Where(o => o.user_id == id)
                .OrderByDescending(o => o.order_date)
                .ToListAsync();

            ViewBag.UserOrders = userOrders;
            ViewBag.OrderCount = userOrders.Count;

            return View(user);
        }

        // GET: UserAdmin/Create
        public IActionResult Create()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new User());
        }

        // POST: UserAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Clear previous model state
            ModelState.Clear();

            // Manual validation
            bool hasErrors = false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.username))
            {
                ModelState.AddModelError("username", "Tên đăng nhập là bắt buộc");
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(user.password))
            {
                ModelState.AddModelError("password", "Mật khẩu là bắt buộc");
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(user.email))
            {
                ModelState.AddModelError("email", "Email là bắt buộc");
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(user.first_name))
            {
                ModelState.AddModelError("first_name", "Họ là bắt buộc");
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(user.last_name))
            {
                ModelState.AddModelError("last_name", "Tên là bắt buộc");
                hasErrors = true;
            }

            // Check if username already exists
            if (!string.IsNullOrWhiteSpace(user.username))
            {
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.username == user.username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("username", "Tên đăng nhập đã tồn tại.");
                    hasErrors = true;
                }
            }

            // Check if email already exists
            if (!string.IsNullOrWhiteSpace(user.email))
            {
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.email == user.email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("email", "Email đã được sử dụng.");
                    hasErrors = true;
                }
            }

            // If no errors, save the user
            if (!hasErrors)
            {
                try
                {
                    // Trim all string fields
                    user.username = user.username?.Trim();
                    user.email = user.email?.Trim();
                    user.first_name = user.first_name?.Trim();
                    user.last_name = user.last_name?.Trim();
                    user.phone_number = user.phone_number?.Trim();
                    user.address = user.address?.Trim();

                    // Hash password using MD5 before saving to database
                    user.password = HashPasswordMD5(user.password);

                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tạo người dùng mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu vào cơ sở dữ liệu: {ex.Message}");
                }
            }

            // Log errors for debugging
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Error at {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(user);
        }

        // GET: UserAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Store current password hash for reference but don't display it
            ViewBag.HasPassword = !string.IsNullOrEmpty(user.password);
            user.password = ""; // Clear password field for display

            return View(user);
        }

        // POST: UserAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if id matches
            if (id != user.user_id)
            {
                return NotFound();
            }

            // Get the existing user
            var existingUser = await _context.User.AsNoTracking().FirstOrDefaultAsync(u => u.user_id == id);
            if (existingUser == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Manual validation
                bool hasErrors = false;

                if (string.IsNullOrWhiteSpace(user.username))
                {
                    ModelState.AddModelError("username", "Tên đăng nhập là bắt buộc");
                    hasErrors = true;
                }

                if (string.IsNullOrWhiteSpace(user.email))
                {
                    ModelState.AddModelError("email", "Email là bắt buộc");
                    hasErrors = true;
                }

                if (string.IsNullOrWhiteSpace(user.first_name))
                {
                    ModelState.AddModelError("first_name", "Họ là bắt buộc");
                    hasErrors = true;
                }

                if (string.IsNullOrWhiteSpace(user.last_name))
                {
                    ModelState.AddModelError("last_name", "Tên là bắt buộc");
                    hasErrors = true;
                }

                // Check for duplicate username
                if (!string.IsNullOrWhiteSpace(user.username) && user.username != existingUser.username)
                {
                    var userWithSameUsername = await _context.User.FirstOrDefaultAsync(u => u.username == user.username && u.user_id != id);
                    if (userWithSameUsername != null)
                    {
                        ModelState.AddModelError("username", "Tên đăng nhập đã tồn tại.");
                        hasErrors = true;
                    }
                }

                // Check for duplicate email
                if (!string.IsNullOrWhiteSpace(user.email) && user.email != existingUser.email)
                {
                    var userWithSameEmail = await _context.User.FirstOrDefaultAsync(u => u.email == user.email && u.user_id != id);
                    if (userWithSameEmail != null)
                    {
                        ModelState.AddModelError("email", "Email đã được sử dụng.");
                        hasErrors = true;
                    }
                }

                if (!hasErrors)
                {
                    // Update all fields
                    existingUser.username = user.username?.Trim();
                    existingUser.email = user.email?.Trim();
                    existingUser.first_name = user.first_name?.Trim();
                    existingUser.last_name = user.last_name?.Trim();
                    existingUser.phone_number = user.phone_number?.Trim();
                    existingUser.address = user.address?.Trim();
                    existingUser.permission = user.permission;

                    // Handle password - only update if new password is provided
                    if (!string.IsNullOrWhiteSpace(user.password))
                    {
                        existingUser.password = HashPasswordMD5(user.password);
                    }
                    // If password is empty, keep the existing password

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }

                // Log errors for debugging
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Error at {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                // If validation failed, restore the view data
                ViewBag.HasPassword = true;
                user.password = ""; // Don't send password back to form

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");

                // Restore view state
                ViewBag.HasPassword = true;
                user.password = "";

                return View(user);
            }
        }

        // GET: UserAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Check if user has orders
            var hasOrders = await _context.Order.AnyAsync(o => o.user_id == id);
            ViewBag.HasOrders = hasOrders;
            ViewBag.OrderCount = hasOrders ? await _context.Order.CountAsync(o => o.user_id == id) : 0;

            return View(user);
        }

        // POST: UserAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.User.FindAsync(id);

            // Check if user exists
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Check if user has orders
            var hasOrders = await _context.Order.AnyAsync(o => o.user_id == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng này vì họ đã có đơn hàng trong hệ thống.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            try
            {
                _context.User.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa người dùng: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.user_id == id);
        }
    }
}