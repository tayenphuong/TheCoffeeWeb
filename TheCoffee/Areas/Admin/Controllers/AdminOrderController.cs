using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using System.Drawing;
using TheCoffee.Models.Enums;


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
                                  .Include(o => o.CancelOrders)
                                  .OrderByDescending(o => o.OrderID)
                                  .ToList();
            return View(orders);
        }

        // Chi tiết đơn hàng
        public ActionResult Details(int id)
        {

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
        public ActionResult Cancel(int? id)
        {
            
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cần hủy.";
                return RedirectToAction("Index");
            }

            var order = db.Orders.Find(id.Value);
            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            if (order.OrderStatus != 1 && order.OrderStatus != 2)
            {
                TempData["Error"] = "Chỉ được hủy đơn đang chờ hoặc đang giao.";
                return RedirectToAction("Index");
            }

            return View(id.Value); // truyền int
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int OrderID, string Reason)
        {
            var order = db.Orders.Find(OrderID);

            if (order != null && (order.OrderStatus == 1 || order.OrderStatus == 2))
            {
                var userIdObj = Session["UserID"];
                if (userIdObj == null)
                {
                    TempData["Error"] = "Chưa đăng nhập. Không thể hủy đơn.";
                    return RedirectToAction("Index");
                }

                int userId = (int)userIdObj;

                order.OrderStatus = 4; // Đã hủy

                var cancel = new CancelOrder
                {
                    OrderID = OrderID,
                    Reason = Reason,
                    CancelDate = DateTime.Now,
                    CanceledBy = userId
                };
                db.CancelOrders.Add(cancel);

                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Đã hủy đơn hàng thành công.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Không thể hủy đơn hàng.";
            return RedirectToAction("Index");
        }
        public ActionResult Complete(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return HttpNotFound();

            if (order.OrderStatus == 2 || order.OrderStatus == 3)
            {
                order.OrderStatus = 5; // 5 = Đã hoàn thành
                //order.CompletedAt = DateTime.Now;
                db.SaveChanges();
            }

            return RedirectToAction("NewIndex");
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
            if (!ModelState.IsValid)
            {
               
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
                UserID = model.UserID,
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
                    OrderQuantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            db.SaveChanges();
            TempData["Success"] = "Tạo đơn thành công!";
            return RedirectToAction("NewIndex");
        }

        public ActionResult NewIndex()
        {
            var orders = db.Orders.ToList();

            var vm = new OrderListVM
            {
                Moi = orders.Where(o => o.OrderStatus == (int)OrderStatus.Moi).ToList(),
                DangXuLy = orders.Where(o => o.OrderStatus == (int)OrderStatus.DangXuLy).ToList(),
                DaGiao = orders.Where(o => o.OrderStatus == (int)OrderStatus.DaGiao).ToList(),
                DaHuy = orders.Where(o => o.OrderStatus == (int)OrderStatus.DaHuy).ToList(),
                DaHoanThanh = orders.Where(o => o.OrderStatus == (int)OrderStatus.DaHoanThanh).ToList()
            };

            return View(vm);
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
