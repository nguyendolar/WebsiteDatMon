using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanDoGiaDung.Models;

namespace WebsiteBanDoGiaDung.Areas.Admin.Controllers
{
    public class GroupUserController : BaseController
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();

        // GET: Admin/GroupUser
        public ActionResult Index()
        {
            var list = db.GroupUsers.Where(m => m.Status != 0).ToList();            
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
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            if (MGroupUser == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            return View(MGroupUser);
        }

        // GET: Admin/GroupUser/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MGroupUser MGroupUser)
        {
            if (ModelState.IsValid)
            {
                MGroupUser.Status = 1;
                MGroupUser.Created_at = DateTime.Now;
                MGroupUser.Created_by = int.Parse(Session["Admin_ID"].ToString());
                MGroupUser.Updated_at = DateTime.Now;
                MGroupUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.GroupUsers.Add(MGroupUser);
                db.SaveChanges();
                Notification.set_flash("Nhóm người dùng đã được thêm!", "success");
                return RedirectToAction("Index", "GroupUser");
            }

            Notification.set_flash("Có lỗi xảy ra khi thêm nhóm người dùng!", "warning");
            return View(MGroupUser);
        }

        public ActionResult Edit(int? id)
        {
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            if (MGroupUser == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "GroupUser");
            }
            return View(MGroupUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MGroupUser MGroupUser)
        {
            if (ModelState.IsValid)
            {
                MGroupUser.Updated_at = DateTime.Now;
                MGroupUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                db.Entry(MGroupUser).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Cập nhật thành công!", "success");
                return RedirectToAction("Index");
            }
            return View(MGroupUser);
        }

        public ActionResult DelTrash(int id)
        {
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            MGroupUser.Status = 0;

            MGroupUser.Created_at = DateTime.Now;
            MGroupUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            MGroupUser.Updated_at = DateTime.Now;
            MGroupUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MGroupUser).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult ReTrash(int? id)
        {
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            if (MGroupUser == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Trash", "GroupUser");
            }
            MGroupUser.Status = 2;

            MGroupUser.Updated_at = DateTime.Now;
            MGroupUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MGroupUser).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "GroupUser");
        }

        //trash
        public ActionResult Trash()
        {
            var list = db.GroupUsers.Where(m => m.Status == 0).ToList();

            return View(list);
        }

        //doi trang thai
        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            MGroupUser.Status = (MGroupUser.Status == 1) ? 2 : 1;

            MGroupUser.Updated_at = DateTime.Now;
            MGroupUser.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MGroupUser).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new
            {
                Status = MGroupUser.Status
            });
        }

        // GET: Admin/GroupUser/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại nhóm người dùng cần xóa!", "warning");
                return RedirectToAction("Trash", "GroupUser");
            }
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            if (MGroupUser == null)
            {
                Notification.set_flash("Không tồn tại nhóm người dùng cần xóa!", "warning");
                return RedirectToAction("Trash", "GroupUser");
            }
            return View(MGroupUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MGroupUser MGroupUser = db.GroupUsers.Find(id);
            db.GroupUsers.Remove(MGroupUser);
            db.SaveChanges();
            Notification.set_flash("Đã xóa hoàn toàn nhóm người dùng!", "success");
            return RedirectToAction("Trash", "GroupUser");
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