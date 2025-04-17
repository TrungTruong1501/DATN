using FashionShop.Models.Entities;
using System.Collections.Generic;

namespace FashionShop.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public User User { get; set; }
        public List<CartItem> CartItems { get; set; }
        public List<Payment> PaymentMethods { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public int SelectedPaymentId { get; set; }
    }
}