using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;
using System.Data.Entity; // Để sử dụng Include

namespace TheCoffee.Controllers
{
    public class HomeController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        public ActionResult CusHome()
        {
            return View();
        }
        public ActionResult HomePage()
        {
            var products = db.Products.ToList(); // Hoặc select vào ProductViewModel
            return View(products);
        }
        
        public ActionResult ViewPage1()
        {
            return View();
        }
        public ActionResult Index(string search, int? categoryId, int? minPrice, int? maxPrice)
        {
            var products = db.Products.AsQueryable();

            // Danh mục (dropdown)
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName");

            // Lọc tìm kiếm
            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.ProductName.Contains(search));

            if (categoryId.HasValue && categoryId != 0)
                products = products.Where(p => p.CategoryID == categoryId);

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice);

            // Để giữ lại giá trị trong view
            ViewBag.Search = search;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products.ToList());
        }
        public ActionResult Detail(int id)
        {
            var product = db.Products.Include(n => n.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            // Gợi ý sản phẩm cùng danh mục, loại trừ chính nó
            var relatedProducts = db.Products
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != id)
                .OrderByDescending(p => p.ProductID)
                .Take(4)
                .ToList();

            ViewBag.Related = relatedProducts;
            return View(product);
        }
    }
}