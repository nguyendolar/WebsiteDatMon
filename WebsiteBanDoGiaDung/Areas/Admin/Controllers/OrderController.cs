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
    public class OrderController : BaseController
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();
        public ActionResult Index()
        {
            ViewBag.countTrash = db.Orders.Where(m => m.Trash == 1).Count();
            var results = (from od in db.Orderdetails
                           join o in db.Orders on od.OrderID equals o.ID
                           join u in db.Users on o.UserID equals u.ID
                           where o.Trash != 1                         
                           group od by new { od.OrderID, o, u } into groupb
                           orderby groupb.Key.o.CreateDate descending
                           select new ListOrder
                           {
                               ID = groupb.Key.OrderID,
                               SAmount = groupb.Sum(m => m.Amount),
                               CustomerName = groupb.Key.o.DeliveryName,
                               Status = groupb.Key.o.Status,
                               CreateDate = groupb.Key.o.CreateDate,
                               ExportDate = groupb.Key.o.ExportDate,
                               Username = groupb.Key.u.Name,
                               UserId = groupb.Key.o.UserID
                           });
            Session["orders"] = results.ToList();
            return View(results.ToList());
        }
        public ActionResult Trash()
        {
            ViewBag.countTrash = db.Orders.Where(m => m.Status == 0).Count();
            var results = (from od in db.Orderdetails
                           join o in db.Orders on od.OrderID equals o.ID
                           where o.Trash == 1

                           group od by new { od.OrderID, o } into groupb
                           orderby groupb.Key.o.CreateDate descending
                           select new ListOrder
                           {
                               ID = groupb.Key.OrderID,
                               SAmount = groupb.Sum(m => m.Amount),
                               CustomerName = groupb.Key.o.DeliveryName,
                               Status = groupb.Key.o.Status,
                               CreateDate = groupb.Key.o.CreateDate,
                               ExportDate = groupb.Key.o.ExportDate,
                           });

            return View(results.ToList());
        }

        public ActionResult DelTrash(int? id)
        {
            MOrder mOrder = db.Orders.Find(id);
            mOrder.Trash = 1;

            mOrder.Updated_at = DateTime.Now;
            mOrder.Updated_by = 1;
            db.Entry(mOrder).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Đã hủy đơn hàng!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult Undo(int? id)
        {
            MOrder mOrder = db.Orders.Find(id);
            mOrder.Trash = 0;

            mOrder.Updated_at = DateTime.Now;
            mOrder.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mOrder).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại đơn hàng!", "warning");
                return RedirectToAction("Index", "Order");
            }
            MOrder mOrder = db.Orders.Find(id);
            if (mOrder == null)
            {
                Notification.set_flash("Không tồn tại  đơn hàng!", "warning");
                return RedirectToAction("Index", "Order");
            }
            ViewBag.orderDetails = db.Orderdetails.Where(m => m.OrderID == id).ToList();
            ViewBag.productOrder = db.Products.ToList();
            return View(mOrder);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại đơn hàng!", "warning");
                return RedirectToAction("Trash", "Order");
            }
            MOrder mOrder = db.Orders.Find(id);
            if (mOrder == null)
            {
                Notification.set_flash("Không tồn tại đơn hàng!", "warning");
                return RedirectToAction("Trash", "Order");
            }
            ViewBag.orderDetails = db.Orderdetails.Where(m => m.OrderID == id).ToList();
            ViewBag.productOrder = db.Products.ToList();
            return View(mOrder);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MOrder mOrder = db.Orders.Find(id);
            db.Orders.Remove(mOrder);
            db.SaveChanges();
            Notification.set_flash("Đã xóa đơn hàng!", "success");
            return RedirectToAction("Trash");
        }

        [HttpPost]
        public ActionResult changeStatus(int id, int op)
        {
            var orders = Session["orders"];
            var result = (IList<ListOrder>)orders;
            MOrder mOrder = db.Orders.Find(id);
            var isCheck = false;
            if (op == 1) {
                mOrder.Status = 1;
            } 
            else if (op == 2) {
                mOrder.Status = 2;
            }
            else {
                mOrder.Status = 3;
                isCheck = true;
            }
            mOrder.ExportDate = DateTime.Now;
            mOrder.Updated_at = DateTime.Now;
            mOrder.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(mOrder).State = EntityState.Modified;
            if(db.SaveChanges() > 0)
            {
                if(isCheck == true)
                {
                    var totalPay = result.Where(x=>x.ID == id).FirstOrDefault().SAmount;
                    var user = db.Users.Where(x => x.ID == mOrder.UserID).FirstOrDefault();
                    user.PayTotal = user.PayTotal + Convert.ToInt32(totalPay);
                    db.Entry(user).State = EntityState.Modified;
                    if(db.SaveChanges() > 0)
                    {
                        UpdateQuantity(id);
                        UpdateGroupUser(user.PayTotal, user.ID);
                    }
                }
            }
            return Json(new { s = mOrder.Status, t = mOrder.ExportDate.ToString() });
        }

        public void UpdateGroupUser(int pay, int userId)
        {
            var list = db.GroupUsers.OrderByDescending(x=>x.Level).ToList();
            foreach(var item in list)
            {
                if(pay > item.Level)
                {
                    var user = db.Users.Find(userId);
                    user.GroupId = item.ID;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    break;
                }
            }
        }
        public void UpdateQuantity(int orderId)
        {
            var listId = db.Orderdetails.Where(x => x.OrderID == orderId).ToList();
            foreach(var item in listId)
            {
                var product = db.Products.Find(item.ProductID);
                product.Quantity = product.Quantity - item.Quantity;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}
