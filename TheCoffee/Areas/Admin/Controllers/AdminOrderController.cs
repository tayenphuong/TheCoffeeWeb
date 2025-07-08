using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TheCoffee.Models;

namespace TheCoffee.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        // Danh sách đơn hàng
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.User)
                                  .Include(o => o.OrderType)
                                  .OrderByDescending(o => o.OrderID)
                                  .ToList();
            return View(orders);
        }

        // Chi tiết đơn hàng
        public ActionResult Details(int id)
        {
            //var order = db.Orders.Include(o => o.User)
            //                      .Include(o => o.OrderDetails.Select(od => od.Product))
            //                      .Include(o => o.Payments)
            //                      .FirstOrDefault(o => o.OrderID == id);
            var order = db.Orders
              .Include(o => o.OrderType) 
              .Include(o => o.User)
              .Include(o => o.OrderDetails.Select(od => od.Product))
              .Include(o => o.Payments.Select(p => p.PaymentMethod)) // cũng nên include
              .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // Xác nhận đơn hàng
        public ActionResult Confirm(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();

            order.OrderStatus = 2; // Đã xác nhận
            db.SaveChanges();

            TempData["Success"] = "Đã xác nhận đơn hàng.";
            return RedirectToAction("Index");
        }

        // Hủy đơn hàng
        public ActionResult Cancel(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();

            order.OrderStatus = 4; // Đã hủy -> 3: đã hoàn thành 
            db.SaveChanges();

            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
