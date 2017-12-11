using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Webbanhang.Controllers
{
    public class SponsoredNewsOrdersController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage LoadAllNewsOrders()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    return Request.CreateResponse(HttpStatusCode.OK, entities.SponsoredNewsOrders.ToList());
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage LoadNewsOrdersByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.SponsoredNewsOrders.FirstOrDefault(e => e.SponsoredNewsOrderID == id);
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
        [Authorize]
        [Route("api/SponsoredNewsOrders/PurchaseVipNews")]
        public HttpResponseMessage PurchaseVipNews([FromUri] int quantity)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    SponsoredNewsOrder sno = new SponsoredNewsOrder();
                    sno.UserID = HttpContext.Current.User.Identity.GetUserId();
                    sno.SponsoredNewsOrderDate = DateTime.Now;
                    sno.Quantity = quantity;
                    sno.SumPrice = sno.Quantity * 50000;
                    entities.SponsoredNewsOrders.Add(sno);
                    entities.UserInfos.Where(x => x.UserID == sno.UserID).FirstOrDefault().VipNewsCount = 
                        entities.UserInfos.Where(x => x.UserID == sno.UserID).FirstOrDefault().VipNewsCount 
                        + quantity;
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "PURCHASE OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
