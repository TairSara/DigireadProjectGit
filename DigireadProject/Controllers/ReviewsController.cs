using System;
using System.Linq;
using System.Web.Mvc;

namespace DigireadProject.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly libraryProject_digireadEntities db = new libraryProject_digireadEntities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBookReview(int bookId, int rating, string reviewText)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "יש להתחבר למערכת" });
            }

            int userId = (int)Session["UserID"];

            // בדיקה האם המשתמש כבר דירג את הספר הזה
            var existingReview = db.Reviews.FirstOrDefault(r => 
                r.UserID == userId && r.BookID == bookId && r.RatingBook.HasValue);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "כבר דירגת את הספר הזה בעבר" });
            }

            // יצירת דירוג חדש
            var review = new Reviews
            {
                UserID = userId,
                BookID = bookId,
                RatingBook = rating,
                ReviewTextBook = reviewText,
                ReviewDateBook = DateTime.Now
            };

            db.Reviews.Add(review);
        
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "אירעה שגיאה בשמירת הדירוג" });
            }
        }
        
        // מתודה לבדיקה האם משתמש כבר דירג ספר מסוים
        [HttpGet]
        public ActionResult HasUserReviewedBook(int bookId)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { hasReviewed = false }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["UserID"];
            bool hasReviewed = db.Reviews.Any(r => 
                r.UserID == userId && r.BookID == bookId && r.RatingBook.HasValue);

            return Json(new { hasReviewed }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public ActionResult HasUserReviewedWebsite()
        {
            if (Session["UserID"] == null)
            {
                return Json(new { hasReviewed = false }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["UserID"];
            bool hasReviewed = db.Reviews.Any(r => 
                r.UserID == userId && r.RatingWeb.HasValue);

            return Json(new { hasReviewed }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWebsiteReview(int rating, string reviewText)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "יש להתחבר למערכת" });
            }

            int userId = (int)Session["UserID"];

            // בדיקה האם המשתמש כבר דירג את האתר
            var existingReview = db.Reviews.FirstOrDefault(r => 
                r.UserID == userId && r.RatingWeb.HasValue);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "כבר דירגת את האתר בעבר" });
            }

            // יצירת דירוג חדש
            var review = new Reviews
            {
                UserID = userId,
                RatingWeb = rating,
                ReviewTextWeb = reviewText,
                ReviewDateWeb = DateTime.Now
            };

            db.Reviews.Add(review);
        
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "אירעה שגיאה בשמירת הדירוג" });
            }
        }
    }
}