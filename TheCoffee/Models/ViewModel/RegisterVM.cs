using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class RegisterVM
    {
        [Required]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string ConfirmPassword { get; set; }

        public string FullName { get; set; }
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}