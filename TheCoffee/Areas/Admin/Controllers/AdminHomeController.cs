using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TheCoffee.Areas.Admin.Controllers
{
    public class AdminHomeController : Controller
    {
        // GET: Admin/AdminHome
        public ActionResult AdminHome()
        {
            //if (Session["UserID"] == null)
            //    return RedirectToAction("Login", "Account", new { area = "" });
            return View();
        }
    }
}