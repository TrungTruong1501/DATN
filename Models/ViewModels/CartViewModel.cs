using FashionShop.Models.Entities;
using System.Collections.Generic;

namespace FashionShop.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemCount { get; set; }
    }
}