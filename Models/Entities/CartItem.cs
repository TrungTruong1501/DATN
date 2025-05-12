using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class CartItem
    {
        [Key]
        public int cart_item_id { get; set; }
        public int user_id { get; set; }
        public int product_id { get; set; }
        public int color_id { get; set; }
        public int quantity { get; set; }
        public string RowState { get; set; } = "Unchanged";
        public int size { get; set; } = 0;


        // Navigation properties
        public User User { get; set; }
        public Product Product { get; set; }
        public Color Color { get; set; }
    }
}