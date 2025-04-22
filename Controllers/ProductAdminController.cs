using FashionShop.Models;
using FashionShop.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FashionShop.Helpers;
namespace FashionShop.Controllers
{
    public class ProductAdminController : Controller
    {
        private readonly FashionShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductAdminController(FashionShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;
            var products = await _context.Product
                .Include(p => p.Category)
                .OrderByDescending(p => p.product_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(_context.Product.Count() / (double)pageSize);

            return View(products);
        }

        // GET method
        public async Task<IActionResult> Add()
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Categories = await _context.Category.ToListAsync();
            return View();
        }

        // POST method
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile mainImage, IFormCollection form)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Process main product image
                    if (mainImage != null && mainImage.Length > 0)
                    {
                        var fileName = Path.GetFileName(mainImage.FileName);
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                        // Ensure directory exists
                        Directory.CreateDirectory(uploadPath);

                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await mainImage.CopyToAsync(fileStream);
                        }

                        product.image = fileName;
                    }

                    // Add product to database first to get product_id
                    _context.Product.Add(product);
                    await _context.SaveChangesAsync();

                    // Process colors
                    var colorNames = form["colorNames[]"];
                    var colorHexes = form["colorHexes[]"];
                    var colorImages = form.Files.GetFiles("colorImages[]");

                    if (colorNames.Count > 0)
                    {
                        for (int i = 0; i < colorNames.Count; i++)
                        {
                            if (i < colorHexes.Count && i < colorImages.Count)
                            {
                                var colorName = colorNames[i];
                                var colorHex = colorHexes[i];
                                var colorImage = colorImages[i];

                                if (colorImage != null && colorImage.Length > 0)
                                {
                                    var fileName = Path.GetFileName(colorImage.FileName);
                                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                                    // Ensure directory exists
                                    Directory.CreateDirectory(uploadPath);

                                    var filePath = Path.Combine(uploadPath, fileName);

                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await colorImage.CopyToAsync(fileStream);
                                    }

                                    // Create new color
                                    var color = new Color
                                    {
                                        product_id = product.product_id,
                                        color_name = colorName,
                                        color_hex = colorHex,
                                        image_url = fileName
                                    };

                                    _context.Color.Add(color);
                                }
                            }
                        }

                        // Save all colors
                        await _context.SaveChangesAsync();

                        // Update gallery images with color images
                        var galleryImages = new List<string>();
                        foreach (var colorImage in colorImages)
                        {
                            if (colorImage != null && colorImage.Length > 0)
                            {
                                galleryImages.Add(Path.GetFileName(colorImage.FileName));
                            }
                        }

                        if (galleryImages.Count > 0)
                        {
                            product.gallery = string.Join(",", galleryImages);
                            _context.Update(product);
                            await _context.SaveChangesAsync();
                        }
                    }

                    TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
            }

            ViewBag.Categories = await _context.Category.ToListAsync();
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Category.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile, IFormFileCollection galleryFiles)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.product_id == product.product_id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (imageFile != null)
                {
                    product.image = await SaveImage(imageFile);
                }
                else
                {
                    product.image = existingProduct.image;
                }

                if (galleryFiles != null && galleryFiles.Count > 0)
                {
                    var galleryPaths = new List<string>();
                    foreach (var file in galleryFiles)
                    {
                        var path = await SaveImage(file, "gallery");
                        if (!string.IsNullOrEmpty(path))
                            galleryPaths.Add(path);
                    }

                    if (galleryPaths.Count > 0)
                    {
                        product.gallery = string.Join(",", galleryPaths);
                    }
                    else
                    {
                        product.gallery = existingProduct.gallery;
                    }
                }
                else
                {
                    product.gallery = existingProduct.gallery;
                }

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = await _context.Category.ToListAsync();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
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