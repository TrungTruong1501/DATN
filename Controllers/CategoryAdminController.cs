using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using FashionShop.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace FashionShop.Controllers
{
    public class CategoryAdminController : Controller
    {
        private readonly FashionShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryAdminController(FashionShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var categories = await _context.Category.Include(c => c.Products).ToListAsync();
            return View(categories);
        }

        public IActionResult Add()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category, IFormFile imageFile)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Clear previous model state
            ModelState.Clear();

            // Validate category name
            if (string.IsNullOrEmpty(category.name))
            {
                ModelState.AddModelError("name", "Tên danh mục là bắt buộc");
            }

            // Validate image
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Hình ảnh danh mục là bắt buộc");
            }

            try
            {
                // Process image if uploaded
                if (imageFile != null && imageFile.Length > 0)
                {
                    category.image = await SaveImage(imageFile);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("imageFile", $"Lỗi khi xử lý file: {ex.Message}");
            }

            // Validate model state
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure Products is not null
                    category.Products = new List<Product>();

                    _context.Category.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                    return RedirectToAction("Index");
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
                    Console.WriteLine($"Lỗi ở {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category category, IFormFile imageFile)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            // Clear previous model state
            ModelState.Clear();

            // Validate category name
            if (string.IsNullOrEmpty(category.name))
            {
                ModelState.AddModelError("name", "Tên danh mục là bắt buộc");
            }

            // Check if category exists
            var existingCategory = await _context.Category.AsNoTracking()
                .FirstOrDefaultAsync(c => c.category_id == category.category_id);

            if (existingCategory == null)
            {
                return NotFound();
            }

            // Handle image processing
            try
            {
                // Process image if uploaded
                if (imageFile != null && imageFile.Length > 0)
                {
                    category.image = await SaveImage(imageFile);
                }
                else
                {
                    // Keep the old image if no new one is uploaded
                    category.image = existingCategory.image;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("imageFile", $"Lỗi khi xử lý file: {ex.Message}");
            }

            // Validate model state
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure Products is not null
                    category.Products = new List<Product>();

                    _context.Entry(category).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật cơ sở dữ liệu: {ex.Message}");
                }
            }

            // Log errors for debugging
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    Console.WriteLine($"Lỗi ở {state.Key}: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {
                // Delete related products first
                var relatedProducts = await _context.Product.Where(p => p.category_id == id).ToListAsync();
                if (relatedProducts.Any())
                {
                    _context.Product.RemoveRange(relatedProducts);
                    await _context.SaveChangesAsync();
                }

                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa danh mục: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // Modified helper method to save just the filename instead of the full path
        private async Task<string> SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            // Path to Images/ProductImage in wwwroot
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImage");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return only the filename instead of the full path
            return uniqueFileName;
        }
    }
}