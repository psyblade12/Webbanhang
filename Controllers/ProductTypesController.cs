using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Webbanhang.Controllers
{
    public class ProductTypesController : ApiController
    {
        // api/product
        [HttpGet]
        public HttpResponseMessage LoadAllProductType()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    return Request.CreateResponse(HttpStatusCode.OK, entities.ProductTypes.ToList());
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/product/1
        [HttpGet]
        public HttpResponseMessage LoadProductTypeByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.ProductTypes.FirstOrDefault(e => e.ProductTypeID == id);
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

        // api/product
        [HttpPost]
        [Authorize]
        public HttpResponseMessage Post([FromBody] ProductType producttype)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    entities.ProductTypes.Add(producttype);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/product/1
        [HttpPut]
        [Authorize]
        public HttpResponseMessage Put(int id, [FromBody]ProductType productype)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.ProductTypes.FirstOrDefault(e => e.ProductTypeID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Product with Id " + id.ToString() + " not found to update");
                    }
                    else
                    {
                        entity.ProductTypeName = productype.ProductTypeName;
                        entity.Detail = productype.Detail;

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

        // api/product/1
        [HttpDelete]
        [Authorize]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.ProductTypes.FirstOrDefault(e => e.ProductTypeID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Product with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        entities.ProductTypes.Remove(entity);
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
