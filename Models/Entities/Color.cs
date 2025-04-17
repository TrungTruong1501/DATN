using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Color
    {
        [Key]
        public int color_id { get; set; }
        public int product_id { get; set; }
        public string color_name { get; set; }
        public string color_hex { get; set; }
        public string image_url { get; set; }
        public string RowState { get; set; } = "Unchanged";

        // Navigation properties
        public Product Product { get; set; }
        public ICollection<Order_item> OrderItems { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}