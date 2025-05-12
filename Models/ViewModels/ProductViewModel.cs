using FashionShop.Models.Entities;
using System.Collections.Generic;

namespace FashionShop.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
        public List<Color> AvailableColors { get; set; }
        public Category Category { get; set; }
        public Dictionary<int, string> SizeOptions { get; set; } // Thêm thuộc tính mới
        public int SelectedSize { get; set; } // Thêm thuộc tính mới
    }
}