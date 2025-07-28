using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TheCoffee.Models.ViewModel
{
    
    public class OrderCreateVM
    {
        [Required(ErrorMessage = "Vui lòng đăng nhập trước khi tạo đơn")]
        public int UserID { get; set; } 
        [Required(ErrorMessage = "Vui lòng chọn loại đơn")]
        public int OrderTypeID { get; set; } // 1 - Tại chỗ, 2 - Giao hàng
        public string Address { get; set; }
        public string Note { get; set; }

        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();
        public List<SelectListItem> OrderTypes { get; set; }
        public List<Product> AvailableProducts { get; set; }
        public List<Category> Categories { get; set; }

        public string ItemsJson { get; set; }
    }

    public class OrderItemVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}