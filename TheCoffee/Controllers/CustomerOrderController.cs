using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;
using System.Net;

namespace TheCoffee.Controllers
{
    public class CustomerOrderController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        // Danh sách đơn hàng của khách hàng hiện tại
        public ActionResult Index()
        {
            int userId = (int)Session["UserID"];
            //if (userId == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            //}
            var orders = db.Orders
                           .Include(o => o.OrderType)
                           .Include(o => o.OrderDetails.Select(od => od.Product))
                           .Where(o => o.UserID == userId)
                           .OrderByDescending(o => o.OrderID)
                           .ToList();

            var ratings = db.Ratings.ToList(); // lấy tất cả đánh giá

            var viewModel = orders.Select(o => new CustomerOrderVM
            {
                Order = o,
                HasUnrated = o.OrderStatus == 3 &&
                             o.OrderDetails.Any(od => !ratings.Any(r => r.OrderID == o.OrderID && r.ProductID == od.ProductID))
            });

            return View(viewModel);
        }


        // Chi tiết đơn hàng
        public ActionResult Details(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderType)
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .Include(o => o.Payments.Select(p => p.PaymentMethod))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return HttpNotFound();

            int userId = (int)Session["UserID"];
            if (order.UserID != userId)
                return new HttpStatusCodeResult(403);

            var ratings = db.Ratings.Where(r => r.OrderID == id).ToList();

            var viewModel = new CustomerOrderVM
            {
                Order = order,
                Ratings = ratings,
                HasUnrated = order.OrderStatus == 3 &&
                             order.OrderDetails.Any(od => !ratings.Any(r => r.ProductID == od.ProductID))
            };

            return View(viewModel);
        }


        // Gửi đánh giá (POST)
        [HttpPost]
        public ActionResult SubmitRating(int productId, int orderId, int rating, string comment)
        {
            int userId = (int)Session["UserID"];
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId && o.UserID == userId && o.OrderStatus == 3);

            if (order == null)
                return HttpNotFound();

            var existing = db.Ratings.FirstOrDefault(r => r.OrderID == orderId && r.ProductID == productId);
            if (existing != null)
            {
                existing.Rating1 = rating;
                existing.Comment = comment;
                existing.CreatedAt = DateTime.Now;
            }
            else
            {
                db.Ratings.Add(new Rating
                {
                    OrderID = orderId,
                    ProductID = productId,
                    Rating1 = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                });
            }

            db.SaveChanges();
            TempData["Success"] = "Đánh giá thành công!";
            return RedirectToAction("Details", new { id = orderId });
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
