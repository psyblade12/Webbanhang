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

namespace Webbanhang.Controllers
{
    public class CartEntity
    {
        public int productID { get; set; }
        public int quantity { get; set; }
    }
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
            client.Credentials = new System.Net.NetworkCredential("dinhtai018@gmail.com", "unpjkmgajosgyxzv");

            MailMessage mm = new MailMessage("dinhtai018@gmail.com", "thuantt190@gmail.com", "test", "test");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/Products/SearchProduct")]
        public HttpResponseMessage SearchProductByType([FromUri] string userid, string name, string productTypeid, string brandId, string priceMin, string priceMax)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;
                var entity = entities.Products.ToList();

                if (userid != "null")
                {
                    entity = entity.Where(x => x.UserID == userid).ToList();
                }
                if (name != "null")
                {
                    entity = entity.Where(x => x.ProductName.Contains(name)).ToList();
                }

                if (productTypeid != "null")
                {
                    entity = entity.Where(x => x.ProductTypeID == Convert.ToInt32(productTypeid)).ToList();
                }

                if (brandId != "null")
                {
                    entity = entity.Where(x => x.BrandID== Convert.ToInt32(brandId)).ToList();
                }

                if (priceMin != "null" && priceMax !="null")
                {
                    entity = entity.Where(x => x.Price <= Convert.ToInt32(priceMax) && x.Price >= Convert.ToInt32(priceMin)).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
        }

        [HttpGet]
        [Route("api/Products/AddToCart")]
        public HttpResponseMessage AddToCart([FromUri]int pid = 1, int q = 1)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;

                string userid = HttpContext.Current.User.Identity.GetUserId();
                List<CartEntity> CartItemList = new List<CartEntity>();
                CartItemList = JsonConvert.DeserializeObject<List<CartEntity>>(entities.UserInfos.FirstOrDefault(e => e.UserID == userid).Cart);

                //Tìm thử xem có sẵn chưa, nếu có rồi thì chỉ cộng thêm số lượng
                bool flag = false;
                foreach (CartEntity item in CartItemList)
                {
                    if (item.productID == pid)
                    {
                        item.quantity = item.quantity + q;
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    CartItemList.Add(new CartEntity { productID = pid, quantity = q });
                }

                var entity = entities.UserInfos.FirstOrDefault(e => e.UserID == userid);
                entity.Cart = JsonConvert.SerializeObject(CartItemList);

                entities.SaveChanges();
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/Products/RemoveFromCart")]
        public HttpResponseMessage RemoveFromCart([FromUri]int pid = 1)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;

                string userid = HttpContext.Current.User.Identity.GetUserId();
                List<CartEntity> CartItemList = new List<CartEntity>();
                CartItemList = JsonConvert.DeserializeObject<List<CartEntity>>(entities.UserInfos.FirstOrDefault(e => e.UserID == userid).Cart);

                CartEntity removeItem = CartItemList.Where(x => x.productID == pid).FirstOrDefault();
                CartItemList.Remove(removeItem);

                var entity = entities.UserInfos.FirstOrDefault(e => e.UserID == userid);
                entity.Cart = JsonConvert.SerializeObject(CartItemList);

                entities.SaveChanges();
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }



        [HttpGet]
        public HttpResponseMessage LoadAllProduct()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    //var b = entities.Products.Include("brands").Select(x => new { brandName = x.Brand.BrandName, productID = x.ProductID, productName = x.ProductName}).ToList();
                    var a = Request.CreateResponse(HttpStatusCode.OK, entities.Products.ToList());
                    //var a = Request.CreateResponse(HttpStatusCode.OK, b.ToList());
                    return a;
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
        [Authorize]
        public HttpResponseMessage Post([FromBody] Product product)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    entities.Products.Add(product);
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
        public HttpResponseMessage Put(int id, [FromBody]Product product)
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
                            "Product with Id " + id.ToString() + " not found to update");
                    }
                    else
                    {
                        entity.UserID = product.UserID;
                        entity.ProductTypeID = product.ProductTypeID;
                        entity.BrandID = product.BrandID;
                        entity.ProductName = product.ProductName;
                        entity.Detail = product.Detail;
                        entity.Stock = product.Stock;
                        entity.Price = product.Price;
                        entity.CreationDate = product.CreationDate;

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
