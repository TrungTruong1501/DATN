using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Wishlist
    {
        [Key]
        public int wishlist_id { get; set; }
        public int? user_id { get; set; }
        public int? product_id { get; set; }
        public string RowState { get; set; } = "Unchanged";

        // Navigation properties
        public User User { get; set; }
        public Product Product { get; set; }
    }
}