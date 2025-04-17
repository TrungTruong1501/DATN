using System;
using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }
        public DateTime order_date { get; set; }
        public int? total_price { get; set; }
        public int? user_id { get; set; }
        public int? order_status { get; set; } // 0: Pending, 1: Processing, 2: Completed
        public string address { get; set; }
        public int? payment_id { get; set; }
        public string phone { get; set; }
        public string RowState { get; set; } = "Unchanged";

        // Navigation properties
        public User User { get; set; }
        public Payment Payment { get; set; }
        public ICollection<Order_item> Order_item { get; set; }
    }
}