using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Webbanhang.Controllers
{
    public class RatingController : ApiController
    {
        // api/product
        [HttpGet]
        public HttpResponseMessage LoadAllRating()
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    return Request.CreateResponse(HttpStatusCode.OK, entities.Ratings.ToList());
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("api/Rating/GetAverageRatingsofProduct")]
        [AllowAnonymous]
        public HttpResponseMessage GetAverageRatingsofProduct([FromUri]int pid)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    var listofRatings = entities.Ratings.Where(x => x.ProductID == pid).ToList();
                    int countRating = listofRatings.Count();
                    double averageRating = Convert.ToDouble(listofRatings.Average(x => x.Rating1));
                    var result = new { average = averageRating, count = countRating };
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }

        //Lấy danh sách các thống kê nhà bán hàng có rating trung bình từ cao xuống thấp
        //Cái này có thể lấy ra từ Analysis, nhưng nếu thấy method Analysis quá phức tạo thì dùng cái này
        [HttpGet]
        [Route("api/Rating/GetListOfAverageRatingListByMerchant")]
        [AllowAnonymous]
        public HttpResponseMessage GetListOfAverageRatingListByMerchant(string sort = "asc", string skip ="0", string take = "5")
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var detailRatingbyMerchant = entities.Ratings.GroupBy(x => new { x.Product.UserID, x.Product.AspNetUser.UserName }).Select(g => new { g.Key.UserID, g.Key.UserName, AverageRating = g.Average(x => x.Rating1), RatingTime = g.Count() }).ToList();
                    if(sort !=null)
                    {
                        if (sort == "dsc")
                        {
                            detailRatingbyMerchant = detailRatingbyMerchant.OrderByDescending(x => x.AverageRating).ToList();
                        }
                        if (sort == "asc")
                        {
                            detailRatingbyMerchant = detailRatingbyMerchant.OrderBy(x => x.AverageRating).ToList();
                        }
                    }
                    if (take != null)
                    {
                        int tempTake = Convert.ToInt32(take);
                        if (skip != null)
                        {
                            int tempSkip = Convert.ToInt32(skip);
                            detailRatingbyMerchant = detailRatingbyMerchant.Skip(tempSkip).Take(tempTake).ToList();
                        }
                        else
                        {
                            detailRatingbyMerchant = detailRatingbyMerchant.Take(tempTake).ToList();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, detailRatingbyMerchant);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }

        //Lấy danh sách các thống kê các sản phẩm có rating trung bình từ cao xuống thấp
        //Cái này có thể lấy ra từ Analysis, nhưng nếu thấy method Analysis quá phức tạo thì dùng cái này
        [HttpGet]
        [Route("api/Rating/GetListOfAverageRatingListByProduct")]
        [AllowAnonymous]
        public HttpResponseMessage GetListOfAverageRatingListByProduct(string sort = null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var detailRatingByItem = entities.Ratings.GroupBy(x => new { x.ProductID, x.Product.ProductName }).Select(g => new { g.Key.ProductID, g.Key.ProductName, AverageRating = g.Average(x => x.Rating1), RatingTime = g.Count() }).OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.RatingTime).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, detailRatingByItem);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }

        [HttpGet]
        [Route("api/Rating/RateaProduct")]
        [AllowAnonymous]
        public HttpResponseMessage RateaProduct([FromUri]int pid, int r)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();

                    //Kiểm tra r (điểm) phải >0 và < 10
                    if(r<0 || r>10)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Phải rate từ 0-> 10");
                    }
                    //Kiểm tra xem người đó đã mua hàng hay chưa, nếu chưa mua thì không được rate
                    var checkBought = entities.OrderItems.Where(x => x.Order.UserID == currentUserID && x.ProductID == pid).FirstOrDefault();
                    if (checkBought == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Bạn chưa mua sản phẩm này nên không được đánh giá");
                    }

                    //Kiểm tra người Rate có phải chủ của Product không. Chủ product ko hể rate sản phẩm của chính mình
                    string IDofProductOwner = entities.Products.FirstOrDefault(x => x.ProductID == pid).UserID;
                    if(currentUserID == IDofProductOwner)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Bạn không thể rate sản phẩm của chính mình");
                    }

                    //Kiểm tra xem đã vote chưa, 1 người chỉ được vote sản phẩm 1 lần
                    var checkIfRated = entities.Ratings.FirstOrDefault(x => x.ProductID == pid && x.UserID == currentUserID);
                    if(checkIfRated != null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,"Bạn đã rate rồi");
                    }

                    Rating newRating = new Rating();
                    newRating.ProductID = pid;
                    newRating.Rating1 = r;
                    newRating.UserID = currentUserID;
                    entities.Ratings.Add(newRating);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "POST OK");
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }

        [HttpGet]
        [Route("api/Rating/EditMyRating")]
        [AllowAnonymous]
        public HttpResponseMessage EditMyRating([FromUri]int pid, int nr)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var checkIfRated = entities.Ratings.FirstOrDefault(x => x.ProductID == pid && x.UserID == currentUserID);
                    if (checkIfRated != null)
                    {
                        checkIfRated.Rating1 = nr;
                        entities.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Đã sửa rating");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
                    }
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }

        [HttpGet]
        [Route("api/Rating/DeleteMyRating")]
        [AllowAnonymous]
        public HttpResponseMessage DeleteMyRating([FromUri]int pid)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var checkIfRated = entities.Ratings.FirstOrDefault(x => x.ProductID == pid && x.UserID == currentUserID);
                    if (checkIfRated != null)
                    {
                        entities.Ratings.Remove(checkIfRated);
                        entities.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, "Đã xóa Rating");
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
                    }
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }


        [HttpGet]
        [Route("api/Rating/HaveIrated")]
        [AllowAnonymous]
        public HttpResponseMessage HaveIrated([FromUri]int pid)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var checkIfRated = entities.Ratings.FirstOrDefault(x => x.ProductID == pid && x.UserID == currentUserID);
                    if (checkIfRated != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { rated = true, rating = checkIfRated.Rating1 });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { rated = false, rating = -1 });
                    }
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, "Có lỗi xảy ra");
            }
        }
        [HttpGet]
        [Route("api/Rating/GetAverageRating")]
        [AllowAnonymous]
        public HttpResponseMessage GetAverageRating([FromUri]string userid = null, string productid =null)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    if (userid != null)
                    {
                        var average = entities.Ratings.Where(x => x.Product.UserID == userid).Average(x => x.Rating1);
                        string merchantName = entities.Ratings.FirstOrDefault(x => x.Product.UserID == userid).Product.UserID;
                        var result = new { MerchantName = merchantName, averageRating = average };
                        return Request.CreateResponse(HttpStatusCode.OK, result);
                    }
                    
                    if (productid != null)
                    {
                        int pid = Convert.ToInt32(productid);
                        var find = entities.Products.FirstOrDefault(x => x.ProductID == pid);
                        if (find != null)
                        {
                            string uid = find.UserID;
                            var average = entities.Ratings.Where(x => x.Product.UserID == uid).Average(x => x.Rating1);
                            string merchantName = entities.AspNetUsers.FirstOrDefault(x => x.Id == uid).UserName;
                            var result = new { MerchantName = merchantName, averageRating = average };
                            return Request.CreateResponse(HttpStatusCode.OK, result);
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        // api/product/1
        [HttpGet]
        public HttpResponseMessage LoadRatingsByID(int id)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Ratings.FirstOrDefault(e => e.RatingID == id);
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
        public HttpResponseMessage Post([FromBody] Rating rating)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    string currentUserID = User.Identity.GetUserId();
                    var checkIfRated = entities.Ratings.FirstOrDefault(x => x.ProductID == rating.Rating1 && x.UserID == currentUserID);
                    if (checkIfRated != null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Bạn đã rate rồi");
                    }
                    entities.Configuration.ProxyCreationEnabled = false;
                    rating.UserID = currentUserID;
                    entities.Ratings.Add(rating);
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
        public HttpResponseMessage Put(int id, [FromBody]Rating rating)
        {
            try
            {
                using (WebbanhangDBEntities entities = new WebbanhangDBEntities())
                {
                    entities.Configuration.ProxyCreationEnabled = false;
                    var entity = entities.Ratings.FirstOrDefault(e => e.RatingID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Product with Id " + id.ToString() + " not found to update");
                    }
                    else
                    {
                        entity.Rating1 = rating.Rating1;
                        entity.Detail = rating.Detail;
                        entity.UserID = rating.UserID;
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
                    var entity = entities.Ratings.FirstOrDefault(e => e.RatingID == id);
                    if (entity == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "Rating with Id = " + id.ToString() + " not found to delete");
                    }
                    else
                    {
                        entities.Ratings.Remove(entity);
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
