using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private libraryProject_digireadEntities db = new libraryProject_digireadEntities();

        public ActionResult Cart()
        {
            int userId = GetCurrentUserId();
            var cartItems = db.ShoppingCart
                .Include(s => s.Books)
                .Where(s => s.UserID == userId)
                .ToList();
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int bookId, bool isRental)
        {
            try
            {
                int userId = GetCurrentUserId();
                var book = db.Books.Find(bookId);
                if (book == null)
                    return Json(new { success = false, message = "הספר לא נמצא" });

                var existingItem = db.ShoppingCart
                    .FirstOrDefault(s => s.UserID == userId && s.BookID == bookId && s.IsRental == isRental);

                if (existingItem != null)
                {
                    if (existingItem.Quantity >= book.StockQuantity)
                    {
                        return Json(new { success = false, message = "אין מספיק מלאי" });
                    }
                    existingItem.Quantity++;
                }
                else
                {
                    if (book.StockQuantity < 1)
                    {
                        return Json(new { success = false, message = "אין מלאי" });
                    }

                    var cartItem = new ShoppingCart
                    {
                        UserID = userId,
                        BookID = bookId,
                        Price = isRental ? book.RentalPrice : book.PurchasePrice,
                        DateAdded = DateTime.Now,
                        Quantity = 1,
                        IsRental = isRental,
                        ImageSrc = book.ImageSrc
                    };
                    db.ShoppingCart.Add(cartItem);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int cartId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var cartItem = db.ShoppingCart
                    .FirstOrDefault(c => c.CartID == cartId && c.UserID == userId);

                if (cartItem == null)
                    return Json(new { success = false, message = "פריט לא נמצא" });

                db.ShoppingCart.Remove(cartItem);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int cartId, int quantity)
        {
            try
            {
                int userId = GetCurrentUserId();
                var cartItem = db.ShoppingCart
                    .Include(s => s.Books)
                    .FirstOrDefault(c => c.CartID == cartId && c.UserID == userId);

                if (cartItem == null)
                    return Json(new { success = false, message = "פריט לא נמצא" });

                if (quantity > cartItem.Books.StockQuantity)
                    return Json(new { success = false, message = "אין מספיק מלאי" });

                if (quantity < 1)
                    return Json(new { success = false, message = "כמות לא תקינה" });

                cartItem.Quantity = quantity;
                db.SaveChanges();

                return Json(new { success = true });
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
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}