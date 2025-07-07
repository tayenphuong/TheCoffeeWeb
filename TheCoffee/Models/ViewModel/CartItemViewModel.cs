using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class CartItemViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; } 
        public decimal Price { get; set; }

        // ✅ Thêm dòng này:
        public string Img { get; set; }
    }
}