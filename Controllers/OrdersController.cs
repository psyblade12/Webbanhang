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
        [Authorize(Roles = "ActivatedUser")]
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
                        orderitem.Price = Convert.ToInt32(entities.Products.FirstOrDefault(x => x.ProductID == maitem).Price.ToString()) * item.quantity;
                        orderitem.FinalPrice = Convert.ToInt32(entities.Products.FirstOrDefault(x => x.ProductID == maitem).Price.ToString())*item.quantity;
                        orderitem.OrderState = "Waiting";
                        orderitem.Paided = false;
                        entities.OrderItems.Add(orderitem);

                        //Reduce quantity.
                        var reduceQuantity = entities.Products.FirstOrDefault(e => e.ProductID == maitem);
                        if (reduceQuantity.Stock < item.quantity)
                        {
                            throw new Exception("Quantity is higher than stock");
                        }
                        reduceQuantity.Stock = reduceQuantity.Stock - item.quantity;

                        //Mua xong thì xóa cart
                        var cartToDelete = entities.UserInfos.FirstOrDefault(x => x.UserID == userid);
                        cartToDelete.Cart = "[]";

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

                    //Chuẩn bị biến để return về hóa đơn
                    int lastIDofOrder = entities.Orders.Max(x => x.OrderID);
                    var invoice = entities.OrderItems.Where(x => x.OrderID == lastIDofOrder).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage, shopName = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).Name, shopPhoneNumber = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).PhoneNumber, shopAddress = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).HomeAddress, shopEmail = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).Email }).ToList() }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, invoice);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Orders/LoadAllOrdersAndOrderItem")]
        public HttpResponseMessage LoadAllOrdersAndOrderItem([FromUri] string month = null, string year = null, string minTotalPrice = null, string maxTotalPrice = null, string orderIDtoSearch = null, string skip = null, string take = null, string date = null, string buyer=null, string seller=null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.OrderItems.GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, sellerID = z.Product.UserID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    if (seller != null)
                    {
                        string sellerIDtofind = entities.UserInfos.FirstOrDefault(x => x.Name.ToLower().Contains(seller)).UserID.ToString();
                        var templist = entities.OrderItems.Where(x => x.ShopID == sellerIDtofind);
                        result = templist.GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, sellerID = z.Product.UserID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    }
                    result = result.OrderByDescending(x => x.orderDate).ToList();
                    if (orderIDtoSearch != null)
                    {
                        int tempOrderIDtoSearch = Convert.ToInt32(orderIDtoSearch);
                        result = result.Where(x => x.orderID == tempOrderIDtoSearch).ToList();
                    }

                    if (month != null)
                    {
                        int tempMonth = Convert.ToInt32(month);
                        result = result.Where(x => x.orderDate.Value.Month == tempMonth).ToList();
                    }

                    if (year != null)
                    {
                        int tempYear = Convert.ToInt32(year);
                        result = result.Where(x => x.orderDate.Value.Year == tempYear).ToList();
                    }

                    if (minTotalPrice != null)
                    {
                        int tempMinTotalPrice = Convert.ToInt32(minTotalPrice);
                        result = result.Where(x => x.orderTotalPrice > tempMinTotalPrice).ToList();
                    }

                    if (minTotalPrice != null)
                    {
                        int tempMinTotalPrice = Convert.ToInt32(minTotalPrice);
                        result = result.Where(x => x.orderTotalPrice >= tempMinTotalPrice).ToList();
                    }

                    if (maxTotalPrice != null)
                    {
                        int tempMaxTotalPrice = Convert.ToInt32(maxTotalPrice);
                        result = result.Where(x => x.orderTotalPrice >= tempMaxTotalPrice).ToList();
                    }

                    //if (buyer != null)
                    //{
                    //    result = result.Where(x => x.orderNameofUser!=null && x.orderNameofUser.Contains(buyer)).ToList();
                    //}
                    if (buyer != null)
                    {
                        result = result.Where(x => x.orderNameofUser != null && x.orderNameofUser.ToLower().Contains(buyer.ToLower())).ToList();
                    }

                    if (date !=null)
                    {
                        DateTime tempDate = Convert.ToDateTime(date);
                        result = result.Where(x => x.orderDate.Value.Day == tempDate.Day &&  x.orderDate.Value.Month == tempDate.Month && x.orderDate.Value.Year == tempDate.Year).ToList();
                    }
                    

                    if (take != null)
                    {
                        int tempTake = Convert.ToInt32(take);
                        if(skip !=null)
                        {
                            int tempSkip = Convert.ToInt32(skip);
                            result = result.Skip(tempSkip).Take(tempTake).ToList();
                        }
                        else
                        {
                            result = result.Take(tempTake).ToList();
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
        [Route("api/Orders/LoadAllMyOrdersAndOrderItem")]
        public HttpResponseMessage LoadAllMyOrdersAndOrderItem([FromUri] string month = null, string year = null, string minTotalPrice = null, string maxTotalPrice = null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var result = entities.OrderItems.Where(x=>x.Order.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    if (month != null)
                    {
                        int tempMonth = Convert.ToInt32(month);
                        result = result.Where(x => x.orderDate.Value.Month == tempMonth).ToList();
                    }

                    if (year != null)
                    {
                        int tempYear = Convert.ToInt32(year);
                        result = result.Where(x => x.orderDate.Value.Year == tempYear).ToList();
                    }
                    
                    if (minTotalPrice != null)
                    {
                        int tempMinTotalPrice = Convert.ToInt32(minTotalPrice);
                        result = result.Where(x => x.orderTotalPrice >= tempMinTotalPrice).ToList();
                    }

                    if (maxTotalPrice != null)
                    {
                        int tempMaxTotalPrice = Convert.ToInt32(maxTotalPrice);
                        result = result.Where(x => x.orderTotalPrice <= tempMaxTotalPrice).ToList();
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
        [Route("api/Orders/LoadAllMyMerchantOrdersAndOrderItem")]
        public HttpResponseMessage LoadAllMyMerchantOrdersAndOrderItem()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var result = entities.OrderItems.Where(x => x.Product.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
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
                    var result = entities.OrderItems.Where(x => x.OrderState == stateToLoad).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
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
                    var result = entities.OrderItems.Where(x => x.OrderState == stateToLoad && x.Order.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
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
                    var result = entities.OrderItems.Where(x => x.OrderState == stateToLoad && x.Product.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID && g.ShopID == x.ShopID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
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
                    var listOrders = entities.OrderItems.GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
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
        public HttpResponseMessage LoadAllMyCompleteOrders([FromUri]string stateToLoad = "Done", string month = null, string year = null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var listOrders = entities.OrderItems.Where(x => x.Order.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g => g.OrderID == x.OrderID).Sum(h => h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
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

                    if(month !=null)
                    {
                        int tempMonth = Convert.ToInt32(month);
                        result = result.Where(x => x.orderDate.Value.Month == tempMonth).ToList();
                    }

                    if (year != null)
                    {
                        int tempYear = Convert.ToInt32(year);
                        result = result.Where(x => x.orderDate.Value.Year == tempYear).ToList();
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
        public HttpResponseMessage LoadAllMyMerchantCompleteOrders([FromUri] string month = null, string year = null, string minTotalPrice = null, string maxTotalPrice = null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    var listOrders = entities.OrderItems.Where(x => x.Product.UserID == uid).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName, totalPrice = entities.OrderItems.Where(g=>g.OrderID == x.OrderID).Sum(h=>h.FinalPrice) }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderTotalPrice = y.Key.totalPrice, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage }).ToList() }).ToList();
                    var result = listOrders.ToList();
                    foreach (var i in listOrders)
                    {
                        foreach (var j in i.orderItemIDs)
                        {
                            if (j.orderItemState == "Waiting" || j.orderItemState == "Shipping")
                            {
                                result.Remove(i);
                                break;
                            }
                        }
                    }
                    if (month != null)
                    {
                        int tempMonth = Convert.ToInt32(month);
                        result = result.Where(x => x.orderDate.Value.Month == tempMonth).ToList();
                    }

                    if (year != null)
                    {
                        int tempYear = Convert.ToInt32(year);
                        result = result.Where(x => x.orderDate.Value.Year == tempYear).ToList();
                    }

                    if (minTotalPrice != null)
                    {
                        int tempMinTotalPrice = Convert.ToInt32(minTotalPrice);
                        result = result.Where(x => x.orderTotalPrice >= tempMinTotalPrice).ToList();
                    }

                    if (maxTotalPrice != null)
                    {
                        int tempMaxTotalPrice = Convert.ToInt32(maxTotalPrice);
                        result = result.Where(x => x.orderTotalPrice <= tempMaxTotalPrice).ToList();
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
