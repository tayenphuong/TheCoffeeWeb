using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Ghi chú đơn hàng")]
        public string OrderNote { get; set; }

        // Danh sách sản phẩm từ giỏ hàng
        public List<CartItemViewModel> CartItems { get; set; }

        public int MethodID { get; set; }
        // Tổng tiền
        public decimal Total { get; set; }
    }
}