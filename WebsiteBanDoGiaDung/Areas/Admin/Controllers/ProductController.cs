using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanDoGiaDung.Models;

namespace WebsiteBanDoGiaDung.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();

        // GET: Admin/Product
        public ActionResult Index()
        {
            ViewBag.countTrash = db.Products.Where(m => m.Status == 0).Count();
            ViewBag.branch = db.ProductOwners.ToList();
            var list = from p in db.Products
                       join c in db.Categorys
                       on p.CateID equals c.ID
                       where p.Status != 0
                       where p.CateID == c.ID
                       orderby p.Created_at descending
                       select new ProductCategory()
                       {
                           ProductId = p.ID,
                           ProductImg = p.Image,
                           ProductName = p.Name,
                           ProductStatus = p.Status,
                           ProductDiscount = p.Discount,
                           CategoryName = c.Name,
                           OwnerId = p.OwnerId
                       };
            return View(list.ToList());
        }
        public ActionResult Trash()
        {
            var list = from p in db.Products
                       join c in db.Categorys
                       on p.CateID equals c.ID
                       where p.Status == 0
                       where p.CateID == c.ID
                       orderby p.Created_at descending
                       select new ProductCategory()
                       {
                           ProductId = p.ID,
                           ProductImg = p.Image,
                           ProductName = p.Name,
                           ProductStatus = p.Status,
                           ProductDiscount = p.Discount,
                           CategoryName = c.Name
                       };
            return View(list.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            MProduct mProduct = db.Products.Find(id);
            if (mProduct == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Index");
            }
            return View(mProduct);
        }

        public ActionResult Create()
        {
            MCategory mCategory = new MCategory();
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);
            ViewBag.ListOwner = new SelectList(db.ProductOwners.Where(x=>x.Status!=0).ToList(), "ID", "Name", 0);
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MProduct mProduct)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);
            
                mProduct.Price = mProduct.Price;
                mProduct.ProPrice = mProduct.ProPrice;

                String strSlug = XString.ToAscii(mProduct.Name);
                mProduct.Slug = strSlug;
                mProduct.NewPromotion = "Khuyến mãi";
                mProduct.Specification = "Đặt món";
                mProduct.MetaKey = "Đặt món";
                mProduct.MetaDesc = "Đặt món";
                mProduct.Discount = 2;
                mProduct.Installment = 2;
                mProduct.Created_at = DateTime.Now;
                mProduct.Created_by = 1;
                mProduct.Updated_at = DateTime.Now;
                mProduct.Updated_by = 1;

                // Upload file
                var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    mProduct.Image = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/products/"), filename);
                    file.SaveAs(Strpath);
                }
                
                db.Products.Add(mProduct);
                db.SaveChanges();
                Notification.set_flash("Thêm mới sản phẩm thành công!", "success");
                return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddBranch(FormCollection form)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);

            int idProduct = Int32.Parse(form["idProduct"]);
            int idBranch = Int32.Parse(form["chiNhanh"]);

            var prodcut = db.Products.FirstOrDefault(x => x.ID == idProduct);
            MProduct mProduct = new MProduct();
            if(idBranch == prodcut.OwnerId)
            {
                Notification.set_flash("Sản phẩm này đã tồn tại chi nhánh!", "danger");
                return Redirect("/Admin/Product/Index");
            }
            else
            {
                mProduct.Price = prodcut.Price;
                mProduct.ProPrice = prodcut.ProPrice;

                String strSlug = XString.ToAscii(prodcut.Name);
                mProduct.Slug = strSlug;
                mProduct.Name = prodcut.Name;
                mProduct.CateID = prodcut.CateID;
                mProduct.Description = prodcut.Description;
                mProduct.Detail = prodcut.Detail;
                mProduct.Quantity = prodcut.Quantity;
                mProduct.Status = 1;
                mProduct.NewPromotion = "Khuyến mãi";
                mProduct.Specification = "Đặt món";
                mProduct.MetaKey = "Đặt món";
                mProduct.MetaDesc = "Đặt món";
                mProduct.Discount = 2;
                mProduct.Installment = 2;
                mProduct.Created_at = DateTime.Now;
                mProduct.Created_by = 1;
                mProduct.Updated_at = DateTime.Now;
                mProduct.Updated_by = 1;
                mProduct.Image = prodcut.Image;
                mProduct.OwnerId = idBranch;
                db.Products.Add(mProduct);
                db.SaveChanges();
                Notification.set_flash("Thêm chi nhánh cho sản phẩm thành công!", "success");
                return Redirect("/Admin/Product/Index");
            }
          
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(x=>x.Status!= 0).ToList(), "ID", "Name", 0);
            ViewBag.ListOwner = new SelectList(db.ProductOwners.Where(x => x.Status != 0).ToList(), "ID", "Name", 0);
            MProduct mProduct = db.Products.Find(id);
            if (mProduct == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "Product");
            }
            return View(mProduct);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MProduct mProduct)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(x => x.Status != 0).ToList(), "ID", "Name", 0);
            ViewBag.ListOwner = new SelectList(db.ProductOwners.Where(x => x.Status != 0).ToList(), "ID", "Name", 0);
                String strSlug = XString.ToAscii(mProduct.Name);
                mProduct.Slug = strSlug;
                mProduct.NewPromotion = "Khuyến mãi";
                mProduct.Specification = "Đặt món";
                mProduct.MetaKey = "Đặt món";
                mProduct.MetaDesc = "Đặt món";
                mProduct.Discount = 2;
                mProduct.Installment = 2;
                mProduct.Updated_at = DateTime.Now;
                mProduct.Updated_by = 1;

                // Upload file
                var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    mProduct.Image = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/images/products/"), filename);
                    file.SaveAs(Strpath);
                }
                
                db.Entry(mProduct).State = EntityState.Modified;
                db.SaveChanges();
                Notification.set_flash("Đã cập nhật lại thông tin sản phẩm!", "success");
                return RedirectToAction("Index");
        }

        public ActionResult DelTrash(int? id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Status = 0;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult Undo(int? id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Status = 2;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash");
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash");
            }
            MProduct mProduct = db.Products.Find(id);
            if (mProduct == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash");
            }
            return View(mProduct);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MProduct mProduct = db.Products.Find(id);
            db.Products.Remove(mProduct);
            db.SaveChanges();
            Notification.set_flash("Đã xóa vĩnh viễn sản phẩm!", "danger");
            return RedirectToAction("Trash");
        }

        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Status = (mProduct.Status == 1) ? 2 : 1;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = mProduct.Status });
        }
        [HttpPost]
        public JsonResult changeDiscount(int id)
        {
            MProduct mProduct = db.Products.Find(id);
            mProduct.Discount = (mProduct.Discount == 1) ? 2 : 1;

            mProduct.Updated_at = DateTime.Now;
            mProduct.Updated_by = 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            
            return Json(new { Discount = mProduct.Discount });
        }

        public ActionResult Statistical()
        {
            var list = from o in db.Orders
                       join od in db.Orderdetails on o.ID equals od.OrderID
                       where o.Trash != 1 && o.Status != 0
                       join p in db.Products on od.ProductID equals p.ID
                       group od by new { p.ID, p.Name, p.Image,od.Quantity } into groupb
                       orderby groupb.Key.ID descending
                       select new ProductTop
                       {
                           Id = groupb.Key.ID,
                           Img = groupb.Key.Image,
                           Name = groupb.Key.Name,
                           Count = groupb.Sum(m => m.Quantity)
                       };

            return View(list.ToList());
        }
    }
}
