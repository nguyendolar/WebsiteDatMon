using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanDoGiaDung.Models;

namespace WebsiteBanDoGiaDung.Areas.Admin.Controllers
{
    public class CategoryController : BaseController
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();

        // GET: Admin/Category
        public ActionResult Index()
        {
            ViewBag.count_trash = db.Categorys.Where(m => m.Status == 0).Count();
            var list = db.Categorys.Where(m => m.Status != 0).ToList();
            ViewBag.GetAllCategory = list;
            foreach (var row in list)
            {
                var temp_link = db.Links.Where(m => m.Type == "category" && m.TableId == row.ID);
                if (temp_link.Count() > 0)
                {
                    var row_link = temp_link.First();
                    row_link.Name = row.Name;
                    row_link.Slug = row.Slug;
                    db.Entry(row_link).State = EntityState.Modified;
                }
                else
                {
                    var row_link = new MLink();
                    row_link.Name = row.Name;
                    row_link.Slug = row.Slug;
                    row_link.Type = "category";
                    row_link.TableId = row.ID;
                    db.Links.Add(row_link);
                }
            }
            db.SaveChanges();
            return View(list);
        }

        // GET: Admin/Category/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            MCategory MCategory = db.Categorys.Find(id);
            if (MCategory == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            return View(MCategory);
        }

        // GET: Admin/Category/Create
        public ActionResult Create()
        {
            ViewBag.listCat = new SelectList(db.Categorys.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Categorys.Where(m => m.Status == 1), "Orders", "Name", 0);

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MCategory MCategory)
        {
            ViewBag.listCat = new SelectList(db.Categorys.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Categorys.Where(m => m.Status == 1), "Orders", "Name", 0);
            if (ModelState.IsValid)
            {
                if (MCategory.ParentID == null)
                {
                    MCategory.ParentID = 0;
                }
                String Slug = XString.ToAscii(MCategory.Name);
                CheckSlug check = new CheckSlug();

                if (!check.KiemTraSlug("Category", Slug, null))
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Category");
                }

                MCategory.Slug = Slug;
                MCategory.Created_at = DateTime.Now;
                MCategory.Created_by = int.Parse(Session["Admin_ID"].ToString());
                MCategory.Updated_at = DateTime.Now;
                MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.Categorys.Add(MCategory);
                db.SaveChanges();
                Notification.set_flash("Danh mục đã được thêm!", "success");
                return RedirectToAction("Index", "Category");
            }

            Notification.set_flash("Có lỗi xảy ra khi thêm danh mục!", "warning");
            return View(MCategory);
        }
        public ActionResult Edit(int? id)
        {
            ViewBag.listCat = new SelectList(db.Categorys.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Categorys.Where(m => m.Status == 1), "Orders", "Name", 0);
            MCategory MCategory = db.Categorys.Find(id);
            if (MCategory == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "Category");
            }
            return View(MCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MCategory MCategory)
        {
            ViewBag.listCat = new SelectList(db.Categorys.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Categorys.Where(m => m.Status == 1), "Orders", "Name", 0);
            if (ModelState.IsValid)
            {
                String Slug = XString.ToAscii(MCategory.Name);
                int ID = MCategory.ID;
                if (db.Categorys.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }
                if (db.Topics.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong TOPIC, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }
                if (db.Posts.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong POST, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }
                if (db.Products.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong PRODUCT, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }

                MCategory.Slug = Slug;

                MCategory.Updated_at = DateTime.Now;
                MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.Entry(MCategory).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Cập nhật thành công!", "success");
                return RedirectToAction("Index");
            }
            return View(MCategory);
        }
        public ActionResult DelTrash(int id)
        {
            MCategory MCategory = db.Categorys.Find(id);
            if (MCategory == null)
            {
                Notification.set_flash("Không tồn tại danh mục cần xóa vĩnh viễn!", "warning");
                return RedirectToAction("Index");
            }
            int count_child = db.Categorys.Where(m => m.ParentID == id).Count();
            if (count_child != 0)
            {
                Notification.set_flash("Không thể xóa, danh mục có chứa danh mục con!", "warning");
                return RedirectToAction("Index");
            }
            MCategory.Status = 0;

            MCategory.Created_at = DateTime.Now;
            MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            MCategory.Updated_at = DateTime.Now;
            MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MCategory).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult ReTrash(int? id)
        {
            MCategory MCategory = db.Categorys.Find(id);
            if (MCategory == null)
            {
                Notification.set_flash("Không tồn tại danh mục!", "warning");
                return RedirectToAction("Trash", "Category");
            }
            MCategory.Status = 2;

            MCategory.Updated_at = DateTime.Now;
            MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MCategory).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "Category");
        }
        //trash
        public ActionResult Trash()
        {
            var list = db.Categorys.Where(m => m.Status == 0).ToList();

            return View(list);
        }
        //doi trang thai
        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MCategory MCategory = db.Categorys.Find(id);
            MCategory.Status = (MCategory.Status == 1) ? 2 : 1;

            MCategory.Updated_at = DateTime.Now;
            MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MCategory).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new
            {
                Status = MCategory.Status
            });
        }
        // GET: Admin/Category/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại danh mục cần xóa!", "warning");
                return RedirectToAction("Trash", "Category");
            }
            MCategory MCategory = db.Categorys.Find(id);
            if (MCategory == null)
            {
                Notification.set_flash("Không tồn tại danh mục cần xóa!", "warning");
                return RedirectToAction("Trash", "Category");
            }
            return View(MCategory);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MCategory MCategory = db.Categorys.Find(id);
            db.Categorys.Remove(MCategory);
            db.SaveChanges();
            Notification.set_flash("Đã xóa hoàn toàn danh mục!", "success");
            return RedirectToAction("Trash", "Category");
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
