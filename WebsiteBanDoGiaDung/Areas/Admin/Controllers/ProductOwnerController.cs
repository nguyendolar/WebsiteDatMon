using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanDoGiaDung.Library;
using WebsiteBanDoGiaDung.Models;

namespace WebsiteBanDoGiaDung.Areas.Admin.Controllers
{
    public class ProductOwnerController : Controller
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();

        // GET: Admin/GroupUser
        public ActionResult Index()
        {
            var list = db.ProductOwners.Where(x=>x.Status != 0).ToList();
            return View(list);
        }

        // GET: Admin/GroupUser/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            if (MProductOwner == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            return View(MProductOwner);
        }

        // GET: Admin/GroupUser/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MProductOwner MProductOwner)
        {
            if (ModelState.IsValid)
            {
                MProductOwner.Status = 1;
                MProductOwner.Created_at = DateTime.Now;
                MProductOwner.Created_by = int.Parse(Session["Admin_ID"].ToString());
                MProductOwner.Updated_at = DateTime.Now;
                MProductOwner.Updated_by = int.Parse(Session["Admin_ID"].ToString());
                db.ProductOwners.Add(MProductOwner);
                db.SaveChanges();
                Notification.set_flash("Chi nhánh đã được thêm!", "success");
                return RedirectToAction("Index", "ProductOwner");
            }

            Notification.set_flash("Có lỗi xảy ra khi thêm chi nhánh!", "warning");
            return View(MProductOwner);
        }

        public ActionResult Edit(int? id)
        {
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            if (MProductOwner == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "ProductOwner");
            }
            return View(MProductOwner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MProductOwner MProductOwner)
        {
            if (ModelState.IsValid)
            {
                MProductOwner.Updated_at = DateTime.Now;
                MProductOwner.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.Entry(MProductOwner).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Cập nhật thành công!", "success");
                return RedirectToAction("Index");
            }
            return View(MProductOwner);
        }

        public ActionResult DelTrash(int id)
        {
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            MProductOwner.Status = 0;

            MProductOwner.Created_at = DateTime.Now;
            MProductOwner.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            MProductOwner.Updated_at = DateTime.Now;
            MProductOwner.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MProductOwner).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult ReTrash(int? id)
        {
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            if (MProductOwner == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Trash", "ProductOwner");
            }
            MProductOwner.Status = 2;

            MProductOwner.Updated_at = DateTime.Now;
            MProductOwner.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MProductOwner).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "ProductOwner");
        }

        //trash
        public ActionResult Trash()
        {
            var list = db.ProductOwners.Where(m => m.Status == 0).ToList();

            return View(list);
        }

        //doi trang thai
        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            MProductOwner.Status = (MProductOwner.Status == 1) ? 2 : 1;

            MProductOwner.Updated_at = DateTime.Now;
            MProductOwner.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MProductOwner).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new
            {
                Status = MProductOwner.Status
            });
        }

        // GET: Admin/GroupUser/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại chi nhánh cần xóa!", "warning");
                return RedirectToAction("Trash", "ProductOwner");
            }
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            if (MProductOwner == null)
            {
                Notification.set_flash("Không tồn tại chi nhánh cần xóa!", "warning");
                return RedirectToAction("Trash", "ProductOwner");
            }
            return View(MProductOwner);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MProductOwner MProductOwner = db.ProductOwners.Find(id);
            db.ProductOwners.Remove(MProductOwner);
            db.SaveChanges();
            Notification.set_flash("Đã xóa hoàn toàn chi nhánh!", "success");
            return RedirectToAction("Trash", "ProductOwner");
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