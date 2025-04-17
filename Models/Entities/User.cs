using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class User
    {
        [Key]
        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public int permission { get; set; }
        public string RowState { get; set; } = "Unchanged";

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; }
    }
}