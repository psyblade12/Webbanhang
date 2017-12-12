using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Webbanhang.Controllers
{
    public class ReportController : ApiController
    {
        [HttpGet]
        [Route("api/Report/GetAllUnreadReport")]
        [Authorize]
        public HttpResponseMessage GetAllUnreadReport()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.Reports.Where(x => x.IsRead == false).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Report/ChangetoRead")]
        [Authorize]
        public HttpResponseMessage ChangetoRead(int rid)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.Reports.FirstOrDefault(x=>x.ReportID == rid);
                    if(result !=null)
                    {
                        result.IsRead = true;
                        entities.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,"Có lỗi");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Authorize]
        public HttpResponseMessage Post([FromBody] Report rp)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string uid = User.Identity.GetUserId();
                    rp.UserID = uid;
                    rp.IsRead = false;
                    entities.Reports.Add(rp);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Report made");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}
