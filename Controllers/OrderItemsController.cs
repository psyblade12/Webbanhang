using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web.Http;

namespace Webbanhang.Controllers
{
    public class OrderItemsController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage LoadAllOrderItems()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var a = Request.CreateResponse(HttpStatusCode.OK, entities.OrderItems.ToList());
                    return a;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Products/LoadAllOrderItemsInTime")]
        public HttpResponseMessage LoadAllOrderItemsInTime()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var a = Request.CreateResponse(HttpStatusCode.OK, entities.OrderItems.ToList());
                    return a;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage LoadOrderItemByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.OrderItems.FirstOrDefault(e => e.OrderItemID == id);
                    if (entity != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, entity);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, id.ToString() + "not found");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/OrderItems/SetToShipping")]
        public HttpResponseMessage SetToShipping([FromUri]int oid)
        {
            try
            {
                //Lát sau viết lại ràng buộc cho method này!!!
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();

                    var result = entities.OrderItems.Include("Order").Include("Product").Where(x => x.OrderItemID == oid).FirstOrDefault();
                    string emailtoSend = entities.AspNetUsers.FirstOrDefault(x => x.Id == result.Order.UserID).Email;

                    if (result != null)
                    {
                        if (result.Product.UserID == currentUserID)
                        {
                            result.OrderState = "Shipping";
                            entities.SaveChanges();

                            //Gửi Email thông báo đã mua hàng
                            SmtpClient client = new SmtpClient();
                            client.Port = 587;
                            client.Host = "smtp.gmail.com";
                            client.EnableSsl = true;
                            client.Timeout = 10000;
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new System.Net.NetworkCredential("psybladebackup@gmail.com", "hoahoa123");
                            MailMessage mm = new MailMessage("psybladebackup@gmail.com", emailtoSend, "Tình trạng đơn hàng", "Sản phẩm có mã đặt hàng là: #" + result.OrderItemID +" đang được vận chuyển.");
                            mm.BodyEncoding = UTF8Encoding.UTF8;
                            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                            client.Send(mm);
                            //Hết phần gửi email.

                            return Request.CreateResponse(HttpStatusCode.OK, "Đã chuyển sang Shipping");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không sửa được vì không phải chủ của sản phẩm này");
                        }
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không tìm thấy");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/OrderItems/SetToDone")]
        public HttpResponseMessage SetToDone([FromUri]int oid)
        {
            try
            {
                //Lát sau viết lại ràng buộc cho method này!!!
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var result = entities.OrderItems.Include("Order").Include("Product").Where(x => x.OrderItemID == oid).FirstOrDefault();
                    string emailtoSend = entities.AspNetUsers.FirstOrDefault(x => x.Id == result.Order.UserID).Email;
                    if (result != null)
                    {
                        if (result.Product.UserID == currentUserID)
                        {
                            result.OrderState = "Done";
                            entities.SaveChanges();

                            //Gửi Email thông báo đã mua hàng
                            SmtpClient client = new SmtpClient();
                            client.Port = 587;
                            client.Host = "smtp.gmail.com";
                            client.EnableSsl = true;
                            client.Timeout = 10000;
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new System.Net.NetworkCredential("psybladebackup@gmail.com", "hoahoa123");
                            MailMessage mm = new MailMessage("psybladebackup@gmail.com", emailtoSend, "Tình trạng đơn hàng", "Sản phẩm có mã đặt hàng là: #" + result.OrderItemID + " đã được vận chuyển thành công.");
                            mm.BodyEncoding = UTF8Encoding.UTF8;
                            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                            client.Send(mm);
                            //Hết phần gửi email.

                            return Request.CreateResponse(HttpStatusCode.OK, "Đã chuyển sang 'Xong'");
                            //Lát phải thêm đoạn gửi email cho khách


                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không sửa được vì không phải chủ của sản phẩm này");
                        }
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không tìm thấy");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/OrderItems/SetToCanceled")]
        public HttpResponseMessage SetToCancel([FromUri]int oid)
        {
            try
            {
                //Lát sau viết lại ràng buộc cho method này!!!
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();

                    var result = entities.OrderItems.Include("Product").Include("Order").Where(x => x.OrderItemID == oid).FirstOrDefault();
                    if (result != null)
                    {
                        if (result.Product.UserID == currentUserID || result.Order.UserID == currentUserID)
                        {
                            if (result.OrderState != "Done")
                            {
                                result.OrderState = "Canceled";
                                var producttoIncreaseBack = entities.Products.FirstOrDefault(x => x.ProductID == result.ProductID);
                                producttoIncreaseBack.Stock = producttoIncreaseBack.Stock + result.Quantity;
                                entities.SaveChanges();

                                //Gửi Email thông báo đã hủy cho khách hàng
                                SmtpClient client = new SmtpClient();
                                client.Port = 587;
                                client.Host = "smtp.gmail.com";
                                client.EnableSsl = true;
                                client.Timeout = 10000;
                                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                                client.UseDefaultCredentials = false;
                                client.Credentials = new System.Net.NetworkCredential("psybladebackup@gmail.com", "hoahoa123");
                                MailMessage mm = new MailMessage("psybladebackup@gmail.com", User.Identity.Name, "Tình trạng đơn hàng", "Sản phẩm có mã đặt hàng là: #" + result.OrderItemID + " đã bị hủy.");
                                mm.BodyEncoding = UTF8Encoding.UTF8;
                                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                                client.Send(mm);

                                //Hết phần gửi email.
                                //Gửi Email thông báo đã hủy cho người bán
                                string emailtoSend = entities.AspNetUsers.FirstOrDefault(x => x.Id == result.Product.UserID).Email;
                                SmtpClient client2 = new SmtpClient();
                                client2.Port = 587;
                                client2.Host = "smtp.gmail.com";
                                client2.EnableSsl = true;
                                client2.Timeout = 10000;
                                client2.DeliveryMethod = SmtpDeliveryMethod.Network;
                                client2.UseDefaultCredentials = false;
                                client2.Credentials = new System.Net.NetworkCredential("psybladebackup@gmail.com", "hoahoa123");
                                MailMessage mm2 = new MailMessage("psybladebackup@gmail.com", emailtoSend, "Thông báo khách hàng hủy đơn hàng", "Sản phẩm có mã đặt hàng là: #" + result.OrderItemID + " đã bị hủy.");
                                mm2.BodyEncoding = UTF8Encoding.UTF8;
                                mm2.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                                client2.Send(mm2);
                                //Hết phần gửi email.

                                return Request.CreateResponse(HttpStatusCode.OK, "Đã chuyển sang 'Cancel'");
                            }
                            else return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sản phẩm đã hoàn thành thì không thể hủy");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không sửa được vì không phải người liên quan của sản phẩm này");
                        }
                        //Nhớ cộng lại vào số lượng sản phẩm
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không tìm thấy");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/OrderItems/Analysis")]
        public HttpResponseMessage Analysis(string month = null, string year = null, string shopid =null, string userid = null, string productid = null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    string uid = User.Identity.GetUserId();
                    //var list = entities.OrderItems.Where(x=>x.Order.UserID == uid).ToList();
                    var list = entities.OrderItems.ToList();
                    if (year != null)
                    {
                        int temp = Convert.ToInt32(year);
                        list = list.Where(x => x.Order.OrderDate.Value.Year == temp).ToList();
                    }
                    if (month != null)
                    {
                        int temp = Convert.ToInt32(month);
                        list = list.Where(x => x.Order.OrderDate.Value.Month == temp).ToList();
                    }

                    if (shopid != null)
                    {
                        string temp = shopid;
                        list = list.Where(x => x.ShopID == temp).ToList();
                    }

                    if (userid != null)
                    {
                        string temp = userid;
                        list = list.Where(x => x.Order.UserID == temp).ToList();
                    }

                    if (productid != null)
                    {
                        int temp = Convert.ToInt32(productid);
                        list = list.Where(x => x.ProductID == temp).ToList();
                    }

                    //TÍnh doanh thu
                    int turnOver = list.Where(x=>x.OrderState=="Done").Sum(x => x.FinalPrice);
                    int soldItemNumber = list.Where(x => x.OrderState == "Done").Sum(x => x.Quantity);
                    var detailSoldItem = list.Where(x => x.OrderState == "Done").GroupBy(x => new { x.ProductID, x.Product.ProductName, x.Product.ProductImage }).Select(g=>new { g.Key.ProductID, g.Key.ProductName, g.Key.ProductImage, SumQuantity = g.Sum(x=>x.Quantity), sumPrice = g.Sum(x=>x.FinalPrice)}).OrderByDescending(x=>x.SumQuantity).ThenByDescending(x=>x.sumPrice).ToList();
                    var detailShop = list.Where(x => x.OrderState == "Done").GroupBy(x => new { x.ShopID, x.AspNetUser.UserName }).Select(g => new { g.Key.ShopID, g.Key.UserName, SumQuantity = g.Sum(x => x.Quantity), sumPrice = g.Sum(x => x.FinalPrice) }).OrderByDescending(x=>x.SumQuantity).ThenByDescending(x=>x.sumPrice).ToList();
                    var detailUser = list.Where(x => x.OrderState == "Done").GroupBy(x => new { x.Order.UserID, x.Order.AspNetUser.UserName }).Select(g => new { g.Key.UserID, g.Key.UserName, SumQuantity = g.Sum(x => x.Quantity), sumPrice = g.Sum(x => x.FinalPrice) }).OrderByDescending(x=>x.SumQuantity).ThenByDescending(x=>x.sumPrice).ToList();
                    var detailRating = entities.Ratings.GroupBy(x => new { x.Product.UserID, x.Product.AspNetUser.UserName }).Select(g => new { g.Key.UserID, g.Key.UserName, AverageRating = g.Average(x => x.Rating1), RatingTime = g.Count() }).OrderByDescending(x=>x.AverageRating).ThenByDescending(x=>x.RatingTime).ToList();
                    var detailRatingByItem = entities.Ratings.GroupBy(x => new { x.ProductID, x.Product.ProductName}).Select(g => new { g.Key.ProductID, g.Key.ProductName, AverageRating = g.Average(x => x.Rating1), RatingTime = g.Count() }).OrderByDescending(x=>x.AverageRating).ThenByDescending(x=>x.RatingTime).ToList();

                    var result = new { turnover = turnOver, solditemnumber = soldItemNumber, detailsolditem = detailSoldItem, detailshop = detailShop, detailuser = detailUser, detailrating = detailRating, detailratingbyitem = detailRatingByItem };

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/orderitems
        [HttpPost]
        [Authorize]
        public HttpResponseMessage Post([FromBody] OrderItem orderitem)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    entities.OrderItems.Add(orderitem);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/orderitems/1
        [HttpPut]
        [Authorize]
        public HttpResponseMessage Put(int id, [FromBody]OrderItem orderitem)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.OrderItems.FirstOrDefault(e => e.OrderItemID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Order item with Id " + id.ToString() + " not found to update");
                    }
                    else
                    {
                        entity.OrderState = orderitem.OrderState;
                        entity.Paided = orderitem.Paided;
                        entities.SaveChanges();

                        return Request.CreateResponse(HttpStatusCode.OK, entity);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/orderitems/1
        [HttpDelete]
        [Authorize]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.OrderItems.FirstOrDefault(e => e.OrderItemID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Order item with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        entities.OrderItems.Remove(entity);
                        entities.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Delete OK");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
