using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;
using Newtonsoft.Json;


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

       

        [HttpGet]
        public ActionResult Create()
        {
            var vm = new OrderCreateVM
            {
                OrderTypes = db.OrderTypes.Select(o => new SelectListItem
                {
                    Value = o.OrderTypeID.ToString(),
                    Text = o.OrderTypeName
                }).ToList(),

                AvailableProducts = db.Products.ToList(),
                Categories = db.Categories.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(OrderCreateVM model)
        {
            System.Diagnostics.Debug.WriteLine("OrderTypeID: " + model.OrderTypeID);
            System.Diagnostics.Debug.WriteLine("ItemsJson: " + model.ItemsJson);
            if (model.OrderTypeID <= 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn loại đơn.");
                model.OrderTypes = db.OrderTypes.Select(o => new SelectListItem
                {
                    Value = o.OrderTypeID.ToString(),
                    Text = o.OrderTypeName
                }).ToList();
                model.AvailableProducts = db.Products.ToList();
                model.Categories = db.Categories.ToList();
                return View(model);
            }
            if (string.IsNullOrEmpty(model.ItemsJson))
            {
                ModelState.AddModelError("", "Chưa chọn món nào.");
                model.OrderTypes = db.OrderTypes.Select(o => new SelectListItem
                {
                    Value = o.OrderTypeID.ToString(),
                    Text = o.OrderTypeName
                }).ToList();
                model.AvailableProducts = db.Products.ToList();
                model.Categories = db.Categories.ToList();
                return View(model);
            }

            

            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderItemVM>>(model.ItemsJson);

            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", "Không có sản phẩm nào được chọn.");
                model.OrderTypes = db.OrderTypes.Select(o => new SelectListItem
                {
                    Value = o.OrderTypeID.ToString(),
                    Text = o.OrderTypeName
                }).ToList();
                model.AvailableProducts = db.Products.ToList();
                model.Categories = db.Categories.ToList();
                return View(model);
            }

            var order = new Order
            {
                OrderTypeID = model.OrderTypeID,
                Address = model.Address,
                OrderNote = model.Note,
                OrderStatus = 1,
                UserID = model.UserID ?? 1,
                CreateAt = DateTime.Now
            };

            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in items)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    OrderQuantity = item.Quantity
                });
            }

            db.SaveChanges();
            TempData["Success"] = "Tạo đơn thành công!";
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
