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
    }
}