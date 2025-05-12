using FashionShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Services
{
    public class ProductRecommendationService
    {
        private readonly FashionShopContext _context;

        public ProductRecommendationService(FashionShopContext context)
        {
            _context = context;
        }

        // Triển khai các phương thức đề xuất sản phẩm ở đây
        // Ví dụ:
        public async Task<List<Product>> GetSimilarProductsAsync(int productId, int limit = 3)
        {
            var product = await _context.Product
                .FirstOrDefaultAsync(p => p.product_id == productId);

            if (product == null) return new List<Product>();

            // Tìm sản phẩm cùng danh mục
            var similarProducts = await _context.Product
                .Where(p => p.product_id != productId &&
                       p.category_id == product.category_id &&
                       p.stock > 0)
                .OrderByDescending(p => p.category_id)
                .Take(limit)
                .ToListAsync();

            return similarProducts;
        }
    }
}