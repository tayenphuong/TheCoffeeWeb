using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class ProductViewModel
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public double Rating { get; set; }
        public bool IsPopular { get; set; }
    }
}