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
        
        public ActionResult CustomerHome(string search, int? categoryId, string sortOrder, int page = 1)
        {
            int pageSize = 8;
            var query = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                //case "newest":
                //    query = query.OrderByDescending(p => p.CreatedDate);
                //    break;
                default:
                    query = query.OrderBy(p => p.ProductName);
                    break;
            }

            int totalItems = query.Count();
            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new HomeIndexVM
            {
                Products = products,
                Categories = db.Categories.ToList(),
                SearchKeyword = search,
                SelectedCategoryId = categoryId,
                SortOrder = sortOrder,
                Page = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(vm);
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