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

        public async Task<IActionResult> Index(int page = 1, string searchTerm = null)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 10;

            // Tạo truy vấn cơ sở
            var query = _context.Product
                .Include(p => p.Category)
                .AsQueryable();

            // Áp dụng tìm kiếm nếu có searchTerm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.name.Contains(searchTerm) ||
                    p.description.Contains(searchTerm) ||
                    p.Category.name.Contains(searchTerm));

                ViewBag.SearchTerm = searchTerm;
            }

            // Đếm tổng số kết quả
            var totalItems = await query.CountAsync();

            // Áp dụng phân trang
            var products = await query
                .OrderByDescending(p => p.product_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

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

            // DEBUG: Kiểm tra và log các lỗi từ ModelState
            if (!ModelState.IsValid)
            {
                var errorMessages = new List<string>();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errorMessages.Add($"Lỗi ở trường '{state.Key}': {error.ErrorMessage}");
                        // Log ra console cho phát triển
                        System.Diagnostics.Debug.WriteLine($"Lỗi ở trường '{state.Key}': {error.ErrorMessage}");
                    }
                }

                // Thêm tất cả lỗi vào TempData để có thể hiển thị trong View
                TempData["ValidationErrors"] = errorMessages;
            }

            try
            {
                // Bỏ điều kiện ModelState.IsValid để xem lỗi nào xảy ra trong quá trình lưu
                // Hoặc giữ lại nếu bạn muốn chỉ hiển thị lỗi của ModelState
                // if (ModelState.IsValid)
                // {
                // Process main product image
                if (mainImage != null && mainImage.Length > 0)
                {
                    var fileName = Path.GetFileName(mainImage.FileName);
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImage");

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
                                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImage");

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
                // }
            }
            catch (Exception ex)
            {
                // Chi tiết hơn về lỗi
                ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
                // Log toàn bộ exception để debug
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");

                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", "Chi tiết lỗi: " + ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.ToString()}");
                }
            }

            // Thêm thông báo vào view để hiển thị
            ViewBag.ModelStateErrors = !ModelState.IsValid;
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
        public async Task<IActionResult> Edit(Product product, IFormFile mainImage, IFormCollection form)
        {
            // Check if user is admin
            if (HttpContext.Session.GetInt32("permission") != 1)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Get existing product first
                var existingProduct = await _context.Product.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.product_id == product.product_id);

                if (existingProduct == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy sản phẩm để cập nhật.");
                    ViewBag.Categories = await _context.Category.ToListAsync();
                    return View(product);
                }

                // Tạo một instance mới từ dữ liệu existing
                var productToUpdate = new Product
                {
                    product_id = existingProduct.product_id,
                    // Copy tất cả giá trị từ existing product
                    sku = existingProduct.sku,
                    name = existingProduct.name,
                    description = existingProduct.description,
                    price = existingProduct.price,
                    stock = existingProduct.stock,
                    category_id = existingProduct.category_id,
                    image = existingProduct.image,
                    gallery = existingProduct.gallery,
                    RowState = "Modified", // Đánh dấu là đã sửa đổi
                };

                // Attach object to context
                _context.Attach(productToUpdate);

                // Chỉ update những field mà bạn muốn thay đổi
                bool hasChanges = false;

                // 1. SKU - chỉ cập nhật nếu có giá trị
                if (!string.IsNullOrEmpty(product.sku) && product.sku != existingProduct.sku)
                {
                    productToUpdate.sku = product.sku;
                    _context.Entry(productToUpdate).Property(p => p.sku).IsModified = true;
                    hasChanges = true;
                }

                // 2. Name - chỉ cập nhật nếu có giá trị
                if (!string.IsNullOrEmpty(product.name) && product.name != existingProduct.name)
                {
                    productToUpdate.name = product.name;
                    _context.Entry(productToUpdate).Property(p => p.name).IsModified = true;
                    hasChanges = true;
                }

                // 3. Description - có thể là null
                if (product.description != existingProduct.description)
                {
                    productToUpdate.description = product.description;
                    _context.Entry(productToUpdate).Property(p => p.description).IsModified = true;
                    hasChanges = true;
                }

                // 4. Price - kiểm tra nếu thay đổi
                if (product.price != existingProduct.price)
                {
                    productToUpdate.price = product.price;
                    _context.Entry(productToUpdate).Property(p => p.price).IsModified = true;
                    hasChanges = true;
                }

                // 5. Stock - kiểm tra nếu thay đổi
                if (product.stock != existingProduct.stock)
                {
                    productToUpdate.stock = product.stock;
                    _context.Entry(productToUpdate).Property(p => p.stock).IsModified = true;
                    hasChanges = true;
                }

                // 6. Category - có thể là null
                if (product.category_id != existingProduct.category_id)
                {
                    productToUpdate.category_id = product.category_id;
                    _context.Entry(productToUpdate).Property(p => p.category_id).IsModified = true;
                    hasChanges = true;
                }

                // 7. Process main product image nếu có
                if (mainImage != null && mainImage.Length > 0)
                {
                    var fileName = Path.GetFileName(mainImage.FileName);
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImage");
                    Directory.CreateDirectory(uploadPath);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await mainImage.CopyToAsync(fileStream);
                    }

                    productToUpdate.image = fileName;
                    _context.Entry(productToUpdate).Property(p => p.image).IsModified = true;
                    hasChanges = true;
                }

                // 8. Cập nhật RowState nếu có thay đổi
                if (hasChanges)
                {
                    _context.Entry(productToUpdate).Property(p => p.RowState).IsModified = true;
                }

                // Save changes to main product
                await _context.SaveChangesAsync();

                // 9. Process colors - chỉ update khi có dữ liệu mới
                var colorNames = form["colorNames[]"];
                if (colorNames.Count > 0)
                {
                    // Delete existing colors
                    var existingColors = await _context.Color.Where(c => c.product_id == product.product_id).ToListAsync();
                    _context.Color.RemoveRange(existingColors);
                    await _context.SaveChangesAsync();

                    // Add new colors
                    var colorHexes = form["colorHexes[]"];
                    var colorImages = form.Files.GetFiles("colorImages[]");
                    var galleryImages = new List<string>();

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
                                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImage");
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
                                galleryImages.Add(fileName);
                            }
                        }
                    }

                    // Save colors
                    await _context.SaveChangesAsync();

                    // Update gallery if there are new images
                    if (galleryImages.Count > 0)
                    {
                        // Attach lại product để update gallery
                        var productForGallery = new Product { product_id = product.product_id };
                        _context.Attach(productForGallery);
                        productForGallery.gallery = string.Join(",", galleryImages);
                        productForGallery.RowState = "Modified";
                        _context.Entry(productForGallery).Property(p => p.gallery).IsModified = true;
                        _context.Entry(productForGallery).Property(p => p.RowState).IsModified = true;
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
                ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm: " + ex.Message);

                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", "Chi tiết lỗi: " + ex.InnerException.Message);
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException}");
                }
            }

            // Return to view with error
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

            try
            {
                var product = await _context.Product.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Xóa tất cả các bản ghi màu sắc liên quan trước
                var relatedColors = await _context.Color.Where(c => c.product_id == id).ToListAsync();
                _context.Color.RemoveRange(relatedColors);
                await _context.SaveChangesAsync();

                // Xóa các bản ghi order item liên quan (nếu có)
                var relatedOrderItems = await _context.Order_item.Where(oi => oi.product_id == id).ToListAsync();
                if (relatedOrderItems.Any())
                {
                    _context.Order_item.RemoveRange(relatedOrderItems);
                    await _context.SaveChangesAsync();
                }

                // Sau đó mới xóa sản phẩm
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và thông báo
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Helper method for image handling
        private async Task<string> SaveImage(IFormFile file, string folder = "")
        {
            if (file == null || file.Length == 0)
                return null;

            // Đường dẫn đến thư mục Images/ProductImage trong wwwroot
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "ProductImage", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn URL tương đối
            return $"/Images/ProductImage/{folder}/{uniqueFileName}";
        }
    }
}