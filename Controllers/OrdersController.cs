using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using Newtonsoft.Json;
using Webbanhang.Models;
using System.Net.Mail;
using System.Text;

namespace Webbanhang.Controllers
{
    public class OrdersController : ApiController
    {
        [HttpPost]
        [Route("api/Orders/MakeOrder")]
        public HttpResponseMessage MakeOrder(InfoBindingModel info)
        {
            try
            {
                //Kiểm tra chuẩn
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
                //Get cart
                string cart;
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string userid = HttpContext.Current.User.Identity.GetUserId();
                    cart = entities.UserInfos.FirstOrDefault(x => x.UserID == userid).Cart;
                    List<CartEntity> itemBuyList = JsonConvert.DeserializeObject<List<CartEntity>>(cart);
                    Order newOrder = new Order();
                    newOrder.UserID = userid;
                    newOrder.OrderDate = DateTime.Now;
                    newOrder.OrderAddress = info.homeAddress;
                    newOrder.OrderNameofUser = info.name;
                    newOrder.OrderPhoneNumber = info.phoneNumber;
                    entities.Orders.Add(newOrder);
                    entities.SaveChanges();
                    foreach (CartEntity item in itemBuyList)
                    {
                        int maitem = Convert.ToInt32(item.productID);
                        OrderItem orderitem = new OrderItem();
                        orderitem.OrderID = entities.Orders.ToList()[entities.Orders.ToList().Count-1].OrderID;
                        orderitem.ShopID = entities.Products.FirstOrDefault(x => x.ProductID == maitem).UserID.ToString();
                        //var temp = entities.Products.Where(x => x.ProductID == maitem).ToList();
                        orderitem.ProductID = maitem;
                        orderitem.Quantity = item.quantity;
                        orderitem.Price = Convert.ToInt32(entities.Products.FirstOrDefault(x => x.ProductID == maitem).Price.ToString());
                        orderitem.FinalPrice = Convert.ToInt32(entities.Products.FirstOrDefault(x => x.ProductID == maitem).Price.ToString());
                        orderitem.OrderState = "Waiting";
                        orderitem.Paided = false;
                        entities.OrderItems.Add(orderitem);

                        //Reduce quantity.
                        var reduceQuantity = entities.Products.FirstOrDefault(e => e.ProductID == maitem);
                        if (reduceQuantity.Stock <= item.quantity)
                        {
                            throw new Exception("Quantity is higher than stock");
                        }
                        reduceQuantity.Stock = reduceQuantity.Stock - item.quantity;

                        //Save changes
                        entities.SaveChanges();
                    }

                    //Gửi Email thông báo đã mua hàng
                    SmtpClient client = new SmtpClient();
                    client.Port = 587;
                    client.Host = "smtp.gmail.com";
                    client.EnableSsl = true;
                    client.Timeout = 10000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("psybladebackup@gmail.com", "hoahoa123");
                    MailMessage mm = new MailMessage("psybladebackup@gmail.com", User.Identity.Name, "Mua hàng", "Bạn đã đặt hóa đơn thành công");
                    mm.BodyEncoding = UTF8Encoding.UTF8;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    client.Send(mm);
                    //Hết phần gửi email.


                    var a = Request.CreateResponse(HttpStatusCode.OK, "Order made");
                    return a;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllOrdersAndOrderItem")]
        public HttpResponseMessage LoadAllOrdersAndOrderItem()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.OrderItems.GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName}).Select(y=>new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id , orderItemIDs = y.Select(z=>new {orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage}).ToList()}).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllMyOrdersAndOrderItem")]
        public HttpResponseMessage LoadAllMyOrdersAndOrderItem()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var result = entities.OrderItems.Where(x=>x.Order.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllMyMerchantOrdersAndOrderItem")]
        public HttpResponseMessage LoadAllMyMerchantOrdersAndOrderItem()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var result = entities.OrderItems.Where(x => x.Product.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllOrdersAndOrderItemByState")]
        public HttpResponseMessage LoadAllOrdersAndOrderItemInWatingState([FromUri]string stateToLoad = "Done")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.OrderItems.Where(x => x.OrderState == stateToLoad).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllMyOrdersAndOrderItemByState")]
        public HttpResponseMessage LoadAllMyOrdersAndOrderItemByState([FromUri]string stateToLoad = "Done")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var result = entities.OrderItems.Where(x => x.OrderState == stateToLoad && x.Order.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllMyMerchantOrdersAndOrderItemByState")]
        public HttpResponseMessage LoadAllMyMerchantOrdersAndOrderItemByState([FromUri]string stateToLoad = "Done")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var result = entities.OrderItems.Where(x => x.OrderState == stateToLoad && x.Product.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllCompleteOrders")]
        public HttpResponseMessage LoadAllCompleteOrders([FromUri]string stateToLoad = "Done")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var listOrders = entities.OrderItems.GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    var result = listOrders.ToList();
                    foreach (var i in listOrders)
                    {
                        foreach (var j in i.orderItemIDs)
                        {
                            if(j.orderItemState != "Done")
                            {
                                result.Remove(i);
                                break;
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllMyCompleteOrders")]
        public HttpResponseMessage LoadAllMyCompleteOrders([FromUri]string stateToLoad = "Done")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var listOrders = entities.OrderItems.Where(x => x.Order.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    var result = listOrders.ToList();
                    foreach (var i in listOrders)
                    {
                        foreach (var j in i.orderItemIDs)
                        {
                            if (j.orderItemState != "Done")
                            {
                                result.Remove(i);
                                break;
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllMyMerchantCompleteOrders")]
        public HttpResponseMessage LoadAllMyMerchantCompleteOrders([FromUri]string stateToLoad = "Done")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var listOrders = entities.OrderItems.Where(x => x.Product.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    var result = listOrders.ToList();
                    foreach (var i in listOrders)
                    {
                        foreach (var j in i.orderItemIDs)
                        {
                            if (j.orderItemState != "Done")
                            {
                                result.Remove(i);
                                break;
                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage LoadAllOrders()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var a = Request.CreateResponse(HttpStatusCode.OK, entities.Products.ToList());
                    return a;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage LoadOrderByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Orders.FirstOrDefault(e => e.OrderID == id);
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

        // api/orders
        [HttpPost]
        [Authorize]
        public HttpResponseMessage Post([FromBody] Order order)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    entities.Orders.Add(order);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/orders/1
        [HttpPut]
        [Authorize]
        public HttpResponseMessage Put(int id, [FromBody]Order order)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Orders.FirstOrDefault(e => e.OrderID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Order with Id " + id.ToString() + " not found to update");
                    }
                    else
                    {
                        entity.UserID = order.UserID;
                        entity.OrderDate = order.OrderDate;
                        entity.OrderAddress = order.OrderAddress;

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

        // api/order/1
        [HttpDelete]
        [Authorize]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Orders.FirstOrDefault(e => e.OrderID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Order with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        entities.Orders.Remove(entity);
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
