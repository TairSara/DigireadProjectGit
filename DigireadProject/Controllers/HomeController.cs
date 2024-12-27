using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly libraryProject_digireadEntities db = new libraryProject_digireadEntities();
        
        public ActionResult HomePage()
        {
            var popularBooks = db.Books
                .Where(b => b.Reviews.Any(r => r.RatingBook.HasValue)) // רק ספרים עם דירוגים
                .Select(b => new
                {
                    Book = b,
                    AverageRating = b.Reviews
                        .Where(r => r.RatingBook.HasValue)
                        .Select(r => (decimal)r.RatingBook.Value)
                        .DefaultIfEmpty(0m)
                        .Average(),
                    ReviewCount = b.Reviews.Count(r => r.RatingBook.HasValue)
                })
                .Where(x => x.AverageRating >= 4m)
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .Take(6)
                .AsEnumerable()
                .Select(x => new PopularBookViewModel
                {
                    BookID = x.Book.BookID,
                    Title = x.Book.Title,
                    MainAuthor = x.Book.MainAuthor,
                    ImageSrc = x.Book.ImageSrc,
                    AverageRating = x.AverageRating,
                    ReviewCount = x.ReviewCount,
                    Reviews = x.Book.Reviews
                        .Where(r => r.RatingBook.HasValue)
                        .OrderByDescending(r => r.ReviewDateBook)
                        .Select(r => new ReviewDetails
                        {
                            Rating = r.RatingBook ?? 0,
                            Comment = r.ReviewTextBook ?? string.Empty,
                            ReviewDate = r.ReviewDateBook ?? DateTime.Now
                        })
                        .Take(2) // רק 2 ביקורות אחרונות
                        .ToList()
                })
                .ToList();

            var websiteReviews = db.Reviews
                .Where(r => r.RatingWeb.HasValue)
                .Select(r => new WebsiteReviewViewModel
                {
                    Username = r.Users.Username,
                    Rating = r.RatingWeb.Value,
                    ReviewText = r.ReviewTextWeb,
                    ReviewDate = r.ReviewDateWeb.Value
                })
                .OrderByDescending(r => r.ReviewDate)
                .Take(6)
                .ToList();

            return View(new HomeViewModel 
            { 
                PopularBooks = popularBooks,
                WebsiteReviews = websiteReviews 
            });
        }
    }
}






