using System;
using System.Linq;
using System.Web.Mvc;
using TheCoffee.Models;
using Newtonsoft.Json;
using System.Data.Entity;

namespace TheCoffee.Areas.Admin.Controllers
{
    public class AdminHomeController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        public ActionResult AdminHome(DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.Now;

            // Nếu không truyền gì thì mặc định 7 ngày gần nhất
            if (!fromDate.HasValue)
                fromDate = now.AddDays(-6).Date; // 6 ngày trước + hôm nay = 7 ngày

            if (!toDate.HasValue)
                toDate = now.Date;

            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");

            // 1. THỐNG KÊ NHANH
            ViewBag.ChoDuyet = db.Orders.Count(o => o.OrderStatus == 1);
            ViewBag.DaDuyet = db.Orders.Count(o => o.OrderStatus == 2 || o.OrderStatus == 3);
            ViewBag.BiHuy = db.Orders.Count(o => o.OrderStatus == 4);
            ViewBag.AdminCount = db.Users.Count(u => u.RoleID == 1); // giả định RoleID = 1 là admin

            // 2. BIỂU ĐỒ SỐ ĐƠN THEO NGÀY
            var postQuery = db.Orders
                .Where(o => o.CreateAt >= fromDate && o.CreateAt <= toDate)
                .AsEnumerable() // Hoặc .ToList()
                .GroupBy(o => o.CreateAt.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd/MM"),
                    Count = g.Count()
                })
                .ToList();

            ViewBag.Dates = JsonConvert.SerializeObject(postQuery.Select(x => x.Date));
            ViewBag.PostsByDate = JsonConvert.SerializeObject(postQuery.Select(x => x.Count));

            //// 3. SẢN PHẨM BÁN CHẠY
            var topProducts = db.OrderDetails
                 .Include(od => od.Order)
                 .Where(od => od.Order.CreateAt != null && od.Order.CreateAt >= fromDate && od.Order.CreateAt <= toDate)
                 .GroupBy(od => od.Product.ProductName)
                 .Select(g => new
                 {
                     ProductName = g.Key,
                     QuantitySold = g.Sum(x => x.OrderQuantity)
                 })
                 .OrderByDescending(x => x.QuantitySold)
                 .Take(5)
                 .ToList();


            ViewBag.TopProductNames = JsonConvert.SerializeObject(topProducts.Select(x => x.ProductName));
                  ViewBag.TopProductQuantities = JsonConvert.SerializeObject(topProducts.Select(x => x.QuantitySold));
            // BIỂU ĐỒ DOANH THU THEO NGÀY
            var revenueData = db.OrderDetails
               .Include(od => od.Order)
               .Include(od => od.Product)
               .Where(od => od.Order.CreateAt != null && od.Order.CreateAt >= fromDate && od.Order.CreateAt <= toDate)
               .ToList()
               .GroupBy(od => od.Order.CreateAt.Value.Date)
               .Select(g => new
               {
                   Date = g.Key.ToString("dd/MM"),
                   Revenue = g.Sum(x => x.OrderQuantity * x.Product.Price)
               })
               .ToList();


            ViewBag.RevenueDates = JsonConvert.SerializeObject(revenueData.Select(x => x.Date));
            ViewBag.RevenueAmounts = JsonConvert.SerializeObject(revenueData.Select(x => x.Revenue));

            // THỐNG KÊ SẢN PHẨM BÁN RA (TẤT CẢ)
            var productStats = db.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.CreateAt != null && od.Order.CreateAt >= fromDate && od.Order.CreateAt <= toDate)
                .GroupBy(od => od.Product.ProductName)
                .Select(g => new
                {
                    Name = g.Key,
                    TotalSold = g.Sum(x => x.OrderQuantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToList();


            ViewBag.AllProductNames = JsonConvert.SerializeObject(productStats.Select(x => x.Name));
            ViewBag.AllProductSold = JsonConvert.SerializeObject(productStats.Select(x => x.TotalSold));

            return View();
        }
    }
}
