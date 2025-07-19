using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheCoffee.Models.ViewModel
{
    public class HomeIndexVM
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }

        public string SearchKeyword { get; set; }
        public int? SelectedCategoryId { get; set; }

        public string SortOrder { get; set; }

        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}