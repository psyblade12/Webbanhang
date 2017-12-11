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
