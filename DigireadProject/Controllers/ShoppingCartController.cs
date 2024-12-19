using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;  

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
        
        [HttpGet]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout()
        {
            try
            {
                int userId = GetCurrentUserId();
                var cartItems = db.ShoppingCart
                    .Include(s => s.Books)
                    .Where(s => s.UserID == userId)
                    .ToList();

                foreach (var item in cartItems)
                {
                    if (item.IsRental.GetValueOrDefault())
                    {
                        // טיפול בהשאלה
                        var rental = new Rentals  // שימי לב - משתמשים במודל Rentals ולא ב-ViewModel
                        {
                            UserID = userId,
                            BookID = item.BookID,
                            RentalDate = DateTime.Now,
                            ReturnDate = DateTime.Now.AddDays(30),
                            ImageSrc = item.Books.ImageSrc,
                            DaysOverdue = 0
                        };
    
                        db.Rentals.Add(rental);  
                    }
                    else
                    {
                        // טיפול ברכישה
                        var purchase = new Purchases
                        {
                            UserID = userId,
                            BookID = item.BookID,
                            PurchaseDate = DateTime.Now,
                            PaymentStatus = true,
                            PaymentMethod = "כרטיס אשראי" // או כל שיטת תשלום אחרת שתרצי
                        };
                
                        db.Purchases.Add(purchase);
                    }

                    // עדכון מלאי הספר
                    var book = item.Books;
                    book.StockQuantity -= item.Quantity ?? 1;

                    // מחיקת הפריט מהעגלה
                    db.ShoppingCart.Remove(item);
                }

                db.SaveChanges();
                return RedirectToAction("Success", "Order");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Order");
            }
        }
    }
}