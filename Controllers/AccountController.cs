using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly FashionShopContext _context;

        public AccountController(FashionShopContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "username and password are required";
                return View();
            }

            // Hash password
            var hashedPassword = GetMD5Hash(password);

            var user = await _context.User.FirstOrDefaultAsync(u => u.username == username && u.password == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // Set session
            HttpContext.Session.SetInt32("user_id", user.user_id);
            HttpContext.Session.SetString("username", user.username);
            HttpContext.Session.SetInt32("permission", user.permission);
            if (user.permission == 1)
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string confirm_password)
        {
            if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.password) ||
                string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(confirm_password))
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(user);
            }

            if (user.password != confirm_password)
            {
                ViewBag.Error = "Passwords do not match";
                return View(user);
            }

            // Check if username already exists
            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.username == user.username);
            if (existingUser != null)
            {
                ViewBag.Error = "username already exists";
                return View(user);
            }

            // Check if email already exists
            existingUser = await _context.User.FirstOrDefaultAsync(u => u.email == user.email);
            if (existingUser != null)
            {
                ViewBag.Error = "email already exists";
                return View(user);
            }

            // Hash password
            user.password = GetMD5Hash(user.password);
            user.permission = 0; // Regular user

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.User.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.User.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            user.first_name = updatedUser.first_name;
            user.last_name = updatedUser.last_name;
            user.email = updatedUser.email;
            user.address = updatedUser.address;
            user.phone_number = updatedUser.phone_number;

            _context.User.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Profile updated successfully";
            return View("Profile", user);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.User.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            if (GetMD5Hash(currentPassword) != user.password)
            {
                ViewBag.PasswordError = "Current password is incorrect";
                return View("Profile", user);
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.PasswordError = "New passwords do not match";
                return View("Profile", user);
            }

            user.password = GetMD5Hash(newPassword);
            _context.User.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.PasswordSuccess = "password changed successfully";
            return View("Profile", user);
        }

        public async Task<IActionResult> Orders()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var orders = await _context.Order.Include(o => o.Order_item)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Order_item)
                .ThenInclude(oi => oi.Color)
                .Include(o => o.Payment)
                .Where(o => o.user_id == userId.Value)
                .OrderByDescending(o => o.order_date)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var order = await _context.Order
                .Include(o => o.Order_item)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Order_item)
                    .ThenInclude(oi => oi.Color)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.order_id == id && o.user_id == userId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private string GetMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}