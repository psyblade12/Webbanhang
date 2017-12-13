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
    public class SponsoredItemsController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage LoadAllSponsored()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    return Request.CreateResponse(HttpStatusCode.OK, entities.SponsoredItems.ToList());
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/SponsoredItems/LoadAllSponsoredItemsInTime")]
        public HttpResponseMessage LoadAllSponsoredItemsInTime()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.SponsoredItems.Where(x => x.EndDate > DateTime.Now).Select(y=>new {
                        sponsoredItemID = y.SponsoredItemID,
                        startDate = y.StartDate,
                        endDate = y.EndDate,
                        product = entities.Products.FirstOrDefault(z => y.ProductID == z.ProductID)
                    }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/SponsoredItems/LoadAllMySponsored")]
        [Authorize]
        public HttpResponseMessage LoadAllMySponsored()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var result = entities.SponsoredItems.Where(x => x.Product.UserID == currentUserID).Select(x => new { sponsoredItemID = x.SponsoredItemID, productName = x.Product.ProductName, startDate = x.StartDate, endDate = x.EndDate }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/product/1
        [HttpGet]
        public HttpResponseMessage LoadSponsoredByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.SponsoredItems.FirstOrDefault(e => e.SponsoredItemID == id);
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
        [Route("api/SponsoredItem/Additem")]
        public HttpResponseMessage PurchaseVipNews([FromUri] int productID)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    //Check if product is currently being promoted:
                    string currentUserID = User.Identity.GetUserId();
                    var listtoCheck = entities.SponsoredItems.Where(x => x.Product.UserID == currentUserID).ToList();
                    foreach (var x in listtoCheck)
                    {
                        if(x.ProductID == productID && x.EndDate>=DateTime.Now)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Sản phẩm hiện đang được promte");
                        }
                    }

                    SponsoredItem sno = new SponsoredItem();
                    sno.ProductID = productID;
                    sno.StartDate = DateTime.Now;
                    sno.EndDate = sno.StartDate.Value.AddDays(7);
                    entities.SponsoredItems.Add(sno);
                    var user = entities.UserInfos.FirstOrDefault(x=>x.UserID == currentUserID);

                    //Kiểm tra còn lượng tin đăng hay không
                    if (user.VipNewsCount == 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Hết số lượng được đăng");
                    }


                    user.VipNewsCount = user.VipNewsCount - 1;
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/SponsoredItem/ExtendTime")]
        public HttpResponseMessage ExtendTime([FromUri] int SponsoredItemID)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    SponsoredItem sno = entities.SponsoredItems.FirstOrDefault(x => x.SponsoredItemID == SponsoredItemID);
                    if (sno != null)
                    {
                        var user = entities.UserInfos.FirstOrDefault(x => x.UserID == currentUserID);
                        if (user.VipNewsCount == 0)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Hết số lượng được đăng");
                        }

                        sno.EndDate = sno.EndDate.Value.AddDays(7);
                        user.VipNewsCount = user.VipNewsCount - 1;
                        entities.SaveChanges();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
