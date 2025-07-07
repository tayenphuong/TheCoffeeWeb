using System.Collections.Generic;
using System.Data.Entity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;
using System.Web.SessionState;


namespace TheCoffee.Controllers
{
    public class CartController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

                if (cart != null)
                {
                    var items = from ci in db.CartItems
                                join p in db.Products on ci.ProductID equals p.ProductID
                                where ci.CartID == cart.CartID
                                select new CartItemViewModel
                                {
                                    ProductID = p.ProductID,
                                    ProductName = p.ProductName,
                                    Img = p.Img,
                                    Price = p.Price,
                                    Quantity = ci.CartQuantity
                                };

                    return View(items.ToList());
                }
            }

            // Nếu chưa login, đọc từ Session
            var sessionCart = CartSessionHelper.GetCart();
            return View(sessionCart);
        }


        public JsonResult GetCartCount()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

                if (cart != null)
                {
                    var count = db.CartItems
                                 .Where(ci => ci.CartID == cart.CartID)
                                 .Sum(ci => (int?)ci.CartQuantity) ?? 0;
                    return Json(new { count }, JsonRequestBehavior.AllowGet);
                }
            }

            // Trường hợp chưa đăng nhập
            var sessionCart = CartSessionHelper.GetCart();
            //var sessionCart = Session["Cart"] as List<CartItemViewModel> ?? new List<CartItemViewModel>();
            int sessionCount = sessionCart.Sum(i => i.Quantity);
            return Json(new { count = sessionCount }, JsonRequestBehavior.AllowGet);
        }




        // Thêm vào giỏ hàng
        [HttpPost]
        public JsonResult Add(int productId)
        {
            // Nếu chưa login
            if (Session["UserID"] == null)
            {
                var cart = CartSessionHelper.GetCart();

                var existing = cart.FirstOrDefault(c => c.ProductID == productId);
                if (existing != null)
                {
                    existing.Quantity++;
                }
                else
                {
                    var product = db.Products.Find(productId); // ✅ lấy thông tin từ DB
                    if (product != null)
                    {
                        cart.Add(new CartItemViewModel
                        {
                            ProductID = product.ProductID,
                            ProductName = product.ProductName,
                            Price = product.Price,
                            Img = product.Img,
                            Quantity = 1
                        });
                    }
                }

                CartSessionHelper.SaveCart(cart);
            }
            else
            {
                // login → lưu vào DB
                int userId = Convert.ToInt32(Session["UserID"]);
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);
                if (cart == null)
                {
                    cart = new Cart { UserID = userId };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                var existingItem = db.CartItems.FirstOrDefault(ci => ci.CartID == cart.CartID && ci.ProductID == productId);
                if (existingItem != null)
                    existingItem.CartQuantity++;
                else
                    db.CartItems.Add(new CartItem { CartID = cart.CartID, ProductID = productId, CartQuantity = 1 });

                db.SaveChanges();
            }

            return Json(new { success = true }); // ✅ trả về JSON
        }


        // Cập nhật số lượng
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);
                if (cart != null)
                {
                    var item = db.CartItems.FirstOrDefault(ci => ci.CartID == cart.CartID && ci.ProductID == productId);
                    if (item != null && quantity > 0)
                    {
                        item.CartQuantity = quantity;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                var cart = CartSessionHelper.GetCart();
                var item = cart.FirstOrDefault(p => p.ProductID == productId);
                if (item != null && quantity > 0)
                {
                    item.Quantity = quantity;
                    CartSessionHelper.SaveCart(cart); // ✅ nhớ lưu lại
                }
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
 
        public JsonResult UpdateQuantityAjax(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Số lượng không hợp lệ" });
            }

            decimal subtotal = 0;
            decimal total = 0;

            if (Session["UserID"] == null)
            {
                var cart = CartSessionHelper.GetCart();
                var item = cart.FirstOrDefault(p => p.ProductID == productId);
                if (item == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ" });

                item.Quantity = quantity;
                CartSessionHelper.SaveCart(cart);

                subtotal = item.Price * item.Quantity;
                total = cart.Sum(x => x.Price * x.Quantity);
            }
            else
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);
                if (cart == null)
                    return Json(new { success = false, message = "Không tìm thấy giỏ hàng" });

                var item = db.CartItems.Include(ci => ci.Product)
                                       .FirstOrDefault(ci => ci.CartID == cart.CartID && ci.ProductID == productId);
                if (item == null || item.Product == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm trong DB" });

                item.CartQuantity = quantity;
                db.SaveChanges();

                subtotal = item.Product.Price * item.CartQuantity;

                total = db.CartItems.Include(ci => ci.Product)
                                    .Where(ci => ci.CartID == cart.CartID)
                                    .ToList()
                                    .Sum(ci => ci.Product.Price * ci.CartQuantity);
            }

            return Json(new
            {
                success = true,
                subtotal = subtotal,
                total = total
            });
        }


        // Xóa một sản phẩm
        public ActionResult Remove(int productId)
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);
                if (cart != null)
                {
                    var item = db.CartItems.FirstOrDefault(ci => ci.CartID == cart.CartID && ci.ProductID == productId);
                    if (item != null)
                    {
                        db.CartItems.Remove(item);
                        db.SaveChanges();
                    }
                }
            }
            else
            {
               
                var cart = CartSessionHelper.GetCart();
                var item = cart.FirstOrDefault(p => p.ProductID == productId);
                if (item != null)
                {
                    cart.Remove(item);
                  
                    CartSessionHelper.SaveCart(cart);
                }
            }

            return RedirectToAction("Index");
        }


        // Xóa toàn bộ giỏ hàng
        public ActionResult Clear()
        {
            CartSessionHelper.ClearCart();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserID"]);
            var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

            if (cart == null)
            {
                TempData["Error"] = "Không tìm thấy giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = db.CartItems
                              .Where(ci => ci.CartID == cart.CartID)
                              .Include(ci => ci.Product)
                              .Select(ci => new CartItemViewModel
                              {
                                  ProductID = ci.ProductID,
                                  ProductName = ci.Product.ProductName,
                                  Img = ci.Product.Img,
                                  Price = ci.Product.Price,
                                  Quantity = ci.CartQuantity
                              }).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Address = "",
                OrderNote = "",
                CartItems = cartItems,
                Total = cartItems.Sum(x => x.Price * x.Quantity)
            };

            ViewBag.PaymentMethods = db.PaymentMethods.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserID"]);
            var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

            if (cart == null)
            {
                TempData["Error"] = "Không tìm thấy giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = db.CartItems
                              .Where(ci => ci.CartID == cart.CartID)
                              .Include(ci => ci.Product)
                              .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError("Address", "Vui lòng nhập địa chỉ giao hàng.");
            }

            if (!ModelState.IsValid)
            {
                // Nạp lại dữ liệu giỏ hàng khi form lỗi
                model.CartItems = cartItems.Select(ci => new CartItemViewModel
                {
                    ProductID = ci.ProductID,
                    ProductName = ci.Product.ProductName,
                    Img = ci.Product.Img,
                    Price = ci.Product.Price,
                    Quantity = ci.CartQuantity
                }).ToList();

                model.Total = model.CartItems.Sum(x => x.Price * x.Quantity);
                ViewBag.PaymentMethods = db.PaymentMethods.ToList();
                return View(model);
            }

            // Tạo Order
            var order = new Order
            {
                Address = model.Address,
                OrderNote = model.OrderNote,
                OrderStatus = 1,          // "Chờ xác nhận"
                OrderTypeID = 1,          // "Online"
                UserID = userId,
                CartID = cart.CartID
            };
            db.Orders.Add(order);
            db.SaveChanges();

            // Order Details
            foreach (var item in cartItems)
            {
                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    OrderQuantity = item.CartQuantity
                });
            }

            // Payment
            db.Payments.Add(new Payment
            {
                OrderID = order.OrderID,
                MethodID = 1,
                PaidAmount = cartItems.Sum(ci => ci.Product.Price * ci.CartQuantity),
                PaidAt = DateTime.Now,
                PaymentStatus = "Chờ xác nhận"
            });

            db.SaveChanges();

            // Xóa giỏ hàng
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công!";
            return View("Checkout", new CheckoutViewModel());
        }



    }
}
