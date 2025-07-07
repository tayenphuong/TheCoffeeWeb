using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TheCoffee.Models;

namespace TheCoffee.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        // Danh sách người dùng
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Role).ToList();
            return View(users);
        }

        // Chi tiết
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users
                .Include(u => u.Role)
                .Include(u => u.Orders.Select(o => o.OrderDetails))
                .Include(u => u.Orders.Select(o => o.Payments))
                .FirstOrDefault(u => u.UserID == id);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }


        // Sửa
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "RoleName", user.RoleID);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserID,UserName,Password,FullName,Phone,Email,RoleID")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "RoleName", user.RoleID);
            return View(user);
        }

        // Xoá
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();
            TempData["Success"] = "Đã xoá người dùng.";
            return RedirectToAction("Index");
        }
    }
}
