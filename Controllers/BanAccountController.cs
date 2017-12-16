using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Webbanhang.Models;

namespace Webbanhang.Controllers
{
    public class BanAccountController : ApiController
    {
        [HttpGet]
        //[Route("api/BanAccount/LoadAllBanAccount")]
        [Authorize]
        public HttpResponseMessage Get()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.BanAccounts.ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        //Xuất ra danh sách các account có thời gian bị ban lớn hơn thời gian hiện tại 
        //(Nghĩa là lấy ra danh sách account hiện tại đang bị ban)
        [HttpGet]
        [Route("api/BanAccount/LoadAllBanAccountInTime")]
        [Authorize]
        public HttpResponseMessage LoadAllBanAccountInTime()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.BanAccounts.Where(x=>x.LiftDate> DateTime.Now).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        //Kiểm tra xem user đang dùng hiện tại có bị ban hay không
        [HttpGet]
        [Route("api/BanAccount/CheckBan")]
        [Authorize]
        public HttpResponseMessage CheckBan()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    bool flag = false;
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var list = entities.BanAccounts.Where(x => x.UserID == currentUserID && x.LiftDate > DateTime.Now).ToList();
                    if(list.Count != 0)
                    {
                        flag = true;
                        var respond = new { banned = flag, reason = list[list.Count - 1].Reason };
                        return Request.CreateResponse(HttpStatusCode.OK, respond);
                    }
                    else
                    {
                        var respond2 = new { banned = flag, reason = ""};
                        return Request.CreateResponse(HttpStatusCode.OK, respond2);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/BanAccount/CheckBanByUserID")]
        [Authorize]
        public HttpResponseMessage CheckBanByUserID(string uid)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    bool flag = false;
                    entities.Configuration.ProxyCreationEnabled = false;
                    var list = entities.BanAccounts.Where(x => x.UserID == uid && x.LiftDate > DateTime.Now).ToList();
                    if (list.Count != 0)
                    {
                        flag = true;
                        var respond = new { banned = flag, reason = list[list.Count - 1].Reason };
                        return Request.CreateResponse(HttpStatusCode.OK, respond);
                    }
                    else
                    {
                        var respond2 = new { banned = flag, reason = "" };
                        return Request.CreateResponse(HttpStatusCode.OK, respond2);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/BanAccount/RemoveBan")]
        [Authorize]
        public HttpResponseMessage RemoveBan(string uid)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var list = entities.BanAccounts.Where(x => x.UserID == uid && x.LiftDate > DateTime.Now).ToList();
                    foreach (var s in list)
                    {
                        entities.BanAccounts.Remove(s);
                    }
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Đã gỡ ban");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Authorize]
        public HttpResponseMessage Post([FromBody] BanAccountModel ba)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;

                    BanAccount banacc = new BanAccount();
                    banacc.UserID = ba.UserID;
                    banacc.Reason = ba.Reason;
                    banacc.LiftDate = Convert.ToDateTime(ba.LiftDate);
                    entities.BanAccounts.Add(banacc);

                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        [Authorize]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.BanAccounts.FirstOrDefault(e => e.BanAccountID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Order item with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        entities.BanAccounts.Remove(entity);
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
