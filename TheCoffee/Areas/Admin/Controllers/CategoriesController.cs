using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TheCoffee.Models;
using System.Data.Entity.Infrastructure;

namespace TheCoffee.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private TheCoffeeEntities db = new TheCoffeeEntities();

        // GET: Admin/Categories
        public async Task<ActionResult> Index()
        {
            return View(await db.Categories.ToListAsync());
        }

        // GET: Admin/Categories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            bool isCategoryExists = db.Categories.Any(c => c.CategoryName == category.CategoryName);
            //kiểm tra ô dữ liệu bị trống
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục không được để trống.");
                return View(category);
            }
            else
            {
                if (isCategoryExists)
                {
                    // Thông báo lỗi nếu danh mục đã tồn tại
                    ModelState.AddModelError("CategoryName", "Danh mục này đã tồn tại.");
                    return View(category);
                }

                if (ModelState.IsValid)
                {
                    //db.Entry(category).State = EntityState.Modified;
                    //await db.SaveChangesAsync();
                    //return RedirectToAction("Index");
                    db.Categories.Add(category);  // ✅ PHẢI là Add()
                    await db.SaveChangesAsync();  // ❌ Đừng dùng Entry(...).State = Modified
                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            bool isCategoryExists = db.Categories.Any(c => c.CategoryName == category.CategoryName);
            //kiểm tra ô dữ liệu bị trống
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục không được để trống.");
                return View(category);
            }
            else
            {
                if (isCategoryExists)
                {
                    // Thông báo lỗi nếu danh mục đã tồn tại
                    ModelState.AddModelError("CategoryName", "Danh mục này đã tồn tại.");
                    return View(category);
                }

                if (ModelState.IsValid)
                {
                    db.Entry(category).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
           
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        // GET
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }


        // POST
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    Category category = await db.Categories.FindAsync(id);

        //    if (category == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if (category.Products.Any())
        //    {
        //        ModelState.AddModelError("CategoryName", "Danh mục này đang tồn tại sản phẩm");
        //        return View(category);
        //    }

        //    db.Categories.Remove(category); // Xóa cứng
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id); // ✅ Tìm đúng theo PK

            if (category == null)
            {
                return HttpNotFound();
            }

            if (category.Products.Any())
            {
                ModelState.AddModelError("", "Danh mục này đang tồn tại sản phẩm");
                return View(category);
            }

            db.Categories.Remove(category); // ✅ Nếu xóa cứng
                                            // hoặc
                                            // category.TrangThai = false; db.Entry(category).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Không thể cập nhật vì dữ liệu đã thay đổi hoặc không tồn tại.");
                return View(category);
            }

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
