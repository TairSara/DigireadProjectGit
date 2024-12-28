using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq; // חשוב להוסיף

namespace DigireadProject.Controllers
{
    [Authorize] // נוסיף את זה ברמת ה-Controller
    public class WishlistController : Controller
    {
        private readonly libraryProject_digireadEntities db;

        public WishlistController()
        {
            db = new libraryProject_digireadEntities();
        }

        // הפונקציה שמציגה את רשימת המשאלות
        public async Task<ActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var wishlistItems = await db.Wishlist
                .Where(w => w.UserID == userId)
                .Include(w => w.Books)
                .ToListAsync();

            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<JsonResult> Toggle(int bookId)
        {
            try
            {
                // בדיקה ראשונית
                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Json(new { success = false, message = "משתמש לא מחובר" });
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "משתמש לא זוהה" });
                }

                var existingItem = await db.Wishlist
                    .FirstOrDefaultAsync(w => w.UserID == userId && w.BookID == bookId);

                if (existingItem != null)
                {
                    db.Wishlist.Remove(existingItem);
                    await db.SaveChangesAsync();
                    return Json(new { success = true, isAdded = false });
                }
                else
                {
                    db.Wishlist.Add(new Wishlist
                    {
                        UserID = userId,
                        BookID = bookId,
                        DateAdded = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                    return Json(new { success = true, isAdded = true });
                }
            }
            catch (Exception ex)
            {
                // הדפסת השגיאה המלאה
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Remove(int wishlistId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var wishlistItem = await db.Wishlist
                    .FirstOrDefaultAsync(w => w.WishlistID == wishlistId && w.UserID == userId);

                if (wishlistItem != null)
                {
                    db.Wishlist.Remove(wishlistItem);
                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "פריט לא נמצא" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserID ?? 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}