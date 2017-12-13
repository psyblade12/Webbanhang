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

                    var result = entities.OrderItems.Include("Product").Where(x => x.OrderItemID == oid).FirstOrDefault();
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
                            MailMessage mm = new MailMessage("psybladebackup@gmail.com", User.Identity.Name, "Tình trạng đơn hàng", "Sản phẩm có mã đặt hàng là: #" + result.OrderItemID +" đang được vận chuyển.");
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

                    var result = entities.OrderItems.Include("Product").Where(x => x.OrderItemID == oid).FirstOrDefault();
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
                            MailMessage mm = new MailMessage("psybladebackup@gmail.com", User.Identity.Name, "Tình trạng đơn hàng", "Sản phẩm có mã đặt hàng là: #" + result.OrderItemID + " đã được vận chuyển thành công.");
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

                                //Gửi Email thông báo đã mua hàng
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
