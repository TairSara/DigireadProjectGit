using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly libraryProject_digireadEntities db = new libraryProject_digireadEntities();
        
        public async Task<ActionResult> HomePage()
        {
            ViewBag.TotalBooks = await db.Books.CountAsync();

            var popularBooks = db.Books
                .Where(b => b.Reviews.Any(r => r.RatingBook.HasValue))
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
                .Take(8)
                .AsEnumerable()
                .Select(x => new PopularBookViewModel
                {
                    BookID = x.Book.BookID,
                    Title = x.Book.Title,
                    MainAuthor = x.Book.MainAuthor,
                    ImageSrc = x.Book.ImageSrc,
                    AverageRating = x.AverageRating,
                    Description = string.IsNullOrEmpty(x.Book.Description) ? "תקציר לא זמין" : x.Book.Description,
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
                        .Take(2)
                        .ToList()
                })
                .ToList();

            var onSaleBooks = db.Books
                .Where(b => b.PurchasePrice.HasValue && 
                            b.OriginalPrice.HasValue && 
                            b.PurchasePrice < b.OriginalPrice)
                .AsEnumerable()
                .Select(b => new PopularBookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title,
                    MainAuthor = b.MainAuthor,
                    ImageSrc = b.ImageSrc,
                    OriginalPrice = b.OriginalPrice.Value,
                    PurchasePrice = b.PurchasePrice.Value,
                    Description = b.Description ?? "תקציר לא זמין"
                })
                .ToList();

            var romanceSeriesIds = new[] { 21,22,301,302 };
            var romanceSeries = db.Books
                .Where(b => romanceSeriesIds.Contains(b.BookID))
                .Select(b => new PopularBookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title,
                    MainAuthor = b.MainAuthor,
                    ImageSrc = b.ImageSrc,
                    Description = b.Description ?? "תקציר לא זמין",
                    AverageRating = b.Reviews
                        .Where(r => r.RatingBook.HasValue)
                        .Select(r => (decimal)r.RatingBook.Value)
                        .DefaultIfEmpty(0m)
                        .Average()
                })
                .ToList();

            var scifiSeriesIds = new[] { 13,14,15,16,17,18 };
            var scifiSeries = db.Books
                .Where(b => scifiSeriesIds.Contains(b.BookID))
                .Select(b => new PopularBookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title,
                    MainAuthor = b.MainAuthor,
                    ImageSrc = b.ImageSrc,
                    Description = b.Description ?? "תקציר לא זמין",
                    AverageRating = b.Reviews
                        .Where(r => r.RatingBook.HasValue)
                        .Select(r => (decimal)r.RatingBook.Value)
                        .DefaultIfEmpty(0m)
                        .Average()
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
                WebsiteReviews = websiteReviews,
                OnSaleBooks = onSaleBooks,
                RomanceSeries = romanceSeries,
                SciFiSeries = scifiSeries
            });
        }
        
        public ActionResult About()
        {
            return View("~/Views/Home/About.cshtml");
        }
            
    }
    
    
}