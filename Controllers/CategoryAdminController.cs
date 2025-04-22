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

            var categories = await _context.Category.ToListAsync();
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

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    category.image = await SaveImage(imageFile, "Categories");
                }

                _context.Category.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
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

            if (ModelState.IsValid)
            {
                var existingCategory = await _context.Category.AsNoTracking().FirstOrDefaultAsync(c => c.category_id == category.category_id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                if (imageFile != null)
                {
                    category.image = await SaveImage(imageFile, "Categories");
                }
                else
                {
                    category.image = existingCategory.image;
                }

                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
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

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Helper method for image handling
        private async Task<string> SaveImage(IFormFile file, string folder = "")
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
        }
    }
}