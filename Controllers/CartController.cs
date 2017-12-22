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
    public class FullCartEntity
    {
        public int productID { get; set; }
        public string productName { get; set; }
        public string productImage { get; set; }
        public int quantity { get; set; }
        public int sumprice { get; set; }
    }
    public class CartController : ApiController
    {
        [HttpGet]
        [Route("api/Cart/ViewCart")]
        public HttpResponseMessage ViewCart()
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                string userid = HttpContext.Current.User.Identity.GetUserId();
                List<CartEntity> CartItemList = new List<CartEntity>();
                CartItemList = JsonConvert.DeserializeObject<List<CartEntity>>(entities.UserInfos.FirstOrDefault(e => e.UserID == userid).Cart);
                List<FullCartEntity> result = new List<FullCartEntity>();
                foreach (CartEntity x in CartItemList)
                {
                    FullCartEntity newtoAdd = new FullCartEntity();
                    var productInfo = entities.Products.Where(a => a.ProductID == x.productID).FirstOrDefault();
                    newtoAdd.productID = x.productID;
                    newtoAdd.productName = productInfo.ProductName;
                    newtoAdd.productImage = productInfo.ProductImage;
                    newtoAdd.quantity = x.quantity;
                    newtoAdd.sumprice = x.quantity * Convert.ToInt32(productInfo.Price);
                    result.Add(newtoAdd);
                }
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }

        [HttpGet]
        [Route("api/Cart/AddToCart")]
        public HttpResponseMessage AddToCart([FromUri]int pid = 1, int q = 1)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;

                string userid = HttpContext.Current.User.Identity.GetUserId();
                List<CartEntity> CartItemList = new List<CartEntity>();
                CartItemList = JsonConvert.DeserializeObject<List<CartEntity>>(entities.UserInfos.FirstOrDefault(e => e.UserID == userid).Cart);
                
                //Kiểm tra xem sản phẩm đang định bỏ vào giỏ hàng có phải của chính mình hay không:
                var producttoCheck = entities.Products.Where(x => x.ProductID == pid).FirstOrDefault();
                if (producttoCheck.UserID == userid)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Không được mua hàng của chính mình.");
                }

                //Kiểm tra xem sản phẩm đang định bỏ vào giỏ hàng có phải nhỏ hơn stock hay không:
                var checkCart = CartItemList.FirstOrDefault(x => x.productID == pid);
                if (checkCart != null)
                {
                    if ( q + checkCart.quantity > producttoCheck.Stock)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Chỉ được đặt mua số lượng nhỏ hơn stock.");
                    }
                }

                if(q > producttoCheck.Stock)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Chỉ được đặt mua số lượng nhỏ hơn stock.");
                }

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
            return Request.CreateResponse(HttpStatusCode.OK,"Đã thêm vào giỏ hàng");
        }

        [HttpGet]
        [Route("api/Cart/RemoveFromCart")]
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
            return Request.CreateResponse(HttpStatusCode.OK, "Đã xóa khỏi giỏ hàng");
        }

        [HttpGet]
        [Route("api/Cart/EditCart")]
        public HttpResponseMessage EditCart([FromUri]int pid = 1, int q = 1)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;

                string userid = HttpContext.Current.User.Identity.GetUserId();
                List<CartEntity> CartItemList = new List<CartEntity>();
                CartItemList = JsonConvert.DeserializeObject<List<CartEntity>>(entities.UserInfos.FirstOrDefault(e => e.UserID == userid).Cart);

                //Kiểm tra xem sản phẩm đang định bỏ vào giỏ hàng có phải của chính mình hay không:
                var producttoCheck = entities.Products.Where(x => x.ProductID == pid).FirstOrDefault();
                if (producttoCheck.UserID == userid)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Không được mua hàng của chính mình.");
                }

                //Kiểm tra xem sản phẩm đang định bỏ vào giỏ hàng có phải nhỏ hơn stock hay không:
                var checkCart = CartItemList.FirstOrDefault(x => x.productID == pid);
                if (checkCart != null)
                {
                    if (q + checkCart.quantity > producttoCheck.Stock)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Chỉ được đặt mua số lượng nhỏ hơn stock.");
                    }
                }

                if (q > producttoCheck.Stock)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Chỉ được đặt mua số lượng nhỏ hơn stock.");
                }

                //Tìm thử xem có sẵn chưa, nếu có rồi thì chỉ cộng thêm số lượng
                bool flag = false;
                foreach (CartEntity item in CartItemList)
                {
                    if (item.productID == pid)
                    {
                        item.quantity = q;
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
            return Request.CreateResponse(HttpStatusCode.OK, "Đã sửa thành công");
        }

        [HttpGet]
        [Route("api/Cart/CheckValid")]
        public HttpResponseMessage CheckValid([FromUri]int pid = 1, int q = 1, int qInCart = 0)
        {
            using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
            {
                entities.Configuration.ProxyCreationEnabled = false;
                var producttoCheck = entities.Products.Where(x => x.ProductID == pid).FirstOrDefault();
                //Kiểm tra xem sản phẩm đang định bỏ vào giỏ hàng có phải nhỏ hơn stock hay không:
                if (q + qInCart > producttoCheck.Stock)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, true);
                }
            }
        }
    }
}
