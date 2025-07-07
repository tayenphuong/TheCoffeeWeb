using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;

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
        //public ActionResult FeaturedProducts()
        //{
        //    var products = db.Products.ToList(); // Hoặc select vào ProductViewModel
        //    return View(products);
        //}
    }
}