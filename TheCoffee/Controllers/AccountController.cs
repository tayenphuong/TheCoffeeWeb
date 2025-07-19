using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheCoffee.Models;
using TheCoffee.Models.ViewModel;
namespace TheCoffee.Controllers
{
    public class AccountController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        // ĐĂNG KÝ
        public ActionResult Register()
        {
            return View();
        }

        // ĐĂNG KÝ PHƯƠNG THỨC POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var exists = db.Users.Any(u => u.Email == vm.Email);
            if (exists)
            {
                ViewBag.Error = "Email đã được sử dụng.";
                return View(vm);
            }

            var user = new User
            {
                UserName = vm.UserName,
                Password = vm.Password, // bạn có thể mã hóa sau
                FullName = vm.FullName,
                Phone = vm.Phone,
                Email = vm.Email,
                RoleID = 2
            };

            db.Users.Add(user);
            db.SaveChanges();

            ViewBag.RegOk = "Đăng ký thành công!";
            return View("Register");
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(FormCollection userlog)
        {
            string userMail = userlog["userMail"];
            string password = userlog["password"];

            // Lấy user kèm Role từ DB
            var user = db.Users.Include("Role")
                               .SingleOrDefault(u => u.Email == userMail && u.Password == password);

            if (user != null)
            {
                Session["use"] = user;
                Session["role"] = user.Role?.RoleName;
                Session["UserID"] = user.UserID;

                // Phân quyền
                if (user.Role?.RoleName == "Admin")
                {
                    return RedirectToAction("AdminHome", "AdminHome", new { area = "Admin" });
                }
                else
                {
                    return RedirectToAction("HomePage", "Home");
                }
            }
            else
            {
                ViewBag.Fail = "Tài khoản hoặc mật khẩu không chính xác.";
                return View("Login");
            }
        }

        public ActionResult Logout()
        {
            TempData["LogoutMessage"] = "Bạn đã đăng xuất thành công!";
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Profile()
        {
            var user = Session["use"] as User;

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }
        [HttpGet]
        public ActionResult EditProfile()
        {
            var user = Session["use"] as User;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User updatedUser)
        {
            var user = Session["use"] as User;

            if (user == null) return RedirectToAction("Login");

            var dbUser = db.Users.Find(user.UserID);

            if (dbUser != null)
            {
                dbUser.FullName = updatedUser.FullName;
                dbUser.Email = updatedUser.Email;
                dbUser.Phone = updatedUser.Phone;

                db.SaveChanges();

                Session["use"] = dbUser; // Cập nhật lại session
                ViewBag.Success = "Cập nhật thông tin thành công!";
                return View(dbUser);
            }

            ViewBag.Fail = "Có lỗi xảy ra khi cập nhật.";
            return View(updatedUser);
        }
        [HttpGet]
        public ActionResult ChangePassword()
        {
            var user = Session["use"] as User;
            if (user == null) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPass, string newPass, string confirmPass)
        {
            var user = Session["use"] as User;
            if (user == null) return RedirectToAction("Login");

            var dbUser = db.Users.Find(user.UserID);

            if (dbUser.Password != oldPass)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng.";
                return View();
            }

            if (newPass != confirmPass)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp.";
                return View();
            }

            dbUser.Password = newPass;
            db.SaveChanges();
            ViewBag.Success = "Đổi mật khẩu thành công.";
            return View();
        }




    }
}