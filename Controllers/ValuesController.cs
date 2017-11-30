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
        public string Get(int id)
        {
            
            return "value";
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
