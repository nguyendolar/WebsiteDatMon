using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanDoGiaDung.Library;
using WebsiteBanDoGiaDung.Models;

namespace WebsiteBanDoGiaDung.Controllers
{
    public class CartController : Controller
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();
        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add(int pid, int qty)
        {
            var p = db.Products.Where(m => m.Status == 1 && m.ID == pid).First();

            var cart = Session["Cart"];
            if (cart == null)
            {
                var item = new ModelCart();
                item.ProductID = p.ID;
                item.Name = p.Name;
                item.Slug = p.Slug;
                item.Image = p.Image;
                item.Quantity = qty;
                item.Price = p.Price;
                var list = new List<ModelCart>();
                list.Add(item);

                Session["Cart"] = list;
                return Json(new { result = 1 });
            }
            else
            {
                var list = (List<ModelCart>)cart;

                if (list.Exists(m => m.ProductID == pid))
                {
                    foreach (var item in list)
                    {
                        if (item.ProductID == pid)
                            item.Quantity += qty;
                        return Json(new { result = 2 });
                    }
                }
                else
                {
                    var item = new ModelCart();
                    item.ProductID = p.ID;
                    item.Name = p.Name;
                    item.Slug = p.Slug;
                    item.Image = p.Image;
                    item.Quantity = qty;
                    item.Price = p.Price;
                    list.Add(item);
                    return Json(new { result = 1 });
                }
            }
            return Json(new { result = 0 });
        }

        public JsonResult Update(int pid, String option)
        {
            var sCart = (List<ModelCart>)Session["Cart"];
            ModelCart c = sCart.First(m => m.ProductID == pid);
            var product = db.Products.Find(pid);
            if (c != null && product != null)
            {
                switch (option)
                {
                    case "add":
                        c.Quantity++;
                        if(c.Quantity > product.Quantity)
                        {
                            c.Quantity = product.Quantity;
                            return Json(10);
                        }
                        return Json(1);
                    case "minus":
                        c.Quantity--;
                        return Json(2);
                    case "remove":
                        sCart.Remove(c);
                        if (sCart.Count() == 0)
                            Session.Remove("Cart");
                        return Json(3);
                    default:
                        break;
                }
            }
            return Json(null);
        }
        public ActionResult RemoveAll()
        {
            Session.Remove("Cart");
            Notification.set_flash("Đã xóa toàn bộ sản phẩm trong giỏ hàng!", "success");
            return Redirect("~/gio-hang");
        }
        public ActionResult Checkout()
        {
            if (Session["User_Name"] != null && Session["Cart"] != null)
            {
                int user_id = Convert.ToInt32(Session["User_ID"]);
                ViewBag.Info = db.Users.Where(m => m.ID == user_id).First();
            }
            else
                return RedirectToAction("Index", "Cart");
            return View();
        }

        [HttpPost]
        public JsonResult Payment(String Address, String FullName, String Phone, String Email)
        {
            var order = new MOrder();
            int user_id = Convert.ToInt32(Session["User_ID"]);
            order.Code = DateTime.Now.ToString("yyyyMMddhhMMss"); // yyyy-MM-dd hh:MM:ss
            order.UserID = user_id;
            order.CreateDate = DateTime.Now;
            order.DeliveryAddress = Address;
            order.DeliveryEmail = Email;
            order.DeliveryPhone = Phone;
            order.DeliveryName = FullName;
            order.Status = 1;
            db.Orders.Add(order);
            db.SaveChanges();

            var OrderID = order.ID;

            foreach (var c in (List<ModelCart>)Session["Cart"])
            {
                var orderdetails = new MOrderDetail();
                orderdetails.OrderID = OrderID;
                orderdetails.ProductID = c.ProductID;
                orderdetails.Price = c.Price;
                orderdetails.Quantity = c.Quantity;
                orderdetails.Amount = c.Price * c.Quantity;
                db.Orderdetails.Add(orderdetails);
            }
            db.SaveChanges();

            Session.Remove("Cart");
            Notification.set_flash("Bạn đã đặt hàng thành công!", "success");
            return Json(true);

        }

        public JsonResult Tesst()
        {
            if (Session["User_Name"] != null)
                return Json(1);
            return Json(0);
        }
        public JsonResult CheckAuth()
        {
            if (Session["User_Name"] != null)
                return Json(1);
            return Json(0);
        }
    }
}