using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class OrderViewModel
    {
        public string Address { get; set; }
        public string Note { get; set; } // Ghi chú đơn hàng
        public int OrderTypeID { get; set; } // ví dụ: mang về, giao hàng
        public int MethodID { get; set; } // phương thức thanh toán
    }
}