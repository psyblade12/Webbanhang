using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Net.Mail;
using System.Text;
using Webbanhang.Models;

namespace Webbanhang.Controllers
{
    public class ProductController : ApiController
    {
        [HttpGet]
        [Route("api/Products/KichHoatTaiKhoan")]
        public HttpResponseMessage SendMail(MailMessage Message)
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("psybladebackup@gmail.com", "hoahoa123");

            MailMessage mm = new MailMessage("psybladebackup@gmail.com", "thuantt190@gmail.com", "test", "test");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/Products/SearchProduct")]
        public HttpResponseMessage SearchProductByType([FromUri] string userid = null, string name = null, string productTypeid = null, string brandId = null, string priceMin = null, string priceMax = null, string sort="dsc", string skip =null, string take =null)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;
                var entity = entities.Products.ToList();

                if (userid != null)
                {
                    entity = entity.Where(x => x.UserID == userid).ToList();
                }
                if (name != null)
                {
                    entity = entity.Where(x => x.ProductName.ToLower().Contains(name.ToLower())).ToList();
                }

                if (productTypeid != null)
                {
                    entity = entity.Where(x => x.ProductTypeID == Convert.ToInt32(productTypeid)).ToList();
                }

                if (brandId != null)
                {
                    entity = entity.Where(x => x.BrandID== Convert.ToInt32(brandId)).ToList();
                }

                if (priceMin != null)
                {
                    entity = entity.Where(x => x.Price >= Convert.ToInt32(priceMin)).ToList();
                }

                if(priceMax !=null)
                {
                    entity = entity.Where(x => x.Price <= Convert.ToInt32(priceMax)).ToList();
                }

                if(sort !=null)
                {
                    if(sort=="dsc")
                    {
                        entity = entity.OrderByDescending(x => x.ProductID).ToList();
                    }
                    if(sort == "asc")
                    {
                        entity = entity.OrderBy(x => x.ProductID).ToList();
                    }
                    if(sort == "ran")
                    {
                        var rnd = new Random();
                        entity =  entity.OrderBy(x => rnd.Next()).ToList();
                    }
                }
                if(take != null)
                {
                    int tempTake = Convert.ToInt32(take);
                    if(skip !=null)
                    {
                        int tempSkip = Convert.ToInt32(skip);
                        entity = entity.Skip(tempSkip).Take(tempTake).ToList();
                    }
                    else
                    {
                        entity = entity.Take(tempTake).ToList();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
        }

        [HttpGet]
        public HttpResponseMessage LoadAllProduct(string sort=null, string take = null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var result = entities.Products.ToList();
                    result = result.OrderByDescending(x => x.ProductID).ToList();
                    if(take!=null)
                    {
                        int takeTemp = Convert.ToInt32(take);
                        result = result.Take(takeTemp).ToList();
                    }
                    if (sort == "dsc")
                    {
                        result = result.OrderByDescending(x => x.ProductID).ToList();
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
        [Route("api/Products/LoadAllMyProducts")]
        [Authorize]
        public HttpResponseMessage LoadAllMyProduct()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var result = entities.Products.Where(x => x.UserID == currentUserID).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpGet]
        public HttpResponseMessage LoadProductByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Products.Include("AspNetUser").Include("ProductType").Include("Brand").FirstOrDefault(e => e.ProductID == id);
                    if (entity != null)
                    {
                        var result = new {
                            productID = entity.ProductID,
                            userName = entity.AspNetUser.UserName,
                            productType = entity.ProductType.ProductTypeName,
                            brandName =  entity.Brand.BrandName,
                            productName = entity.ProductName,
                            Detail = entity.Detail,
                            Stock = entity.Stock,
                            ProductImage = entity.ProductImage,
                            OldPrice = entity.OldPrice,
                            Price = entity.Price,
                            CreationDate = entity.CreationDate
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, result);
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
        [Route("api/Products/LoadProductByIDforEdit")]
        public HttpResponseMessage LoadProductByIDforEdit(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Products.FirstOrDefault(e => e.ProductID == id);
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
        [Authorize(Roles = "Merchant")]
        public HttpResponseMessage Post([FromBody] ProductModel product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                //Kiểm tra giá cũ có lớn hơn giá mới không
                if (product.Price > product.OldPrice)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Giá cũ phải cao hơn giá mới");
                }

                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    product.CreationDate = DateTime.Now;
                    product.UserID = User.Identity.GetUserId();

                    //Kiểm tra xem có đang bị ban hay không
                    string currentUserID = User.Identity.GetUserId();
                    var list = entities.BanAccounts.Where(x => x.UserID == currentUserID && x.LiftDate > DateTime.Now).ToList();
                    if (list.Count != 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Bạn đang bị ban, lý do: "+list[0].Reason);
                    }
                    //Hết kiểm tra bị ban

                    Product newproduct = new Product();
                    newproduct.UserID = product.UserID;
                    newproduct.ProductTypeID = product.ProductTypeID;
                    newproduct.BrandID = product.BrandID;
                    newproduct.ProductName = product.ProductName;
                    newproduct.Detail = product.Detail;
                    newproduct.Stock = product.Stock;
                    newproduct.ProductImage = product.ProductImage;
                    newproduct.Price = product.Price;
                    newproduct.OldPrice = product.OldPrice;
                    newproduct.CreationDate = DateTime.Now;

                    entities.Products.Add(newproduct);
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
        public HttpResponseMessage Put(int id, [FromBody]ProductModel product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                //Kiểm tra giá phải nhỏ hơn giá cũ
                if (product.Price > product.OldPrice)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Giá cũ phải cao hơn giá mới");
                }

                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Products.FirstOrDefault(e => e.ProductID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Product with Id " + id.ToString() + " not found to update");
                    }
                    else
                    {
                        if (entity.UserID == User.Identity.GetUserId())
                        {
                            entity.ProductTypeID = product.ProductTypeID;
                            entity.BrandID = product.BrandID;
                            entity.ProductName = product.ProductName;
                            entity.Detail = product.Detail;
                            entity.Stock = product.Stock;
                            entity.OldPrice = product.OldPrice;
                            entity.Price = product.Price;
                            entity.ProductImage = product.ProductImage;

                            entities.SaveChanges();

                            return Request.CreateResponse(HttpStatusCode.OK, "Đã sửa");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,"Có lỗi xảy ra");
                        }
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
                    var entity = entities.Products.FirstOrDefault(e => e.ProductID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Product with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        if (entity.UserID == User.Identity.GetUserId())
                        {
                            entities.Products.Remove(entity);
                            entities.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, "Delete OK");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Có lỗi xảy ra");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("api/Products/DeleteProductForAdmin")]
        public HttpResponseMessage DeleteProductForAdmin(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Products.FirstOrDefault(e => e.ProductID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Product with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        entities.Products.Remove(entity);
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
