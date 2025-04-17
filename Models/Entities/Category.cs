using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Category
    {
        [Key]
        public int category_id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public string rowstate { get; set; } = "Unchanged";

        // Navigation properties
        public ICollection<Product> Products { get; set; }
    }
}