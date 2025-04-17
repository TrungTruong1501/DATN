using FashionShop.Models.Entities;
using System.Collections.Generic;

namespace FashionShop.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Product> NewArrivals { get; set; }
        public List<Blog> RecentBlogs { get; set; }
    }
}