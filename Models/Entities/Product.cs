using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FashionShop.Models.Entities
{
    public class Product
    {
        [Key]
        public int product_id { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public int stock { get; set; }
        public int? category_id { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string gallery { get; set; }
        public string RowState { get; set; } = "Unchanged";
        public int size { get; set; } = 0;

        // Navigation properties
        public Category Category { get; set; }
        public ICollection<Color> Colors { get; set; }
        public ICollection<Order_item> OrderItems { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; }
    }
}