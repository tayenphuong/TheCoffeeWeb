using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class CustomerOrderVM
    {
        public Order Order { get; set; }
        public bool HasUnrated { get; set; }
        public List<Rating> Ratings { get; set; }
    }
}