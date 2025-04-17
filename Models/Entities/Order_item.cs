using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Order_item
    {
        [Key]
        public int order_item_id { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public int? product_id { get; set; }
        public int? order_id { get; set; }
        public int? color_id { get; set; }
        public string RowState { get; set; } = "Unchanged";

        // Navigation properties
        public Product Product { get; set; }
        public Order Order { get; set; }
        public Color Color { get; set; }
    }
}