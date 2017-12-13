using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Webbanhang.Models;
using Webbanhang.Providers;
using Webbanhang.Results;
using System.Net;
using System.Linq;

namespace Webbanhang.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public HttpResponseMessage TestLoad()
        {
            //return User.Identity.GetUserName();
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;
                var a = entities.Products.Include("brands").ToString();
                return Request.CreateErrorResponse(HttpStatusCode.OK, "not found");
            }

        }

        // GET api/values/5
        public HttpResponseMessage Get(int id)
        {

            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;
                int lastIDofOrder = entities.Orders.Max(x => x.OrderID);
                var invoice = entities.OrderItems.Where(x => x.OrderID == lastIDofOrder).GroupBy(x => new { x.OrderID, x.Order.OrderDate, x.Order.AspNetUser.Id, x.Order.OrderNameofUser, x.Order.OrderPhoneNumber, x.Order.OrderAddress, x.Order.AspNetUser.UserName }).Select(y => new { orderID = y.Key.OrderID, orderDate = y.Key.OrderDate, orderUser = y.Key.UserName, orderUserID = y.Key.Id, orderNameofUser = y.Key.OrderNameofUser, orderAddress = y.Key.OrderAddress, orderPhoneNumber = y.Key.OrderPhoneNumber, orderItemIDs = y.Select(z => new { orderItemID = z.OrderItemID, orderItemState = z.OrderState, orderItemQuantity = z.Quantity, orderItemPrice = z.FinalPrice, itemID = z.Product.ProductID, productName = z.Product.ProductName, productImage = z.Product.ProductImage, shopName = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).Name, shopPhoneNumber = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).PhoneNumber, shopAddress = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).HomeAddress, shopEmail = entities.UserInfos.FirstOrDefault(c => c.UserID == z.ShopID).Email }).ToList() }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
