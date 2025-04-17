using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Payment
    {
        [Key]
        public int payment_id { get; set; }
        public string name { get; set; }
        public string RowState { get; set; } = "Unchanged";

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
    }
}